using Android.Views;

namespace Vision2Audio.App.Services;

/// <summary>
/// Android USB/UVC camera discovery and access boundary.
/// </summary>
public interface IUsbCameraService
{
    /// <summary>Detects USB camera candidates and requests permission when needed.</summary>
    Task<bool> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>Starts USB camera preview on the supplied Android preview view.</summary>
    Task StartPreviewAsync(TextureView previewView, CancellationToken cancellationToken = default);

    /// <summary>Captures one frame from a USB camera when a UVC backend is available.</summary>
    Task<byte[]> CaptureFrameAsync(CancellationToken cancellationToken = default);

    /// <summary>Closes any active USB camera session and releases Android UVC resources.</summary>
    Task CloseSessionAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns human-readable USB diagnostics for alerts.</summary>
    Task<string> GetDiagnosticsAsync(CancellationToken cancellationToken = default);
}
