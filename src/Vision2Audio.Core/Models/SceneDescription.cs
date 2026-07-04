namespace Vision2Audio.Core.Models;

/// <summary>
/// Represents a generated scene description.
/// </summary>
public sealed record SceneDescription(string Text, string Model, DateTimeOffset GeneratedAtUtc);
