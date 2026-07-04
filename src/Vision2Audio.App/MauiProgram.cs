using Microsoft.Extensions.Logging;
using Vision2Audio.App.Controls;
using Vision2Audio.App.Services;
using Vision2Audio.App.ViewModels;
using Vision2Audio.Core.Abstractions;
using Vision2Audio.Core.Models;
using Vision2Audio.Core.Options;
using Vision2Audio.Core.Services;

namespace Vision2Audio.App;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureMauiHandlers(handlers =>
			{
				handlers.AddHandler<CameraPreviewView, CameraPreviewViewHandler>();
			})
			.RegisterAppServices()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if DEBUG
		builder.Logging.AddDebug();
#endif

		var app = builder.Build();
		App.SetServices(app.Services);
		return app;
	}

	private static MauiAppBuilder RegisterAppServices(this MauiAppBuilder builder)
	{
		var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? "gpt-4.1-mini";
		var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? "https://api.openai.com/v1/chat/completions";
		var historyPath = Path.Combine(FileSystem.AppDataDirectory, "vision2audio-history.json");
		var cameraSelectionPath = Path.Combine(FileSystem.AppDataDirectory, "camera-selection.json");

		builder.Services.AddSingleton(new OpenAiOptions
		{
			Model = model,
			Endpoint = endpoint
		});
		builder.Services.AddSingleton<IOpenAiSecretsProvider, AppPackageOpenAiSecretsProvider>();
		builder.Services.AddSingleton<TriggerHub>();
		builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
		builder.Services.AddSingleton<ICameraErrorNotifier, CameraErrorNotifier>();
		builder.Services.AddSingleton<IUsbCameraService, UsbCameraService>();
		builder.Services.AddSingleton<ICameraPreviewSource, UsbCameraPreviewSource>();
		builder.Services.AddSingleton<ICameraPreviewSource>(_ => new NativeCameraPreviewSource(CameraSelectionKind.Front, "Câmera frontal"));
		builder.Services.AddSingleton<ICameraPreviewSource>(_ => new NativeCameraPreviewSource(CameraSelectionKind.Rear, "Câmera traseira"));
		builder.Services.AddSingleton<ICameraPreviewFrameProvider, CameraPreviewFrameProvider>();
		builder.Services.AddSingleton<ICameraSelectionStore>(_ => new FileCameraSelectionStore(cameraSelectionPath));
		builder.Services.AddSingleton<ICameraSourceCoordinator, CameraSourceCoordinator>();
		builder.Services.AddSingleton<IUsbCameraCaptureService, UsbCameraCaptureService>();
		builder.Services.AddSingleton<INativeCameraCaptureService, NativeCameraCaptureService>();
		builder.Services.AddSingleton<ICaptureService, CaptureOrchestrator>();
		builder.Services.AddSingleton<ILocationService, LocationService>();
		builder.Services.AddSingleton<IHistoryRepository>(_ => new FileHistoryRepository(historyPath));
		builder.Services.AddSingleton<ITextToSpeechService, TextToSpeechService>();
		builder.Services.AddSingleton<HttpClient>();
		builder.Services.AddSingleton<IOpenAiSceneDescriptionService>(sp =>
			new OpenAiSceneDescriptionService(
				sp.GetRequiredService<HttpClient>(),
				sp.GetRequiredService<OpenAiOptions>(),
				sp.GetRequiredService<IOpenAiSecretsProvider>()));
		builder.Services.AddSingleton<SceneDescriptionCoordinator>();
		builder.Services.AddSingleton<MainViewModel>();
		builder.Services.AddSingleton<MainPage>();
		builder.Services.AddSingleton<AppShell>();

		return builder;
	}
}
