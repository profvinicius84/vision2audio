using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Captures a scene from an external OTG/UVC camera.
/// </summary>
public interface IUsbCameraCaptureService
{
    /// <summary>Captures a scene image from a USB camera.</summary>
    Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken);
}
