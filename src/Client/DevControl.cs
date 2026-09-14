using Godot;
using IronDoctrine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronDoctrine.Client;

/// <summary>
/// Development control and Godot-backed input evidence. Drives production MatchClient._Input
/// through Viewport.PushInput. ProofPilot.Send / HandleCommand / Submit are not the evidence path.
/// </summary>
public partial class DevControl : Node
{
    public const int GuiWidth = 1440;
    public const int GuiHeight = 900;
    // Same legal pad the seam uses; camera fixture only — the click still goes through _Input.
    private static readonly WorldPoint GhostPad = new(3300, 4500);

    private MatchClient _client = null!;
    private string _scenario = "";
    private bool _quit;
    private string _framePath = "";
    private int _frames;
    private double _age;
    private double _hold;
    private long _pauseTick = -1;
    private int _phase;
    private bool _done;
    private Vector2 _placeScreen;
    private WorldPoint _placePoint;

    public void Initialize(MatchClient client, string scenario, bool quit, string framePath)
    {
        _client = client;
        _scenario = scenario;
        _quit = quit;
        _framePath = framePath;
        Name = "DevControl";
    }

    public override void _Ready()
    {
        GD.Print($"VERIFY_STARTED scenario={_scenario} viewport={GuiWidth}x{GuiHeight} display={DisplayServer.GetName()}");
    }

    public override void _Process(double delta)
    {
        if (_done) return;
        _frames++;
        _age += delta;
        try
        {
            switch (_scenario)
            {
                case "client.no_fog_world_leak":
                case "client.hud_chrome_no_move":
                    TickChromeLeak();
                    break;
                case "client.input_build_ghost_point":
                    TickBuildGhost();
                    break;
                case "match.pause_freezes_clock":
                    TickPause();
                    break;
                default:
                    Fail("unknown scenario id");
                    break;
            }
        }
        catch (Exception ex)
        {
            Fail(ex.Message);
        }
    }

    private void TickChromeLeak()
    {
        if (!HudReady()) return;
        if (_phase == 0)
        {
            AssertFogHud();
            var chrome = new Vector2(120, 36);
            Require(_client.Hud.BlocksWorld(chrome), "header chrome must block world orders");
            int before = _client.Submits.Count;
            Click(chrome, MouseButton.Right, true);
            Click(chrome, MouseButton.Right, false);
            Require(_client.Submits.Count == before, "HUD chrome right-click must not submit a world order");
            Require(!_client.Submits.Any(s => s.Order.Kind is OrderKind.Move or OrderKind.AttackMove or OrderKind.Build), "HUD chrome must not leak a world Move/Build");
            GD.Print("VERIFY_INPUT chrome=120,36 button=right submits=0");
            _phase = 1;
            return;
        }
        if (_phase == 1)
        {
            var world = new Vector2(GuiWidth / 2f, 380);
            Require(!_client.Hud.BlocksWorld(world), "world sample point must not be HUD chrome");
            Click(world, MouseButton.Right, true);
            Click(world, MouseButton.Right, false);
            var moves = _client.Submits.Where(s => s.Order.Kind == OrderKind.Move).ToArray();
            Require(moves.Length > 0 && moves[^1].Receipt.Accepted, "world right-click must dispatch a Move (negative control)");
            var move = moves[^1].Order;
            GD.Print($"VERIFY_INPUT world={world.X:0},{world.Y:0} kind=Move accepted=true pos={move.Position.X},{move.Position.Z}");
            Pass("HUD chrome does not submit Move; world click does; fog HUD redacts private fields");
        }
    }

    private void TickBuildGhost()
    {
        if (!HudReady()) return;
        if (_phase == 0)
        {
            PushKey(Godot.Key.Tab, true);
            PushKey(Godot.Key.Tab, false);
            _phase = 1;
            _hold = _age;
            return;
        }
        if (_phase == 1)
        {
            if (_age - _hold < 0.05) return;
            var dozer = LiveDozer();
            Require(dozer != null, "dozer must exist");
            Require(_client.Selected.Contains(dozer!.Id), "Tab must select the dozer through _Input");
            if (!_client.Hud.TryButton("build", "power.fusion", out var card))
            {
                if (_age < 2) return;
                Fail("Fusion build card not in HUD button regions (HUD not drawn?)");
                return;
            }
            var center = card.Position + card.Size / 2;
            Require(center.Y > 0 && _client.Hud.BlocksWorld(center), "Fusion card must sit in HUD chrome");
            Click(center, MouseButton.Left, true);
            Click(center, MouseButton.Left, false);
            GD.Print($"VERIFY_INPUT fusion-card={center.X:0},{center.Y:0}");
            _phase = 2;
            _hold = _age;
            return;
        }
        if (_phase == 2)
        {
            if (_age - _hold < 0.05) return;
            Require(_client.Mode == OrderKind.Build && _client.BuildRole == "power.fusion", "HUD Fusion card must enter build/place mode");
            var dozer = LiveDozer();
            Require(dozer != null, "dozer must exist");
            var legal = CanPlaceGhost(dozer!.Id, out _placePoint, out _placeScreen);
            Require(legal, $"ghost pad {_placePoint.X},{_placePoint.Z} must be CanPlace-legal and on-screen");
            Require(!_client.Hud.BlocksWorld(_placeScreen), "place click must hit the world, not HUD chrome");
            Click(_placeScreen, MouseButton.Left, true);
            Click(_placeScreen + new Vector2(0, 24), MouseButton.Left, false);
            _phase = 3;
            _hold = _age;
            return;
        }
        if (_phase == 3)
        {
            if (_age - _hold < 0.05) return;
            var builds = _client.Submits.Where(s => s.Order.Kind == OrderKind.Build).ToArray();
            Require(builds.Length > 0, "place release must submit a Build through _Input");
            var build = builds[^1];
            Require(build.Receipt.Accepted, "Build receipt must be accepted: " + build.Receipt.Reason);
            Require(build.Order.ProductId == "power.fusion", "Build product must be power.fusion");
            Require(build.Order.TargetId == 0, "Build must not carry a picked entity id");
            Require(build.Order.Position.Equals(_placePoint), $"Build must use the clicked ghost {_placePoint.X},{_placePoint.Z} not {build.Order.Position.X},{build.Order.Position.Z}");
            GD.Print($"VERIFY_RECEIPT kind=Build accepted=true product={build.Order.ProductId} pos={build.Order.Position.X},{build.Order.Position.Z}");
            var before = SnapshotCopy();
            _client.AdvanceTicks(2);
            var after = _client.Snapshot;
            Require(after.Tick > before.Tick, "clock must advance after accepted Build");
            var dozer = after.Entities.FirstOrDefault(e => e.OwnerSlot == _client.Slot && e.RoleId == "build.dozer");
            Require(dozer != null, "dozer still present");
            bool changed = dozer!.Activity == EntityActivity.Building || !dozer.Position.Equals(before.DozerPos) || !dozer.Destination.Equals(before.DozerDest)
                || after.Entities.Any(e => e.OwnerSlot == _client.Slot && e.RoleId == "power.fusion");
            Require(changed, "snapshot must change after Build (activity, destination, or fusion site)");
            GD.Print($"VERIFY_SNAPSHOT tick={after.Tick} dozer={dozer.Activity} dest={dozer.Destination.X},{dozer.Destination.Z}");
            Pass("real input placed Fusion at CanPlace ghost point");
        }
    }

    private void TickPause()
    {
        if (!HudReady()) return;
        if (_phase == 0)
        {
            if (_client.Snapshot.Tick < 2)
            {
                if (_age > 4) Fail("clock never advanced before pause");
                return;
            }
            long running = _client.Snapshot.Tick;
            PushKey(Godot.Key.Escape, true);
            PushKey(Godot.Key.Escape, false);
            Require(_client.Snapshot.Paused, "Escape must freeze the local match clock");
            Require(_client.Hud.MenuOpen, "Escape opens local pause");
            _pauseTick = _client.Snapshot.Tick;
            Require(_pauseTick >= running, "pause snapshot tick is live");
            GD.Print($"VERIFY_INPUT key=Escape paused=true tick={_pauseTick}");
            _phase = 1;
            _hold = _age;
            return;
        }
        if (_phase == 1)
        {
            if (_age - _hold < 0.4) return;
            Require(_client.Snapshot.Paused, "match must remain paused");
            Require(_client.Snapshot.Tick == _pauseTick, $"paused clock must stay at {_pauseTick}, was {_client.Snapshot.Tick}");
            Pass("pause key freezes the match clock");
        }
    }

    private void AssertFogHud()
    {
        var snap = _client.Snapshot;
        foreach (var entity in snap.Entities)
        {
            var role = _client.Match.Config.Role(entity.RoleId);
            if (SelectionIntel.OwnLive(entity, snap.ViewerSlot)) continue;
            Require(SelectionIntel.Status(entity, snap.ViewerSlot) is "LAST SEEN" or "STATUS UNKNOWN", "foreign/fog activity must not display as a live order");
            Require(!SelectionIntel.Veterancy(entity, snap.ViewerSlot).Contains("XP", StringComparison.Ordinal), "foreign XP must be omitted");
            if (role.Capacity > 0)
                Require(SelectionIntel.Passengers(entity, role, snap.ViewerSlot).Contains("unknown", StringComparison.Ordinal), "foreign passengers/cargo must be unknown");
        }
        // Radar is not LOS: world presence is snapshot visibility, not Player.Radar.
        Require(snap.Player.Radar, "slice-1 Aegis command starts with radar");
        bool shroudHidden = snap.Entities.Where(e => e.OwnerSlot != snap.ViewerSlot && !e.IsRemembered)
            .All(e => _client.Field.VisibilityAt(e.Position) != Visibility.Shroud);
        Require(shroudHidden, "live foreign units must not appear from shroud");
    }

    private bool CanPlaceGhost(int dozerId, out WorldPoint point, out Vector2 screen)
    {
        point = GhostPad;
        _client.Field.SetFocus(point);
        screen = _client.Field.ScreenPosition(new EntitySnapshot { Position = point, RoleId = "power.fusion" }, 0.2f);
        if (screen.X < 80 || screen.X > GuiWidth - 80 || _client.Hud.BlocksWorld(screen))
        {
            _client.Field.SetFocus(point);
            screen = _client.Field.Camera.UnprojectPosition(_client.Field.ToWorld(point) + Vector3.Up * 0.05f);
        }
        point = _client.Field.ScreenPoint(screen);
        var place = _client.Match.CanPlace(_client.Slot, dozerId, "power.fusion", point);
        if (!place.Allowed)
        {
            foreach (var candidate in GhostCandidates())
            {
                _client.Field.SetFocus(candidate);
                screen = _client.Field.Camera.UnprojectPosition(_client.Field.ToWorld(candidate) + Vector3.Up * 0.05f);
                if (_client.Hud.BlocksWorld(screen)) continue;
                var mapped = _client.Field.ScreenPoint(screen);
                if (_client.Match.CanPlace(_client.Slot, dozerId, "power.fusion", mapped).Allowed)
                {
                    point = mapped;
                    return true;
                }
            }
        }
        return place.Allowed && !_client.Hud.BlocksWorld(screen);
    }

    private static IEnumerable<WorldPoint> GhostCandidates()
    {
        yield return GhostPad;
        for (int ring = 1; ring <= 8; ring++)
        for (int z = -ring; z <= ring; z++)
        for (int x = -ring; x <= ring; x++)
        {
            if (Math.Abs(x) != ring && Math.Abs(z) != ring) continue;
            yield return new WorldPoint(GhostPad.X + x * 200, GhostPad.Z + z * 200);
        }
    }

    private (long Tick, WorldPoint DozerPos, WorldPoint DozerDest) SnapshotCopy()
    {
        var dozer = LiveDozer();
        return (_client.Snapshot.Tick, dozer?.Position ?? default, dozer?.Destination ?? default);
    }

    private EntitySnapshot? LiveDozer() =>
        _client.Snapshot.Entities.FirstOrDefault(e => e.OwnerSlot == _client.Slot && e.RoleId == "build.dozer");

    private bool HudReady()
    {
        if (_client.Hud == null || _client.Field == null) return false;
        if (_frames < 4) return false;
        if (_client.Hud.Size.X < 1000 || _client.Hud.Size.Y < 600)
        {
            if (_frames > 180) Fail($"HUD size {_client.Hud.Size.X}x{_client.Hud.Size.Y} never reached {GuiWidth}x{GuiHeight}");
            return false;
        }
        return true;
    }

    private void Click(Vector2 pos, MouseButton button, bool pressed)
    {
        var ev = new InputEventMouseButton
        {
            ButtonIndex = button,
            Pressed = pressed,
            Position = pos,
            GlobalPosition = pos,
            ButtonMask = pressed ? (button == MouseButton.Left ? MouseButtonMask.Left : MouseButtonMask.Right) : 0
        };
        GetViewport().PushInput(ev, true);
    }

    private void PushKey(Godot.Key key, bool pressed)
    {
        var ev = new InputEventKey
        {
            Keycode = key,
            PhysicalKeycode = key,
            Pressed = pressed,
            Echo = false
        };
        GetViewport().PushInput(ev, true);
    }

    private static void Require(bool ok, string message)
    {
        if (!ok) throw new InvalidOperationException(message);
    }

    private void Pass(string detail)
    {
        if (_done) return;
        _done = true;
        if (!string.IsNullOrEmpty(_framePath)) _client.CaptureFrame(_framePath);
        GD.Print($"VERIFY_PASS scenario={_scenario} tick={_client.Snapshot.Tick} display={DisplayServer.GetName()} {detail}");
        if (DisplayServer.GetName() == "headless") GD.Print("VERIFY_RENDER pending headless — dispatch/receipt only");
        Finish(_quit ? 0 : -1);
    }

    private void Fail(string reason)
    {
        if (_done) return;
        _done = true;
        GD.Print($"VERIFY_FAIL scenario={_scenario} tick={_client.Snapshot.Tick} {reason}");
        Finish(_quit ? 1 : -1);
    }

    private void Finish(int code)
    {
        SetProcess(false);
        if (code >= 0) GetTree()?.Quit(code);
    }
}
