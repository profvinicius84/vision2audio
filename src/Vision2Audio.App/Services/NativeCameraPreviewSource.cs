using Vision2Audio.Core;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Services;

/// <summary>
/// Reports native camera preview as fallback.
/// </summary>
public sealed class NativeCameraPreviewSource : ICameraPreviewSource
{
    private readonly CameraSelectionKind _selectionKind;
    private readonly string _displayName;

    public NativeCameraPreviewSource(CameraSelectionKind selectionKind, string displayName)
    {
        _selectionKind = selectionKind;
        _displayName = displayName;
    }

    /// <inheritdoc />
    public CameraSelectionKind SelectionKind => _selectionKind;

    /// <inheritdoc />
    public string DisplayName => _displayName;

    /// <inheritdoc />
    public Task<Result<CameraSourceState>> TryStartPreviewAsync(CancellationToken cancellationToken)
        => Task.FromResult(Result<CameraSourceState>.Success(
            new CameraSourceState(_selectionKind, CameraSourceKind.Native, DisplayName, "Prévia nativa ativa", true)));

    /// <inheritdoc />
    public Task StopPreviewAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
