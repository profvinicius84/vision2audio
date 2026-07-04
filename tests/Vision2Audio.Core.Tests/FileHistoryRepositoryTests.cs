using Vision2Audio.Core.Models;
using Vision2Audio.Core.Services;

namespace Vision2Audio.Core.Tests;

public sealed class FileHistoryRepositoryTests
{
    [Fact]
    public async Task AddGetAndClear_WorkAsExpected()
    {
        var filePath = Path.Combine(Path.GetTempPath(), $"vision2audio-{Guid.NewGuid():N}.json");
        try
        {
            var repository = new FileHistoryRepository(filePath);
            var description = new SceneDescription("Descrição teste", "test-model", DateTimeOffset.UtcNow);
            var addResult = await repository.AddAsync(description, new GeoCoordinate(1.23, 4.56), CancellationToken.None);

            Assert.True(addResult.IsSuccess);

            var entries = await repository.GetRecentAsync(CancellationToken.None);
            Assert.Single(entries);
            Assert.Equal("Descrição teste", entries[0].Description);

            await repository.ClearAllAsync(CancellationToken.None);
            entries = await repository.GetRecentAsync(CancellationToken.None);
            Assert.Empty(entries);
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
