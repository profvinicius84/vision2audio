namespace Vision2Audio.Core.Models;

/// <summary>
/// Describes the selected and active camera source plus status.
/// </summary>
public sealed record CameraSourceState(
    CameraSelectionKind SelectedKind,
    CameraSourceKind ActiveKind,
    string DisplayName,
    string StatusMessage,
    bool IsFallback,
    CameraSelectionKind? ActiveSelectionKind = null);
