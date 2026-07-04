using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Vision2Audio.App.Services;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Vision2Audio.Core.Services;

namespace Vision2Audio.App.ViewModels;

/// <summary>
/// Main page state and behavior.
/// </summary>
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SceneDescriptionCoordinator _coordinator;
    private readonly IHistoryRepository _historyRepository;
    private readonly ITextToSpeechService _textToSpeechService;
    private readonly ICameraSourceCoordinator _cameraSourceCoordinator;
    private readonly TriggerHub _triggerHub;
    private readonly AsyncRelayCommand _captureCommand;
    private readonly AsyncRelayCommand _clearHistoryCommand;
    private bool _isSynchronizingCameraSelection;
    private bool _isBusy;
    private string _statusMessage = "Pronto para capturar.";
    private string _latestDescription = "Nenhuma descrição ainda.";

    /// <summary>Requests the UI to show an alert.</summary>
    public event EventHandler<string>? AlertRequested;

    /// <summary>Creates the view model.</summary>
    public MainViewModel(
        SceneDescriptionCoordinator coordinator,
        IHistoryRepository historyRepository,
        ITextToSpeechService textToSpeechService,
        ICameraSourceCoordinator cameraSourceCoordinator,
        TriggerHub triggerHub)
    {
        _coordinator = coordinator;
        _historyRepository = historyRepository;
        _textToSpeechService = textToSpeechService;
        _cameraSourceCoordinator = cameraSourceCoordinator;
        _triggerHub = triggerHub;
        _captureCommand = new AsyncRelayCommand(CaptureAsync, () => !IsBusy);
        _clearHistoryCommand = new AsyncRelayCommand(ClearHistoryAsync, () => !IsBusy);
        _triggerHub.Triggered += HandleTriggered;
        CaptureCommand = _captureCommand;
        ClearHistoryCommand = _clearHistoryCommand;
        CameraChoices =
        [
            new CameraSelectionChoice(CameraSelectionKind.Front, "Câmera frontal"),
            new CameraSelectionChoice(CameraSelectionKind.Rear, "Câmera traseira"),
            new CameraSelectionChoice(CameraSelectionKind.Otg, "Câmera OTG/USB")
        ];
    }

    /// <summary>History entries shown in read-only mode.</summary>
    public ObservableCollection<HistoryEntry> History { get; } = [];

    /// <summary>Fires the capture workflow.</summary>
    public ICommand CaptureCommand { get; }

    /// <summary>Clears all local history.</summary>
    public ICommand ClearHistoryCommand { get; }

    /// <summary>Camera options shown in the selector.</summary>
    public IReadOnlyList<CameraSelectionChoice> CameraChoices { get; }

    /// <summary>Shows whether the app is busy.</summary>
    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value)
            {
                return;
            }

            _isBusy = value;
            OnPropertyChanged();
            _captureCommand.RaiseCanExecuteChanged();
            _clearHistoryCommand.RaiseCanExecuteChanged();
        }
    }

    /// <summary>Current status message.</summary>
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Latest response text.</summary>
    public string LatestDescription
    {
        get => _latestDescription;
        private set
        {
            if (_latestDescription == value)
            {
                return;
            }

            _latestDescription = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Current camera source shown in the preview panel.</summary>
    public string ActiveCameraSource { get; private set; } = "Aguardando câmera...";

    /// <summary>Status line for the preview panel.</summary>
    public string CameraPreviewStatus { get; private set; } = "Sem preview ainda.";

    /// <summary>Indicates whether fallback is active.</summary>
    public bool IsFallbackActive { get; private set; }

    /// <summary>Current camera kind used by the live preview.</summary>
    public CameraSelectionKind SelectedCameraKind { get; private set; } = CameraSelectionKind.Front;

    /// <summary>Token that forces the preview handler to restart.</summary>
    public long PreviewRefreshToken { get; private set; }

    /// <summary>Current selected camera choice.</summary>
    public CameraSelectionChoice? SelectedCameraChoice
    {
        get => CameraChoices.FirstOrDefault(choice => choice.Kind == _selectedCameraKind);
        set
        {
            if (value is null || value.Kind == _selectedCameraKind)
            {
                return;
            }

            _selectedCameraKind = value.Kind;
            OnPropertyChanged();

            if (_isSynchronizingCameraSelection)
            {
                return;
            }

            _ = ApplyCameraSelectionAsync(value.Kind);
        }
    }

    private CameraSelectionKind _selectedCameraKind = CameraSelectionKind.Front;

    /// <summary>Loads persisted history.</summary>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        _isSynchronizingCameraSelection = true;

        try
        {
            var cameraState = await _cameraSourceCoordinator.InitializeAsync(cancellationToken);
            _selectedCameraKind = cameraState.SelectedKind;
            SelectedCameraKind = GetPreviewSelection(cameraState);
            PreviewRefreshToken = DateTimeOffset.UtcNow.UtcTicks;
            ActiveCameraSource = cameraState.DisplayName;
            CameraPreviewStatus = cameraState.StatusMessage;
            IsFallbackActive = cameraState.IsFallback;
            OnPropertyChanged(nameof(SelectedCameraChoice));
            OnPropertyChanged(nameof(SelectedCameraKind));
            OnPropertyChanged(nameof(PreviewRefreshToken));
            OnPropertyChanged(nameof(ActiveCameraSource));
            OnPropertyChanged(nameof(CameraPreviewStatus));
            OnPropertyChanged(nameof(IsFallbackActive));

            ValidateCameraState(cameraState);
        }
        finally
        {
            _isSynchronizingCameraSelection = false;
        }

        await RefreshHistoryAsync(cancellationToken);
    }

    private async Task ApplyCameraSelectionAsync(CameraSelectionKind selection)
    {
        try
        {
            var cameraState = await _cameraSourceCoordinator.SetPreferredSelectionAsync(selection, CancellationToken.None);
            SelectedCameraKind = GetPreviewSelection(cameraState);
            PreviewRefreshToken = DateTimeOffset.UtcNow.UtcTicks;
            ActiveCameraSource = cameraState.DisplayName;
            CameraPreviewStatus = cameraState.StatusMessage;
            IsFallbackActive = cameraState.IsFallback;
            OnPropertyChanged(nameof(SelectedCameraChoice));
            OnPropertyChanged(nameof(SelectedCameraKind));
            OnPropertyChanged(nameof(PreviewRefreshToken));
            OnPropertyChanged(nameof(ActiveCameraSource));
            OnPropertyChanged(nameof(CameraPreviewStatus));
            OnPropertyChanged(nameof(IsFallbackActive));
            ValidateCameraState(cameraState);
        }
        catch (Exception ex)
        {
            StatusMessage = "Não foi possível trocar a câmera. Tente outra fonte.";
            RequestAlert($"Erro ao trocar câmera: {ex.Message}");
        }
    }

    private async Task CaptureAsync()
    {
        IsBusy = true;
        StatusMessage = "Capturando cena...";

        try
        {
            var result = await _coordinator.CaptureAndDescribeAsync(CancellationToken.None);
            if (!result.IsSuccess || result.Value is null)
            {
                StatusMessage = result.Error ?? "Falha ao descrever a cena.";
                if (!string.IsNullOrWhiteSpace(result.Error))
                {
                    RequestAlert(result.Error);
                }
                return;
            }

            LatestDescription = result.Value.Text;
            StatusMessage = "Descrição pronta.";
            await _textToSpeechService.SpeakAsync(result.Value, CancellationToken.None);
            await RefreshHistoryAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            RequestAlert(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearHistoryAsync()
    {
        IsBusy = true;
        StatusMessage = "Limpando histórico...";

        try
        {
            await _historyRepository.ClearAllAsync(CancellationToken.None);
            History.Clear();
            StatusMessage = "Histórico limpo.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RefreshHistoryAsync(CancellationToken cancellationToken)
    {
        var entries = await _historyRepository.GetRecentAsync(cancellationToken);
        History.Clear();
        foreach (var entry in entries)
        {
            History.Add(entry);
        }
    }

    private void HandleTriggered(object? sender, EventArgs e)
    {
        if (!IsBusy)
        {
            _ = CaptureAsync();
        }
    }

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private void RequestAlert(string message)
        => AlertRequested?.Invoke(this, message);

    private void ValidateCameraState(CameraSourceState cameraState)
    {
        if (cameraState.ActiveKind == CameraSourceKind.Unavailable)
        {
            StatusMessage = "Nenhuma fonte de câmera disponível. Verifique a câmera selecionada, permissões ou conexão OTG.";
            RequestAlert($"Câmera indisponível: {cameraState.StatusMessage}");
            return;
        }

        if (cameraState.IsFallback)
        {
            StatusMessage = "Câmera selecionada indisponível. Fallback ativo com outra câmera disponível.";
            RequestAlert($"Câmera selecionada indisponível. Fallback ativo: {cameraState.DisplayName}. {cameraState.StatusMessage}");
        }
    }

    private static CameraSelectionKind GetPreviewSelection(CameraSourceState cameraState)
        => cameraState.ActiveKind == CameraSourceKind.Unavailable
            ? cameraState.SelectedKind
            : cameraState.ActiveSelectionKind ?? cameraState.SelectedKind;
}

/// <summary>Selectable camera option for the UI.</summary>
public sealed record CameraSelectionChoice(CameraSelectionKind Kind, string DisplayName);
