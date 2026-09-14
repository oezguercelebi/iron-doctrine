using System;
using System.Collections.Generic;
using IronDoctrine.Contracts;

namespace IronDoctrine.Proofs;

// Verification scenario names are not catalog prefixes. I-verify fills file I/O.
// Client GUI proof: 1440×900. Viewport.PushInput is the only legal GUI driver.
// ProofPilot.Send is not evidence for client.input_* rows.

public enum VerificationLayer { Simulation, Movement, ClientInput, Presentation, RenderedOutput, Integration }
public enum ScenarioInputKind { Order, Pause, InputEvent }

public static class ScenarioIds
{
    public const string PathGroundTraversesUnbuildable = "path.ground_traverses_unbuildable";
    public const string CombatInstantTracerTiedToShot = "combat.instant_tracer_tied_to_shot";
    public const string PathDistinguishWaitVsStuck = "path.distinguish_wait_vs_stuck";
    public const string BuildGhostMatchesArrivalOccupancy = "build.ghost_matches_arrival_occupancy";
    public const string AssetsNoSkeletalWalk = "assets.no_skeletal_walk";
}

public static class ProofGroups
{
    public const string Acceptance = "acceptance";
    public const string Seam = "seam";
    public const string Sim = "sim";
    public const string Client = "client";
    public const string Assets = "assets";
    public const string Integration = "integration";
}

public sealed record ScenarioInput(long Tick, int Seq, ScenarioInputKind Kind, MatchOrder? Order = null, bool? Paused = null, string InputEvent = "");
public sealed record ScenarioAssertion
{
    public int DeadlineTicks { get; init; }
    public string[] Expect { get; init; } = Array.Empty<string>();
    public string[] Forbid { get; init; } = Array.Empty<string>();
}
public sealed record ScenarioDefinition
{
    public string Id { get; init; } = "";
    public VerificationLayer Layer { get; init; }
    public string CatalogRule { get; init; } = "";
    public uint Seed { get; init; }
    public MatchSetup? SetupOverlay { get; init; }
    public ScenarioInput[] Inputs { get; init; } = Array.Empty<ScenarioInput>();
    public ScenarioAssertion Assertions { get; init; } = new();
    public string ProofGroup { get; init; } = "";
    public string[] Unverified { get; init; } = Array.Empty<string>();
}

public static class ScenarioHarness
{
    public const int GuiWidth = 1440;
    public const int GuiHeight = 900;

    private static readonly Dictionary<string, ScenarioDefinition> Stubs = new(StringComparer.Ordinal)
    {
        [ScenarioIds.PathGroundTraversesUnbuildable] = new()
        {
            Id = ScenarioIds.PathGroundTraversesUnbuildable,
            Layer = VerificationLayer.Movement,
            CatalogRule = "TERRAIN.md ter.unbuildable ground yes / build no",
            ProofGroup = ProofGroups.Acceptance,
            Assertions = new ScenarioAssertion
            {
                DeadlineTicks = 800,
                Expect = new[] { "ground unit occupies or traverses ter.unbuildable" },
                Forbid = new[] { "treating ter.unbuildable as ground-impassable", "CanPlace/build on ter.unbuildable" }
            },
            Unverified = new[] { "file I/O", "air ignore-clutter atomic" }
        },
        [ScenarioIds.CombatInstantTracerTiedToShot] = new()
        {
            Id = ScenarioIds.CombatInstantTracerTiedToShot,
            Layer = VerificationLayer.Simulation,
            CatalogRule = "DELIVERY.md del.instant; diagnostic CombatTrace, not a catalog row",
            ProofGroup = ProofGroups.Acceptance,
            Assertions = new ScenarioAssertion
            {
                DeadlineTicks = 1600,
                Expect = new[] { "CombatTraces Launch and Impact sharing Id after del.instant hit" },
                Forbid = new[] { "FX from Activity+cooldown alone" }
            },
            Unverified = new[] { "file I/O", "rendered tracer frames" }
        },
        [ScenarioIds.PathDistinguishWaitVsStuck] = new()
        {
            Id = ScenarioIds.PathDistinguishWaitVsStuck,
            Layer = VerificationLayer.Movement,
            CatalogRule = "MECHANICS movement; MovementClassification Wait vs Stuck vs Oscillate",
            ProofGroup = ProofGroups.Sim,
            Assertions = new ScenarioAssertion
            {
                Expect = new[] { "dock exclusive loader classified Wait" },
                Forbid = new[] { "full-match fail on raw audit stuck/oscillate counts" }
            },
            Unverified = new[] { "calibrated gate thresholds", "file I/O" }
        },
        [ScenarioIds.BuildGhostMatchesArrivalOccupancy] = new()
        {
            Id = ScenarioIds.BuildGhostMatchesArrivalOccupancy,
            Layer = VerificationLayer.Simulation,
            CatalogRule = "MECHANICS§3 CanPlace; provisional visible-unit occupancy (Q2)",
            ProofGroup = ProofGroups.Acceptance,
            Assertions = new ScenarioAssertion
            {
                Expect = new[] { "empty legal ghost Allowed", "visible unit overlap blocked (provisional)" },
                Forbid = new[] { "CanPlace reason codes revealing hidden enemy units" }
            },
            Unverified = new[] { "full fog-safe occupancy", "file I/O" }
        },
        [ScenarioIds.AssetsNoSkeletalWalk] = new()
        {
            Id = ScenarioIds.AssetsNoSkeletalWalk,
            Layer = VerificationLayer.Presentation,
            CatalogRule = "locomotion-requirements: no armatures / skeletal walk cycles",
            ProofGroup = ProofGroups.Assets,
            Assertions = new ScenarioAssertion
            {
                Expect = new[] { "verify_assets rejects skins/armatures", "zero glTF animations allowed" },
                Forbid = new[] { "skeletal walk/run cycles" }
            },
            Unverified = new[] { "runtime root bob / rotor spin" }
        }
    };

    public static IReadOnlyList<string> ListIds() => new List<string>(Stubs.Keys);

    public static bool TryLoad(string id, out ScenarioDefinition? scenario)
    {
        if (id != null && Stubs.TryGetValue(id, out scenario)) return true;
        scenario = null;
        return false;
    }
}
