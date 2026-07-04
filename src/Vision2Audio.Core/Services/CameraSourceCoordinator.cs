using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;

namespace Vision2Audio.Core.Services;

/// <summary>
/// Chooses the preferred source and falls back deterministically.
/// </summary>
public sealed class CameraSourceCoordinator(ICameraSelectionStore selectionStore, IEnumerable<ICameraPreviewSource> previewSources) : ICameraSourceCoordinator
{
    private CameraSourceState _current = new(CameraSelectionKind.Front, CameraSourceKind.Unavailable, "Câmera indisponível", "Nenhuma fonte de câmera disponível no momento.", false);
    private ICameraPreviewSource? _currentSource;

    private ICameraPreviewSource? ResolvePreviewSource(CameraSelectionKind selection) => previewSources.FirstOrDefault(source => source.SelectionKind == selection);

    /// <inheritdoc />
    public CameraSourceState Current => _current;

    /// <inheritdoc />
    public async Task<CameraSourceState> InitializeAsync(CancellationToken cancellationToken)
    {
        var loadedSelection = await selectionStore.LoadAsync(cancellationToken);
        var preferredSelection = loadedSelection.IsSuccess && loadedSelection.Value is { } saved
            ? saved.SelectedKind
            : CameraSelectionKind.Front;

        return await TryStartWithFallbackAsync(preferredSelection, cancellationToken);
    }

    /// <inheritdoc />
    public Task<CameraSourceState> EnsureActiveSourceAsync(CancellationToken cancellationToken)
        => _current.ActiveKind == CameraSourceKind.Unavailable ? InitializeAsync(cancellationToken) : Task.FromResult(_current);

    /// <inheritdoc />
    public async Task<CameraSourceState> SetPreferredSelectionAsync(CameraSelectionKind selection, CancellationToken cancellationToken)
    {
        await selectionStore.SaveAsync(new CameraSelection(selection), cancellationToken);
        return await TryStartWithFallbackAsync(selection, cancellationToken);
    }

    private async Task<CameraSourceState> TryStartWithFallbackAsync(CameraSelectionKind preferredSelection, CancellationToken cancellationToken)
    {
        var errors = new List<string>();

        if (_currentSource is not null)
        {
            await _currentSource.StopPreviewAsync(cancellationToken);
            _currentSource = null;
        }

        foreach (var selection in GetAttemptOrder(preferredSelection))
        {
            var source = ResolvePreviewSource(selection);
            if (source is null)
            {
                errors.Add($"{selection}: fonte de preview não registrada.");
                continue;
            }

            var result = await source.TryStartPreviewAsync(cancellationToken);
            if (result.IsSuccess && result.Value is { } state)
            {
                var isFallback = selection != preferredSelection;
                var status = isFallback && errors.Count > 0
                    ? $"Fallback ativo: a fonte selecionada não está disponível. {state.StatusMessage}. Motivo: {string.Join(" | ", errors)}"
                    : state.StatusMessage;

                _current = state with { SelectedKind = preferredSelection, StatusMessage = status, IsFallback = isFallback, ActiveSelectionKind = selection };
                _currentSource = source;
                return _current;
            }

            errors.Add($"{selection}: {result.Error ?? "falha desconhecida"}");
        }

        var errorMessage = errors.Count == 0
            ? "Nenhuma fonte de câmera disponível no momento."
            : $"Nenhuma fonte de câmera disponível no momento. {string.Join(" | ", errors)}";
        _current = new CameraSourceState(preferredSelection, CameraSourceKind.Unavailable, "Câmera indisponível", errorMessage, false, null);
        return _current;
    }

    private static IEnumerable<CameraSelectionKind> GetAttemptOrder(CameraSelectionKind preferredSelection)
        => preferredSelection switch
        {
            CameraSelectionKind.Front => [CameraSelectionKind.Front, CameraSelectionKind.Rear, CameraSelectionKind.Otg],
            CameraSelectionKind.Rear => [CameraSelectionKind.Rear, CameraSelectionKind.Front, CameraSelectionKind.Otg],
            CameraSelectionKind.Otg => [CameraSelectionKind.Otg, CameraSelectionKind.Front, CameraSelectionKind.Rear],
            _ => [CameraSelectionKind.Front, CameraSelectionKind.Rear, CameraSelectionKind.Otg]
        };
}
