using Vision2Audio.Core;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Diagnostics;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Services;

/// <summary>
/// Captures still images from an external USB/UVC camera on Android.
/// </summary>
public sealed class UsbCameraCaptureService(IUsbCameraService usbCameraService) : IUsbCameraCaptureService
{
    /// <inheritdoc />
    public async Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken)
    {
        try
        {
            var jpegBytes = await usbCameraService.CaptureFrameAsync(cancellationToken);
            var fileName = $"otg-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.jpg";
            return Result<SceneCapture>.Success(new SceneCapture(jpegBytes, fileName, "image/jpeg", DateTimeOffset.UtcNow));
        }
        catch (System.OperationCanceledException)
        {
            return Result<SceneCapture>.Failure("Captura OTG cancelada.");
        }
        catch (Exception ex)
        {
            return Result<SceneCapture>.Failure($"Falha na câmera OTG: {SanitizedExceptionDiagnostics.SanitizeForStatus(ex.Message)}");
        }
    }
}
