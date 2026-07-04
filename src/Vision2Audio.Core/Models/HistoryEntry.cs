namespace Vision2Audio.Core.Models;

/// <summary>
/// Represents one read-only local history entry.
/// </summary>
public sealed record HistoryEntry(
    long Id,
    DateTimeOffset CapturedAtUtc,
    string Description,
    string? Model,
    double? Latitude,
    double? Longitude);
