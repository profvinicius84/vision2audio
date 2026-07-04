using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Captures one scene image on demand.
/// </summary>
public interface ICaptureService
{
    /// <summary>Captures an image from the current scene source.</summary>
    Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken);
}
