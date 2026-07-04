using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Vision2Audio.Core.Services;

namespace Vision2Audio.Core.Tests;

public sealed class CameraSourceCoordinatorTests
{
    [Fact]
    public async Task InitializeAsync_ChoosesPreferredOtgWhenAvailable()
    {
        var coordinator = new CameraSourceCoordinator(
            new FakeSelectionStore(CameraSelectionKind.Otg),
            [
                new FakePreviewSource(CameraSelectionKind.Otg, shouldStart: true, CameraSourceKind.Usb, "USB preview"),
                new FakePreviewSource(CameraSelectionKind.Front, shouldStart: true, CameraSourceKind.Native, "Front preview"),
                new FakePreviewSource(CameraSelectionKind.Rear, shouldStart: true, CameraSourceKind.Native, "Rear preview")
            ]);

        var state = await coordinator.InitializeAsync(CancellationToken.None);

        Assert.Equal(CameraSelectionKind.Otg, state.SelectedKind);
        Assert.Equal(CameraSourceKind.Usb, state.ActiveKind);
        Assert.Equal(CameraSelectionKind.Otg, state.ActiveSelectionKind);
        Assert.Equal("USB preview", state.DisplayName);
        Assert.False(state.IsFallback);
    }

    [Fact]
    public async Task InitializeAsync_FallsBackToFrontWhenOtgPreviewFails()
    {
        var coordinator = new CameraSourceCoordinator(
            new FakeSelectionStore(CameraSelectionKind.Otg),
            [
                new FakePreviewSource(CameraSelectionKind.Otg, shouldStart: false, CameraSourceKind.Usb, "USB preview"),
                new FakePreviewSource(CameraSelectionKind.Front, shouldStart: true, CameraSourceKind.Native, "Front preview"),
                new FakePreviewSource(CameraSelectionKind.Rear, shouldStart: true, CameraSourceKind.Native, "Rear preview")
            ]);

        var state = await coordinator.InitializeAsync(CancellationToken.None);

        Assert.Equal(CameraSelectionKind.Otg, state.SelectedKind);
        Assert.Equal(CameraSourceKind.Native, state.ActiveKind);
        Assert.Equal(CameraSelectionKind.Front, state.ActiveSelectionKind);
        Assert.Equal("Front preview", state.DisplayName);
        Assert.True(state.IsFallback);
        Assert.Contains("Fallback ativo", state.StatusMessage);
        Assert.Contains("fonte selecionada não está disponível", state.StatusMessage);
    }

    [Fact]
    public async Task InitializeAsync_WhenAllSourcesFailReportsNoCameraSourceAvailable()
    {
        var coordinator = new CameraSourceCoordinator(
            new FakeSelectionStore(CameraSelectionKind.Otg),
            [
                new FakePreviewSource(CameraSelectionKind.Otg, shouldStart: false, CameraSourceKind.Usb, "USB preview"),
                new FakePreviewSource(CameraSelectionKind.Front, shouldStart: false, CameraSourceKind.Native, "Front preview"),
                new FakePreviewSource(CameraSelectionKind.Rear, shouldStart: false, CameraSourceKind.Native, "Rear preview")
            ]);

        var state = await coordinator.InitializeAsync(CancellationToken.None);

        Assert.Equal(CameraSelectionKind.Otg, state.SelectedKind);
        Assert.Equal(CameraSourceKind.Unavailable, state.ActiveKind);
        Assert.Null(state.ActiveSelectionKind);
        Assert.False(state.IsFallback);
        Assert.Contains("Nenhuma fonte de câmera disponível", state.StatusMessage);
    }

    [Fact]
    public async Task SetPreferredSelectionAsync_StopsPreviousSourceBeforeStartingNewSelection()
    {
        var events = new List<string>();
        var otg = new FakePreviewSource(CameraSelectionKind.Otg, shouldStart: true, CameraSourceKind.Usb, "USB preview", events);
        var front = new FakePreviewSource(CameraSelectionKind.Front, shouldStart: true, CameraSourceKind.Native, "Front preview", events);
        var coordinator = new CameraSourceCoordinator(
            new FakeSelectionStore(CameraSelectionKind.Otg),
            [otg, front]);

        var initial = await coordinator.InitializeAsync(CancellationToken.None);
        var changed = await coordinator.SetPreferredSelectionAsync(CameraSelectionKind.Front, CancellationToken.None);

        Assert.Equal(CameraSourceKind.Usb, initial.ActiveKind);
        Assert.Equal(CameraSelectionKind.Front, changed.SelectedKind);
        Assert.Equal(CameraSourceKind.Native, changed.ActiveKind);
        Assert.Equal(1, otg.StopCount);
        Assert.Equal(1, front.StartCount);
        Assert.Equal(["start:USB preview", "stop:USB preview", "start:Front preview"], events);
    }

    private sealed class FakePreviewSource(
        CameraSelectionKind selectionKind,
        bool shouldStart,
        CameraSourceKind activeKind,
        string displayName,
        List<string>? events = null) : ICameraPreviewSource
    {
        public CameraSelectionKind SelectionKind => selectionKind;
        public string DisplayName => displayName;
        public int StartCount { get; private set; }
        public int StopCount { get; private set; }

        public Task<Result<CameraSourceState>> TryStartPreviewAsync(CancellationToken cancellationToken)
        {
            StartCount++;
            events?.Add($"start:{displayName}");
            return Task.FromResult(
                shouldStart
                    ? Result<CameraSourceState>.Success(new CameraSourceState(selectionKind, activeKind, displayName, "Preview ready", activeKind == CameraSourceKind.Native))
                    : Result<CameraSourceState>.Failure($"{displayName} unavailable"));
        }

        public Task StopPreviewAsync(CancellationToken cancellationToken)
        {
            StopCount++;
            events?.Add($"stop:{displayName}");
            return Task.CompletedTask;
        }
    }

    private sealed class FakeSelectionStore(CameraSelectionKind selectedKind) : ICameraSelectionStore
    {
        public Task<Result<CameraSelection>> LoadAsync(CancellationToken cancellationToken)
            => Task.FromResult(Result<CameraSelection>.Success(new CameraSelection(selectedKind)));

        public Task SaveAsync(CameraSelection selection, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
