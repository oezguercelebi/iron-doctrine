using IronDoctrine.Proofs;
using IronDoctrine.Sim;

var data = Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json");
string? outDir = null;
var positional = new List<string>();
for (var i = 0; i < args.Length; i++)
{
    if (args[i] is "--out" or "--outcomes-json" && i + 1 < args.Length)
    {
        outDir = args[++i];
        continue;
    }
    positional.Add(args[i]);
}
args = positional.ToArray();

if (args.Length > 0 && args[0] is "--help" or "-h" or "help")
{
    Console.WriteLine("SimProof: no args = seam, timing, mechanics, regressions, boundaries, full match.");
    Console.WriteLine("  acceptance            frozen C acceptance contracts");
    Console.WriteLine("  audit                 behavior audit report");
    Console.WriteLine("  --list                named groups and ScenarioHarness ids");
    Console.WriteLine("  --scenario <id>       one scenario (acceptance group runs the frozen C suite)");
    return;
}

if (args.Length > 0 && args[0] is "--list" or "list")
{
    ListScenarios();
    return;
}

if (args is ["audit", ..])
{
    BehaviorAudit.Run(data);
    return;
}
if (args is ["acceptance", ..])
{
    AcceptanceContracts.Run(new MatchFactory(), data);
    return;
}
if (args.Length > 0 && args[0] is "--scenario" or "scenario")
{
    if (args.Length < 2 || string.IsNullOrWhiteSpace(args[1]))
        throw new InvalidOperationException("missing --scenario id");
    RunScenario(args[1], data, outDir);
    return;
}

DefaultProof(data);

static void DefaultProof(string data)
{
    var factory = new MatchFactory();
    SeamConformance.Run(factory, data);
    SubmissionTiming.Run(factory, data);
    MechanicsProof.Run(data);
    ReviewRegressionProof.Run(data);
    OrderBoundaryProof.Run(data);
    FullMatchProof.Run(data);
}

static void ListScenarios()
{
    Console.WriteLine("group\tacceptance");
    Console.WriteLine("group\taudit");
    Console.WriteLine("group\tsim");
    foreach (var id in ScenarioHarness.ListIds().OrderBy(v => v, StringComparer.Ordinal))
        Console.WriteLine("scenario\t" + id);
    Console.WriteLine("scenario\tverify.sealed_replay");
}

static void RunScenario(string id, string data, string? outDir)
{
    if (string.IsNullOrWhiteSpace(id))
        throw new InvalidOperationException("missing --scenario id");
    if (id == "verify.sealed_replay")
    {
        ReplayPackage.RunProof(new MatchFactory(), data, outDir);
        return;
    }
    if (!ScenarioHarness.TryLoad(id, out var scenario) || scenario is null)
        throw new InvalidOperationException("unknown scenario " + id);
    if (scenario.ProofGroup == ProofGroups.Acceptance)
    {
        AcceptanceContracts.Run(new MatchFactory(), data);
        return;
    }
    if (scenario.ProofGroup == ProofGroups.Assets)
        throw new InvalidOperationException("PENDING " + id + ": run via tools/verify.sh --group assets");
    if (scenario.ProofGroup == ProofGroups.Sim)
        throw new InvalidOperationException("PENDING " + id + ": i-sim atomic not present; audit is diagnostic only (Q5).");
    throw new InvalidOperationException("PENDING " + id + ": no executable runner in SimProof");
}
