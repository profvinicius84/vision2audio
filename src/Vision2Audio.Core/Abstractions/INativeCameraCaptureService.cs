using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Captures a scene from the native Android camera.
/// </summary>
public interface INativeCameraCaptureService
{
    /// <summary>Captures a scene image from the native camera.</summary>
    Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken);
}
