using IronDoctrine.Proofs;
var data = Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json");
if (args is ["audit", ..])
{
    BehaviorAudit.Run(data);
    return;
}
SeamConformance.Run(new IronDoctrine.Sim.MatchFactory(), data);
SubmissionTiming.Run(new IronDoctrine.Sim.MatchFactory(), Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json"));
MechanicsProof.Run(Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json"));
ReviewRegressionProof.Run(Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json"));
OrderBoundaryProof.Run(Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json"));
FullMatchProof.Run(Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json"));
