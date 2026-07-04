using Vision2Audio.Core;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Services;

/// <summary>
/// Captures a scene with the native Android camera app.
/// </summary>
public sealed class NativeCameraCaptureService : INativeCameraCaptureService
{
    private readonly ICameraPreviewFrameProvider _previewFrameProvider;

    public NativeCameraCaptureService(ICameraPreviewFrameProvider previewFrameProvider)
    {
        _previewFrameProvider = previewFrameProvider;
    }

    /// <inheritdoc />
    public async Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _previewFrameProvider.CaptureAsync(cancellationToken);
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
