using Android.Graphics;
using Android.OS;
using Android.Views;
using Microsoft.Maui.ApplicationModel;
using Vision2Audio.Core;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Services;

/// <summary>
/// Captures still frames from the current preview texture.
/// </summary>
public sealed class CameraPreviewFrameProvider : ICameraPreviewFrameProvider
{
    private readonly object _gate = new();
    private TextureView? _textureView;
    private CameraSelectionKind _selectionKind;

    public void Register(TextureView textureView, CameraSelectionKind selectionKind)
    {
        lock (_gate)
        {
            _textureView = textureView;
            _selectionKind = selectionKind;
        }
    }

    public void Unregister(TextureView textureView)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_textureView, textureView))
            {
                _textureView = null;
            }
        }
    }

    public async Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken)
    {
        TextureView? textureView;
        CameraSelectionKind selectionKind;

        lock (_gate)
        {
            textureView = _textureView;
            selectionKind = _selectionKind;
        }

        if (textureView is null || !textureView.IsAvailable)
        {
            return Result<SceneCapture>.Failure("Prévia da câmera indisponível.");
        }

        try
        {
            var bitmap = await MainThread.InvokeOnMainThreadAsync(() => textureView.Bitmap);
            if (bitmap is null)
            {
                return Result<SceneCapture>.Failure("Não foi possível ler a prévia da câmera.");
            }

            await using var memory = new MemoryStream();
            bitmap.Compress(Bitmap.CompressFormat.Jpeg!, 90, memory);
            bitmap.Dispose();

            var prefix = selectionKind switch
            {
                CameraSelectionKind.Front => "front",
                CameraSelectionKind.Rear => "rear",
                CameraSelectionKind.Otg => "otg",
                _ => "camera"
            };

            var fileName = $"{prefix}-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}.jpg";
            return Result<SceneCapture>.Success(new SceneCapture(memory.ToArray(), fileName, "image/jpeg", DateTimeOffset.UtcNow));
        }
        catch (Exception ex)
        {
            return Result<SceneCapture>.Failure($"Falha ao capturar a prévia: {ex.Message}");
        }
    }
}
