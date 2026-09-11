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
        var construction = factory.Create(config, config.CreateSetup());
        var buildBefore = construction.Snapshot(0);
        var buildJson = JsonSerializer.Serialize(buildBefore);
        var builder = buildBefore.Entities.Single(e => e.OwnerSlot == 0 && e.RoleId == "build.dozer");
        var build = construction.Submit(new MatchOrder(0, OrderKind.Build, new[] { builder.Id }, Position: new WorldPoint(3300, 4500), ProductId: "power.fusion"));
        if (!build.Accepted) throw new InvalidOperationException("Submission timing: legal construction rejected.");
        var justSubmitted = construction.Snapshot(0);
        if (JsonSerializer.Serialize(justSubmitted) != buildJson || construction.Tick != buildBefore.Tick || justSubmitted.Player.Money != buildBefore.Player.Money || justSubmitted.Entities.Length != buildBefore.Entities.Length)
            throw new InvalidOperationException("Submission timing: Build changed cash, entities, or gameplay before Step.");
        construction.Step();
        var buildAfter = construction.Snapshot(0);
        if (construction.Tick != buildBefore.Tick + 1 || buildAfter.Entities.Single(e => e.Id == builder.Id).Position == builder.Position)
            throw new InvalidOperationException("Submission timing: construction approach did not advance on the next Step.");
        for (var i = 0; i < config.Role("power.fusion").BuildTicks + config.Rules.TickRate * 10; i++) construction.Step();
        var complete = construction.Snapshot(0);
        if (!complete.Entities.Any(e => e.RoleId == "power.fusion" && e.OwnerSlot == 0 && e.Completed) || complete.Player.Money != config.Rules.StartingCash - config.Role("power.fusion").Cost)
            throw new InvalidOperationException("Submission timing: construction must complete with exactly one debit.");
        Console.WriteLine("PASS submission timing: Submit leaves gameplay unchanged; next Step executes detached order.");
    }
}
