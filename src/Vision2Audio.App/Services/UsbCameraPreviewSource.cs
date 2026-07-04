using Android.Content;
using Android.Hardware.Camera2;
using System.Text;
using Vision2Audio.Core;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Diagnostics;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Services;

/// <summary>
/// Reports OTG/USB preview availability for Android.
/// </summary>
public sealed class UsbCameraPreviewSource(IUsbCameraService usbCameraService) : ICameraPreviewSource
{
    /// <inheritdoc />
    public CameraSelectionKind SelectionKind => CameraSelectionKind.Otg;

    /// <inheritdoc />
    public string DisplayName => "OTG/USB";

    /// <inheritdoc />
    public async Task<Result<CameraSourceState>> TryStartPreviewAsync(CancellationToken cancellationToken)
    {
        try
        {
            var usbInitialized = await usbCameraService.InitializeAsync(cancellationToken);
            if (usbInitialized)
            {
                return Result<CameraSourceState>.Success(
                    new CameraSourceState(CameraSelectionKind.Otg, CameraSourceKind.Usb, DisplayName, "Prévia OTG/AUSBC pronta", false));
            }

            var context = Android.App.Application.Context;
            var cameraManager = (CameraManager?)context.GetSystemService(Context.CameraService)
                ?? throw new InvalidOperationException("CameraManager indisponível.");

            var diagnostics = new StringBuilder();
            diagnostics.Append("Câmeras nativas visíveis: ");
            var cameraIds = cameraManager.GetCameraIdList();
            diagnostics.Append(cameraIds.Length == 0 ? "nenhuma" : cameraIds.Length.ToString(System.Globalization.CultureInfo.InvariantCulture));

            diagnostics.Append(" | USB: ");
            diagnostics.Append(await usbCameraService.GetDiagnosticsAsync(cancellationToken));
            diagnostics.Append($" | OTG inicializado={(usbInitialized ? "sim" : "não")}");

            await CloseSessionBestEffortAsync("otg-preview-unavailable-cleanup", CancellationToken.None);
            return Result<CameraSourceState>.Failure($"Prévia OTG/AUSBC indisponível. {diagnostics}.");
        }
        catch (System.OperationCanceledException)
        {
            await CloseSessionBestEffortAsync("otg-preview-cancel-cleanup", CancellationToken.None);
            return Result<CameraSourceState>.Failure("Prévia OTG/AUSBC cancelada.");
        }
        catch (Exception ex)
        {
            await CloseSessionBestEffortAsync("otg-preview-error-cleanup", CancellationToken.None);
            return Result<CameraSourceState>.Failure($"OTG indisponível: {SanitizedExceptionDiagnostics.SanitizeForStatus(ex.Message)}");
        }
    }

    /// <inheritdoc />
    public Task StopPreviewAsync(CancellationToken cancellationToken) => usbCameraService.CloseSessionAsync(cancellationToken);

    private async Task CloseSessionBestEffortAsync(string operation, CancellationToken cancellationToken)
    {
        try
        {
            await usbCameraService.CloseSessionAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not System.OperationCanceledException)
        {
#if ANDROID
            AndroidDiagnosticLog.Exception(operation, ex);
#endif
        }
    }
}
