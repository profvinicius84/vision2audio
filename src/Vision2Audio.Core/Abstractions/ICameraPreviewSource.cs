using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Starts and stops a preview for a specific camera source.
/// </summary>
public interface ICameraPreviewSource
{
    /// <summary>Source kind.</summary>
    CameraSelectionKind SelectionKind { get; }

    /// <summary>Friendly display name.</summary>
    string DisplayName { get; }

    /// <summary>Starts preview and returns the selected state.</summary>
    Task<Result<CameraSourceState>> TryStartPreviewAsync(CancellationToken cancellationToken);

    /// <summary>Stops preview.</summary>
    Task StopPreviewAsync(CancellationToken cancellationToken);
}
