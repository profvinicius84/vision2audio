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
                : Result<GeoCoordinate>.Success(new GeoCoordinate(
                    location.Latitude,
                    location.Longitude,
                    location.Accuracy,
                    await TryGetApproximateAddressAsync(location, cancellationToken)));
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

    private static async Task<string?> TryGetApproximateAddressAsync(Location location, CancellationToken cancellationToken)
    {
        try
        {
            var placemarks = await Geocoding.Default.GetPlacemarksAsync(location.Latitude, location.Longitude);
            cancellationToken.ThrowIfCancellationRequested();

            var placemark = placemarks?.FirstOrDefault();
            return placemark is null ? null : FormatAddress(placemark);
        }
        catch (Exception ex) when (ex is FeatureNotSupportedException or PermissionException or IOException or ArgumentException)
        {
            return null;
        }
    }

    private static string? FormatAddress(Placemark placemark)
    {
        var street = JoinDistinct(" ", placemark.Thoroughfare, placemark.SubThoroughfare);
        var cityArea = JoinDistinct(", ", placemark.SubLocality, placemark.Locality);
        var region = JoinDistinct(", ", placemark.AdminArea, placemark.CountryName);
        var address = JoinDistinct(", ", street, cityArea, region);

        return string.IsNullOrWhiteSpace(address) ? null : address;
    }

    private static string JoinDistinct(string separator, params string?[] parts)
    {
        var values = parts
            .Where(part => !string.IsNullOrWhiteSpace(part))
            .Select(part => part!.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase);

        return string.Join(separator, values);
    }
}
