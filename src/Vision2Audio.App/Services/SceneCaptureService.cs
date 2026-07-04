using Vision2Audio.Core;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Services;

/// <summary>
/// Uses the device capture flow to get a scene image.
/// </summary>
public sealed class SceneCaptureService : ICaptureService
{
    /// <inheritdoc />
    public async Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken)
    {
        try
        {
            var photo = await MediaPicker.Default.CapturePhotoAsync();
            if (photo is null)
            {
                return Result<SceneCapture>.Failure("A captura foi cancelada.");
            }

            await using var source = await photo.OpenReadAsync();

            await using var memory = new MemoryStream();
            await source.CopyToAsync(memory, cancellationToken);

            var originalFileName = photo.FileName ?? string.Empty;
            var fileName = string.IsNullOrWhiteSpace(originalFileName)
                ? $"scene-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.jpg"
                : originalFileName;

            return Result<SceneCapture>.Success(new SceneCapture(memory.ToArray(), fileName, "image/jpeg", DateTimeOffset.UtcNow));
        }
        catch (TaskCanceledException)
        {
            return Result<SceneCapture>.Failure("Captura cancelada.");
        }
        catch (Exception ex)
        {
            return Result<SceneCapture>.Failure($"Falha na captura da cena: {ex.Message}");
        }
    }
}
