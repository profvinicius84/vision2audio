#if ANDROID
using Android.App;
using Android.Content;
using Android.Hardware.Usb;
using Android.OS;
using Android.Util;
using Android.Views;
using Com.Jiangdg.Ausbc;
using Com.Jiangdg.Ausbc.Callback;
using Com.Jiangdg.Ausbc.Camera;
using Com.Jiangdg.Ausbc.Camera.Bean;
using Com.Jiangdg.Ausbc.Widget;
using Microsoft.Maui.ApplicationModel;
using System.Security.Cryptography;
using Vision2Audio.App.Services;

namespace Vision2Audio.App;

/// <summary>
/// USB Host API implementation for detecting USB/UVC cameras on Android.
/// </summary>
public sealed class UsbCameraService : IUsbCameraService
{
    private const string UsbPermissionAction = "com.companyname.vision2audio.app.USB_PERMISSION";
    private const string UsbPermissionRequestIdExtra = "com.companyname.vision2audio.app.USB_PERMISSION_REQUEST_ID";
    private const int DefaultPreviewWidth = 640;
    private const int DefaultPreviewHeight = 480;
    private static readonly TimeSpan CaptureTimeout = TimeSpan.FromSeconds(5);
    private static readonly CameraResolution PreferredPreviewResolution = new(DefaultPreviewWidth, DefaultPreviewHeight);
    private readonly Context _context;
    private readonly UsbManager _usbManager;
    private readonly SemaphoreSlim _sessionGate = new(1, 1);
    private MultiCameraClient? _client;
    private UsbDetachReceiver? _usbDetachReceiver;
    private CameraUVC? _camera;
    private CameraStateCallback? _cameraStateCallback;
    private TextureView? _previewView;
    private string? _activeDeviceName;
    private bool _openedWithPreview;

    public UsbCameraService()
    {
        _context = Android.App.Application.Context;
        _usbManager = (UsbManager?)_context.GetSystemService(Context.UsbService)
            ?? throw new InvalidOperationException("UsbManager indisponível.");
    }

    public async Task<bool> InitializeAsync(CancellationToken cancellationToken = default)
    {
        await _sessionGate.WaitAsync(cancellationToken);
        try
        {
            return await InitializeSessionCoreAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not System.OperationCanceledException)
        {
            AndroidDiagnosticLog.Exception("otg-initialize", ex);
            throw;
        }
        finally
        {
            _sessionGate.Release();
        }
    }

    public async Task StartPreviewAsync(TextureView previewView, CancellationToken cancellationToken = default)
    {
        await _sessionGate.WaitAsync(cancellationToken);
        try
        {
            _previewView = previewView;
            var initialized = await InitializeSessionCoreAsync(cancellationToken);
            if (!initialized)
            {
                throw new InvalidOperationException("Nenhuma câmera OTG/UVC disponível para preview AUSBC.");
            }

            EnsurePreviewAttached(previewView);
        }
        catch (Exception ex) when (ex is not System.OperationCanceledException)
        {
            AndroidDiagnosticLog.Exception("otg-start-preview", ex);
            throw;
        }
        finally
        {
            _sessionGate.Release();
        }
    }

    public async Task<byte[]> CaptureFrameAsync(CancellationToken cancellationToken = default)
    {
        CameraUVC camera;

        await _sessionGate.WaitAsync(cancellationToken);
        try
        {
            if (_camera is not { IsCameraOpened: true })
            {
                var initialized = await InitializeSessionCoreAsync(cancellationToken);
                if (!initialized)
                {
                    throw new InvalidOperationException("Sessão OTG/AUSBC indisponível para captura.");
                }
            }

            camera = _camera ?? throw new InvalidOperationException("Sessão OTG/AUSBC indisponível para captura.");
        }
        catch (Exception ex) when (ex is not System.OperationCanceledException)
        {
            AndroidDiagnosticLog.Exception("otg-capture-prepare", ex);
            throw;
        }
        finally
        {
            _sessionGate.Release();
        }

        return await CaptureAusbcImageAsync(camera, cancellationToken);
    }

    private async Task<byte[]> CaptureAusbcImageAsync(CameraUVC camera, CancellationToken cancellationToken)
    {
        var capturePath = CreateCapturePath();
        string? completedCapturePath = null;
        using var timeoutCts = new CancellationTokenSource(CaptureTimeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        using var callback = new CaptureImageCallback();

        try
        {
            camera.CaptureImage(callback, capturePath);
            var completedPath = await callback.WaitForCompletionAsync(linkedCts.Token);
            var pathToRead = string.IsNullOrWhiteSpace(completedPath) ? capturePath : completedPath;
            completedCapturePath = pathToRead;

            if (!File.Exists(pathToRead))
            {
                throw new InvalidOperationException("Captura OTG/AUSBC não gerou arquivo de imagem.");
            }

            return await File.ReadAllBytesAsync(pathToRead, cancellationToken);
        }
        catch (System.OperationCanceledException) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            var exception = new InvalidOperationException("Captura OTG/AUSBC excedeu o tempo limite.");
            AndroidDiagnosticLog.Exception("otg-capture-timeout", exception);
            throw exception;
        }
        catch (Exception ex) when (ex is not System.OperationCanceledException)
        {
            AndroidDiagnosticLog.Exception("otg-capture-ausbc-image", ex);
            throw;
        }
        finally
        {
            DeleteCaptureFileBestEffort(capturePath);
            if (!string.Equals(capturePath, completedCapturePath, StringComparison.Ordinal))
            {
                DeleteCaptureFileBestEffort(completedCapturePath);
            }
        }
    }

    private string CreateCapturePath()
    {
        var captureDirectory = Path.Combine(_context.CacheDir?.AbsolutePath ?? FileSystem.CacheDirectory, "otg-captures");
        Directory.CreateDirectory(captureDirectory);
        return Path.Combine(captureDirectory, $"otg-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.jpg");
    }

    private static void DeleteCaptureFileBestEffort(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception ex)
        {
            AndroidDiagnosticLog.Exception("otg-capture-cache-delete", ex);
        }
    }

    public async Task CloseSessionAsync(CancellationToken cancellationToken = default)
    {
        await _sessionGate.WaitAsync(cancellationToken);
        try
        {
            CloseSessionCore();
        }
        catch (Exception ex) when (ex is not System.OperationCanceledException)
        {
            AndroidDiagnosticLog.Exception("otg-close-session", ex);
            throw;
        }
        finally
        {
            _sessionGate.Release();
        }
    }

    public Task<string> GetDiagnosticsAsync(CancellationToken cancellationToken = default)
    {
        if (_usbManager.DeviceList is null || _usbManager.DeviceList.Count == 0)
        {
            return Task.FromResult("USB Host: nenhum dispositivo USB visível para o Android.");
        }

        var devices = _usbManager.DeviceList.Values.ToList();
        var candidateCount = devices.Count(IsVideoClassCandidate);
        var permittedCandidateCount = devices.Count(device => IsVideoClassCandidate(device) && _usbManager.HasPermission(device));
        var hasActiveSession = _activeDeviceName is not null ? "sim" : "não";

        return Task.FromResult(
            $"USB Host: dispositivos visíveis={devices.Count}; candidatos UVC={candidateCount}; candidatos com permissão={permittedCandidateCount}; sessão ativa={hasActiveSession}");
    }

    private async Task<bool> InitializeSessionCoreAsync(CancellationToken cancellationToken)
    {
        var device = FindUsbCameraCandidate();
        if (device is null)
        {
            CloseSessionCore();
            return false;
        }

        EnsureClientRegistered();

        if (!_usbManager.HasPermission(device))
        {
            var granted = await RequestPermissionAsync(device, cancellationToken);
            if (!granted || !_usbManager.HasPermission(device))
            {
                CloseSessionCore();
                return false;
            }
        }

        if (!_usbManager.HasPermission(device))
        {
            CloseSessionCore();
            return false;
        }

        if (_camera is { IsCameraOpened: true } && _activeDeviceName == device.DeviceName)
        {
            if (_previewView is not null && !_openedWithPreview)
            {
                OpenAusbcSession(device);
            }

            return true;
        }

        OpenAusbcSession(device);
        return true;
    }

    private void EnsureClientRegistered()
    {
        if (_client is not null)
        {
            return;
        }

        _usbDetachReceiver = new UsbDetachReceiver(this);
        RegisterUsbReceiver(_usbDetachReceiver, new IntentFilter(UsbManager.ActionUsbDeviceDetached));
        _client = new MultiCameraClient(_context, null);
        _client.Register();
    }

    private void OpenAusbcSession(UsbDevice device)
    {
        CloseCameraOnly();

        _cameraStateCallback = new CameraStateCallback(this);
        _camera = new CameraUVC(_context, device);
        _camera.SetCameraStateCallBack(_cameraStateCallback);
        var previewResolution = SelectPreviewResolution(_camera);
        ConfigurePreviewView(previewResolution);
        _camera.OpenCamera(_previewView, CreateSessionRequest(previewResolution));
        _activeDeviceName = device.DeviceName;
        _openedWithPreview = _previewView is not null;
    }

    private void EnsurePreviewAttached(TextureView previewView)
    {
        if (_camera is null || !_openedWithPreview)
        {
            throw new InvalidOperationException("Sessão OTG/AUSBC ainda não está criada.");
        }
    }

    private void ConfigurePreviewView(CameraResolution previewResolution)
    {
        if (_previewView is IAspectRatio aspectRatioView)
        {
            aspectRatioView.SetAspectRatio(previewResolution.Width, previewResolution.Height);
        }

        _previewView?.SurfaceTexture?.SetDefaultBufferSize(previewResolution.Width, previewResolution.Height);
    }

    private static CameraResolution SelectPreviewResolution(CameraUVC camera)
    {
        using var preferredSize = new PreviewSize(PreferredPreviewResolution.Width, PreferredPreviewResolution.Height);
        try
        {
            if (camera.IsPreviewSizeSupported(preferredSize))
            {
                return PreferredPreviewResolution;
            }

            var suitableSize = camera.GetSuitableSize(PreferredPreviewResolution.Width, PreferredPreviewResolution.Height);
            if (suitableSize.Width > 0 && suitableSize.Height > 0)
            {
                return new CameraResolution(suitableSize.Width, suitableSize.Height);
            }
        }
        catch
        {
            // Some AUSBC size helpers depend on device state; keep the known-safe UVC default.
        }

        return PreferredPreviewResolution;
    }

    private static CameraRequest CreateSessionRequest(CameraResolution previewResolution)
    {
        var builder = new CameraRequest.Builder()
            .SetPreviewWidth(previewResolution.Width)
            .SetPreviewHeight(previewResolution.Height)
            .SetRawPreviewData(false)
            .SetCaptureRawImage(false)
            .SetAspectRatioShow(true);

        if (CameraRequest.AudioSource.None is { } audioSource)
        {
            builder.SetAudioSource(audioSource);
        }

        if (CameraRequest.RenderMode.Normal is { } renderMode)
        {
            builder.SetRenderMode(renderMode);
        }

        return builder.Create();
    }

    private readonly record struct CameraResolution(int Width, int Height);

    private void CloseSessionCore()
    {
        CloseCameraOnly();

        if (_client is not null)
        {
            try
            {
                _client.UnRegister();
                _client.Destroy();
            }
            catch
            {
                // AUSBC cleanup must be best-effort across Android detach/lifecycle races.
            }
            finally
            {
                _client.Dispose();
                _client = null;
                if (_usbDetachReceiver is not null)
                {
                    try
                    {
                        _context.UnregisterReceiver(_usbDetachReceiver);
                    }
                    catch
                    {
                        // Receiver may already be unregistered by Android lifecycle edge cases.
                    }

                    _usbDetachReceiver.Dispose();
                    _usbDetachReceiver = null;
                }
            }
        }
    }

    private void CloseCameraOnly()
    {
        if (_camera is not null)
        {
            try
            {
                _camera.SetCameraStateCallBack(null);
                if (_camera.IsCameraOpened)
                {
                    _camera.CloseCamera();
                }
            }
            catch
            {
                // AUSBC close can race with USB detach; release managed references regardless.
            }
            finally
            {
                _camera.Dispose();
                _camera = null;
                _activeDeviceName = null;
                _previewView = null;
                _openedWithPreview = false;
                _cameraStateCallback?.Dispose();
                _cameraStateCallback = null;
            }
        }
    }

    private UsbDevice? FindUsbCameraCandidate()
        => _usbManager.DeviceList?.Values.FirstOrDefault(IsVideoClassCandidate);

    private static bool IsVideoClassCandidate(UsbDevice device)
    {
        if (device.DeviceClass == UsbClass.Video)
        {
            return true;
        }

        for (var index = 0; index < device.InterfaceCount; index++)
        {
            var usbInterface = device.GetInterface(index);
            if (usbInterface?.InterfaceClass == UsbClass.Video)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> RequestPermissionAsync(UsbDevice device, CancellationToken cancellationToken)
    {
        var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var requestId = CreatePermissionRequestId();
        var receiver = new UsbPermissionReceiver(device.DeviceName, requestId, tcs);
        var filter = new IntentFilter(UsbPermissionAction);
        RegisterUsbReceiver(receiver, filter);

        var flags = PendingIntentFlags.UpdateCurrent | PendingIntentFlags.OneShot;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
        {
#pragma warning disable CA1416
            flags |= PendingIntentFlags.Mutable;
#pragma warning restore CA1416
        }

        var intent = new Intent(UsbPermissionAction)
            .SetPackage(_context.PackageName)
            .PutExtra(UsbPermissionRequestIdExtra, requestId);
        var permissionIntent = PendingIntent.GetBroadcast(_context, CreatePermissionRequestCode(), intent, flags);
        _usbManager.RequestPermission(device, permissionIntent);
        _client?.RequestPermission(device);

        using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
        return await AwaitPermissionAsync(tcs.Task, receiver);
    }

    private static string CreatePermissionRequestId()
        => Convert.ToHexString(RandomNumberGenerator.GetBytes(16));

    private static int CreatePermissionRequestCode()
        => BitConverter.ToInt32(RandomNumberGenerator.GetBytes(sizeof(int)));

    private async Task<bool> AwaitPermissionAsync(Task<bool> task, BroadcastReceiver receiver)
    {
        try
        {
            return await task;
        }
        finally
        {
            try
            {
                _context.UnregisterReceiver(receiver);
            }
            catch
            {
                // Receiver may already be unregistered by Android lifecycle edge cases.
            }
        }
    }

    private void RegisterUsbReceiver(BroadcastReceiver receiver, IntentFilter filter)
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
#pragma warning disable CA1416
            _context.RegisterReceiver(receiver, filter, ReceiverFlags.NotExported);
#pragma warning restore CA1416
            return;
        }

        _context.RegisterReceiver(receiver, filter);
    }

    private void ScheduleDetachedDeviceCleanup(string? detachedDeviceName)
    {
        if (string.IsNullOrWhiteSpace(detachedDeviceName))
        {
            return;
        }

        _ = CloseDetachedDeviceSessionAsync(detachedDeviceName);
    }

    private async Task CloseDetachedDeviceSessionAsync(string detachedDeviceName)
    {
        await _sessionGate.WaitAsync();
        try
        {
            if (_camera is not null && (_activeDeviceName is null || _activeDeviceName == detachedDeviceName))
            {
                CloseCameraOnly();
            }
        }
        catch
        {
            // AUSBC cleanup is best-effort after USB detach.
        }
        finally
        {
            _sessionGate.Release();
        }
    }

    private sealed class UsbPermissionReceiver(string expectedDeviceName, string expectedRequestId, TaskCompletionSource<bool> taskCompletionSource) : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent?.Action != UsbPermissionAction)
            {
                return;
            }

            if (intent.GetStringExtra(UsbPermissionRequestIdExtra) != expectedRequestId)
            {
                return;
            }

#pragma warning disable CA1422
            var device = (UsbDevice?)intent.GetParcelableExtra(UsbManager.ExtraDevice);
#pragma warning restore CA1422
            if (device?.DeviceName != expectedDeviceName)
            {
                return;
            }

            var granted = intent.GetBooleanExtra(UsbManager.ExtraPermissionGranted, false);
            taskCompletionSource.TrySetResult(granted);
        }
    }

    private sealed class UsbDetachReceiver(UsbCameraService owner) : BroadcastReceiver
    {
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent?.Action != UsbManager.ActionUsbDeviceDetached)
            {
                return;
            }

#pragma warning disable CA1422
            var device = (UsbDevice?)intent.GetParcelableExtra(UsbManager.ExtraDevice);
#pragma warning restore CA1422
            if (device is not null)
            {
                owner.ScheduleDetachedDeviceCleanup(device.DeviceName);
            }
        }
    }

    private sealed class CameraStateCallback(UsbCameraService owner) : Java.Lang.Object, ICameraStateCallBack
    {
        public void OnCameraState(MultiCameraClient.ICamera self, ICameraStateCallBack.State code, string? msg)
        {
            if (code == ICameraStateCallBack.State.Error)
            {
                Log.Debug("Vision2Audio", $"[Diagnostics] Operation=otg-camera-state; State=Error; Message={Vision2Audio.Core.Diagnostics.SanitizedExceptionDiagnostics.SanitizeForStatus(msg)}");
            }

            if (code == ICameraStateCallBack.State.Closed || code == ICameraStateCallBack.State.Error)
            {
                owner._activeDeviceName = null;
            }
        }
    }

    private sealed class CaptureImageCallback : Java.Lang.Object, ICaptureCallBack
    {
        private readonly TaskCompletionSource<string> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public void OnBegin()
        {
        }

        public void OnComplete(string? path)
            => _completion.TrySetResult(path ?? string.Empty);

        public void OnError(string? error)
            => _completion.TrySetException(new InvalidOperationException($"Falha AUSBC ao capturar imagem OTG: {Vision2Audio.Core.Diagnostics.SanitizedExceptionDiagnostics.SanitizeForStatus(error)}"));

        public async Task<string> WaitForCompletionAsync(CancellationToken cancellationToken)
        {
            await using var registration = cancellationToken.Register(() => _completion.TrySetCanceled(cancellationToken));
            return await _completion.Task;
        }
    }
}
#endif
