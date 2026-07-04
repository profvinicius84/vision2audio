using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Services;

/// <summary>
/// Stores history entries locally in a JSON file.
/// </summary>
public sealed class FileHistoryRepository(string filePath) : IHistoryRepository
{
    private readonly SemaphoreSlim _gate = new(1, 1);

    /// <inheritdoc />
    public async Task<Result<HistoryEntry>> AddAsync(SceneDescription description, GeoCoordinate? location, CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var entries = await ReadAllAsync(cancellationToken);
            var entry = new HistoryEntry(
                entries.Count == 0 ? 1 : entries.Max(x => x.Id) + 1,
                description.GeneratedAtUtc,
                description.Text,
                description.Model,
                location?.Latitude,
                location?.Longitude);

            entries.Add(entry);
            await WriteAllAsync(entries, cancellationToken);
            return Result<HistoryEntry>.Success(entry);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<HistoryEntry>> GetRecentAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var entries = await ReadAllAsync(cancellationToken);
            return entries.OrderByDescending(x => x.CapturedAtUtc).ThenByDescending(x => x.Id).ToList();
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task ClearAllAsync(CancellationToken cancellationToken)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<List<HistoryEntry>> ReadAllAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return [];
        }

        await using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var entries = await JsonSerializer.DeserializeAsync<List<HistoryEntry>>(stream, cancellationToken: cancellationToken);
        return entries ?? [];
    }

    private async Task WriteAllAsync(List<HistoryEntry> entries, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, entries, new JsonSerializerOptions { WriteIndented = true }, cancellationToken);
    }
}
