using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IronDoctrine.Proofs;

// Structured verify outcome. Status is pass, fail, or pending. Missing/skipped/timeout is never pass.
public sealed record VerifyOutcome
{
    [JsonPropertyName("id")] public string Id { get; init; } = "";
    [JsonPropertyName("status")] public string Status { get; init; } = "";
    [JsonPropertyName("duration")] public double Duration { get; init; }
    [JsonPropertyName("artifacts")] public string[] Artifacts { get; init; } = Array.Empty<string>();
    [JsonPropertyName("reason")] public string Reason { get; init; } = "";
}

public sealed record VerifyReport
{
    [JsonPropertyName("exit")] public int Exit { get; init; }
    [JsonPropertyName("fail_closed")] public bool FailClosed { get; init; } = true;
    [JsonPropertyName("outcomes")] public VerifyOutcome[] Outcomes { get; init; } = Array.Empty<VerifyOutcome>();
}

public static class VerifyOutcomes
{
    public const string Pass = "pass";
    public const string Fail = "fail";
    public const string Pending = "pending";

    public static readonly JsonSerializerOptions Serializer = CreateSerializer();

    private static JsonSerializerOptions CreateSerializer()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static VerifyOutcome Make(string id, string status, double duration, IEnumerable<string>? artifacts = null, string reason = "")
    {
        if (status is not (Pass or Fail or Pending))
            throw new ArgumentOutOfRangeException(nameof(status), status, "status must be pass, fail, or pending");
        return new VerifyOutcome
        {
            Id = id,
            Status = status,
            Duration = duration,
            Artifacts = artifacts?.ToArray() ?? Array.Empty<string>(),
            Reason = reason ?? ""
        };
    }

    public static string ToJson(VerifyOutcome outcome) => JsonSerializer.Serialize(outcome, Serializer);
    public static string ToJson(IEnumerable<VerifyOutcome> outcomes) =>
        JsonSerializer.Serialize(new VerifyReport { FailClosed = true, Outcomes = outcomes.ToArray() }, Serializer);
    public static string ToJson(VerifyReport report) => JsonSerializer.Serialize(report, Serializer);

    public static void WriteFile(string path, IEnumerable<VerifyOutcome> outcomes, int exit = 0)
    {
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
        var report = new VerifyReport { Exit = exit, FailClosed = true, Outcomes = outcomes.ToArray() };
        File.WriteAllText(path, ToJson(report) + "\n");
    }

    public static int FailClosedExit(IEnumerable<VerifyOutcome> outcomes, IReadOnlySet<string>? gatedIds = null)
    {
        var exit = 0;
        foreach (var outcome in outcomes)
        {
            if (outcome.Status == Pass) continue;
            if (outcome.Status == Fail) { exit = 1; continue; }
            if (outcome.Status == Pending)
            {
                if (gatedIds == null || gatedIds.Contains(outcome.Id)) exit = 1;
                continue;
            }
            exit = 1;
        }
        return exit;
    }
}
