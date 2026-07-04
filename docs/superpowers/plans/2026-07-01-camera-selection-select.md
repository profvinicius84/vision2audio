# Camera Selection Select Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a persisted camera selection control so the user can choose front, rear, or OTG, restore the last choice on startup, and fall back automatically when the chosen source is unavailable.

**Architecture:** Extend the current camera-source coordinator so it reads a saved `CameraSelectionKind` and resolves it to the matching preview/capture source. The main screen exposes a single picker plus preview/status panel; selection changes are saved locally, reloaded on startup, and kept synchronized with preview and capture.

**Tech Stack:** .NET MAUI, Android 11+, existing Core/App split, local JSON persistence, xUnit.

---

## Spec traceability

- Approved spec: `docs/superpowers/specs/2026-07-01-camera-selection-select-design.md`
- Scope references: `In scope 42-47`, `Acceptance 59-77`, `Business rules 81-84`, `Constraints 88-91`, `Risks 105-107`

## File structure

- `src/Vision2Audio.Core/Models/CameraSelectionKind.cs` — persisted user choice (`Front`, `Rear`, `Otg`).
- `src/Vision2Audio.Core/Models/CameraSelection.cs` — local preference record.
- `src/Vision2Audio.Core/Abstractions/ICameraSelectionStore.cs` — load/save preference contract.
- `src/Vision2Audio.Core/Services/FileCameraSelectionStore.cs` — JSON persistence in app data.
- `src/Vision2Audio.Core/Abstractions/ICameraPreviewSource.cs` — preview source contract keyed by selection kind.
- `src/Vision2Audio.Core/Abstractions/ICameraSourceCoordinator.cs` — resolves preferred source and exposes current state.
- `src/Vision2Audio.Core/Models/CameraSourceState.cs` — selected source, active source, and fallback state.
- `src/Vision2Audio.Core/Services/CameraSourceCoordinator.cs` — preference resolution and fallback order.
- `src/Vision2Audio.Core/Services/CaptureOrchestrator.cs` — captures from the current active selection.
- `tests/Vision2Audio.Core.Tests/CameraSelectionStoreTests.cs` — persistence behavior.
- `tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs` — preference and fallback behavior.
- `src/Vision2Audio.App/Services/NativeCameraPreviewSource.cs` — native front/rear preview implementation (parameterized by facing).
- `src/Vision2Audio.App/Services/NativeCameraCaptureService.cs` — native front/rear capture implementation (parameterized by facing).
- `src/Vision2Audio.App/Services/UsbCameraPreviewSource.cs` — OTG preview implementation.
- `src/Vision2Audio.App/Services/UsbCameraCaptureService.cs` — OTG capture implementation.
- `src/Vision2Audio.App/MauiProgram.cs` — register front/rear/otg services plus store/coordinator.
- `src/Vision2Audio.App/MainPage.xaml` — picker control plus preview panel.
- `src/Vision2Audio.App/ViewModels/MainViewModel.cs` — expose selection options, selected value, and change handling.
- `.opencode/context/architecture.md`, `.opencode/context/current-state.md`, `.opencode/context/decisions.md`, `.opencode/context/stack.md` — record the new selection/persistence behavior.

## Tasks

### Task 1: Add persisted camera selection storage and core models

**Files:**
- Create: `src/Vision2Audio.Core/Models/CameraSelectionKind.cs`
- Create: `src/Vision2Audio.Core/Models/CameraSelection.cs`
- Create: `src/Vision2Audio.Core/Abstractions/ICameraSelectionStore.cs`
- Create: `src/Vision2Audio.Core/Services/FileCameraSelectionStore.cs`
- Test: `tests/Vision2Audio.Core.Tests/CameraSelectionStoreTests.cs`

- [ ] **Step 1: Write the failing test**

Create `tests/Vision2Audio.Core.Tests/CameraSelectionStoreTests.cs` with this exact case:

```csharp
using Vision2Audio.Core.Models;
using Vision2Audio.Core.Services;

namespace Vision2Audio.Core.Tests;

public sealed class CameraSelectionStoreTests
{
    [Fact]
    public async Task SaveAndLoad_ReturnsLastSelection()
    {
        var path = Path.Combine(Path.GetTempPath(), $"camera-selection-{Guid.NewGuid():N}.json");
        try
        {
            var store = new FileCameraSelectionStore(path);

            await store.SaveAsync(new CameraSelection(CameraSelectionKind.Rear), CancellationToken.None);

            var loaded = await store.LoadAsync(CancellationToken.None);

            Assert.True(loaded.IsSuccess);
            Assert.Equal(CameraSelectionKind.Rear, loaded.Value!.SelectedKind);
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj --filter CameraSelectionStoreTests`
Expected: FAIL because the new store/model types do not exist yet.

- [ ] **Step 3: Write the minimal implementation**

Implement these types exactly:

```csharp
namespace Vision2Audio.Core.Models;

public enum CameraSelectionKind
{
    Front,
    Rear,
    Otg
}

public sealed record CameraSelection(CameraSelectionKind SelectedKind);
```

```csharp
namespace Vision2Audio.Core.Abstractions;

public interface ICameraSelectionStore
{
    Task<Result<CameraSelection>> LoadAsync(CancellationToken cancellationToken);
    Task SaveAsync(CameraSelection selection, CancellationToken cancellationToken);
}
```

`FileCameraSelectionStore` should persist the selection as a small JSON file in app data and return a default `Front` selection when the file is missing.

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj --filter CameraSelectionStoreTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Vision2Audio.Core/Models/CameraSelectionKind.cs src/Vision2Audio.Core/Models/CameraSelection.cs src/Vision2Audio.Core/Abstractions/ICameraSelectionStore.cs src/Vision2Audio.Core/Services/FileCameraSelectionStore.cs tests/Vision2Audio.Core.Tests/CameraSelectionStoreTests.cs
git commit -m "feat: add camera selection storage"
```

### Task 2: Extend camera source coordination to honor the saved preference

**Files:**
- Create: `src/Vision2Audio.Core/Models/CameraSourceState.cs`
- Create: `src/Vision2Audio.Core/Abstractions/ICameraPreviewSource.cs`
- Create: `src/Vision2Audio.Core/Abstractions/ICameraSourceCoordinator.cs`
- Modify: `src/Vision2Audio.Core/Services/CameraSourceCoordinator.cs`
- Modify: `src/Vision2Audio.Core/Services/CaptureOrchestrator.cs`
- Modify: `tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs`

- [ ] **Step 1: Write the failing test**

Extend `tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs` with a preference-driven case:

```csharp
[Fact]
public async Task InitializeAsync_UsesPreferredRearWhenAvailable()
{
    var coordinator = new CameraSourceCoordinator(
        new FakeSelectionStore(CameraSelectionKind.Rear),
        [
            new FakePreviewSource(CameraSelectionKind.Otg, shouldStart: false, "OTG preview"),
            new FakePreviewSource(CameraSelectionKind.Rear, shouldStart: true, "Rear preview"),
            new FakePreviewSource(CameraSelectionKind.Front, shouldStart: true, "Front preview")
        ]);

    var state = await coordinator.InitializeAsync(CancellationToken.None);

    Assert.Equal(CameraSelectionKind.Rear, state.SelectedKind);
    Assert.Equal("Rear preview", state.DisplayName);
    Assert.False(state.IsFallback);
}

private sealed class FakeSelectionStore(CameraSelectionKind selectedKind) : ICameraSelectionStore
{
    public Task<Result<CameraSelection>> LoadAsync(CancellationToken cancellationToken) =>
        Task.FromResult(Result<CameraSelection>.Success(new CameraSelection(selectedKind)));

    public Task SaveAsync(CameraSelection selection, CancellationToken cancellationToken) => Task.CompletedTask;
}
```

This test should fail until the coordinator reads and applies the saved selection.

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj --filter CameraSourceCoordinatorTests`
Expected: FAIL until the preference-aware coordinator exists.

- [ ] **Step 3: Write the minimal implementation**

Implement these core types exactly:

```csharp
namespace Vision2Audio.Core.Models;

public sealed record CameraSourceState(
    CameraSelectionKind SelectedKind,
    CameraSelectionKind ActiveKind,
    string DisplayName,
    string StatusMessage,
    bool IsFallback);
```

```csharp
namespace Vision2Audio.Core.Abstractions;

public interface ICameraPreviewSource
{
    CameraSelectionKind SelectionKind { get; }
    string DisplayName { get; }
    Task<Result<CameraSourceState>> TryStartPreviewAsync(CancellationToken cancellationToken);
    Task StopPreviewAsync(CancellationToken cancellationToken);
}

public interface ICameraSourceCoordinator
{
    CameraSourceState Current { get; }
    Task<CameraSourceState> InitializeAsync(CancellationToken cancellationToken);
    Task<CameraSourceState> EnsureActiveSourceAsync(CancellationToken cancellationToken);
    Task<CameraSourceState> SetPreferredSelectionAsync(CameraSelectionKind selection, CancellationToken cancellationToken);
}
```

Update `CameraSourceCoordinator` so it:
1. loads the saved `CameraSelection`
2. attempts the matching source first
3. falls back in a deterministic order when the preferred source is unavailable
4. exposes the active source state for the UI and capture flow

Update `CaptureOrchestrator` so capture always uses the current active source decided by the coordinator.

- [ ] **Step 4: Run the test to verify it passes**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj --filter CameraSourceCoordinatorTests`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Vision2Audio.Core/Models/CameraSourceState.cs src/Vision2Audio.Core/Abstractions/ICameraPreviewSource.cs src/Vision2Audio.Core/Abstractions/ICameraSourceCoordinator.cs src/Vision2Audio.Core/Services/CameraSourceCoordinator.cs src/Vision2Audio.Core/Services/CaptureOrchestrator.cs tests/Vision2Audio.Core.Tests/CameraSourceCoordinatorTests.cs
git commit -m "feat: honor saved camera preference"
```

### Task 3: Add the camera selection control to the main screen

**Files:**
- Modify: `src/Vision2Audio.App/MainPage.xaml`
- Modify: `src/Vision2Audio.App/ViewModels/MainViewModel.cs`
- Modify: `src/Vision2Audio.App/MauiProgram.cs`
- Modify: `src/Vision2Audio.App/Services/NativeCameraPreviewSource.cs`
- Modify: `src/Vision2Audio.App/Services/NativeCameraCaptureService.cs`
- Modify: `src/Vision2Audio.App/Services/UsbCameraPreviewSource.cs`
- Modify: `src/Vision2Audio.App/Services/UsbCameraCaptureService.cs`

- [ ] **Step 1: Write the failing build target**

Run: `dotnet build src\Vision2Audio.App\Vision2Audio.App.csproj -f net10.0-android`
Expected: FAIL until the new selection properties and UI bindings exist.

- [ ] **Step 2: Implement the view-model selection contract**

Add these properties to `MainViewModel`:

```csharp
public IReadOnlyList<CameraSelectionKind> CameraOptions { get; } = [CameraSelectionKind.Front, CameraSelectionKind.Rear, CameraSelectionKind.Otg];
public CameraSelectionKind SelectedCameraKind { get; private set; }
```

Add a change handler that persists the new selection and reinitializes the camera coordinator:

```csharp
public async Task ChangeCameraSelectionAsync(CameraSelectionKind selection, CancellationToken cancellationToken)
{
    await _cameraSelectionStore.SaveAsync(new CameraSelection(selection), cancellationToken);
    SelectedCameraKind = selection;
    OnPropertyChanged(nameof(SelectedCameraKind));
    await InitializeAsync(cancellationToken);
}
```

Initialize `SelectedCameraKind` from the saved preference at startup.

- [ ] **Step 3: Update the XAML**

Add a picker to the main page near the preview panel:

```xml
<Picker Title="Câmera"
        ItemsSource="{Binding CameraOptions}"
        SelectedItem="{Binding SelectedCameraKind}" />
```

Keep the preview panel showing the current active source and fallback state.

- [ ] **Step 4: Run the build and verify it passes**

Run: `dotnet build src\Vision2Audio.App\Vision2Audio.App.csproj -f net10.0-android`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/Vision2Audio.App/MainPage.xaml src/Vision2Audio.App/ViewModels/MainViewModel.cs src/Vision2Audio.App/MauiProgram.cs src/Vision2Audio.App/Services/NativeCameraPreviewSource.cs src/Vision2Audio.App/Services/NativeCameraCaptureService.cs src/Vision2Audio.App/Services/UsbCameraPreviewSource.cs src/Vision2Audio.App/Services/UsbCameraCaptureService.cs
git commit -m "feat: add camera selection picker"
```

### Task 4: Validate persistence, fallback, and emulator behavior

**Files:**
- Modify: `.opencode/context/architecture.md`
- Modify: `.opencode/context/current-state.md`
- Modify: `.opencode/context/decisions.md`
- Modify: `.opencode/context/stack.md`

- [ ] **Step 1: Run the tests again**

Run: `dotnet test tests\Vision2Audio.Core.Tests\Vision2Audio.Core.Tests.csproj`
Expected: PASS.

- [ ] **Step 2: Validate on the emulator**

Open the app in the Visual Studio Android emulator and confirm:
1. the camera picker appears
2. selecting OTG falls back gracefully when OTG is unavailable
3. selecting front/rear remains usable in the emulator
4. the selected choice is remembered after restart

- [ ] **Step 3: Validate on Android 11 hardware**

Use a physical Android 11 phone and confirm:
1. OTG can still be selected on hardware that supports it
2. front/rear selections work on the device cameras
3. the selection is persisted and restored
4. preview and capture remain synchronized

- [ ] **Step 4: Update context**

Record the selected-camera behavior and fallback rule in the project context files so future agents know the app now supports user camera selection with persistence.

- [ ] **Step 5: Commit**

```bash
git add .opencode/context/architecture.md .opencode/context/current-state.md .opencode/context/decisions.md .opencode/context/stack.md
git commit -m "docs: record camera selection support"
```

## Execution order

1. Task 1 — camera selection storage
2. Task 2 — coordinator preference and fallback
3. Task 3 — picker UI and binding
4. Task 4 — emulator/device validation and context updates

## Human review checklist

- The user can choose front, rear, or OTG from the main screen.
- The last choice is restored on app start.
- Unavailable sources fall back automatically.
- Preview and capture stay synchronized with the selected source.
- Emulator remains usable even when OTG is unavailable.
