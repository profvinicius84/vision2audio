namespace Vision2Audio.Core.Models;

/// <summary>
/// Represents a geographic coordinate.
/// </summary>
public sealed record GeoCoordinate(double Latitude, double Longitude, double? AccuracyMeters = null);
