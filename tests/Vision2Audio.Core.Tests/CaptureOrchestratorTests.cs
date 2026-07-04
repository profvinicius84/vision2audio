using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Vision2Audio.Core.Services;

namespace Vision2Audio.Core.Tests;

public sealed class CaptureOrchestratorTests
{
    [Fact]
    public async Task CaptureAsync_UsesUsbCapture_WhenUsbSucceeds()
    {
        var orchestrator = new CaptureOrchestrator(
            new FakeCoordinator(CameraSourceKind.Usb),
            new FakeUsbCaptureService(success: true),
            new FakeNativeCaptureService());

        var result = await orchestrator.CaptureAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("usb.jpg", result.Value!.FileName);
    }

    [Fact]
    public async Task CaptureAsync_FailsWithoutNativeFallback_WhenActiveSourceIsUsbAndUsbFails()
    {
        var orchestrator = new CaptureOrchestrator(
            new FakeCoordinator(CameraSourceKind.Usb),
            new FakeUsbCaptureService(success: false),
            new FakeNativeCaptureService());

        var result = await orchestrator.CaptureAsync(CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("usb failed", result.Error);
    }

    private sealed class FakeUsbCaptureService(bool success) : IUsbCameraCaptureService
    {
        public Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken) =>
            Task.FromResult(success
                ? Result<SceneCapture>.Success(new SceneCapture([1], "usb.jpg", "image/jpeg", DateTimeOffset.UtcNow))
                : Result<SceneCapture>.Failure("usb failed"));
    }

    private sealed class FakeNativeCaptureService : INativeCameraCaptureService
    {
        public Task<Result<SceneCapture>> CaptureAsync(CancellationToken cancellationToken) =>
            Task.FromResult(Result<SceneCapture>.Success(new SceneCapture([2], "native.jpg", "image/jpeg", DateTimeOffset.UtcNow)));
    }

    private sealed class FakeCoordinator(CameraSourceKind activeKind) : ICameraSourceCoordinator
    {
        public CameraSourceState Current { get; private set; } = new(CameraSelectionKind.Front, activeKind, activeKind.ToString(), "ready", false);

        public Task<CameraSourceState> InitializeAsync(CancellationToken cancellationToken) => Task.FromResult(Current);

        public Task<CameraSourceState> EnsureActiveSourceAsync(CancellationToken cancellationToken) => Task.FromResult(Current);

        public Task<CameraSourceState> SetPreferredSelectionAsync(CameraSelectionKind selection, CancellationToken cancellationToken) => Task.FromResult(Current);
    }
}
