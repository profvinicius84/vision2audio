using Vision2Audio.Core.Models;

namespace Vision2Audio.App.Controls;

/// <summary>
/// Hosts the live Android camera preview.
/// </summary>
public sealed class CameraPreviewView : View
{
    /// <summary>Bindable selected camera kind.</summary>
    public static readonly BindableProperty SelectionKindProperty = BindableProperty.Create(
        nameof(SelectionKind),
        typeof(CameraSelectionKind),
        typeof(CameraPreviewView),
        CameraSelectionKind.Front);

    /// <summary>Bindable refresh token to restart preview after permission/state changes.</summary>
    public static readonly BindableProperty RefreshTokenProperty = BindableProperty.Create(
        nameof(RefreshToken),
        typeof(long),
        typeof(CameraPreviewView),
        0L);

    /// <summary>Current selection to preview.</summary>
    public CameraSelectionKind SelectionKind
    {
        get => (CameraSelectionKind)GetValue(SelectionKindProperty);
        set => SetValue(SelectionKindProperty, value);
    }

    /// <summary>Token used to force preview restart.</summary>
    public long RefreshToken
    {
        get => (long)GetValue(RefreshTokenProperty);
        set => SetValue(RefreshTokenProperty, value);
    }

    /// <summary>Creates the preview host.</summary>
    public CameraPreviewView() => BackgroundColor = Colors.Black;
}
