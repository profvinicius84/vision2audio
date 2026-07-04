using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Vision2Audio.Core.Services;

namespace Vision2Audio.Core.Tests;

public sealed class SceneDescriptionCoordinatorTests
{
    [Fact]
    public async Task CaptureAndDescribeAsync_ReturnsOfflineFailure_WhenNoInternet()
    {
        var coordinator = new SceneDescriptionCoordinator(
            new FakeConnectivityService(false),
            new FakeCaptureService(),
            new FakeLocationService(),
            new FakeOpenAiService(),
            new InMemoryHistoryRepository());

        var result = await coordinator.CaptureAndDescribeAsync(CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("Sem conexão com a internet.", result.Error);
    }

    [Fact]
    public async Task CaptureAndDescribeAsync_SavesHistory_WhenRequestSucceeds()
    {
        var history = new InMemoryHistoryRepository();
        var coordinator = new SceneDescriptionCoordinator(
            new FakeConnectivityService(true),
            new FakeCaptureService(),
            new FakeLocationService(),
            new FakeOpenAiService(),
            history);

        var result = await coordinator.CaptureAndDescribeAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Uma pessoa em pé ao lado de uma mesa.", result.Value?.Text);
        Assert.Single(history.Entries);
    }

    [Fact]
    public async Task CaptureAndDescribeAsync_ContinuesWithNullLocation_WhenLocationUnavailable()
    {
        var openAi = new FakeOpenAiService();
        var history = new InMemoryHistoryRepository();
        var coordinator = new SceneDescriptionCoordinator(
            new FakeConnectivityService(true),
            new FakeCaptureService(),
            new FailingLocationService(),
            openAi,
            history);

        var result = await coordinator.CaptureAndDescribeAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(openAi.LastLocation);
        Assert.Single(history.Entries);
        Assert.Null(history.Entries[0].Latitude);
        Assert.Null(history.Entries[0].Longitude);
    }

    private sealed class FakeConnectivityService(bool isOnline) : IConnectivityService
    {
        public bool IsOnline() => isOnline;
    }

    private sealed class FakeCaptureService : ICaptureService
    {
        public Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Result<SceneCapture>.Success(new SceneCapture([1, 2, 3], "capture.jpg", "image/jpeg", DateTimeOffset.UtcNow)));
    }

    private sealed class FakeLocationService : ILocationService
    {
        public Task<Result<GeoCoordinate>> GetCurrentLocationAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Result<GeoCoordinate>.Success(new GeoCoordinate(-23.5, -46.6)));
    }

    private sealed class FailingLocationService : ILocationService
    {
        public Task<Result<GeoCoordinate>> GetCurrentLocationAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Result<GeoCoordinate>.Failure("GPS indisponível."));
    }

    private sealed class FakeOpenAiService : IOpenAiSceneDescriptionService
    {
        public GeoCoordinate? LastLocation { get; private set; }

        public Task<Result<SceneDescription>> DescribeAsync(SceneCapture capture, GeoCoordinate? location, CancellationToken cancellationToken) =>
            Task.FromResult(Describe(location));

        private Result<SceneDescription> Describe(GeoCoordinate? location)
        {
            LastLocation = location;
            return Result<SceneDescription>.Success(new SceneDescription("Uma pessoa em pé ao lado de uma mesa.", "test-model", DateTimeOffset.UtcNow));
        }
    }

    private sealed class InMemoryHistoryRepository : IHistoryRepository
    {
        public List<HistoryEntry> Entries { get; } = [];

        public Task<Result<HistoryEntry>> AddAsync(SceneDescription description, GeoCoordinate? location, CancellationToken cancellationToken)
        {
            var entry = new HistoryEntry(Entries.Count + 1, description.GeneratedAtUtc, description.Text, description.Model, location?.Latitude, location?.Longitude);
            Entries.Add(entry);
            return Task.FromResult(Result<HistoryEntry>.Success(entry));
        }

        public Task<IReadOnlyList<HistoryEntry>> GetRecentAsync(CancellationToken cancellationToken) =>
            Task.FromResult((IReadOnlyList<HistoryEntry>)Entries.OrderByDescending(x => x.Id).ToList());

        public Task ClearAllAsync(CancellationToken cancellationToken)
        {
            Entries.Clear();
            return Task.CompletedTask;
        }
    }
}
