using Godot;
using IronDoctrine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronDoctrine.Client;

/// <summary>Resolution-aware field HUD; every actionable region dispatches an ordinary client command.</summary>
public partial class FieldHud : Control
{
    public MatchSnapshot Snapshot { get; set; } = new();
    public GameConfig Config { get; set; } = null!;
    public Battlefield Field { get; set; } = null!;
    public HashSet<int> Selection { get; set; } = new();
    public Action<string, string, int, int>? Command;
    public Action<WorldPoint, bool>? MapClick;
    public EntitySnapshot? Hovered { get; set; }
    public string ModeText { get; set; } = "";
    public string PlacementText { get; set; } = "";
    public bool PlacementAllowed { get; set; }
    public string Notice { get; set; } = "Select your Dozer to begin construction.";
    public string ProofText { get; set; } = "";
    public bool HelpOpen { get; set; }
    public bool DragSelecting { get; set; }
    public Vector2 DragStart { get; set; }
    public Vector2 DragEnd { get; set; }
    public bool MenuOpen { get; set; }
    private readonly List<ButtonRegion> _buttons = new();
    private Font _font = null!;
    private Font _bold = null!;
    private ImageTexture? _mapTexture;
    private long _mapTick = -1;
    private Rect2 _mapRect;
    private string _tooltip = "";
    private readonly Color _panel = new("111d23"), _line = new("304149"), _ink = new("e5eee9"), _muted = new("92a8aa"), _accent = new("84d6c1"), _amber = new("edbb77");
    private float Bottom => Size.Y - 224;
    private sealed record ButtonRegion(Rect2 Rect, string Action, string Product, int Index, int ActorId, bool Enabled, string Tooltip);

    public override void _Ready()
    {
        SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        _font = ThemeDB.FallbackFont;
        _bold = ThemeDB.FallbackFont;
    }

    public bool BlocksWorld(Vector2 point) => HelpOpen || MenuOpen || Snapshot.Phase == MatchPhase.Finished || point.Y < 72 || point.Y >= Bottom || (point.Y < 172 && point.X < 355);

    public bool Click(Vector2 point, bool right)
    {
        if (!MenuOpen && !HelpOpen && Snapshot.Phase != MatchPhase.Finished && _mapRect.HasPoint(point))
        {
            var ratio = (point - _mapRect.Position) / _mapRect.Size;
            MapClick?.Invoke(new WorldPoint((int)(ratio.X * Config.Map.WidthCells * Config.Map.CellSize), (int)(ratio.Y * Config.Map.HeightCells * Config.Map.CellSize)), right);
            return true;
        }
        if (!right)
        {
            var region = _buttons.LastOrDefault(b => b.Rect.HasPoint(point));
            if (region != null)
            {
                if (region.Enabled) Command?.Invoke(region.Action, region.Product, region.Index, region.ActorId);
                else Notice = region.Tooltip;
                return true;
            }
        }
        return BlocksWorld(point);
    }

    public override void _Draw()
    {
        if (Config == null || Field == null) return;
        _buttons.Clear();
        _tooltip = "";
        DrawWorldLabels();
        if (DragSelecting)
        {
            var rect = new Rect2(DragStart, DragEnd - DragStart).Abs();
            DrawRect(rect, new Color(.35f, .9f, .8f, .1f));
            DrawRect(rect, _accent, false, 1);
        }
        Header();
        Objectives();
        DrawRect(new Rect2(0, Bottom, Size.X, 224), _panel);
        DrawLine(new Vector2(0, Bottom), new Vector2(Size.X, Bottom), _accent.Darkened(.45f), 2);
        Minimap();
        SelectionPanel();
        CommandPanel();
        if (!string.IsNullOrEmpty(Notice))
        {
            Text(new Vector2(288, Bottom - 16), Notice, 15, _ink);
        }
        if (!string.IsNullOrEmpty(ModeText))
        {
            var rect = new Rect2(Size.X / 2 - 250, 88, 500, 56);
            Panel(rect);
            Text(rect.Position + new Vector2(16, 23), ModeText, 17, _accent);
            Text(rect.Position + new Vector2(16, 43), string.IsNullOrEmpty(PlacementText) ? "Left click a target · Shift appends · Esc cancels" : PlacementText, 13, PlacementAllowed ? _accent : _amber);
        }
        if (Snapshot.Player.LowPower)
        {
            var rect = new Rect2(Size.X - 335, 91, 310, 55);
            Panel(rect, new Color("402920"));
            Text(rect.Position + new Vector2(14, 23), "LOW POWER", 18, _amber);
            Text(rect.Position + new Vector2(14, 43), "Radar and defenses offline. Build Fusion.", 13, _ink);
        }
        if (!string.IsNullOrEmpty(_tooltip) && !MenuOpen && !HelpOpen)
        {
            float width = Math.Min(650, _font.GetStringSize(_tooltip, fontSize: 14).X + 28);
            var rect = new Rect2(Mathf.Clamp(GetLocalMousePosition().X - width / 2, 12, Size.X - width - 12), Bottom - 57, width, 30);
            Panel(rect);
            Text(rect.Position + new Vector2(12, 20), _tooltip, 14, _ink);
        }
        if (HelpOpen) Help();
        else if (Snapshot.Phase == MatchPhase.Finished) Results();
        else if (MenuOpen) PauseMenu();
    }

    private void Header()
    {
        DrawRect(new Rect2(0, 0, Size.X, 72), new Color("0c171d"));
        DrawRect(new Rect2(0, 0, 6, 72), _accent);
        Text(new Vector2(25, 32), "IRON DOCTRINE", 23, _ink);
        Text(new Vector2(26, 54), "LOCAL SKIRMISH  /  FIELD TEST 01", 11, _muted);
        float time = Snapshot.Tick / (float)Config.Rules.TickRate;
        Text(new Vector2(317, 28), "ELAPSED", 10, _muted);
        Text(new Vector2(317, 52), $"{(int)time / 60:00}:{(int)time % 60:00}", 20, _ink);
        float x = Size.X - 652;
        Text(new Vector2(x, 27), "FUNDS", 11, _muted);
        Text(new Vector2(x, 52), $"$ {Snapshot.Player.Money:N0}", 24, _accent);
        x += 174;
        Text(new Vector2(x, 27), "POWER  /  DEMAND", 11, _muted);
        Text(new Vector2(x, 51), $"{Snapshot.Player.PowerSupply}  /  {Snapshot.Player.PowerDemand}", 22, Snapshot.Player.LowPower ? _amber : _ink);
        Bar(new Rect2(x, 59, 126, 3), Snapshot.Player.PowerSupply == 0 ? (Snapshot.Player.PowerDemand == 0 ? 1 : 0) : Math.Min(1, (float)Snapshot.Player.PowerDemand / Snapshot.Player.PowerSupply), Snapshot.Player.LowPower ? _amber : _accent);
        Button(new Rect2(Size.X - 255, 18, 104, 38), "HELP  H", "help", tooltip: "Full controls and first-match guide");
        Button(new Rect2(Size.X - 139, 18, 114, 38), Snapshot.Paused ? "RESUME" : "PAUSE  Esc", "pause", tooltip: "Local pause, resign, and quit");
        DrawLine(new Vector2(0, 71), new Vector2(Size.X, 71), _line);
    }

    private void Objectives()
    {
        var own = Snapshot.Entities.Where(e => e.OwnerSlot == Snapshot.ViewerSlot && e.Completed).ToArray();
        Panel(new Rect2(18, 88, 328, 80), new Color(.055f, .092f, .105f, .95f));
        Text(new Vector2(32, 110), "OBJECTIVE  /  ELIMINATE ENEMY BUILDINGS", 11, _accent);
        string hint = !own.Any(e => e.RoleId == "power.fusion") ? "01  Select Dozer → build Fusion." : !own.Any(e => e.RoleId == "eco.dropoff") ? "02  Build a Drop-off near a supply dock." : !own.Any(e => e.RoleId == "prod.barracks") ? "03  Build Barracks. Train Rifle and Rocket." : !own.Any(e => e.RoleId == "prod.factory") ? "04  Add a Factory. Assemble your force." : "Advance east. Explore and clear enemy buildings.";
        Text(new Vector2(32, 134), hint, 13, _ink);
        Text(new Vector2(32, 154), string.IsNullOrEmpty(ProofText) ? "LMB select · RMB order · H controls" : ProofText, 11, _muted);
    }

    private void Minimap()
    {
        Text(new Vector2(22, Bottom + 24), "TACTICAL MAP", 11, _accent);
        Text(new Vector2(152, Bottom + 24), Snapshot.Player.Radar ? "RADAR ONLINE" : "RADAR OFFLINE", 10, Snapshot.Player.Radar ? _muted : _amber);
        _mapRect = new Rect2(22, Bottom + 35, 240, 160);
        if (_mapTick != Snapshot.Tick || _mapTexture == null)
        {
            int width = Config.Map.WidthCells, height = Config.Map.HeightCells;
            var image = Image.CreateEmpty(width, height, false, Image.Format.Rgb8);
            for (int z = 0; z < height; z++)
            for (int x = 0; x < width; x++)
            {
                int index = z * width + x;
                var vis = index < Snapshot.Visibility.Length ? Snapshot.Visibility[index] : Visibility.Shroud;
                Color color = Config.Map.TerrainAt(x, z) == "ter.unbuildable" ? new Color("525247") : Config.Map.TerrainAt(x, z) == "ter.start" ? new Color("657068") : new Color("777764");
                if (vis == Visibility.Shroud) color = new Color("080e13");
                else if (vis == Visibility.Fog) color *= .4f;
                image.SetPixel(x, z, color);
            }
            if (_mapTexture == null) _mapTexture = ImageTexture.CreateFromImage(image); else _mapTexture.Update(image);
            _mapTick = Snapshot.Tick;
        }
        DrawTextureRect(_mapTexture, _mapRect, false);
        DrawRect(_mapRect, _line, false, 1);
        foreach (var entity in Snapshot.Entities.Where(e => e.ContainerId == 0))
        {
            var role = Config.Role(entity.RoleId);
            if (Field.VisibilityAt(entity.Position) == Visibility.Shroud || !Snapshot.Player.Radar) continue;
            if (entity.IsRemembered && !role.IsBuilding) continue;
            var position = MapPosition(entity.Position);
            var color = Field.TeamColor(entity.OwnerSlot);
            if (entity.IsRemembered) color = color.Darkened(.6f);
            if (role.IsBuilding || role.IsNeutral) DrawRect(new Rect2(position - Vector2.One * 2.5f, Vector2.One * 5), color);
            else DrawCircle(position, Selection.Contains(entity.Id) ? 2.8f : 1.8f, color);
        }
        var corners = new[] { new Vector2(0, 72), new Vector2(Size.X, 72), new Vector2(Size.X, Bottom), new Vector2(0, Bottom) }.Select(s => MapPosition(Field.ScreenPoint(s))).ToArray();
        for (int i = 0; i < 4; i++) DrawLine(corners[i], corners[(i + 1) % 4], new Color(.84f, .95f, .9f, .65f), 1);
        Text(new Vector2(22, Bottom + 212), "LMB camera  /  RMB order", 11, _muted);
        DrawLine(new Vector2(279, Bottom + 17), new Vector2(279, Size.Y - 17), _line);
    }

    private Vector2 MapPosition(WorldPoint point) => _mapRect.Position + new Vector2(point.X / (float)(Config.Map.WidthCells * Config.Map.CellSize), point.Z / (float)(Config.Map.HeightCells * Config.Map.CellSize)) * _mapRect.Size;

    private void SelectionPanel()
    {
        var chosen = SelectionState.Ordered(Snapshot, Selection);
        float x = 298, y = Bottom;
        Text(new Vector2(x, y + 24), "SELECTION", 11, _accent);
        if (chosen.Length == 0)
        {
            Text(new Vector2(x, y + 62), "Awaiting orders", 23, _ink);
            Text(new Vector2(x, y + 94), "Click a unit or drag to select.", 13, _muted);
            Text(new Vector2(x, y + 120), "Build power and economy first.", 13, _muted);
            Text(new Vector2(x, y + 148), "Ctrl + 1–9 assigns a control group.", 12, _muted);
            Button(new Rect2(x, y + 170, 261, 32), "FIND DOZER  Tab", "builder", tooltip: "Select and centre an idle builder");
            return;
        }
        var first = chosen[0];
        var role = Config.Role(first.RoleId);
        Text(new Vector2(x, y + 55), chosen.Length == 1 ? role.Label : $"{chosen.Length} units selected", 23, _ink);
        bool own = first.OwnerSlot == Snapshot.ViewerSlot;
        string affiliation = own ? "YOUR FORCE" : first.OwnerSlot < 0 ? "NEUTRAL" : "CONTACT";
        Text(new Vector2(x, y + 76), affiliation + (first.IsRemembered ? "  /  LAST SEEN" : "  /  " + first.Activity.ToString().ToUpperInvariant()), 10, own ? _accent : _amber);
        int hp = chosen.Sum(e => e.Hp), max = chosen.Sum(e => e.MaxHp);
        Bar(new Rect2(x, y + 89, 261, 7), max == 0 ? 0 : (float)hp / max, _accent);
        Text(new Vector2(x, y + 115), $"{hp:N0} / {max:N0} integrity", 13, _ink);
        string rank = first.VeterancyRank switch { 1 => "›  VETERAN", 2 => "››  ELITE", 3 => "›››  HEROIC", _ => "UNRANKED" };
        if (!role.IsBuilding) Text(new Vector2(x, y + 137), rank + $"  ·  {first.Experience} XP", 11, _amber);
        if (first.RoleId == "map.dock") Text(new Vector2(x, y + 140), $"Supplies remaining  {first.SuppliesLeft:N0}", 13, _amber);
        else if (role.Capacity > 0) Text(new Vector2(x, y + 155), $"Passengers  {first.OccupantIds.Length}/{role.Capacity}" + (role.CargoCapacity > 0 ? $"  ·  Cargo ${first.Cargo}" : ""), 12, _muted);
        if (!first.Completed)
        {
            Text(new Vector2(x, y + 148), "UNDER CONSTRUCTION", 11, _amber);
            Bar(new Rect2(x, y + 160, 261, 5), 1 - (float)first.BuildTicksLeft / Math.Max(1, first.BuildTicksTotal), _amber);
        }
        if (first.CaptureTicksTotal > 0 && first.CaptureTicksLeft > 0)
        {
            Text(new Vector2(x, y + 154), "CAPTURING", 11, _amber);
            Bar(new Rect2(x, y + 162, 261, 5), 1 - (float)first.CaptureTicksLeft / first.CaptureTicksTotal, _amber);
        }
        if (own && first.Queue.Length > 0)
        {
            var item = first.Queue[0];
            Text(new Vector2(x, y + 154), $"QUEUE {first.Queue.Length}  ·  {Config.Role(item.ProductId).Label}", 11, _muted);
            Bar(new Rect2(x, y + 164, 261, 4), 1 - (float)item.RemainingTicks / Math.Max(1, item.TotalTicks), _accent);
            int shown = Math.Min(6, first.Queue.Length);
            for (int i = 0; i < shown; i++)
                Button(new Rect2(x + i * 44, y + 179, 39, 28), $"{i + 1} ×", "cancel", index: i, actorId: first.Id, tooltip: $"Cancel {Config.Role(first.Queue[i].ProductId).Label}; refund {Config.Rules.QueueRefundPercent}%");
        }
        else if (chosen.Length > 1)
        {
            string summary = string.Join(" · ", chosen.GroupBy(e => e.RoleId).Select(g => $"{g.Count()} {Config.Role(g.Key).Label}"));
            Text(new Vector2(x, y + 187), summary, 11, _muted, 264);
        }
    }

    private void CommandPanel()
    {
        float x = 592, y = Bottom, width = Size.X - x - 24;
        DrawLine(new Vector2(x - 14, y + 17), new Vector2(x - 14, Size.Y - 17), _line);
        Text(new Vector2(x, y + 24), "COMMAND", 11, _accent);
        Text(new Vector2(x + 105, y + 24), "SHIFT APPENDS ORDERS", 10, _muted);
        var selected = SelectionState.Ordered(Snapshot, Selection);
        if (selected.Length == 0 || selected.Any(e => e.OwnerSlot != Snapshot.ViewerSlot))
        {
            Text(new Vector2(x, y + 69), "Your selection determines the available orders.", 16, _muted);
            Text(new Vector2(x, y + 106), "Select your Dozer to place buildings. Select a producer to train units.", 13, _muted);
            Text(new Vector2(x, y + 142), "Destroy all enemy buildings to win. A surviving army can rebuild.", 13, _muted);
            Text(new Vector2(x, y + 192), "WASD / arrows pan   ·   Wheel zoom   ·   Middle drag   ·   Space focus", 12, _ink);
            return;
        }
        bool units = selected.All(e => !Config.Role(e.RoleId).IsBuilding);
        bool infantry = selected.All(e => Config.Role(e.RoleId).IsInfantry);
        bool builders = selected.All(e => Config.Role(e.RoleId).AbilityIds.Contains("ab.build"));
        bool buildings = selected.All(e => Config.Role(e.RoleId).IsBuilding);
        bool complete = selected.All(e => e.Completed);
        var actions = new List<(string Label, string Action, string Help)>();
        if (units)
        {
            actions.Add(("MOVE", "move", "Move to a point; right click is contextual"));
            if (selected.All(e => Config.Role(e.RoleId).Damage > 0)) actions.Add(("ATTACK  Z", "attack", "Attack a visible enemy; available after capture research"));
            actions.Add(("ATTACK-MOVE  Q", "attackmove", "Engage enemies on the way to a point"));
            actions.Add(("STOP  X", "stop", "Stop the current order and clear its queue"));
            actions.Add(("GUARD  G", "guard", "Guard a point and engage nearby enemies"));
            actions.Add(("WAYPOINT  T", "waypoint", "Append a movement waypoint"));
            if (selected.All(e => Config.Role(e.RoleId).Damage > 0)) actions.Add(("FORCE FIRE  F", "force", "Attack ground or any target; can harm allies"));
        }
        if (buildings && complete) actions.Add(("RALLY  Y", "rally", "Set where produced units move"));
        if (selected.All(e => Config.Role(e.RoleId).AbilityIds.Contains("ab.sell"))) actions.Add(("SELL  Del", "sell", $"Sell selected buildings; refund {Config.Rules.SellRefundPercent}%"));
        if (builders) actions.Add(("REPAIR  R", "repair", "Repair a friendly building; consumes funds"));
        if (selected.All(e => Config.Role(e.RoleId).AbilityIds.Contains("ab.gather"))) actions.Add(("GATHER", "gather", "Choose a supply dock; deliveries go to your own Drop-off"));
        if (infantry) actions.Add(("ENTER  E", "enter", "Enter your transport or a neutral/owned garrison"));
        if (selected.All(e => Config.Role(e.RoleId).AbilityIds.Contains("ab.capture")) && Snapshot.Player.Upgrades.Contains("up.capture")) actions.Add(("CAPTURE  C", "capture", "Capture an enemy building through an interruptible channel"));
        if (selected.All(e => e.ContainerId != 0 || Config.Role(e.RoleId).Capacity > 0)) actions.Add(("EXIT  V", "exit", "Unload passengers onto legal ground"));
        float gap = 7;
        int columns = Math.Min(6, Math.Max(1, actions.Count));
        float bw = (width - gap * (columns - 1)) / columns;
        for (int i = 0; i < actions.Count; i++)
        {
            var action = actions[i];
            Button(new Rect2(x + (i % columns) * (bw + gap), y + 38 + (i / columns) * 38, bw, 31), action.Label, action.Action, tooltip: action.Help, fontSize: 11);
        }
        float productsY = y + (actions.Count > columns ? 123 : 87);
        var products = builders ? Config.Roles.Where(r => r.IsBuilding && !r.IsNeutral).ToArray() : selected.Length == 1 && complete ? Config.Roles.Where(r => r.ProducerId == selected[0].RoleId).ToArray() : Array.Empty<RoleConfig>();
        if (products.Length > 0)
        {
            Text(new Vector2(x, productsY - 3), builders ? "CONSTRUCTION" : "PRODUCTION / RESEARCH", 10, _muted);
            float cardWidth = Math.Min(140, (width - gap * (products.Length - 1)) / products.Length);
            for (int i = 0; i < products.Length; i++)
            {
                var role = products[i];
                bool prerequisites = role.Prerequisites.All(id => Snapshot.Entities.Any(e => e.OwnerSlot == Snapshot.ViewerSlot && e.RoleId == id && e.Completed));
                bool researched = Snapshot.Player.Upgrades.Contains(role.Id);
                bool pendingResearch = role.Id.StartsWith("up.", StringComparison.Ordinal) && selected.Any(e => e.Queue.Any(q => q.ProductId == role.Id));
                bool allowed = prerequisites && !researched && !pendingResearch && Snapshot.Player.Money >= role.Cost;
                string hint = researched ? "Research complete" : pendingResearch ? "Research already queued" : !prerequisites ? "Requires " + string.Join(", ", role.Prerequisites.Select(id => Config.Role(id).Label)) : $"{role.Label} · ${role.Cost} · {role.BuildTicks / (float)Config.Rules.TickRate:0.#}s" + (role.PowerDrain > 0 ? $" · power −{role.PowerDrain}" : role.PowerSupply > 0 ? $" · power +{role.PowerSupply}" : "");
                if (Snapshot.Player.Money < role.Cost) hint += " · insufficient funds";
                var rect = new Rect2(x + i * (cardWidth + gap), productsY + 8, cardWidth, 64);
                Button(rect, role.Label, builders ? "build" : "queue", role.Id, actorId: selected[0].Id, enabled: allowed, tooltip: hint, fontSize: 13, card: true);
                Text(rect.Position + new Vector2(10, 49), researched ? "COMPLETE" : $"$ {role.Cost:N0}", 13, allowed ? _accent : _muted);
            }
        }
        if (productsY + 90 < Size.Y - 9) Text(new Vector2(x, Size.Y - 15), "RMB contextual order   ·   Ctrl + 1–9 save   ·   1–9 recall   ·   Double-tap group to focus", 11, _muted);
    }

    private void DrawWorldLabels()
    {
        foreach (var entity in Snapshot.Entities.Where(e => e.ContainerId == 0 && !e.IsRemembered))
        {
            bool selected = Selection.Contains(entity.Id);
            if (!selected && Hovered?.Id != entity.Id && entity.Hp >= entity.MaxHp && entity.Completed) continue;
            var role = Config.Role(entity.RoleId);
            var screen = Field.ScreenPosition(entity, role.IsBuilding ? 4 : 1.9f);
            if (screen.X < 0 || screen.X > Size.X || screen.Y < 75 || screen.Y > Bottom - 12) continue;
            var color = Field.TeamColor(entity.OwnerSlot);
            float width = role.IsBuilding ? 64 : 38;
            Bar(new Rect2(screen.X - width / 2, screen.Y, width, 4), (float)entity.Hp / Math.Max(1, entity.MaxHp), color);
            if (!entity.Completed) Bar(new Rect2(screen.X - width / 2, screen.Y + 6, width, 3), 1 - (float)entity.BuildTicksLeft / Math.Max(1, entity.BuildTicksTotal), _amber);
            if (entity.VeterancyRank > 0) Text(screen + new Vector2(-9, -7), new string('›', entity.VeterancyRank), 18, _amber);
            if (Hovered?.Id == entity.Id) Text(screen + new Vector2(-width / 2, -9), role.Label, 12, _ink);
        }
    }

    private void PauseMenu()
    {
        _buttons.Clear();
        Shade();
        var rect = new Rect2(Size.X / 2 - 225, Size.Y / 2 - 196, 450, 392);
        Panel(rect);
        Text(rect.Position + new Vector2(32, 49), "LOCAL PAUSE", 27, _ink);
        Text(rect.Position + new Vector2(32, 78), "The match clock is stopped.", 14, _muted);
        Button(new Rect2(rect.Position + new Vector2(32, 108), new Vector2(386, 46)), "RESUME MATCH", "pause");
        Button(new Rect2(rect.Position + new Vector2(32, 170), new Vector2(386, 46)), "CONTROLS & FIELD GUIDE", "help");
        Button(new Rect2(rect.Position + new Vector2(32, 232), new Vector2(386, 46)), "RESIGN MATCH", "resign", tooltip: "Concede the match and show the result");
        Button(new Rect2(rect.Position + new Vector2(32, 294), new Vector2(386, 46)), "QUIT TO DESKTOP", "quit");
    }

    private void Results()
    {
        _buttons.Clear();
        Shade();
        bool won = Snapshot.WinningSlots.Contains(Snapshot.ViewerSlot);
        var rect = new Rect2(Size.X / 2 - 280, Size.Y / 2 - 197, 560, 394);
        Panel(rect);
        DrawRect(new Rect2(rect.Position, new Vector2(560, 5)), won ? _accent : _amber);
        Text(rect.Position + new Vector2(36, 51), "MATCH COMPLETE", 12, _muted);
        Text(rect.Position + new Vector2(34, 108), won ? "VICTORY" : "DEFEAT", 48, won ? _accent : _amber);
        Text(rect.Position + new Vector2(36, 142), won ? "All opposing buildings have been eliminated." : "Your force has been eliminated or resigned.", 15, _ink);
        int seconds = (int)(Snapshot.Tick / Config.Rules.TickRate);
        Text(rect.Position + new Vector2(36, 186), $"{seconds / 60:00}:{seconds % 60:00} elapsed   ·   ${Snapshot.Player.Money:N0} remaining", 16, _muted);
        Text(rect.Position + new Vector2(36, 216), "Local skirmish · Original field test · Working role labels", 12, _muted);
        Button(new Rect2(rect.Position + new Vector2(36, 254), new Vector2(488, 48)), "REMATCH", "rematch");
        Button(new Rect2(rect.Position + new Vector2(36, 317), new Vector2(488, 42)), "QUIT TO DESKTOP", "quit");
    }

    private void Help()
    {
        _buttons.Clear();
        Shade();
        var rect = new Rect2(Size.X / 2 - 440, Size.Y / 2 - 284, 880, 568);
        Panel(rect);
        Text(rect.Position + new Vector2(30, 46), "FIELD GUIDE", 28, _ink);
        Text(rect.Position + new Vector2(30, 74), "Build a base, secure supplies, destroy every enemy building.", 15, _muted);
        string[] left = { "SELECT & NAVIGATE", "LMB / drag     Select / box-select your units", "Shift + LMB     Add or remove from selection", "Ctrl + 1–9     Assign a control group", "1–9 / double-tap     Recall / focus group", "WASD / arrows     Pan camera", "Middle drag / wheel     Pan / zoom", "Space     Focus selection", "Tab     Find and select a Dozer", "H / Esc     Help / cancel mode or pause" };
        string[] right = { "ISSUE ORDERS", "RMB / Z     Contextual order / attack target", "Q / X / G     Attack-move / stop / guard", "T / F     Append waypoint / force attack", "R / E / C     Repair / enter / capture", "V / Y / Del     Exit / rally / sell", "Shift + order     Append to unit order queue", "Build card → LMB     Place a legal footprint", "Producer card     Train a unit or research", "Queue number ×     Cancel and refund that item" };
        for (int i = 0; i < left.Length; i++)
        {
            Text(rect.Position + new Vector2(30, 113 + i * 28), left[i], i == 0 ? 12 : 13, i == 0 ? _accent : _ink);
            Text(rect.Position + new Vector2(450, 113 + i * 28), right[i], i == 0 ? 12 : 13, i == 0 ? _accent : _ink);
        }
        Text(rect.Position + new Vector2(30, 420), "Start: Fusion → Drop-off near the dock → Barracks → Factory. The first Gatherer is free.", 14, _amber);
        Text(rect.Position + new Vector2(30, 448), "Rockets and AA can hit aircraft. Tanks cannot. Research capture at Barracks before using Rifle.", 13, _muted);
        Text(rect.Position + new Vector2(30, 476), "Dark terrain is unexplored. Dim terrain is remembered. Radar blips still require current sight.", 13, _muted);
        Button(new Rect2(rect.Position + new Vector2(30, 506), new Vector2(820, 37)), "CLOSE GUIDE  H", "help");
    }

    private void Shade() => DrawRect(new Rect2(Vector2.Zero, Size), new Color(.015f, .035f, .04f, .8f));
    private void Panel(Rect2 rect, Color? color = null) { DrawRect(rect, color ?? _panel); DrawRect(rect, _line, false, 1); }
    private void Text(Vector2 position, string text, int size, Color color, float width = -1) => DrawString(_font, position, text, HorizontalAlignment.Left, width, size, color);
    private void Bar(Rect2 rect, float fraction, Color color)
    {
        DrawRect(rect.Grow(1), new Color("081016"));
        DrawRect(rect, new Color("28363a"));
        DrawRect(new Rect2(rect.Position, new Vector2(rect.Size.X * Mathf.Clamp(fraction, 0, 1), rect.Size.Y)), color);
    }
    private void Button(Rect2 rect, string label, string action, string product = "", int index = 0, int actorId = 0, bool enabled = true, string tooltip = "", int fontSize = 13, bool card = false)
    {
        bool hover = rect.HasPoint(GetLocalMousePosition());
        DrawRect(rect, !enabled ? new Color("18242a") : hover ? new Color("34504f") : new Color("203239"));
        DrawRect(rect, hover && enabled ? _accent : _line, false, 1);
        if (card) DrawRect(new Rect2(rect.Position, new Vector2(3, rect.Size.Y)), enabled ? _accent : _line);
        Text(rect.Position + new Vector2(card ? 10 : 8, card ? 25 : rect.Size.Y / 2 + fontSize * .35f), label, fontSize, enabled ? _ink : _muted, rect.Size.X - 15);
        _buttons.Add(new ButtonRegion(rect, action, product, index, actorId, enabled, tooltip));
        if (hover) _tooltip = tooltip;
    }
}
