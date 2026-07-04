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
