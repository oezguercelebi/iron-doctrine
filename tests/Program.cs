using IronDoctrine.Proofs;
SeamConformance.Run(new IronDoctrine.Sim.MatchFactory(), Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json"));
SubmissionTiming.Run(new IronDoctrine.Sim.MatchFactory(), Path.Combine(AppContext.BaseDirectory, "data/slice1.placeholders.json"));
