using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Chooses the active camera source and exposes preview state.
/// </summary>
public interface ICameraSourceCoordinator
{
    /// <summary>Current active source state.</summary>
    CameraSourceState Current { get; }

    /// <summary>Initializes the camera preview selection.
    /// </summary>
    Task<CameraSourceState> InitializeAsync(CancellationToken cancellationToken);

    /// <summary>Ensures an active source is available.
    /// </summary>
    Task<CameraSourceState> EnsureActiveSourceAsync(CancellationToken cancellationToken);

    /// <summary>Updates the preferred selection and refreshes preview state.</summary>
    Task<CameraSourceState> SetPreferredSelectionAsync(CameraSelectionKind selection, CancellationToken cancellationToken);
}
