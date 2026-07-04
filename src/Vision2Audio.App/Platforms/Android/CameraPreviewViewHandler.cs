using Android.Content;
using Android.Graphics;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Jiangdg.Ausbc.Widget;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Handlers;
using Vision2Audio.App.Controls;
using Vision2Audio.App.Services;
using Vision2Audio.Core.Models;

namespace Vision2Audio.App;

/// <summary>
/// Android live camera preview handler.
/// </summary>
public sealed class CameraPreviewViewHandler : ViewHandler<CameraPreviewView, FrameLayout>
{
    private const string PreviewUnavailableMessage = "Visualização indisponível. Tente novamente.";

    private readonly CameraPreviewPlatformView _platformView;
    private ICameraPreviewFrameProvider? _frameProvider;
    private ICameraErrorNotifier? _errorNotifier;
    private IUsbCameraService? _usbCameraService;

    public CameraPreviewViewHandler() : base(ViewMapper)
    {
        Log.Debug("Vision2Audio", "[Preview] Handler constructed");
        _platformView = new CameraPreviewPlatformView();
    }

    public static new IPropertyMapper<CameraPreviewView, CameraPreviewViewHandler> ViewMapper = new PropertyMapper<CameraPreviewView, CameraPreviewViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(CameraPreviewView.SelectionKind)] = MapSelectionKind,
        [nameof(CameraPreviewView.RefreshToken)] = MapRefreshToken,
        [nameof(CameraPreviewView.IsPaused)] = MapIsPaused
    };

    protected override FrameLayout CreatePlatformView() => _platformView;

    protected override void ConnectHandler(FrameLayout platformView)
    {
        Log.Debug("Vision2Audio", "[Preview] ConnectHandler");
        base.ConnectHandler(platformView);
        _frameProvider = MauiContext?.Services.GetRequiredService<ICameraPreviewFrameProvider>();
        _errorNotifier = MauiContext?.Services.GetRequiredService<ICameraErrorNotifier>();
        _usbCameraService = MauiContext?.Services.GetRequiredService<IUsbCameraService>();
        _platformView.SetFrameProvider(_frameProvider);
        _platformView.SetErrorNotifier(_errorNotifier);
        _platformView.SetUsbCameraService(_usbCameraService);
        _platformView.SetSelection(VirtualView?.SelectionKind ?? CameraSelectionKind.Front);
    }

    protected override void DisconnectHandler(FrameLayout platformView)
    {
        Log.Debug("Vision2Audio", "[Preview] DisconnectHandler");
        _platformView.DisposePreview();
        _frameProvider = null;
        _errorNotifier = null;
        _usbCameraService = null;
        _platformView.SetUsbCameraService(null);
        base.DisconnectHandler(platformView);
    }

    private static void MapSelectionKind(CameraPreviewViewHandler handler, CameraPreviewView view)
        => handler._platformView.SetSelection(view.SelectionKind);

    private static void MapRefreshToken(CameraPreviewViewHandler handler, CameraPreviewView view)
        => handler._platformView.RestartPreview();

    private static void MapIsPaused(CameraPreviewViewHandler handler, CameraPreviewView view)
        => handler._platformView.SetPaused(view.IsPaused);

    private sealed class CameraPreviewPlatformView : FrameLayout, TextureView.ISurfaceTextureListener
    {
        private readonly AspectRatioTextureView _textureView;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private ICameraPreviewFrameProvider? _frameProvider;
        private ICameraErrorNotifier? _errorNotifier;
        private IUsbCameraService? _usbCameraService;
        private CameraSelectionKind _selectionKind = CameraSelectionKind.Front;
        private CameraDevice? _cameraDevice;
        private CameraCaptureSession? _session;
        private Surface? _previewSurface;
        private bool _isReady;
        private bool _isPaused;

        public CameraPreviewPlatformView() : base(Android.App.Application.Context)
        {
            _textureView = new AspectRatioTextureView(Android.App.Application.Context!)
            {
                LayoutParameters = new LayoutParams(LayoutParams.MatchParent, LayoutParams.MatchParent)
            };
            _textureView.SurfaceTextureListener = this;
            AddView(_textureView);
        }

        public void SetSelection(CameraSelectionKind selection)
        {
            Log.Debug("Vision2Audio", $"[Preview] SetSelection {selection}");
            _selectionKind = selection;
            if (_isReady && !_isPaused)
            {
                ObserveLifecycleTask(RestartPreviewAsync(), "preview-selection-restart");
            }
        }

        public void DisposePreview()
        {
            Log.Debug("Vision2Audio", "[Preview] DisposePreview");
            _frameProvider?.Unregister(_textureView);
            ObserveLifecycleTask(StopPreviewAsync(_usbCameraService), "preview-dispose-cleanup");
        }

        public void SetFrameProvider(ICameraPreviewFrameProvider? frameProvider)
        {
            Log.Debug("Vision2Audio", "[Preview] SetFrameProvider");
            _frameProvider = frameProvider;
            if (_isReady)
            {
                _frameProvider?.Register(_textureView, _selectionKind);
            }
        }

        public void SetErrorNotifier(ICameraErrorNotifier? errorNotifier) => _errorNotifier = errorNotifier;

        public void SetUsbCameraService(IUsbCameraService? usbCameraService) => _usbCameraService = usbCameraService;

        public void SetPaused(bool isPaused)
        {
            if (_isPaused == isPaused)
            {
                return;
            }

            _isPaused = isPaused;
            ObserveLifecycleTask(
                isPaused ? StopPreviewAsync(_usbCameraService) : RestartPreviewAsync(),
                isPaused ? "preview-pause-stop" : "preview-pause-resume");
        }

        protected override void OnDetachedFromWindow()
        {
            _isReady = false;
            ObserveLifecycleTask(StopPreviewAsync(_usbCameraService), "preview-detach-cleanup");
            base.OnDetachedFromWindow();
        }

        public void OnSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
        {
            Log.Debug("Vision2Audio", "[Preview] Surface available");
            _isReady = true;
            _frameProvider?.Register(_textureView, _selectionKind);
            if (!_isPaused)
            {
                ObserveLifecycleTask(RestartPreviewAsync(), "preview-surface-available-restart");
            }
        }

        public void RestartPreview()
        {
            if (_isPaused)
            {
                return;
            }

            ObserveLifecycleTask(RestartPreviewAsync(), "preview-refresh-restart");
        }

        public bool OnSurfaceTextureDestroyed(SurfaceTexture surface)
        {
            Log.Debug("Vision2Audio", "[Preview] Surface destroyed");
            _isReady = false;
            _frameProvider?.Unregister(_textureView);
            ObserveLifecycleTask(StopPreviewAsync(_usbCameraService), "preview-surface-destroyed-cleanup");
            return true;
        }

        public void OnSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
        {
        }

        public void OnSurfaceTextureUpdated(SurfaceTexture surface)
        {
        }

        private async Task RestartPreviewAsync()
        {
            Log.Debug("Vision2Audio", "[Preview] RestartPreviewAsync enter");
            if (_isPaused)
            {
                return;
            }

            await _gate.WaitAsync();
            try
            {
                if (_isPaused)
                {
                    return;
                }

                await StopPreviewLockedAsync();

                var surfaceTexture = _textureView.SurfaceTexture;
                if (surfaceTexture is null)
                {
                    return;
                }

                var androidContext = Android.App.Application.Context!;

                if (_selectionKind == CameraSelectionKind.Otg)
                {
                    if (_usbCameraService is null)
                    {
                        throw new InvalidOperationException("Serviço OTG/AUSBC indisponível.");
                    }

                    await _usbCameraService.StartPreviewAsync(_textureView, CancellationToken.None);
                    Log.Debug("Vision2Audio", "[Preview] AUSBC OTG preview started");
                    return;
                }

                var cameraManager = (CameraManager?)androidContext.GetSystemService(Context.CameraService)
                    ?? throw new InvalidOperationException("CameraManager indisponível.");

                var cameraId = FindCameraId(cameraManager, _selectionKind)
                    ?? FindFallbackCameraId(cameraManager)
                    ?? throw new InvalidOperationException("Nenhuma câmera disponível para preview.");

                var mainLooper = Looper.MainLooper ?? throw new InvalidOperationException("Main looper indisponível.");
                _cameraDevice = await OpenCameraAsync(cameraManager, cameraId, mainLooper, CancellationToken.None);

                _previewSurface = new Surface(surfaceTexture);
                _session = await CreatePreviewSessionAsync(_cameraDevice, _previewSurface, mainLooper, CancellationToken.None);

                var requestBuilder = _cameraDevice.CreateCaptureRequest(CameraTemplate.Preview);
                requestBuilder.AddTarget(_previewSurface);
                requestBuilder.Set(CaptureRequest.ControlAfMode!, (int)ControlAFMode.ContinuousPicture);
                requestBuilder.Set(CaptureRequest.ControlAeMode!, (int)ControlAEMode.OnAutoFlash);
                _session.SetRepeatingRequest(requestBuilder.Build(), null, new Handler(mainLooper));
                Log.Debug("Vision2Audio", "[Preview] Preview started");
            }
            catch (Exception ex)
            {
                AndroidDiagnosticLog.Exception($"preview-restart-{_selectionKind}", ex);
                _errorNotifier?.Report(PreviewUnavailableMessage);
                await StopPreviewBestEffortLockedAsync($"preview-restart-{_selectionKind}-cleanup");
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task StopPreviewAsync(IUsbCameraService? usbCameraServiceToClose = null)
        {
            try
            {
                await _gate.WaitAsync();
                try
                {
                    await StopPreviewLockedAsync(usbCameraServiceToClose);
                }
                finally
                {
                    _gate.Release();
                }
            }
            catch (Exception ex) when (ex is not System.OperationCanceledException)
            {
                AndroidDiagnosticLog.Exception("preview-stop-cleanup", ex);
            }
        }

        private async Task StopPreviewBestEffortLockedAsync(string operation)
        {
            try
            {
                await StopPreviewLockedAsync();
            }
            catch (Exception ex) when (ex is not System.OperationCanceledException)
            {
                AndroidDiagnosticLog.Exception(operation, ex);
            }
        }

        private async Task StopPreviewLockedAsync(IUsbCameraService? usbCameraServiceToClose = null)
        {
            var usbCameraService = usbCameraServiceToClose ?? _usbCameraService;
            if (usbCameraService is not null)
            {
                await usbCameraService.CloseSessionAsync(CancellationToken.None);
            }

            _session?.Close();
            _session?.Dispose();
            _session = null;

            _cameraDevice?.Close();
            _cameraDevice?.Dispose();
            _cameraDevice = null;

            _previewSurface?.Release();
            _previewSurface?.Dispose();
            _previewSurface = null;
        }

        private static void ObserveLifecycleTask(Task task, string operation)
            => task.ContinueWith(
                completedTask => AndroidDiagnosticLog.Exception(operation, completedTask.Exception!.GetBaseException()),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted,
                TaskScheduler.Default);

        private static string? FindCameraId(CameraManager cameraManager, CameraSelectionKind selection)
        {
            foreach (var id in cameraManager.GetCameraIdList())
            {
                var characteristics = cameraManager.GetCameraCharacteristics(id);
                var facing = characteristics.Get(CameraCharacteristics.LensFacing) as Java.Lang.Integer;
                var facingValue = facing?.IntValue();

                if (selection == CameraSelectionKind.Otg)
                {
                    if (OperatingSystem.IsAndroidVersionAtLeast(23) && facingValue == (int)LensFacing.External)
                    {
                        return id;
                    }

                    continue;
                }

                if (selection == CameraSelectionKind.Front && facingValue == (int)LensFacing.Front)
                {
                    return id;
                }

                if (selection == CameraSelectionKind.Rear && facingValue == (int)LensFacing.Back)
                {
                    return id;
                }
            }

            return null;
        }

        private static string? FindFallbackCameraId(CameraManager cameraManager)
        {
            return FindCameraId(cameraManager, CameraSelectionKind.Front)
                ?? FindCameraId(cameraManager, CameraSelectionKind.Rear)
                ?? FindCameraId(cameraManager, CameraSelectionKind.Otg);
        }

        private static Task<CameraDevice> OpenCameraAsync(CameraManager cameraManager, string cameraId, Looper looper, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<CameraDevice>(TaskCreationOptions.RunContinuationsAsynchronously);
            var callback = new CameraDeviceStateCallback(
                onOpened: device => tcs.TrySetResult(device),
                onDisconnected: device =>
                {
                    device.Close();
                    tcs.TrySetException(new IOException("Câmera desconectada durante preview."));
                },
                onError: (_, error) => tcs.TrySetException(new IOException($"Erro ao abrir preview: {error}")));

            cameraManager.OpenCamera(cameraId, callback, new Handler(looper));
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            return tcs.Task;
        }

        private static Task<CameraCaptureSession> CreatePreviewSessionAsync(CameraDevice device, Surface surface, Looper looper, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<CameraCaptureSession>(TaskCreationOptions.RunContinuationsAsynchronously);
            var surfaces = new List<Surface> { surface };
            var callback = new CaptureSessionStateCallback(
                onConfigured: session => tcs.TrySetResult(session),
                onConfigureFailed: _ => tcs.TrySetException(new IOException("Falha ao configurar preview.")));

#pragma warning disable CA1422
            device.CreateCaptureSession(surfaces, callback, new Handler(looper));
#pragma warning restore CA1422
            cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            return tcs.Task;
        }

        private sealed class CameraDeviceStateCallback(Action<CameraDevice> onOpened, Action<CameraDevice> onDisconnected, Action<CameraDevice, CameraError> onError) : CameraDevice.StateCallback
        {
            public override void OnOpened(CameraDevice camera) => onOpened(camera);
            public override void OnDisconnected(CameraDevice camera) => onDisconnected(camera);
            public override void OnError(CameraDevice camera, CameraError error) => onError(camera, error);
        }

        private sealed class CaptureSessionStateCallback(Action<CameraCaptureSession> onConfigured, Action<CameraCaptureSession> onConfigureFailed) : CameraCaptureSession.StateCallback
        {
            public override void OnConfigured(CameraCaptureSession session) => onConfigured(session);
            public override void OnConfigureFailed(CameraCaptureSession session) => onConfigureFailed(session);
        }
    }
}
