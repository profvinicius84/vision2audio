using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Services;

/// <summary>
/// Stores the camera preference as local JSON.
/// </summary>
public sealed class FileCameraSelectionStore(string filePath) : ICameraSelectionStore
{
    /// <inheritdoc />
    public async Task<Result<CameraSelection>> LoadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return Result<CameraSelection>.Success(new CameraSelection(CameraSelectionKind.Front));
        }

        await using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var selection = await JsonSerializer.DeserializeAsync<CameraSelection>(stream, cancellationToken: cancellationToken);
        return selection is null
            ? Result<CameraSelection>.Success(new CameraSelection(CameraSelectionKind.Front))
            : Result<CameraSelection>.Success(selection);
    }

    /// <inheritdoc />
    public async Task SaveAsync(CameraSelection selection, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath) ?? Environment.CurrentDirectory);
        await using var stream = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await JsonSerializer.SerializeAsync(stream, selection, cancellationToken: cancellationToken);
    }
}
