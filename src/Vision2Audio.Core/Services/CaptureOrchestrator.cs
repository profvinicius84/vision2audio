using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Services;

/// <summary>
/// Captures from the currently active camera source selected by the coordinator.
/// </summary>
public sealed class CaptureOrchestrator(ICameraSourceCoordinator cameraSourceCoordinator, IUsbCameraCaptureService usbCameraCaptureService, INativeCameraCaptureService nativeCameraCaptureService) : ICaptureService
{
    /// <inheritdoc />
    public async Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken)
    {
        var state = await cameraSourceCoordinator.EnsureActiveSourceAsync(cancellationToken);

        if (state.ActiveKind == CameraSourceKind.Usb)
        {
            var usbResult = await usbCameraCaptureService.CaptureAsync(cancellationToken);
            if (usbResult.IsSuccess)
            {
                return usbResult;
            }

            return Result<SceneCapture>.Failure(usbResult.Error ?? "Falha ao capturar pela câmera OTG/USB ativa.");
        }

        var nativeOnlyResult = await nativeCameraCaptureService.CaptureAsync(cancellationToken);
        if (nativeOnlyResult.IsSuccess)
        {
            return nativeOnlyResult;
        }

        var usbFallbackResult = await usbCameraCaptureService.CaptureAsync(cancellationToken);
        return usbFallbackResult.IsSuccess
            ? usbFallbackResult
            : Result<SceneCapture>.Failure(nativeOnlyResult.Error ?? usbFallbackResult.Error ?? "Nenhuma câmera disponível.");
    }
}
