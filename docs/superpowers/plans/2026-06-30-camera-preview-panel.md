# Camera Preview Panel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a live preview panel inside the main screen that shows the active camera source, tries OTG/USB first, and falls back to the native camera without breaking the existing GPS → OpenAI → speech → history flow.

**Architecture:** Introduce a small camera-source coordination layer in `Vision2Audio.Core` so preview selection and capture selection share the same source state. On Android, implement a MAUI preview control backed by a native preview surface and two source implementations: OTG/USB first, native fallback second. The UI only binds to the coordinator state and preview control, keeping source selection logic isolated and testable.

**Tech Stack:** .NET MAUI, Android 11+, Android Camera2, MAUI custom handler, xUnit, existing Core/App split.

---

## Spec traceability

- Approved spec: `docs/superpowers/specs/2026-06-29-camera-preview-panel-design.md`
- Scope references: `In scope 41-46`, `Acceptance 58-76`, `Business rules 80-82`, `Constraints 86-89`, `Risks 103-105`

## File structure

- `src/Vision2Audio.Core/Models/CameraSourceKind.cs` — enum for `Usb`, `Native`, `Unavailable`.
- `src/Vision2Audio.Core/Models/CameraSourceState.cs` — active source, status text, and fallback flag.
- `src/Vision2Audio.Core/Abstractions/ICameraPreviewSource.cs` — source contract for preview startup/stop.
- `src/Vision2Audio.Core/Abstractions/ICameraSourceCoordinator.cs` — shared selection/state contract.
- `src/Vision2Audio.Core/Services/CameraSourceCoordinator.cs` — chooses OTG first, then native, and keeps the current state.
- `src/Vision2Audio.Core/Services/CaptureOrchestrator.cs` — update to capture from the active source selected by the coordinator.
- `tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs` — verifies source selection, fallback, and unavailable state.
- `src/Vision2Audio.App/Controls/CameraPreviewView.cs` — MAUI preview host control.
- `src/Vision2Audio.App/Platforms/Android/CameraPreviewViewHandler.cs` — Android handler that maps the control to a native preview surface.
- `src/Vision2Audio.App/Services/AndroidCameraSessionFactory.cs` — shared Android Camera2 session helper for preview and still capture.
- `src/Vision2Audio.App/Services/UsbCameraPreviewSource.cs` — OTG/USB preview implementation.
- `src/Vision2Audio.App/Services/NativeCameraPreviewSource.cs` — native camera fallback preview implementation.
- `src/Vision2Audio.App/MauiProgram.cs` — DI registrations for preview sources, coordinator, and control.
- `src/Vision2Audio.App/Platforms/Android/AndroidManifest.xml` — ensure the camera and USB host permissions/features remain correct.
- `src/Vision2Audio.App/MainPage.xaml` — add the live preview panel and active-source status.
- `src/Vision2Audio.App/ViewModels/MainViewModel.cs` — expose the preview status and active source text.
- none — the preview state is covered by the core coordinator tests and the app build target.

## Tasks

### Task 1: Add camera-source state and fallback coordinator

**Files:**
- Create: `src/Vision2Audio.Core/Models/CameraSourceKind.cs`
- Create: `src/Vision2Audio.Core/Models/CameraSourceState.cs`
- Create: `src/Vision2Audio.Core/Abstractions/ICameraPreviewSource.cs`
- Create: `src/Vision2Audio.Core/Abstractions/ICameraSourceCoordinator.cs`
- Create: `src/Vision2Audio.Core/Services/CameraSourceCoordinator.cs`
- Modify: `src/Vision2Audio.Core/Services/CaptureOrchestrator.cs`
- Test: `tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs` with these cases:

```csharp
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Vision2Audio.Core.Services;

namespace Vision2Audio.Core.Tests;

public sealed class CameraSourceCoordinatorTests
{
    [Fact]
    public async Task InitializeAsync_ChoosesUsbWhenUsbPreviewStarts()
    {
        var coordinator = new CameraSourceCoordinator(
            new FakePreviewSource(CameraSourceKind.Usb, shouldStart: true, "USB preview"),
            new FakePreviewSource(CameraSourceKind.Native, shouldStart: true, "Native preview"));

        var state = await coordinator.InitializeAsync(CancellationToken.None);

        Assert.Equal(CameraSourceKind.Usb, state.ActiveKind);
        Assert.Equal("USB preview", state.DisplayName);
        Assert.False(state.IsFallback);
    }

    [Fact]
    public async Task InitializeAsync_FallsBackToNativeWhenUsbPreviewFails()
    {
        var coordinator = new CameraSourceCoordinator(
            new FakePreviewSource(CameraSourceKind.Usb, shouldStart: false, "USB preview"),
            new FakePreviewSource(CameraSourceKind.Native, shouldStart: true, "Native preview"));

        var state = await coordinator.InitializeAsync(CancellationToken.None);

        Assert.Equal(CameraSourceKind.Native, state.ActiveKind);
        Assert.Equal("Native preview", state.DisplayName);
        Assert.True(state.IsFallback);
    }

    private sealed class FakePreviewSource(CameraSourceKind kind, bool shouldStart, string displayName) : ICameraPreviewSource
    {
        public CameraSourceKind Kind => kind;
        public string DisplayName => displayName;

        public Task<Result<CameraSourceState>> TryStartPreviewAsync(CancellationToken cancellationToken) =>
            Task.FromResult(
                shouldStart
                    ? Result<CameraSourceState>.Success(new CameraSourceState(kind, displayName, "Preview ready", isFallback: kind == CameraSourceKind.Native))
                    : Result<CameraSourceState>.Failure($"{displayName} unavailable"));

        public Task StopPreviewAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj --filter CameraSourceCoordinatorTests`
Expected: FAIL because `CameraSourceCoordinator` and the new camera-source types do not exist yet.

- [ ] **Step 3: Write the minimal implementation**

Implement the new core types exactly as follows:

```csharp
namespace Vision2Audio.Core.Models;

public enum CameraSourceKind
{
    Usb,
    Native,
    Unavailable
}

public sealed record CameraSourceState(
    CameraSourceKind ActiveKind,
    string DisplayName,
    string StatusMessage,
    bool IsFallback);
```

```csharp
namespace Vision2Audio.Core.Abstractions;

public interface ICameraPreviewSource
{
    CameraSourceKind Kind { get; }
    string DisplayName { get; }
    Task<Result<CameraSourceState>> TryStartPreviewAsync(CancellationToken cancellationToken);
    Task StopPreviewAsync(CancellationToken cancellationToken);
}

public interface ICameraSourceCoordinator
{
    CameraSourceState Current { get; }
    Task<CameraSourceState> InitializeAsync(CancellationToken cancellationToken);
    Task<CameraSourceState> EnsureActiveSourceAsync(CancellationToken cancellationToken);
}
```

`CameraSourceCoordinator` should try the USB preview source first, then native, and cache the current state so the UI and capture flow read the same source.

Update `CaptureOrchestrator` so it captures from the source selected by `ICameraSourceCoordinator` instead of choosing a different source independently.

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj --filter CameraSourceCoordinatorTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Vision2Audio.Core/Models/CameraSourceKind.cs src/Vision2Audio.Core/Models/CameraSourceState.cs src/Vision2Audio.Core/Abstractions/ICameraPreviewSource.cs src/Vision2Audio.Core/Abstractions/ICameraSourceCoordinator.cs src/Vision2Audio.Core/Services/CameraSourceCoordinator.cs src/Vision2Audio.Core/Services/CaptureOrchestrator.cs tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs
git commit -m "feat: add camera source coordinator"
```

### Task 2: Build the Android preview host and source services

**Files:**
- Create: `src/Vision2Audio.App/Controls/CameraPreviewView.cs`
- Create: `src/Vision2Audio.App/Platforms/Android/CameraPreviewViewHandler.cs`
- Create: `src/Vision2Audio.App/Services/AndroidCameraSessionFactory.cs`
- Create: `src/Vision2Audio.App/Services/UsbCameraPreviewSource.cs`
- Create: `src/Vision2Audio.App/Services/NativeCameraPreviewSource.cs`
- Modify: `src/Vision2Audio.App/Services/UsbCameraCaptureService.cs`
- Modify: `src/Vision2Audio.App/Services/NativeCameraCaptureService.cs`
- Modify: `src/Vision2Audio.App/MauiProgram.cs`
- Modify: `src/Vision2Audio.App/Platforms/Android/AndroidManifest.xml`

- [ ] **Step 1: Write the failing build target**

Run: `dotnet build src\Vision2Audio.App\Vision2Audio.App.csproj -f net10.0-android`
Expected: FAIL until the new preview control, handler, and services are wired up.

- [ ] **Step 2: Implement the preview control and handler**

Create the MAUI control as a thin shell around native rendering:

```csharp
namespace Vision2Audio.App.Controls;

public sealed class CameraPreviewView : ContentView
{
    public static readonly BindableProperty ActiveSourceProperty = BindableProperty.Create(
        nameof(ActiveSource),
        typeof(string),
        typeof(CameraPreviewView),
        "Câmera indisponível");

    public static readonly BindableProperty StatusTextProperty = BindableProperty.Create(
        nameof(StatusText),
        typeof(string),
        typeof(CameraPreviewView),
        "Aguardando câmera...");

    public string ActiveSource
    {
        get => (string)GetValue(ActiveSourceProperty);
        set => SetValue(ActiveSourceProperty, value);
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }
}
```

On Android, map the control to a native preview surface so the active source can render live frames. The handler should:
1. create the native preview view
2. connect it to `AndroidCameraSessionFactory`
3. start the active source when the handler connects
4. stop the preview when the handler disconnects

- [ ] **Step 3: Implement the Android camera session helper**

Add `AndroidCameraSessionFactory` so `UsbCameraPreviewSource`, `NativeCameraPreviewSource`, `UsbCameraCaptureService`, and `NativeCameraCaptureService` share the same camera-opening and session setup logic. The factory should encapsulate camera-id selection, camera open/close, and preview/capture session creation so source switching stays synchronized.

- [ ] **Step 4: Wire DI and permissions**

Register these services in `MauiProgram.cs`:
1. `ICameraSourceCoordinator` → `CameraSourceCoordinator`
2. `ICameraPreviewSource` for USB → `UsbCameraPreviewSource`
3. `ICameraPreviewSource` for native → `NativeCameraPreviewSource`
4. `CameraPreviewViewHandler`
5. the existing capture services should reuse the same session helper

Keep `AndroidManifest.xml` aligned with the existing camera and USB host permissions/features so the preview can run on device.

- [ ] **Step 5: Run the app build to verify it passes**

Run: `dotnet build src\Vision2Audio.App\Vision2Audio.App.csproj -f net10.0-android`
Expected: PASS.

- [ ] **Step 6: Commit**

```bash
git add src/Vision2Audio.App/Controls/CameraPreviewView.cs src/Vision2Audio.App/Controls/CameraPreviewViewHandler.cs src/Vision2Audio.App/Services/AndroidCameraSessionFactory.cs src/Vision2Audio.App/Services/UsbCameraPreviewSource.cs src/Vision2Audio.App/Services/NativeCameraPreviewSource.cs src/Vision2Audio.App/Services/UsbCameraCaptureService.cs src/Vision2Audio.App/Services/NativeCameraCaptureService.cs src/Vision2Audio.App/MauiProgram.cs src/Vision2Audio.App/Platforms/Android/AndroidManifest.xml
git commit -m "feat: add android camera preview host"
```

### Task 3: Integrate the preview panel into the main screen

**Files:**
- Modify: `src/Vision2Audio.App/MainPage.xaml`
- Modify: `src/Vision2Audio.App/ViewModels/MainViewModel.cs`
- Modify: `src/Vision2Audio.App/MainPage.xaml.cs` only if the page needs binding startup changes
- Test: `tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs` may need an extra assertion for the fallback status text

- [ ] **Step 1: Run the app build to verify it fails**

Run: `dotnet build src\Vision2Audio.App\Vision2Audio.App.csproj -f net10.0-android`
Expected: FAIL until `MainViewModel` exposes `ActiveCameraSource` and `CameraPreviewStatus` and the new preview control exists.

- [ ] **Step 2: Update the view model**

Add these bindable properties to `MainViewModel` so the main page can show the active source and preview status without duplicating selection logic:

```csharp
public string ActiveCameraSource { get; private set; } = "Aguardando câmera...";
public string CameraPreviewStatus { get; private set; } = "Sem preview ainda.";
```

On initialization, call `ICameraSourceCoordinator.InitializeAsync(...)` and assign both values from the returned `CameraSourceState`.

- [ ] **Step 3: Update the main page layout**

Insert the preview panel near the top of the page and bind it to the view model:

```xml
<Grid RowDefinitions="Auto,240,Auto,Auto,Auto,*,Auto" Padding="20" RowSpacing="16">
    <Label Text="Vision 2 Audio" ... />

    <controls:CameraPreviewView
        Grid.Row="1"
        ActiveSource="{Binding ActiveCameraSource}"
        StatusText="{Binding CameraPreviewStatus}" />

    <VerticalStackLayout Grid.Row="2" ...>
        ...
    </VerticalStackLayout>
</Grid>
```

Keep the existing capture and history sections intact so the preview panel does not break the current flow.

- [ ] **Step 4: Run the app build and tests**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj` and `dotnet build src\Vision2Audio.App\Vision2Audio.App.csproj -f net10.0-android`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Vision2Audio.App/MainPage.xaml src/Vision2Audio.App/ViewModels/MainViewModel.cs src/Vision2Audio.App/MainPage.xaml.cs tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs
git commit -m "feat: show camera preview panel"
```

### Task 4: Validate on emulator and Android 11 hardware, then update context

**Files:**
- Modify: `.opencode/context/architecture.md`
- Modify: `.opencode/context/current-state.md`
- Modify: `.opencode/context/decisions.md`
- Modify: `.opencode/context/stack.md`
- Modify: `docs/superpowers/plans/2026-06-30-camera-preview-panel.md` if a short validation note is needed after execution

- [ ] **Step 1: Run the tests again**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj`
Expected: PASS.

- [ ] **Step 2: Validate on the emulator**

Open the app in the Visual Studio Android emulator and confirm:
1. the preview panel appears
2. the app shows the native camera fallback when OTG is absent
3. the app remains usable even if no physical camera is available
4. the capture button still works for the previewed source or shows a clear status if the emulator camera is unavailable

- [ ] **Step 3: Validate on an Android 11 device**

Use a physical Android 11 phone with OTG camera hardware attached and confirm:
1. OTG/USB preview becomes active first
2. native fallback only appears when OTG cannot be opened
3. the capture source matches the previewed source
4. GPS, OpenAI, speech, and history still work

- [ ] **Step 4: Record the validation result and update context**

Write the preview/capture result into the project context files and keep the source-selection rule explicit so future agents know the preview panel is now part of the active architecture.

- [ ] **Step 5: Commit**

```bash
git add .opencode/context/architecture.md .opencode/context/current-state.md .opencode/context/decisions.md .opencode/context/stack.md
git commit -m "docs: record camera preview integration state"
```

## Execution order

1. Task 1 — camera-source state and fallback coordinator
2. Task 2 — Android preview host and source services
3. Task 3 — main-screen preview panel integration
4. Task 4 — emulator/device validation and context updates

## Human review checklist

- Preview panel is inside the main screen, not a separate page.
- OTG/USB remains preferred, native camera remains fallback.
- Preview and capture use the same active source.
- Emulator remains usable even without OTG hardware.
- Android 11 device validation is still required before the feature is considered complete.
