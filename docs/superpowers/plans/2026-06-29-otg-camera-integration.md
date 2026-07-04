# OTG Camera Integration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make OTG/UVC camera capture the primary image source on Android, with the native camera kept as a fallback, while preserving the current GPS → OpenAI → speech → history flow.

**Architecture:** The app keeps one capture entry point (`ICaptureService`) but routes it through a small orchestrator that tries the USB/OTG camera first and falls back to the native Android camera if OTG is unavailable or fails. The Android-specific capture code stays in `Vision2Audio.App`; the selection/fallback logic and tests stay in `Vision2Audio.Core` so the behavior is easy to verify without hardware.

**Tech Stack:** .NET MAUI, Android 11+, Android Camera APIs/UVC-compatible USB camera access, xUnit, existing Core/App split.

---

## Spec traceability

- Approved spec: `docs/superpowers/specs/2026-06-27-vision-2-audio-design.md`
- Scope references: `Scope 42-53`, `Acceptance 66-68`, `Constraints 99-104`, `Risks 120-123`

## File structure

- `src/Vision2Audio.Core/Abstractions/INativeCameraCaptureService.cs` — contract for the native fallback capture source.
- `src/Vision2Audio.Core/Abstractions/IUsbCameraCaptureService.cs` — contract for the OTG/UVC capture source.
- `src/Vision2Audio.Core/Services/CaptureOrchestrator.cs` — `ICaptureService` implementation that tries USB first, then native.
- `src/Vision2Audio.Core/Result.cs` and the existing models stay unchanged.
- `src/Vision2Audio.App/Services/UsbCameraCaptureService.cs` — Android OTG/UVC capture implementation.
- `src/Vision2Audio.App/Services/NativeCameraCaptureService.cs` — implement the fallback contract rather than the generic capture interface directly.
- `src/Vision2Audio.App/MauiProgram.cs` — register the orchestrator and both capture sources.
- `src/Vision2Audio.App/Platforms/Android/AndroidManifest.xml` — camera, USB host, and any required permissions/features for external camera access.
- `src/Vision2Audio.App/MainPage.xaml` — update user-facing copy to explain that OTG is preferred and native camera is fallback.
- `tests/Vision2Audio.Core.Tests/CaptureOrchestratorTests.cs` — verify fallback order and error handling.

## Tasks

### Task 1: Add capture-source contracts and fallback orchestrator

**Files:**
- Create: `src/Vision2Audio.Core/Abstractions/IUsbCameraCaptureService.cs`
- Create: `src/Vision2Audio.Core/Abstractions/INativeCameraCaptureService.cs`
- Create: `src/Vision2Audio.Core/Services/CaptureOrchestrator.cs`
- Modify: `src/Vision2Audio.Core/Abstractions/ICaptureService.cs` if the interface needs no signature change but the comments should describe the orchestrated behavior
- Modify: `tests/Vision2Audio.Core.Tests/Vision2Audio.Core.Tests.csproj` only if a missing project reference blocks the new test file

- [ ] **Step 1: Write the failing test**

Create `tests/Vision2Audio.Core.Tests/CaptureOrchestratorTests.cs` with two cases:

```csharp
[Fact]
public async Task CaptureAsync_UsesUsbCapture_WhenUsbSucceeds()
{
    var orchestrator = new CaptureOrchestrator(
        new FakeUsbCaptureService(success: true),
        new FakeNativeCaptureService());

    var result = await orchestrator.CaptureAsync(CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("usb.jpg", result.Value!.FileName);
}

[Fact]
public async Task CaptureAsync_FallsBackToNative_WhenUsbFails()
{
    var orchestrator = new CaptureOrchestrator(
        new FakeUsbCaptureService(success: false),
        new FakeNativeCaptureService());

    var result = await orchestrator.CaptureAsync(CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal("native.jpg", result.Value!.FileName);
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
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj --filter CaptureOrchestratorTests`
Expected: fail because `CaptureOrchestrator` and the new capture contracts do not exist yet.

- [ ] **Step 3: Write the minimal implementation**

Implement `CaptureOrchestrator` so it:
1. calls `IUsbCameraCaptureService.CaptureAsync`
2. returns the USB result when successful
3. otherwise calls `INativeCameraCaptureService.CaptureAsync`
4. returns the native result when successful
5. returns the USB error if both fail and no native result is available

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj --filter CaptureOrchestratorTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Vision2Audio.Core/Abstractions/IUsbCameraCaptureService.cs src/Vision2Audio.Core/Abstractions/INativeCameraCaptureService.cs src/Vision2Audio.Core/Services/CaptureOrchestrator.cs tests/Vision2Audio.Core.Tests/CaptureOrchestratorTests.cs
git commit -m "feat: add capture fallback orchestrator"
```

### Task 2: Implement Android OTG capture and wire the fallback

**Files:**
- Create: `src/Vision2Audio.App/Services/UsbCameraCaptureService.cs`
- Modify: `src/Vision2Audio.App/Services/NativeCameraCaptureService.cs`
- Modify: `src/Vision2Audio.App/MauiProgram.cs`
- Modify: `src/Vision2Audio.App/Platforms/Android/AndroidManifest.xml`
- Modify: `src/Vision2Audio.App/MainPage.xaml`
- Modify: `src/Vision2Audio.App/MainPage.xaml.cs` only if the page needs a different status message or capture label

- [ ] **Step 1: Write the failing test or build target**

There is no practical unit test for the Android camera API wiring in this repo yet, so use the first build as the guardrail:

Run: `dotnet build src\Vision2Audio.App\Vision2Audio.App.csproj -f net10.0-android`

Expected: fail until the new OTG service and DI wiring are added.

- [ ] **Step 2: Implement the OTG service**

Create `UsbCameraCaptureService` as the primary capture source. It should:
1. detect whether an external/USB camera is available on the current Android device
2. capture a still image from the OTG/UVC source
3. return `Result<SceneCapture>.Success(...)` when capture succeeds
4. return a clear `Result<SceneCapture>.Failure(...)` when no external camera is found or capture fails

Use Android 11-compatible APIs and keep the implementation behind the `IUsbCameraCaptureService` contract so the rest of the app never depends on Android-specific types.

- [ ] **Step 3: Wire the fallback path**

Update DI in `MauiProgram.cs` so the app registers:
1. `IUsbCameraCaptureService` → `UsbCameraCaptureService`
2. `INativeCameraCaptureService` → `NativeCameraCaptureService`
3. `ICaptureService` → `CaptureOrchestrator`

Update `MainPage.xaml` text so the primary action communicates that OTG is preferred and native camera is a fallback.

- [ ] **Step 4: Run the app build to verify it passes**

Run: `dotnet build src\Vision2Audio.App\Vision2Audio.App.csproj -f net10.0-android`
Expected: PASS with no warnings introduced by this change.

- [ ] **Step 5: Commit**

```bash
git add src/Vision2Audio.App/Services/UsbCameraCaptureService.cs src/Vision2Audio.App/Services/NativeCameraCaptureService.cs src/Vision2Audio.App/MauiProgram.cs src/Vision2Audio.App/Platforms/Android/AndroidManifest.xml src/Vision2Audio.App/MainPage.xaml
git commit -m "feat: add otg camera capture"
```

### Task 3: Validate on Android 11 hardware and close the loop

**Files:**
- Modify: `.opencode/context/current-state.md`
- Modify: `.opencode/context/architecture.md`
- Modify: `.opencode/context/decisions.md`
- Modify: `.opencode/context/stack.md`
- Modify: `docs/superpowers/tasks/2026-06-27-vision-2-audio-task-1.3.md` or add a new validation note if the wave/task plan needs to record the camera fallback decision

- [ ] **Step 1: Run the unit tests again**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj`
Expected: PASS.

- [ ] **Step 2: Deploy to an Android 11 device**

Use a physical Android 11 phone with a USB OTG camera attached. Confirm:
1. the app opens
2. the capture button triggers the OTG camera path first
3. the capture falls back to native only when OTG is unavailable
4. GPS still reaches OpenAI
5. the response still displays, speaks, and stores in history

- [ ] **Step 3: Record the validation result**

Document which camera path was used, whether fallback occurred, and whether the capture succeeded on Android 11.

- [ ] **Step 4: Commit**

```bash
git add .opencode/context/current-state.md .opencode/context/architecture.md .opencode/context/decisions.md .opencode/context/stack.md
git commit -m "docs: record otg camera integration state"
```

## Execution order

1. Task 1 — capture fallback contracts and orchestrator
2. Task 2 — Android OTG capture implementation and DI wiring
3. Task 3 — Android 11 device validation and context updates

## Human review checklist

- OTG camera is the primary source, not the native camera.
- Native camera remains available as a clear fallback.
- The app still satisfies the approved capture → GPS → OpenAI → speech → history flow.
- Android 11 remains the validation target.
- Build and tests pass before any device validation is treated as complete.
