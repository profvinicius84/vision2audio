namespace Vision2Audio.App;

/// <summary>
/// Simple app context for shared UI state.
/// </summary>
public sealed class AppContext
{
    /// <summary>Indicates whether the app is busy processing a capture.</summary>
    public bool IsBusy { get; set; }
}
