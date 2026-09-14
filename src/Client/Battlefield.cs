using Godot;
using IronDoctrine.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IronDoctrine.Client;

/// <summary>A metre-scale presentation of the viewer's detached snapshot. It owns no match rules.</summary>
public partial class Battlefield : Node3D
{
    private GameConfig _config = null!;
    private MatchSetup _setup = null!;
    private readonly Dictionary<int, BodyView> _bodies = new();
    private readonly Dictionary<int, MeshInstance3D> _missiles = new();
    private readonly Dictionary<string, PackedScene?> _models = new();
    private readonly List<(Node3D Node, WorldPoint Point)> _clutter = new();
    private readonly List<(Node3D Node, float Life)> _effects = new();
    private readonly Dictionary<int, Node3D> _rallyFlags = new();
    private MultiMesh _ground = null!;
    private Color[] _groundColors = Array.Empty<Color>();
    private Visibility[] _lastVisibility = Array.Empty<Visibility>();
    private MatchSnapshot _snapshot = new();
    private Node3D? _ghost;
    private string _ghostRole = "";
    private MeshInstance3D? _ghostRing;
    private Camera3D _camera = null!;
    public Camera3D Camera => _camera;
    public Vector3 Focus { get; private set; }
    public float Zoom { get; private set; } = 58;
    public int LoadedModelCount => _models.Values.Count(m => m != null);
    private float Units => _config.Rules.UnitsPerWorldUnit;
    public float WorldWidth => _config.Map.WidthCells * _config.Map.CellSize / Units;
    public float WorldHeight => _config.Map.HeightCells * _config.Map.CellSize / Units;

    private sealed class BodyView
    {
        public Node3D Root = null!;
        public Node3D Model = null!;
        public MeshInstance3D Ring = null!;
        public EntitySnapshot State = new();
        public Vector3 Desired;
        public Vector3 ObservedHeading;
        public int Team = int.MinValue;
        public bool Remembered;
        public long LastShot;
    }

    public void Initialize(GameConfig config, MatchSetup setup, int slot)
    {
        _config = config;
        _setup = setup;
        var start = config.Map.Starts.First(s => s.Slot == slot);
        Focus = ToWorld(start.Command) + new Vector3(3, 0, 0);
    }

    public override void _Ready()
    {
        var environment = new WorldEnvironment
        {
            Environment = new Godot.Environment
            {
                BackgroundMode = Godot.Environment.BGMode.Color,
                BackgroundColor = new Color("080d10"),
                AmbientLightSource = Godot.Environment.AmbientSource.Color,
                AmbientLightColor = new Color("bfd3df"),
                AmbientLightEnergy = 0.35f,
                TonemapMode = Godot.Environment.ToneMapper.Linear
            }
        };
        AddChild(environment);
        var sun = new DirectionalLight3D { RotationDegrees = new Vector3(-58, -24, 0), LightColor = new Color("fff0d2"), LightEnergy = 0.9f, ShadowEnabled = true };
        AddChild(sun);
        _camera = new Camera3D { Projection = Camera3D.ProjectionType.Orthogonal, Size = Zoom, Far = 350, Near = 0.1f, Current = true };
        AddChild(_camera);
        UpdateCamera();
        BuildTerrain();
        BuildClutter();
    }

    private void BuildTerrain()
    {
        int width = _config.Map.WidthCells, height = _config.Map.HeightCells;
        float cell = _config.Map.CellSize / Units;
        _ground = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, UseColors = true, Mesh = new PlaneMesh { Size = new Vector2(cell, cell) }, InstanceCount = width * height };
        _groundColors = new Color[width * height];
        var random = new Random(7061);
        for (int z = 0; z < height; z++)
        for (int x = 0; x < width; x++)
        {
            int index = z * width + x;
            string terrain = _config.Map.TerrainAt(x, z);
            var color = terrain == "ter.start" ? new Color("51534a") : terrain == "ter.unbuildable" ? new Color("343c32") : new Color("64634e");
            // Low contrast original scrub mosaic; colour is visual only.
            color *= 0.985f + (float)random.NextDouble() * 0.03f;
            color.A = 1;
            _groundColors[index] = color;
            _ground.SetInstanceTransform(index, new Transform3D(Basis.Identity, new Vector3((x + .5f) * cell, 0, (z + .5f) * cell)));
            _ground.SetInstanceColor(index, new Color("080d10"));
        }
        AddChild(new MultiMeshInstance3D { Multimesh = _ground, MaterialOverride = new StandardMaterial3D { VertexColorUseAsAlbedo = true, ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, Roughness = 1 } });
    }

    private void BuildClutter()
    {
        var rock = Model("res://assets/models/rocks.glb");
        if (rock == null) return;
        var random = new Random(841);
        foreach (var rect in _config.Map.Terrain.Where(r => r.TerrainId == "ter.unbuildable"))
        for (int z = rect.Z; z < rect.Z + rect.Height; z += 4)
        for (int x = rect.X; x < rect.X + rect.Width; x += 4)
        {
            var point = new WorldPoint((int)((x + 1.5) * _config.Map.CellSize), (int)((z + 1.5) * _config.Map.CellSize));
            var node = rock.Instantiate<Node3D>();
            node.Position = ToWorld(point);
            node.Rotation = new Vector3(0, (float)random.NextDouble() * Mathf.Tau, 0);
            node.Scale = Vector3.One * (.7f + (float)random.NextDouble() * .7f);
            node.Visible = false;
            AddChild(node);
            _clutter.Add((node, point));
        }
    }

    private PackedScene? Model(string path)
    {
        if (_models.TryGetValue(path, out var model)) return model;
        model = !string.IsNullOrEmpty(path) && ResourceLoader.Exists(path) ? GD.Load<PackedScene>(path) : null;
        _models[path] = model;
        if (model == null && !string.IsNullOrEmpty(path)) GD.PushWarning("Presentation model awaiting import: " + path);
        return model;
    }

    private Node3D CreateModel(RoleConfig role)
    {
        // An absent asset remains a selectable footprint while import is pending; no substitute product mesh.
        return Model(role.Model)?.Instantiate<Node3D>() ?? new Node3D();
    }

    public Vector3 ToWorld(WorldPoint point) => new(point.X / Units, 0, point.Z / Units);
    public WorldPoint ToPoint(Vector3 point) => new((int)Mathf.Round(point.X * Units), (int)Mathf.Round(point.Z * Units));

    public WorldPoint ScreenPoint(Vector2 screen)
    {
        var origin = _camera.ProjectRayOrigin(screen);
        var direction = _camera.ProjectRayNormal(screen);
        if (Mathf.Abs(direction.Y) < .001f) return ToPoint(Focus);
        var point = origin + direction * (-origin.Y / direction.Y);
        return ToPoint(new Vector3(Mathf.Clamp(point.X, 0, WorldWidth - .01f), 0, Mathf.Clamp(point.Z, 0, WorldHeight - .01f)));
    }

    public Vector2 ScreenPosition(EntitySnapshot entity, float height = 0)
    {
        var role = _config.Role(entity.RoleId);
        return _camera.UnprojectPosition(ToWorld(entity.Position) + Vector3.Up * (height + (role.IsFlying ? 3.1f : 0)));
    }

    public EntitySnapshot? Pick(Vector2 screen)
    {
        EntitySnapshot? best = null;
        float bestScore = float.MaxValue;
        foreach (var entity in _snapshot.Entities.Where(e => e.ContainerId == 0))
        {
            var role = _config.Role(entity.RoleId);
            float radius = Math.Max(role.IsInfantry ? 13 : 18, role.Radius / Units * GetViewport().GetVisibleRect().Size.Y / Zoom);
            if (role.IsBuilding) radius *= .42f;
            var center = ScreenPosition(entity, role.IsBuilding ? 1 : .5f);
            float distance = center.DistanceTo(screen);
            if (distance >= radius + 7) continue;
            // Buildings occupy a large disk; scale them so overlapping infantry/mobile win unless the click is clearly on the building.
            float score = distance / radius * (role.IsBuilding ? 3.5f : 1f);
            if (score < bestScore)
            {
                best = entity;
                bestScore = score;
            }
        }
        return best;
    }

    public void Pan(Vector2 direction, double delta)
    {
        Focus += new Vector3(direction.X, 0, direction.Y) * Zoom * .8f * (float)delta;
        ClampFocus();
        UpdateCamera();
    }
    public void Drag(Vector2 relative)
    {
        Focus += new Vector3(-relative.X, 0, -relative.Y * 1.3f) * Zoom / GetViewport().GetVisibleRect().Size.Y;
        ClampFocus();
        UpdateCamera();
    }
    public void SetFocus(WorldPoint point) { Focus = ToWorld(point); ClampFocus(); UpdateCamera(); }
    private void ClampFocus() { Focus = new Vector3(Mathf.Clamp(Focus.X, 0, WorldWidth), 0, Mathf.Clamp(Focus.Z, 0, WorldHeight)); }
    public void AdjustZoom(float amount) { Zoom = Mathf.Clamp(Zoom + amount, 25, 110); UpdateCamera(); }
    private void UpdateCamera()
    {
        _camera.Size = Zoom;
        _camera.Position = Focus + new Vector3(0, 65, 49);
        _camera.LookAt(Focus, Vector3.Up);
    }

    public void ShowSnapshot(MatchSnapshot snapshot, HashSet<int> selection)
    {
        _snapshot = snapshot;
        if (_lastVisibility.Length != snapshot.Visibility.Length) _lastVisibility = Enumerable.Repeat((Visibility)255, snapshot.Visibility.Length).ToArray();
        for (int i = 0; i < snapshot.Visibility.Length; i++)
        {
            if (_lastVisibility[i] == snapshot.Visibility[i]) continue;
            var state = snapshot.Visibility[i];
            Color color = state == Visibility.Shroud ? new Color("080d10") : state == Visibility.Fog ? _groundColors[i] * .35f : _groundColors[i];
            color.A = 1;
            _ground.SetInstanceColor(i, color);
            _lastVisibility[i] = state;
        }
        foreach (var (node, point) in _clutter) node.Visible = VisibilityAt(point) != Visibility.Shroud;
        var present = new HashSet<int>();
        foreach (var entity in snapshot.Entities)
        {
            if (entity.ContainerId != 0) continue;
            if (entity.OwnerSlot != snapshot.ViewerSlot && VisibilityAt(entity.Position) == Visibility.Shroud) continue;
            present.Add(entity.Id);
            var role = _config.Role(entity.RoleId);
            if (!_bodies.TryGetValue(entity.Id, out var view))
            {
                view = new BodyView { Root = new Node3D(), Model = CreateModel(role), Ring = Ring(Math.Max(.65f, role.Radius / Units + .25f), TeamColor(entity.OwnerSlot)), State = entity };
                view.Root.Position = ToWorld(entity.Position) + Vector3.Up * (role.IsFlying ? 3.1f : 0);
                view.Root.AddChild(view.Model);
                view.Root.AddChild(view.Ring);
                AddChild(view.Root);
                _bodies.Add(entity.Id, view);
            }
            if (view.Team != entity.OwnerSlot || view.Remembered != entity.IsRemembered)
            {
                TintModel(view.Model, TeamColor(entity.OwnerSlot), entity.IsRemembered);
                view.Ring.MaterialOverride = Glow(TeamColor(entity.OwnerSlot));
                view.Team = entity.OwnerSlot;
                view.Remembered = entity.IsRemembered;
            }
            // A pair of currently visible public observations is sufficient for movement and damage feedback.
            // Never infer a shot's source/target from proximity, nor use private enemy orders to orient models.
            if (!entity.IsRemembered && !view.State.IsRemembered)
            {
                var movement = ToWorld(entity.Position) - ToWorld(view.State.Position);
                if (movement.LengthSquared() > .0001f) view.ObservedHeading = movement.Normalized();
                if (view.State.Hp > entity.Hp) Pulse(ToWorld(entity.Position) + Vector3.Up, new Color("ffb95e"), .5f);
            }
            view.State = entity;
            view.Desired = ToWorld(entity.Position) + Vector3.Up * (role.IsFlying ? 3.1f : 0);
            view.Ring.Visible = selection.Contains(entity.Id);
            view.Ring.Position = new Vector3(0, role.IsFlying ? -3.04f : .05f, 0);
            view.Model.Scale = entity.Completed ? Vector3.One : new Vector3(1, Mathf.Lerp(.12f, 1, 1 - (float)entity.BuildTicksLeft / Math.Max(1, entity.BuildTicksTotal)), 1);
            // Only our own snapshot contains a current order target. Enemy combat is shown by public impacts above.
            if (entity.OwnerSlot == snapshot.ViewerSlot && entity.Activity == EntityActivity.Attacking && snapshot.Tick - view.LastShot >= Math.Max(1, role.AttackCooldownTicks))
            {
                var target = snapshot.Entities.FirstOrDefault(e => e.Id == entity.TargetId && !e.IsRemembered);
                if (target != null && role.DeliveryId == "del.instant")
                {
                    var end = ToWorld(target.Position) + Vector3.Up * (_config.Role(target.RoleId).IsFlying ? 3.1f : .8f);
                    Tracer(view.Desired + Vector3.Up, end, role.DamageId == "dmg.cannon" ? new Color("ffeeb5") : new Color("ddbf73"));
                    view.LastShot = snapshot.Tick;
                }
            }
        }
        foreach (int id in _bodies.Keys.Where(id => !present.Contains(id)).ToArray())
        {
            var view = _bodies[id];
            // No death effect when an enemy merely leaves vision.
            if (view.State.OwnerSlot == snapshot.ViewerSlot && view.State.ContainerId == 0 && !snapshot.Entities.Any(e => e.Id == id)) Pulse(view.Root.Position, new Color("fb9856"), 1.2f);
            view.Root.QueueFree();
            _bodies.Remove(id);
        }
        var missileIds = snapshot.Projectiles.Select(p => p.Id).ToHashSet();
        foreach (int id in _missiles.Keys.Where(id => !missileIds.Contains(id)).ToArray()) { _missiles[id].QueueFree(); _missiles.Remove(id); }
        foreach (var projectile in snapshot.Projectiles)
        {
            if (!_missiles.TryGetValue(projectile.Id, out var missile))
            {
                missile = new MeshInstance3D { Mesh = new SphereMesh { Radius = .14f, Height = .28f, RadialSegments = 8, Rings = 4 }, MaterialOverride = Glow(new Color("fff0bf")) };
                AddChild(missile);
                _missiles.Add(projectile.Id, missile);
            }
            var target = snapshot.Entities.FirstOrDefault(e => e.Id == projectile.TargetId);
            var newPosition = ToWorld(projectile.Position) + Vector3.Up * (target != null && _config.Role(target.RoleId).IsFlying ? 2.8f : 1.2f);
            if (missile.Position != Vector3.Zero) Tracer(missile.Position, newPosition, new Color("f2a860"));
            missile.Position = newPosition;
        }
        UpdateRallyFlags(snapshot, selection);
    }

    private void UpdateRallyFlags(MatchSnapshot snapshot, HashSet<int> selection)
    {
        var keep = new HashSet<int>();
        foreach (var entity in snapshot.Entities)
        {
            if (!selection.Contains(entity.Id) || entity.OwnerSlot != snapshot.ViewerSlot || !entity.Completed) continue;
            if (!_config.Roles.Any(r => r.ProducerId == entity.RoleId)) continue;
            keep.Add(entity.Id);
            if (!_rallyFlags.TryGetValue(entity.Id, out var flag))
            {
                flag = RallyFlag(TeamColor(entity.OwnerSlot));
                AddChild(flag);
                _rallyFlags.Add(entity.Id, flag);
            }
            flag.Position = ToWorld(entity.RallyPoint);
        }
        foreach (int id in _rallyFlags.Keys.Where(id => !keep.Contains(id)).ToArray())
        {
            _rallyFlags[id].QueueFree();
            _rallyFlags.Remove(id);
        }
    }

    private static Node3D RallyFlag(Color color)
    {
        var root = new Node3D();
        root.AddChild(new MeshInstance3D
        {
            Mesh = new CylinderMesh { TopRadius = .035f, BottomRadius = .045f, Height = 2.4f, RadialSegments = 8 },
            MaterialOverride = Glow(new Color("e8dcc0")),
            Position = new Vector3(0, 1.2f, 0)
        });
        root.AddChild(new MeshInstance3D
        {
            Mesh = new BoxMesh { Size = new Vector3(.8f, .4f, .04f) },
            MaterialOverride = Glow(color),
            Position = new Vector3(.4f, 2.05f, 0)
        });
        return root;
    }

    public Visibility VisibilityAt(WorldPoint point)
    {
        int x = point.X / _config.Map.CellSize, z = point.Z / _config.Map.CellSize;
        int index = z * _config.Map.WidthCells + x;
        return x >= 0 && z >= 0 && x < _config.Map.WidthCells && z < _config.Map.HeightCells && index < _snapshot.Visibility.Length ? _snapshot.Visibility[index] : Visibility.Shroud;
    }
    public Color TeamColor(int slot) => slot < 0 ? new Color("d7c79b") : new Color(_setup.Slots.First(s => s.Index == slot).Color);

    private static void TintModel(Node root, Color color, bool remembered)
    {
        if (root is MeshInstance3D mesh && mesh.Mesh != null)
        {
            for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
            {
                if (mesh.Mesh.SurfaceGetMaterial(i) is not StandardMaterial3D original) continue;
                var material = (StandardMaterial3D)original.Duplicate();
                if (original.ResourceName.Contains("team_color", StringComparison.OrdinalIgnoreCase)) material.AlbedoColor = color;
                if (remembered) { material.AlbedoColor = material.AlbedoColor.Darkened(.65f); material.EmissionEnabled = false; }
                mesh.SetSurfaceOverrideMaterial(i, material);
            }
        }
        foreach (Node child in root.GetChildren()) TintModel(child, color, remembered);
    }

    private static StandardMaterial3D Glow(Color color) => new() { AlbedoColor = color, ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded, NoDepthTest = false };
    private static MeshInstance3D Ring(float radius, Color color) => new() { Mesh = new TorusMesh { InnerRadius = radius - .035f, OuterRadius = radius + .035f, Rings = 36, RingSegments = 6 }, MaterialOverride = Glow(color), Position = new Vector3(0, .06f, 0) };

    public void SetGhost(string roleId, WorldPoint position, bool allowed, int facing = 0)
    {
        if (string.IsNullOrEmpty(roleId)) { _ghost?.QueueFree(); _ghost = null; _ghostRole = ""; return; }
        if (_ghostRole != roleId || _ghost == null)
        {
            _ghost?.QueueFree();
            _ghostRole = roleId;
            var role = _config.Role(roleId);
            _ghost = new Node3D();
            _ghost.AddChild(CreateModel(role));
            _ghostRing = Ring(role.Radius / Units, Colors.White);
            _ghost.AddChild(_ghostRing);
            AddChild(_ghost);
        }
        _ghost.Position = ToWorld(position) + new Vector3(0, .06f, 0);
        _ghost.Rotation = new Vector3(0, Mathf.DegToRad(facing), 0);
        var color = allowed ? new Color(.35f, 1, .72f, .45f) : new Color(1, .28f, .22f, .45f);
        var material = new StandardMaterial3D { AlbedoColor = color, Transparency = BaseMaterial3D.TransparencyEnum.Alpha, ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded };
        Override(_ghost, material);
        if (_ghostRing != null) _ghostRing.MaterialOverride = Glow(new Color(color.R, color.G, color.B));
    }
    private static void Override(Node node, Material material) { if (node is MeshInstance3D mesh) mesh.MaterialOverride = material; foreach (Node child in node.GetChildren()) Override(child, material); }

    public void MarkOrder(WorldPoint point, Color color) => Pulse(ToWorld(point), color, .65f);
    private void Pulse(Vector3 point, Color color, float radius)
    {
        var ring = Ring(Math.Max(.1f, radius), color);
        ring.Position = point + Vector3.Up * .12f;
        AddChild(ring);
        _effects.Add((ring, .65f));
    }
    private void Tracer(Vector3 from, Vector3 to, Color color)
    {
        var mesh = new ImmediateMesh();
        mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, Glow(color));
        mesh.SurfaceAddVertex(from); mesh.SurfaceAddVertex(to); mesh.SurfaceEnd();
        var node = new MeshInstance3D { Mesh = mesh };
        AddChild(node);
        _effects.Add((node, .13f));
    }

    public override void _Process(double delta)
    {
        foreach (var view in _bodies.Values)
        {
            view.Root.Position = view.Root.Position.Lerp(view.Desired, Math.Min(1, (float)delta * 22));
            var role = _config.Role(view.State.RoleId);
            if (!role.IsBuilding)
            {
                Vector3 direction = view.ObservedHeading;
                if (view.State.OwnerSlot == _snapshot.ViewerSlot)
                {
                    direction = ToWorld(view.State.Destination) - view.Desired;
                    var target = _snapshot.Entities.FirstOrDefault(e => e.Id == view.State.TargetId);
                    if (target != null && view.State.Activity == EntityActivity.Attacking) direction = ToWorld(target.Position) - view.Desired;
                    if (view.State.Activity == EntityActivity.Idle) direction = view.ObservedHeading;
                }
                direction.Y = 0;
                if (direction.LengthSquared() > .15f)
                {
                    float angle = Mathf.Atan2(-direction.X, -direction.Z);
                    view.Model.Rotation = new Vector3(0, Mathf.LerpAngle(view.Model.Rotation.Y, angle, Math.Min(1, (float)delta * 9)), 0);
                }
            }
        }
        for (int i = _effects.Count - 1; i >= 0; i--)
        {
            var effect = _effects[i];
            effect.Life -= (float)delta;
            if (effect.Life <= 0) { effect.Node.QueueFree(); _effects.RemoveAt(i); }
            else _effects[i] = effect;
        }
    }
}
