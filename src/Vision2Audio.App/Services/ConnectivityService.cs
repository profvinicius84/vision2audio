using Vision2Audio.Core;
using Vision2Audio.Core.Abstractions;

namespace Vision2Audio.App.Services;

/// <summary>
/// MAUI connectivity implementation.
/// </summary>
public sealed class ConnectivityService : IConnectivityService
{
    /// <inheritdoc />
    public bool IsOnline() => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
}
