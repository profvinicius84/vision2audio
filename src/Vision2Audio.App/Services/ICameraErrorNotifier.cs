namespace Vision2Audio.App.Services;

/// <summary>
/// Publishes camera errors that happen outside the view model pipeline.
/// </summary>
public interface ICameraErrorNotifier
{
    /// <summary>Raised when camera preview/capture infrastructure reports an error.</summary>
    event EventHandler<string>? ErrorReported;

    /// <summary>Reports a camera error to the UI.</summary>
    void Report(string message);
}
