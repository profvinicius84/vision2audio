using Android.Views;
using Vision2Audio.Core;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Services;

/// <summary>
/// Provides snapshots from the active live preview surface.
/// </summary>
public interface ICameraPreviewFrameProvider
{
    /// <summary>Registers the active preview surface.</summary>
    void Register(TextureView textureView, CameraSelectionKind selectionKind);

    /// <summary>Unregisters the active preview surface.</summary>
    void Unregister(TextureView textureView);

    /// <summary>Captures a still frame from the active preview.</summary>
    Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken);
}
