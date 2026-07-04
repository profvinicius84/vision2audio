using Vision2Audio.Core;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Services;

/// <summary>
/// MAUI geolocation implementation.
/// </summary>
public sealed class LocationService : ILocationService
{
    /// <inheritdoc />
    public async Task<Result<GeoCoordinate>> GetCurrentLocationAsync(CancellationToken cancellationToken)
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.GetLocationAsync(request, cancellationToken);
            return location is null
                ? Result<GeoCoordinate>.Failure("Não foi possível obter a localização.")
                : Result<GeoCoordinate>.Success(new GeoCoordinate(location.Latitude, location.Longitude, location.Accuracy));
        }
        catch (FeatureNotSupportedException)
        {
            return Result<GeoCoordinate>.Failure("GPS não suportado neste dispositivo.");
        }
        catch (FeatureNotEnabledException)
        {
            return Result<GeoCoordinate>.Failure("GPS desativado.");
        }
        catch (PermissionException)
        {
            return Result<GeoCoordinate>.Failure("Permissão de localização negada.");
        }
    }
}
