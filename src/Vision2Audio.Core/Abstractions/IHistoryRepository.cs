using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Abstractions;

/// <summary>
/// Stores and retrieves read-only history entries.
/// </summary>
public interface IHistoryRepository
{
    /// <summary>Saves a new history entry.</summary>
    Task<Result<HistoryEntry>> AddAsync(SceneDescription description, GeoCoordinate? location, CancellationToken cancellationToken);

    /// <summary>Returns the most recent history entries first.</summary>
    Task<IReadOnlyList<HistoryEntry>> GetRecentAsync(CancellationToken cancellationToken);

    /// <summary>Deletes all history entries.</summary>
    Task ClearAllAsync(CancellationToken cancellationToken);
}
