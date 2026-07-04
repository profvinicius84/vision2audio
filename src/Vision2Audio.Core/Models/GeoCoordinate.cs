namespace Vision2Audio.Core.Models;

/// <summary>
/// Represents a device location with optional human-readable address context.
/// </summary>
public sealed record GeoCoordinate(double Latitude, double Longitude, double? AccuracyMeters = null, string? ApproximateAddress = null);
