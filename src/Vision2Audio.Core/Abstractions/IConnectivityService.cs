namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Checks whether the device currently has internet access.
/// </summary>
public interface IConnectivityService
{
    /// <summary>Returns true when online.</summary>
    bool IsOnline();
}
