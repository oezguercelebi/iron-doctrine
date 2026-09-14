using Godot;
using IronDoctrine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronDoctrine.Client;

public partial class MatchClient : Node3D
{
    private IMatch _match = null!;
    private Func<IMatch> _rematch = null!;
    private MatchSnapshot _snapshot = new();
    private Battlefield _field = null!;
    private FieldHud _hud = null!;
    private FieldAudio _audio = null!;
    private readonly HashSet<int> _selection = new();
    private readonly Dictionary<int, int[]> _groups = new();
    private int _slot;
    private double _accumulator;
    private long _eventTick = -1;
    private int _eventOrdinal = -1;
    private OrderKind? _mode;
    private string _buildRole = "";
    private bool _leftDown;
    private bool _dragged;
    private Vector2 _dragOrigin;
    private int _lastGroup;
    private ulong _lastGroupTime;
    private double _noticeLife;
    private bool _helpPausedMatch;
    private ProofPilot? _proof;
    private float _proofSpeed = 1;
    private double _resultTime;
    private bool _proofSaved;

    /// <summary>The bootstrap supplies the match before this Node enters the tree.</summary>
    public void Initialize(IMatch match, Func<IMatch> rematch) { _match = match; _rematch = rematch; }

    public override void _Ready()
    {
        if (_match == null) throw new InvalidOperationException("Initialize must be called before AddChild.");
        _slot = _match.Setup.Slots.First(s => s.Occupant == Occupant.Player).Index;
        _field = new Battlefield();
        _field.Initialize(_match.Config, _match.Setup, _slot);
        AddChild(_field);
        _audio = new FieldAudio();
        AddChild(_audio);
        var layer = new CanvasLayer();
        AddChild(layer);
        _hud = new FieldHud { Config = _match.Config, Field = _field, Selection = _selection, Command = HandleCommand, MapClick = HandleMapClick };
        layer.AddChild(_hud);
        string[] args = OS.GetCmdlineUserArgs();
        if (args.Contains("--proof-play"))
        {
            _proof = new ProofPilot(_match, _slot, ReceiveProofOrder);
            string? speed = args.FirstOrDefault(a => a.StartsWith("--proof-speed=", StringComparison.Ordinal));
            if (speed != null && float.TryParse(speed.Split('=')[1], out float parsed)) _proofSpeed = Mathf.Clamp(parsed, 1, 16);
            _hud.ProofText = $"ORDINARY-ORDER PLAY PROOF  ·  {_proofSpeed:0.#}× clock";
            GD.Print("PROOF_PLAY_STARTED ordinary player orders; no simulation access");
        }
        RefreshSnapshot();
        var dozer = _snapshot.Entities.FirstOrDefault(e => e.OwnerSlot == _slot && e.RoleId == "build.dozer");
        if (dozer != null) _selection.Add(dozer.Id);
        _field.ShowSnapshot(_snapshot, _selection);
        GD.Print($"CLIENT_READY slot={_slot} entities={_snapshot.Entities.Length} models={_field.LoadedModelCount}");
    }

    public override void _Process(double delta)
    {
        if (_match == null || _hud == null) return;
        if (_snapshot.Phase == MatchPhase.Running && !_snapshot.Paused)
        {
            _accumulator += delta * _proofSpeed;
            double step = 1.0 / _match.Config.Rules.TickRate;
            // A slow render can take several fixed ticks. Never change Step's size or skip event snapshots.
            int budget = _proof == null ? 8 : 128;
            for (int i = 0; i < budget && _accumulator >= step; i++)
            {
                _proof?.Think(_snapshot);
                _match.Step();
                _accumulator -= step;
                RefreshSnapshot();
                if (_snapshot.Phase != MatchPhase.Running || _snapshot.Paused) { _accumulator = 0; break; }
            }
        }
        else _accumulator = 0;
        if (!_hud.MenuOpen && !_hud.HelpOpen && _snapshot.Phase == MatchPhase.Running)
        {
            var direction = Vector2.Zero;
            if (Input.IsPhysicalKeyPressed(Key.W) || Input.IsPhysicalKeyPressed(Key.Up)) direction.Y--;
            if (Input.IsPhysicalKeyPressed(Key.S) || Input.IsPhysicalKeyPressed(Key.Down)) direction.Y++;
            if (Input.IsPhysicalKeyPressed(Key.A) || Input.IsPhysicalKeyPressed(Key.Left)) direction.X--;
            if (Input.IsPhysicalKeyPressed(Key.D) || Input.IsPhysicalKeyPressed(Key.Right)) direction.X++;
            var mouse = GetViewport().GetMousePosition();
            var size = GetViewport().GetVisibleRect().Size;
            if (new Rect2(Vector2.Zero, size).HasPoint(mouse) && DisplayServer.WindowIsFocused())
            {
                if (mouse.X < 8) direction.X--;
                if (mouse.X > size.X - 8) direction.X++;
                if (mouse.Y < 8) direction.Y--;
                if (mouse.Y > size.Y - 8) direction.Y++;
            }
            if (direction.LengthSquared() > 0) _field.Pan(direction.Normalized(), delta);
            _hud.Hovered = _hud.BlocksWorld(mouse) ? null : _field.Pick(mouse);
            if (_mode == OrderKind.Build && _buildRole.Length > 0)
            {
                int builder = OwnedSelection().FirstOrDefault();
                var point = _field.ScreenPoint(mouse);
                var placement = _match.CanPlace(_slot, builder, _buildRole, point);
                _hud.PlacementText = placement.Allowed ? "Legal footprint · Left click to construct" : placement.Reason;
                _hud.PlacementAllowed = placement.Allowed;
                _field.SetGhost(_buildRole, point, placement.Allowed && !_hud.BlocksWorld(mouse));
            }
        }
        _noticeLife -= delta;
        if (_noticeLife <= 0 && _hud.Notice.Length > 0) _hud.Notice = "";
        if (_snapshot.Phase == MatchPhase.Finished && _proof != null)
        {
            _resultTime += delta;
            if (_resultTime > 1.5 && !_proofSaved) SaveProof();
        }
        _hud.QueueRedraw();
    }

    private void RefreshSnapshot()
    {
        _snapshot = _match.Snapshot(_slot);
        _selection.RemoveWhere(id => !_snapshot.Entities.Any(e => e.Id == id));
        _hud.Snapshot = _snapshot;
        foreach (var evt in _snapshot.Events.OrderBy(e => e.Tick).ThenBy(e => e.Ordinal))
        {
            if (evt.Tick < _eventTick || (evt.Tick == _eventTick && evt.Ordinal <= _eventOrdinal)) continue;
            _eventTick = evt.Tick; _eventOrdinal = evt.Ordinal;
            _audio.Notify(evt.Id);
            string? message = evt.Id switch
            {
                "vo.funds" => "Insufficient funds.",
                "vo.power" => "Low power — radar and defenses offline.",
                "vo.building_done" => "Construction complete.",
                "vo.unit_ready" => "Unit ready.",
                "vo.upgrade_done" => "Research complete.",
                "vo.under_attack" => "Your base is under attack.",
                "vo.victory" => "Victory — enemy buildings eliminated.",
                "vo.defeat" => "Defeat.",
                _ => null
            };
            if (message != null) Notify(message);
            if (evt.Id == "fx.under_attack" || evt.Id == "vo.under_attack") _field.MarkOrder(evt.Position, new Color("ef835d"));
            if (evt.Id == "fx.promote") _field.MarkOrder(evt.Position, new Color("f2d78b"));
        }
        _field.ShowSnapshot(_snapshot, _selection);
        if (_snapshot.Phase == MatchPhase.Finished) { ClearMode(); _hud.MenuOpen = false; _hud.HelpOpen = false; }
    }

    public override void _Input(InputEvent input)
    {
        if (_hud == null) return;
        if (input is InputEventKey key && key.Pressed && !key.Echo) HandleKey(key);
        if (input is InputEventMouseMotion motion)
        {
            if (_leftDown)
            {
                _dragged |= motion.Position.DistanceTo(_dragOrigin) > 6;
                _hud.DragSelecting = _dragged;
                _hud.DragEnd = motion.Position;
            }
            if (Input.IsMouseButtonPressed(MouseButton.Middle) && !_hud.MenuOpen && !_hud.HelpOpen) _field.Drag(motion.Relative);
        }
        if (input is not InputEventMouseButton mouse) return;
        if (mouse.Pressed && mouse.ButtonIndex is MouseButton.WheelUp or MouseButton.WheelDown)
        {
            if (!_hud.BlocksWorld(mouse.Position)) _field.AdjustZoom(mouse.ButtonIndex == MouseButton.WheelUp ? -4 : 4);
            return;
        }
        if (mouse.ButtonIndex == MouseButton.Right && mouse.Pressed)
        {
            if (_hud.Click(mouse.Position, true)) return;
            if (_mode != null) { ClearMode(); return; }
            ContextOrder(_field.Pick(mouse.Position), _field.ScreenPoint(mouse.Position), mouse.ShiftPressed);
            return;
        }
        if (mouse.ButtonIndex != MouseButton.Left) return;
        if (mouse.Pressed)
        {
            if (_hud.Click(mouse.Position, false)) return;
            if (_mode != null)
            {
                ApplyMode(_field.Pick(mouse.Position), _field.ScreenPoint(mouse.Position), mouse.ShiftPressed);
                return;
            }
            _leftDown = true;
            _dragged = false;
            _dragOrigin = mouse.Position;
            _hud.DragStart = mouse.Position;
            _hud.DragEnd = mouse.Position;
        }
        else if (_leftDown)
        {
            _leftDown = false;
            _hud.DragSelecting = false;
            if (_dragged) BoxSelect(new Rect2(_dragOrigin, mouse.Position - _dragOrigin).Abs(), mouse.ShiftPressed);
            else Select(_field.Pick(mouse.Position), mouse.ShiftPressed);
            _field.ShowSnapshot(_snapshot, _selection);
        }
    }

    private void HandleKey(InputEventKey key)
    {
        Key code = key.PhysicalKeycode;
        if (code == Key.Escape)
        {
            if (_hud.HelpOpen) HandleCommand("help", "", 0);
            else if (_mode != null) ClearMode();
            else if (_snapshot.Phase == MatchPhase.Running) TogglePause();
            return;
        }
        if (code == Key.H) { HandleCommand("help", "", 0); return; }
        if (_hud.HelpOpen || _hud.MenuOpen || _snapshot.Phase != MatchPhase.Running) return;
        if (code >= Key.Key1 && code <= Key.Key9)
        {
            int group = (int)(code - Key.Key0);
            if (key.CtrlPressed || key.MetaPressed)
            {
                _groups[group] = OwnedSelection();
                Notify($"Group {group} assigned: {_groups[group].Length} units.");
            }
            else if (_groups.TryGetValue(group, out var ids))
            {
                if (!key.ShiftPressed) _selection.Clear();
                foreach (int id in ids.Where(id => _snapshot.Entities.Any(e => e.Id == id))) _selection.Add(id);
                ulong now = Time.GetTicksMsec();
                if (_lastGroup == group && now - _lastGroupTime < 420) FocusSelection();
                _lastGroup = group; _lastGroupTime = now;
            }
            return;
        }
        string? action = code switch { Key.Z => "attack", Key.Q => "attackmove", Key.X => "stop", Key.G => "guard", Key.T => "waypoint", Key.F => "force", Key.R => "repair", Key.E => "enter", Key.C => "capture", Key.V => "exit", Key.Y => "rally", Key.Delete => "sell", Key.Tab => "builder", Key.Space => "focus", _ => null };
        if (action != null) HandleCommand(action, "", 0);
    }

    private int[] OwnedSelection() => SelectionState.Ordered(_snapshot, _selection).Where(e => e.OwnerSlot == _slot).Select(e => e.Id).ToArray();
    private void Select(EntitySnapshot? entity, bool append)
    {
        if (!append) _selection.Clear();
        if (entity != null)
        {
            if (append && _selection.Contains(entity.Id)) _selection.Remove(entity.Id);
            else _selection.Add(entity.Id);
        }
        _audio.Notify("sfx.click");
    }
    private void BoxSelect(Rect2 rect, bool append)
    {
        float viewH = GetViewport().GetVisibleRect().Size.Y;
        float units = _match.Config.Rules.UnitsPerWorldUnit;
        var candidates = _snapshot.Entities.Where(e =>
        {
            if (e.OwnerSlot != _slot || e.ContainerId != 0) return false;
            var role = _match.Config.Role(e.RoleId);
            var center = _field.ScreenPosition(e, role.IsBuilding ? 1 : .5f);
            float radius = Math.Max(role.IsInfantry ? 13 : 18, role.Radius / units * viewH / _field.Zoom);
            var closest = new Vector2(
                Mathf.Clamp(center.X, rect.Position.X, rect.End.X),
                Mathf.Clamp(center.Y, rect.Position.Y, rect.End.Y));
            return closest.DistanceSquaredTo(center) <= (radius + 7) * (radius + 7);
        }).ToArray();
        if (candidates.Any(e => !_match.Config.Role(e.RoleId).IsBuilding)) candidates = candidates.Where(e => !_match.Config.Role(e.RoleId).IsBuilding).ToArray();
        if (!append) _selection.Clear();
        foreach (var entity in candidates) _selection.Add(entity.Id);
        _audio.Notify("sfx.click");
    }

    private void ContextOrder(EntitySnapshot? target, WorldPoint point, bool append)
    {
        var actors = _snapshot.Entities.Where(e => OwnedSelection().Contains(e.Id)).ToArray();
        if (actors.Length == 0) return;
        var config = _match.Config;
        OrderKind kind = OrderKind.Move;
        if (actors.All(e => config.Role(e.RoleId).IsBuilding)) kind = OrderKind.Rally;
        else if (target != null && !target.IsRemembered)
        {
            var role = config.Role(target.RoleId);
            bool allied = Allied(target.OwnerSlot);
            if (target.RoleId == "map.dock" && actors.All(e => config.Role(e.RoleId).AbilityIds.Contains("ab.gather"))) kind = OrderKind.Gather;
            else if (allied && role.IsBuilding && target.Hp < target.MaxHp && actors.All(e => config.Role(e.RoleId).AbilityIds.Contains("ab.repair"))) kind = OrderKind.Repair;
            else if ((target.OwnerSlot == _slot || target.RoleId == "map.garrison" && target.OwnerSlot < 0) && role.Capacity > 0 && actors.All(e => config.Role(e.RoleId).IsInfantry)) kind = OrderKind.Enter;
            else if (CanCapture(target, actors)) kind = OrderKind.Capture;
            else if (target.OwnerSlot >= 0 && !allied) kind = OrderKind.Attack;
        }
        Submit(kind, actors.Select(e => e.Id).ToArray(), target?.Id ?? 0, point, append: append);
    }

    private bool Allied(int owner)
    {
        if (owner == _slot) return true;
        if (owner < 0) return false;
        var ourSlot = _match.Setup.Slots.First(s => s.Index == _slot);
        return ourSlot.Team > 0 && _match.Setup.Slots.Any(s => s.Index == owner && s.Team == ourSlot.Team);
    }

    private int[] SellActors(EntitySnapshot? target)
    {
        if (target == null || target.OwnerSlot != _slot) return Array.Empty<int>();
        var role = _match.Config.Role(target.RoleId);
        if (!role.IsBuilding || role.IsNeutral) return Array.Empty<int>();
        if (OwnedSelection().Contains(target.Id))
        {
            int[] selected = _snapshot.Entities
                .Where(e => OwnedSelection().Contains(e.Id))
                .Where(e => { var r = _match.Config.Role(e.RoleId); return r.IsBuilding && !r.IsNeutral; })
                .Select(e => e.Id)
                .ToArray();
            if (selected.Length > 0) return selected;
        }
        return target.Completed ? new[] { target.Id } : Array.Empty<int>();
    }

    private bool CanCapture(EntitySnapshot target, EntitySnapshot[] actors)
    {
        var role = _match.Config.Role(target.RoleId);
        return !target.IsRemembered && target.OwnerSlot >= 0 && !Allied(target.OwnerSlot) && role.IsBuilding && !role.IsNeutral
            && actors.Length > 0 && actors.All(e => e.OwnerSlot == _slot && _match.Config.Role(e.RoleId).AbilityIds.Contains("ab.capture"))
            && _snapshot.Player.Upgrades.Contains("up.capture");
    }

    private void HandleMapClick(WorldPoint point, bool right)
    {
        if (right) ContextOrder(null, point, Input.IsPhysicalKeyPressed(Key.Shift));
        else _field.SetFocus(point);
    }

    private void HandleCommand(string action, string product, int index, int actorId = 0)
    {
        _audio.Notify("sfx.click");
        if (action == "quit") { GetTree().Quit(); return; }
        if (action == "rematch") { Restart(); return; }
        if (action == "pause") { TogglePause(); return; }
        if (action == "help")
        {
            if (!_hud.HelpOpen)
            {
                _helpPausedMatch = !_snapshot.Paused && _snapshot.Phase == MatchPhase.Running;
                if (_helpPausedMatch) _match.SetPaused(true);
                _hud.HelpOpen = true;
            }
            else
            {
                _hud.HelpOpen = false;
                if (_helpPausedMatch && !_hud.MenuOpen) _match.SetPaused(false);
                _helpPausedMatch = false;
            }
            RefreshSnapshot();
            return;
        }
        if (_snapshot.Phase != MatchPhase.Running) return;
        if (action == "resign")
        {
            _match.SetPaused(false);
            _hud.MenuOpen = false;
            Submit(OrderKind.Resign, Array.Empty<int>());
            RefreshSnapshot();
            return;
        }
        if (action == "focus") { FocusSelection(); return; }
        if (action == "builder")
        {
            var builders = _snapshot.Entities.Where(e => e.OwnerSlot == _slot && e.RoleId == "build.dozer").OrderBy(e => e.Activity != EntityActivity.Idle).ThenBy(e => _selection.Contains(e.Id)).ToArray();
            if (builders.Length > 0) { Select(builders[0], false); FocusSelection(); }
            else Notify("No Dozer available. Train one at Command.");
            return;
        }
        var actors = OwnedSelection();
        if (action != "sell" && actors.Length == 0) { Notify("Select your units first."); return; }
        if (action is "queue" or "cancel")
        {
            // Bind the button to the producer whose queue was drawn, even if the selection changes before the click.
            var producer = _snapshot.Entities.FirstOrDefault(e => e.Id == actorId && e.OwnerSlot == _slot);
            if (producer == null || action == "cancel" && (index < 0 || index >= producer.Queue.Length)) { Notify("That production queue is no longer available."); return; }
            Submit(action == "queue" ? OrderKind.Queue : OrderKind.CancelQueue, new[] { producer.Id }, product: product, queueIndex: index);
            return;
        }
        if (action == "stop") { Submit(OrderKind.Stop, actors); ClearMode(); return; }
        _mode = action switch { "move" => OrderKind.Move, "attack" => OrderKind.Attack, "attackmove" => OrderKind.AttackMove, "guard" => OrderKind.Guard, "waypoint" => OrderKind.Waypoint, "force" => OrderKind.ForceAttack, "repair" => OrderKind.Repair, "gather" => OrderKind.Gather, "enter" => OrderKind.Enter, "capture" => OrderKind.Capture, "exit" => OrderKind.Exit, "rally" => OrderKind.Rally, "build" => OrderKind.Build, "sell" => OrderKind.Sell, _ => null };
        _buildRole = action == "build" ? product : "";
        _hud.ModeText = action == "build" ? "PLACE  /  " + _match.Config.Role(product).Label.ToUpperInvariant() : _mode switch { OrderKind.AttackMove => "ATTACK-MOVE", OrderKind.ForceAttack => "FORCE FIRE — ALLIES CAN BE HIT", OrderKind.Waypoint => "APPEND WAYPOINT", OrderKind.Sell => "SELL — click your building", OrderKind.Guard => "GUARD — UNIT OR POINT", _ => _mode?.ToString().ToUpperInvariant() ?? "" };
        _hud.PlacementText = "";
    }

    private void ApplyMode(EntitySnapshot? target, WorldPoint point, bool append)
    {
        if (_mode == null) return;
        var actors = OwnedSelection();
        if (_mode == OrderKind.Capture && (target == null || !CanCapture(target, _snapshot.Entities.Where(e => actors.Contains(e.Id)).ToArray())))
        {
            Notify("Capture requires researched Rifle units and a visible enemy building. Garrisons must be attacked.");
            _audio.Notify("sfx.invalid");
            return;
        }
        if (_mode == OrderKind.Sell)
        {
            int[] sell = SellActors(target);
            if (sell.Length == 0) { Notify("Click your building to sell."); _audio.Notify("sfx.invalid"); return; }
            if (Submit(OrderKind.Sell, sell) && !append) ClearMode();
            return;
        }
        if (_mode == OrderKind.Build)
        {
            if (actors.Length == 0) return;
            actors = new[] { actors[0] };
            var legal = _match.CanPlace(_slot, actors[0], _buildRole, point);
            if (!legal.Allowed) { Notify(legal.Reason); _audio.Notify("sfx.invalid"); return; }
        }
        bool accepted = Submit(_mode.Value, actors, target?.Id ?? 0, point, _buildRole, append);
        if (accepted && !append && _mode != OrderKind.Waypoint) ClearMode();
    }

    private bool Submit(OrderKind kind, int[] actors, int target = 0, WorldPoint position = default, string product = "", bool append = false, int queueIndex = 0)
    {
        var receipt = _match.Submit(CommandIntent.Create(_slot, kind, actors, target, position, product, append, queueIndex));
        if (!receipt.Accepted) { Notify(receipt.Reason); _audio.Notify("sfx.invalid"); return false; }
        if (kind == OrderKind.Rally) _audio.Notify("sfx.rally");
        if (kind == OrderKind.Build) _audio.Notify("sfx.place");
        if (kind is OrderKind.Move or OrderKind.AttackMove or OrderKind.Guard or OrderKind.Waypoint or OrderKind.Rally or OrderKind.Build or OrderKind.Exit) _field.MarkOrder(position, new Color("84d6c1"));
        Notify(kind == OrderKind.Queue ? $"Queued {_match.Config.Role(product).Label}." : kind == OrderKind.Build ? $"Constructing {_match.Config.Role(product).Label}." : kind == OrderKind.CancelQueue ? "Queue item cancelled." : kind + " order issued.");
        return true;
    }
    private void ReceiveProofOrder(MatchOrder order)
    {
        var result = _match.Submit(order);
        if (!result.Accepted) return;
        _selection.Clear();
        foreach (int actor in order.ActorIds) _selection.Add(actor);
        if (order.Kind is OrderKind.AttackMove or OrderKind.Attack) _field.SetFocus(order.Position);
        if (order.Kind == OrderKind.Build) _field.SetFocus(order.Position);
        Notify("Play proof: " + order.Kind + (order.ProductId.Length > 0 ? " " + _match.Config.Role(order.ProductId).Label : ""));
    }
    private void Notify(string text) { _hud.Notice = text; _noticeLife = 5; }
    private void ClearMode() { _mode = null; _buildRole = ""; _hud.ModeText = ""; _hud.PlacementText = ""; _field.SetGhost("", default, false); }
    private void FocusSelection()
    {
        var selected = _snapshot.Entities.Where(e => _selection.Contains(e.Id)).ToArray();
        if (selected.Length > 0) _field.SetFocus(new WorldPoint((int)selected.Average(e => e.Position.X), (int)selected.Average(e => e.Position.Z)));
    }
    private void TogglePause()
    {
        if (_snapshot.Phase != MatchPhase.Running) return;
        ClearMode();
        _hud.MenuOpen = !_hud.MenuOpen;
        _match.SetPaused(_hud.MenuOpen);
        RefreshSnapshot();
    }
    private void Restart()
    {
        var replacement = new MatchClient();
        replacement.Initialize(_rematch(), _rematch);
        GetParent().AddChild(replacement);
        QueueFree();
    }
    private void SaveProof()
    {
        _proofSaved = true;
        string? argument = OS.GetCmdlineUserArgs().FirstOrDefault(a => a.StartsWith("--proof-output=", StringComparison.Ordinal));
        string path = argument == null ? "user://proof-play.png" : argument["--proof-output=".Length..];
        if (DisplayServer.GetName() != "headless")
        {
            var image = GetViewport().GetTexture().GetImage();
            var error = image.SavePng(path);
            GD.Print($"PROOF_SCREENSHOT {ProjectSettings.GlobalizePath(path)} result={error}");
        }
        GD.Print($"PROOF_PLAY_FINISHED tick={_snapshot.Tick} result={(_snapshot.WinningSlots.Contains(_slot) ? "victory" : "defeat")} orders={_proof!.OrdersIssued} models={_field.LoadedModelCount}");
        if (OS.GetCmdlineUserArgs().Contains("--proof-quit")) GetTree().Quit();
    }
}
