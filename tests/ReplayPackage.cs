using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using IronDoctrine.Contracts;

namespace IronDoctrine.Proofs;

// Sealed replay bundle using ReplayBundle field names. Inputs are replayed into a fresh IMatch
// via Submit / SetPaused / Step only. InputEvent lines are stored, not applied (client-gui).
public sealed class LoadedReplay
{
    public string Revision { get; init; } = "";
    public string PatchHash { get; init; } = "";
    public string ConfigHash { get; init; } = "";
    public string ManifestHash { get; init; } = "";
    public string Toolchain { get; init; } = "";
    public uint Seed { get; init; }
    public MatchSetup Setup { get; init; } = new();
    public ScenarioInput[] Inputs { get; init; } = Array.Empty<ScenarioInput>();
    public ReplayCheckpoint[] Checkpoints { get; init; } = Array.Empty<ReplayCheckpoint>();
    public ReplayOutcome? Outcome { get; init; }
}

public static class ReplayPackage
{
    public static readonly JsonSerializerOptions Json = CreateJson();
    private static readonly JsonSerializerOptions Jsonl = CreateJsonl();

    private static JsonSerializerOptions CreateJson()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    private static JsonSerializerOptions CreateJsonl()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static string RepoRoot()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "tools", "proof.sh"))) return dir.FullName;
            dir = dir.Parent;
        }
        return Directory.GetCurrentDirectory();
    }

    public static string Sha256Hex(byte[] data) => Convert.ToHexString(SHA256.HashData(data)).ToLowerInvariant();
    public static string Sha256Hex(string text) => Sha256Hex(Encoding.UTF8.GetBytes(text));

    public static LoadedReplay Capture(IMatchFactory factory, GameConfig config, MatchSetup setup, IReadOnlyList<ScenarioInput> inputs, long untilTick)
    {
        var match = factory.Create(config, setup);
        var checkpoints = new List<ReplayCheckpoint> { new(match.Tick, match.StateHash()) };
        var groups = GroupInputs(inputs);
        while (match.Tick < untilTick)
        {
            if (groups.TryGetValue(match.Tick, out var batch)) Apply(match, batch);
            var before = match.Tick;
            match.Step();
            if (match.Tick == before) break;
            checkpoints.Add(new ReplayCheckpoint(match.Tick, match.StateHash()));
        }
        return new LoadedReplay
        {
            Seed = setup.Seed,
            Setup = setup,
            Inputs = inputs.ToArray(),
            Checkpoints = checkpoints.ToArray()
        };
    }

    public static ReplayOutcome Replay(IMatchFactory factory, GameConfig config, LoadedReplay bundle)
    {
        var match = factory.Create(config, bundle.Setup);
        var expected = bundle.Checkpoints ?? Array.Empty<ReplayCheckpoint>();
        long? firstTick = null;
        string firstField = "";
        void Compare(long tick)
        {
            ReplayCheckpoint? hit = null;
            for (var i = expected.Length - 1; i >= 0; i--)
                if (expected[i].Tick == tick) { hit = expected[i]; break; }
            if (hit is null) return;
            var actual = match.StateHash();
            if (!string.Equals(actual, hit.StateHash, StringComparison.Ordinal) && firstTick is null)
            {
                firstTick = tick;
                firstField = "StateHash";
            }
        }

        var groups = GroupInputs(bundle.Inputs ?? Array.Empty<ScenarioInput>());
        Compare(match.Tick);
        long last = expected.Length == 0 ? 0 : expected.Max(c => c.Tick);
        int safety = 0;
        long budget = last + 32;
        while (match.Tick < last && safety++ < budget)
        {
            if (groups.TryGetValue(match.Tick, out var batch)) Apply(match, batch);
            var before = match.Tick;
            match.Step();
            if (match.Tick == before)
            {
                if (firstTick is null && expected.Any(c => c.Tick > before))
                {
                    firstTick = expected.First(c => c.Tick > before).Tick;
                    firstField = "StateHash";
                }
                break;
            }
            Compare(match.Tick);
        }
        return new ReplayOutcome(match.StateHash(), firstTick, firstField);
    }

    public static void Write(string directory, LoadedReplay bundle, ReplayOutcome? outcome = null)
    {
        Directory.CreateDirectory(directory);
        var inputsPath = Path.Combine(directory, ReplayBundle.InputsJsonl);
        var checkpointsPath = Path.Combine(directory, ReplayBundle.CheckpointsJsonl);
        var outcomePath = Path.Combine(directory, ReplayBundle.OutcomeJson);
        var manifestPath = Path.Combine(directory, "manifest.json");
        File.WriteAllText(inputsPath, WriteJsonl(bundle.Inputs.Select(ToLine)));
        File.WriteAllText(checkpointsPath, WriteJsonl(bundle.Checkpoints));
        var manifest = ToManifest(bundle);
        manifest.ManifestHash = ManifestHash(manifest, File.ReadAllBytes(inputsPath), File.ReadAllBytes(checkpointsPath));
        File.WriteAllText(manifestPath, JsonSerializer.Serialize(manifest, Json) + "\n");
        var resolved = outcome ?? bundle.Outcome ?? new ReplayOutcome("", null, "");
        File.WriteAllText(outcomePath, JsonSerializer.Serialize(resolved, Json) + "\n");
    }

    public static LoadedReplay Read(string directory)
    {
        var manifestPath = Path.Combine(directory, "manifest.json");
        var inputsPath = Path.Combine(directory, ReplayBundle.InputsJsonl);
        var checkpointsPath = Path.Combine(directory, ReplayBundle.CheckpointsJsonl);
        var outcomePath = Path.Combine(directory, ReplayBundle.OutcomeJson);
        if (!File.Exists(manifestPath) || !File.Exists(inputsPath) || !File.Exists(checkpointsPath))
            throw new InvalidDataException("sealed replay bundle is missing required files");
        var manifest = JsonSerializer.Deserialize<ReplayManifestFile>(File.ReadAllText(manifestPath), Json)
            ?? throw new InvalidDataException("sealed replay manifest.json is empty");
        var claimed = manifest.ManifestHash ?? "";
        var actual = ManifestHash(manifest, File.ReadAllBytes(inputsPath), File.ReadAllBytes(checkpointsPath));
        if (!string.Equals(claimed, actual, StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException("sealed replay manifest_hash mismatch");
        ReplayOutcome? outcome = null;
        if (File.Exists(outcomePath))
            outcome = JsonSerializer.Deserialize<ReplayOutcome>(File.ReadAllText(outcomePath), Json);
        var setup = manifest.Setup ?? new MatchSetup();
        var seed = manifest.Seed != 0 ? manifest.Seed : setup.Seed;
        return new LoadedReplay
        {
            Revision = manifest.Revision ?? "",
            PatchHash = manifest.PatchHash ?? "",
            ConfigHash = manifest.ConfigHash ?? "",
            ManifestHash = claimed,
            Toolchain = manifest.Toolchain ?? "",
            Seed = seed,
            Setup = setup,
            Inputs = ReadInputs(File.ReadAllText(inputsPath)),
            Checkpoints = ReadCheckpoints(File.ReadAllText(checkpointsPath)),
            Outcome = outcome
        };
    }

    public static void RunProof(IMatchFactory factory, string configPath, string? artifactsDir = null)
    {
        var config = GameConfig.Load(configPath);
        var setup = config.CreateSetup();
        var probe = factory.Create(config, setup);
        var dozer = probe.Snapshot(0).Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer");
        var inputs = new[]
        {
            new ScenarioInput(0, 1, ScenarioInputKind.Order,
                Order: new MatchOrder(0, OrderKind.Move, new[] { dozer.Id }, Position: new WorldPoint(3300, 4500)))
        };
        const int until = 48;
        var captured = Capture(factory, config, setup, inputs, until);
        var root = RepoRoot();
        var (revision, patch) = GitIdentity(root);
        var configHash = Sha256Hex(File.ReadAllBytes(configPath));
        var filled = new LoadedReplay
        {
            Revision = revision,
            PatchHash = patch,
            ConfigHash = configHash,
            Toolchain = Toolchain(),
            Seed = captured.Seed,
            Setup = captured.Setup,
            Inputs = captured.Inputs,
            Checkpoints = captured.Checkpoints
        };
        var dir = artifactsDir ?? Path.Combine(root, "artifacts", "verify", "replay-sealed");
        var outcome = Replay(factory, config, filled);
        if (outcome.FirstDivergentTick is not null)
            throw new InvalidOperationException("sealed replay roundtrip diverged at tick " + outcome.FirstDivergentTick + " field " + outcome.FirstDivergentField);
        Write(dir, filled, outcome);
        var reread = Read(dir);
        var again = Replay(factory, config, reread);
        if (again.FirstDivergentTick is not null)
            throw new InvalidOperationException("sealed replay reread diverged at tick " + again.FirstDivergentTick);
        if (reread.Inputs.Length != 1 || reread.Checkpoints.Length == 0)
            throw new InvalidOperationException("sealed replay bundle lost inputs or checkpoints");
        var mutated = new LoadedReplay
        {
            Revision = reread.Revision,
            PatchHash = reread.PatchHash,
            ConfigHash = reread.ConfigHash,
            ManifestHash = reread.ManifestHash,
            Toolchain = reread.Toolchain,
            Seed = reread.Seed,
            Setup = reread.Setup,
            Checkpoints = reread.Checkpoints,
            Inputs = new[]
            {
                reread.Inputs[0] with
                {
                    Order = reread.Inputs[0].Order! with { Position = new WorldPoint(1800, 1800) }
                }
            }
        };
        var diverged = Replay(factory, config, mutated);
        if (diverged.FirstDivergentTick is null)
            throw new InvalidOperationException("sealed replay negative control did not report a divergent tick");
        if (string.IsNullOrEmpty(diverged.FirstDivergentField))
            throw new InvalidOperationException("sealed replay negative control missing first divergent field");
        Console.WriteLine(
            $"PASS sealed replay: ticks={until} checkpoints={reread.Checkpoints.Length} hash={outcome.StateHash} roundtrip; negative control diverged at tick {diverged.FirstDivergentTick} field {diverged.FirstDivergentField}");
    }

    private static Dictionary<long, ScenarioInput[]> GroupInputs(IReadOnlyList<ScenarioInput> inputs) =>
        inputs.OrderBy(i => i.Tick).ThenBy(i => i.Seq).GroupBy(i => i.Tick).ToDictionary(g => g.Key, g => g.ToArray());

    private static void Apply(IMatch match, IEnumerable<ScenarioInput> batch)
    {
        foreach (var input in batch.OrderBy(i => i.Seq))
        {
            switch (input.Kind)
            {
                case ScenarioInputKind.Order:
                    if (input.Order is null) throw new InvalidDataException("replay order line missing order");
                    match.Submit(input.Order);
                    break;
                case ScenarioInputKind.Pause:
                    match.SetPaused(input.Paused ?? true);
                    break;
                case ScenarioInputKind.InputEvent:
                    break;
                default:
                    throw new InvalidDataException("unknown replay input kind " + input.Kind);
            }
        }
    }

    private static InputLine ToLine(ScenarioInput input) => new()
    {
        Tick = input.Tick,
        Seq = input.Seq,
        Kind = input.Kind.ToString(),
        Order = input.Kind == ScenarioInputKind.Order ? input.Order : null,
        Pause = input.Kind == ScenarioInputKind.Pause ? input.Paused : null,
        InputEvent = input.Kind == ScenarioInputKind.InputEvent ? input.InputEvent : null
    };

    private static ScenarioInput[] ReadInputs(string text)
    {
        var list = new List<ScenarioInput>();
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            var row = JsonSerializer.Deserialize<InputLine>(line, Jsonl) ?? throw new InvalidDataException("bad inputs.jsonl line");
            var kind = ParseKind(row);
            list.Add(new ScenarioInput(row.Tick, row.Seq, kind, row.Order, row.Pause, row.InputEvent ?? ""));
        }
        return list.ToArray();
    }

    private static ScenarioInputKind ParseKind(InputLine row)
    {
        if (!string.IsNullOrEmpty(row.Kind) && Enum.TryParse<ScenarioInputKind>(row.Kind, true, out var parsed))
            return parsed;
        if (row.Order != null) return ScenarioInputKind.Order;
        if (row.Pause != null) return ScenarioInputKind.Pause;
        if (!string.IsNullOrEmpty(row.InputEvent)) return ScenarioInputKind.InputEvent;
        throw new InvalidDataException("inputs.jsonl line missing kind/order/pause/input_event");
    }

    private static ReplayCheckpoint[] ReadCheckpoints(string text)
    {
        var list = new List<ReplayCheckpoint>();
        foreach (var raw in text.Split('\n'))
        {
            var line = raw.Trim();
            if (line.Length == 0) continue;
            var row = JsonSerializer.Deserialize<ReplayCheckpoint>(line, Jsonl) ?? throw new InvalidDataException("bad checkpoints.jsonl line");
            list.Add(row);
        }
        return list.ToArray();
    }

    private static string WriteJsonl<T>(IEnumerable<T> rows)
    {
        var builder = new StringBuilder();
        foreach (var row in rows)
            builder.Append(JsonSerializer.Serialize(row, Jsonl)).Append('\n');
        return builder.ToString();
    }

    private static ReplayManifestFile ToManifest(LoadedReplay bundle) => new()
    {
        Revision = bundle.Revision ?? "",
        PatchHash = bundle.PatchHash ?? "",
        ConfigHash = bundle.ConfigHash ?? "",
        ManifestHash = "",
        Toolchain = bundle.Toolchain ?? "",
        Seed = bundle.Seed,
        Setup = bundle.Setup
    };

    private static string ManifestHash(ReplayManifestFile manifest, byte[] inputs, byte[] checkpoints)
    {
        var copy = new ReplayManifestFile
        {
            Revision = manifest.Revision ?? "",
            PatchHash = manifest.PatchHash ?? "",
            ConfigHash = manifest.ConfigHash ?? "",
            ManifestHash = "",
            Toolchain = manifest.Toolchain ?? "",
            Seed = manifest.Seed,
            Setup = manifest.Setup ?? new MatchSetup()
        };
        var payload = JsonSerializer.Serialize(copy, Json);
        var bytes = Encoding.UTF8.GetBytes(payload).Concat(inputs).Concat(checkpoints).ToArray();
        return Sha256Hex(bytes);
    }

    private sealed class ReplayManifestFile
    {
        [JsonPropertyName(ReplayBundle.Revision)] public string Revision { get; set; } = "";
        [JsonPropertyName(ReplayBundle.PatchHash)] public string PatchHash { get; set; } = "";
        [JsonPropertyName(ReplayBundle.ConfigHash)] public string ConfigHash { get; set; } = "";
        [JsonPropertyName(ReplayBundle.ManifestHash)] public string ManifestHash { get; set; } = "";
        [JsonPropertyName(ReplayBundle.Toolchain)] public string Toolchain { get; set; } = "";
        [JsonPropertyName(ReplayBundle.Seed)] public uint Seed { get; set; }
        [JsonPropertyName(ReplayBundle.Setup)] public MatchSetup Setup { get; set; } = new();
    }

    private static (string Revision, string Patch) GitIdentity(string root)
    {
        try
        {
            var revision = Run("git", "rev-parse HEAD", root).Trim();
            var diff = Run("git", "diff --no-ext-diff HEAD", root);
            return (revision, Sha256Hex(diff));
        }
        catch
        {
            return ("unknown", "unknown");
        }
    }

    private static string Toolchain() =>
        $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}; {System.Runtime.InteropServices.RuntimeInformation.OSDescription}";

    private static string Run(string file, string arguments, string work)
    {
        var start = new ProcessStartInfo
        {
            FileName = file,
            Arguments = arguments,
            WorkingDirectory = work,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        using var process = Process.Start(start) ?? throw new InvalidOperationException("failed to start " + file);
        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0) throw new InvalidOperationException(file + " exited " + process.ExitCode);
        return output;
    }

    private sealed class InputLine
    {
        public long Tick { get; set; }
        public int Seq { get; set; }
        public string Kind { get; set; } = "";
        public MatchOrder? Order { get; set; }
        public bool? Pause { get; set; }
        public string? InputEvent { get; set; }
    }
}
