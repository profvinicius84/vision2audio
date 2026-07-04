using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Loads and stores the user's preferred camera selection.
/// </summary>
public interface ICameraSelectionStore
{
    /// <summary>Loads the saved selection or a default preference.</summary>
    Task<Result<CameraSelection>> LoadAsync(CancellationToken cancellationToken);

    /// <summary>Saves the selected camera.</summary>
    Task SaveAsync(CameraSelection selection, CancellationToken cancellationToken);
}
