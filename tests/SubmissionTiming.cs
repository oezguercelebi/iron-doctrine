using System;
using System.Linq;
using System.Text.Json;
using IronDoctrine.Contracts;

namespace IronDoctrine.Proofs;

public static class SubmissionTiming
{
    public static void Run(IMatchFactory factory, string configPath)
    {
        var config = GameConfig.Load(configPath);
        var match = factory.Create(config, config.CreateSetup());
        var before = match.Snapshot(0);
        var serialized = JsonSerializer.Serialize(before);
        var dozer = before.Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer");
        var actors = new[] { dozer.Id };
        var receipt = match.Submit(new MatchOrder(0, OrderKind.Move, actors, Position: new WorldPoint(3300, 4500)));
        if (!receipt.Accepted) throw new InvalidOperationException("Submission timing: legal move rejected.");
        if (JsonSerializer.Serialize(match.Snapshot(0)) != serialized || match.Tick != before.Tick)
            throw new InvalidOperationException("Submission timing: accepted Submit mutated live gameplay before Step.");
        // Mutating the caller's original array must not redirect an accepted order.
        actors[0] = int.MaxValue;
        match.Step();
        var after = match.Snapshot(0);
        if (match.Tick != before.Tick + 1 || after.Entities.Single(e => e.Id == dozer.Id).Position == dozer.Position)
            throw new InvalidOperationException("Submission timing: queued move did not progress on the next Step.");
        if (JsonSerializer.Serialize(before) != serialized) throw new InvalidOperationException("Submission timing: snapshot was not detached.");
        Console.WriteLine("PASS submission timing: Submit leaves gameplay unchanged; next Step executes detached order.");
    }
}
