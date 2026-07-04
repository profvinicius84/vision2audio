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
    private readonly SemaphoreSlim _captureGate = new(1, 1);
    private bool _isSynchronizingCameraSelection;
    private bool _isBusy;
    private bool _isRequestingDescription;
    private bool _isSpeaking;
    private string _statusMessage = "Pronto para capturar.";
    private string _latestDescription = "Nenhuma descrição ainda.";
    private ImageSource? _capturedImageSource;
    private bool _isCapturedImageVisible;
    private bool _isPreviewPaused;
    private DateTimeOffset? _displayedCaptureTime;
    private CancellationTokenSource? _speechCancellationTokenSource;

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
        _coordinator.DescriptionRequestStateChanged += HandleDescriptionRequestStateChanged;
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

    /// <summary>Shows whether the OpenAI description request is active.</summary>
    public bool IsRequestingDescription
    {
        get => _isRequestingDescription;
        private set
        {
            if (_isRequestingDescription == value)
            {
                return;
            }

            _isRequestingDescription = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Shows whether text-to-speech playback is active.</summary>
    public bool IsSpeaking
    {
        get => _isSpeaking;
        private set
        {
            if (_isSpeaking == value)
            {
                return;
            }

            _isSpeaking = value;
            OnPropertyChanged();
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

    /// <summary>Still image shown after a successful capture.</summary>
    public ImageSource? CapturedImageSource
    {
        get => _capturedImageSource;
        private set
        {
            if (_capturedImageSource == value)
            {
                return;
            }

            _capturedImageSource = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Shows the captured still over the preview.</summary>
    public bool IsCapturedImageVisible
    {
        get => _isCapturedImageVisible;
        private set
        {
            if (_isCapturedImageVisible == value)
            {
                return;
            }

            _isCapturedImageVisible = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Pauses live preview while the captured still is displayed.</summary>
    public bool IsPreviewPaused
    {
        get => _isPreviewPaused;
        private set
        {
            if (_isPreviewPaused == value)
            {
                return;
            }

            _isPreviewPaused = value;
            OnPropertyChanged();
        }
    }

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
            ClearCapturedImage();
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
            ClearCapturedImage();
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
        if (IsSpeaking)
        {
            await StopSpeechAndResumePreviewAsync("Pronto para nova captura.");
            return;
        }

        if (!await _captureGate.WaitAsync(0))
        {
            return;
        }

        IsBusy = true;
        StatusMessage = "Capturando cena...";

        try
        {
            await PreparePreviewForCaptureAsync();
            var result = await _coordinator.CaptureAndDescribeAsync(CancellationToken.None);
            ShowLatestCapturedImageIfAvailable();

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
            _ = SpeakAndResumePreviewAsync(result.Value);
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
            _captureGate.Release();
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
        if (IsSpeaking)
        {
            _ = StopSpeechAndResumePreviewAsync("Áudio interrompido. Pronto para nova captura.");
            return;
        }

        _captureCommand.Execute(null);
    }

    private void HandleDescriptionRequestStateChanged(object? sender, bool isActive)
    {
        IsRequestingDescription = isActive;
        if (isActive)
        {
            StatusMessage = "Analisando com IA...";
        }
    }

    private async Task SpeakAndResumePreviewAsync(SceneDescription description)
    {
        await StopSpeechOnlyAsync();
        using var speechCancellationTokenSource = new CancellationTokenSource();
        _speechCancellationTokenSource = speechCancellationTokenSource;
        IsSpeaking = true;

        try
        {
            await _textToSpeechService.SpeakAsync(description, speechCancellationTokenSource.Token);
            StatusMessage = "Pronto para capturar.";
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_speechCancellationTokenSource, speechCancellationTokenSource))
            {
                _speechCancellationTokenSource = null;
                IsSpeaking = false;
                ClearCapturedImage();
                PreviewRefreshToken = DateTimeOffset.UtcNow.UtcTicks;
                OnPropertyChanged(nameof(PreviewRefreshToken));
            }
        }
    }

    private async Task StopSpeechAndResumePreviewAsync(string statusMessage)
    {
        await StopSpeechOnlyAsync();
        ClearCapturedImage();
        PreviewRefreshToken = DateTimeOffset.UtcNow.UtcTicks;
        OnPropertyChanged(nameof(PreviewRefreshToken));
        StatusMessage = statusMessage;
    }

    private async Task StopSpeechOnlyAsync()
    {
        var cancellationTokenSource = _speechCancellationTokenSource;
        if (cancellationTokenSource is not null)
        {
            cancellationTokenSource.Cancel();
        }

        await _textToSpeechService.StopAsync();
        _speechCancellationTokenSource = null;
        IsSpeaking = false;
    }

    private async Task PreparePreviewForCaptureAsync()
    {
        if (!IsCapturedImageVisible && !IsPreviewPaused)
        {
            return;
        }

        ClearCapturedImage();
        PreviewRefreshToken = DateTimeOffset.UtcNow.UtcTicks;
        OnPropertyChanged(nameof(PreviewRefreshToken));
        await Task.Delay(350);
    }

    private void ShowLatestCapturedImageIfAvailable()
    {
        var capture = _coordinator.LastCapture;
        if (capture is null || capture.CapturedAtUtc == _displayedCaptureTime)
        {
            return;
        }

        var imageBytes = capture.Data.ToArray();
        CapturedImageSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
        IsCapturedImageVisible = true;
        IsPreviewPaused = true;
        _displayedCaptureTime = capture.CapturedAtUtc;
    }

    private void ClearCapturedImage()
    {
        CapturedImageSource = null;
        IsCapturedImageVisible = false;
        IsPreviewPaused = false;
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
