namespace Vision2Audio.App.Services;

/// <summary>
/// In-process hub for hardware trigger notifications.
/// </summary>
public sealed class TriggerHub
{
    /// <summary>Raised when a supported remote or keyboard trigger is pressed.</summary>
    public event EventHandler? Triggered;

    /// <summary>Signals a trigger press.</summary>
    public void SignalTriggered() => Triggered?.Invoke(this, EventArgs.Empty);
}
