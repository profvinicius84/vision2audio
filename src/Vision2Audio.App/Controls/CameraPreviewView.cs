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

    /// <summary>Bindable pause flag used to stop live preview while a captured still is displayed.</summary>
    public static readonly BindableProperty IsPausedProperty = BindableProperty.Create(
        nameof(IsPaused),
        typeof(bool),
        typeof(CameraPreviewView),
        false);

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

    /// <summary>Stops the live preview without disposing the view.</summary>
    public bool IsPaused
    {
        get => (bool)GetValue(IsPausedProperty);
        set => SetValue(IsPausedProperty, value);
    }

    /// <summary>Creates the preview host.</summary>
    public CameraPreviewView() => BackgroundColor = Colors.Black;
}
