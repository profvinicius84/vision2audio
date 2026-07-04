using System.Diagnostics;

namespace Vision2Audio.App;

public partial class App : Application
{
	private readonly AppShell _shell;
	private static IServiceProvider? _services;

	public App(AppShell shell)
	{
		_shell = shell;
		InitializeComponent();
		Debug.WriteLine("[Startup] App constructed");
		AppDomain.CurrentDomain.UnhandledException += (_, e) => Debug.WriteLine($"[Startup] UnhandledException: {e.ExceptionObject?.GetType().Name ?? "Unknown"}");
		TaskScheduler.UnobservedTaskException += (_, e) => Debug.WriteLine($"[Startup] UnobservedTaskException: {e.Exception.GetType().Name}");
	}

	/// <summary>Service provider available after app startup.</summary>
	public static IServiceProvider Services => _services
		?? throw new InvalidOperationException("The Maui service provider is not available yet.");

	/// <summary>Sets the app service provider once the MAUI app is built.</summary>
	public static void SetServices(IServiceProvider services) => _services = services;

	protected override Window CreateWindow(IActivationState? activationState)
	{
		Debug.WriteLine("[Startup] CreateWindow");
		return new(_shell);
	}
}
