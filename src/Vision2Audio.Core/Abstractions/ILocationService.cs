using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Gets the current device location.
/// </summary>
public interface ILocationService
{
    /// <summary>Gets the current location or a failure.</summary>
    Task<Result<GeoCoordinate>> GetCurrentLocationAsync(CancellationToken cancellationToken);
}
