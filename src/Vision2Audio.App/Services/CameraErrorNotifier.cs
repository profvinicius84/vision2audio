namespace Vision2Audio.App.Services;

/// <summary>
/// In-process camera error event hub.
/// </summary>
public sealed class CameraErrorNotifier : ICameraErrorNotifier
{
    public event EventHandler<string>? ErrorReported;

    public void Report(string message) => ErrorReported?.Invoke(this, message);
}
