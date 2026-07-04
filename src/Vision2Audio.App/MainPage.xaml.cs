using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.ApplicationModel;
using Vision2Audio.App.Services;
using Vision2Audio.App.ViewModels;

namespace Vision2Audio.App;

public partial class MainPage : ContentPage
{
	private readonly MainViewModel _viewModel;
	private readonly ICameraErrorNotifier _cameraErrorNotifier;

	public MainPage() : this(
		App.Services.GetRequiredService<MainViewModel>(),
		App.Services.GetRequiredService<ICameraErrorNotifier>())
	{
	}

    public MainPage(MainViewModel viewModel, ICameraErrorNotifier cameraErrorNotifier)
    {
        _viewModel = viewModel;
		_cameraErrorNotifier = cameraErrorNotifier;
        Debug.WriteLine("[Startup] MainPage constructed");
        InitializeComponent();
        BindingContext = _viewModel;
        _viewModel.AlertRequested += HandleAlertRequested;
		_cameraErrorNotifier.ErrorReported += HandleAlertRequested;
    }

	protected override async void OnAppearing()
	{
		Debug.WriteLine("[Startup] MainPage.OnAppearing enter");
		base.OnAppearing();
		try
		{
			Debug.WriteLine("[Startup] Requesting camera permission");
			await Permissions.RequestAsync<Permissions.Camera>();
			Debug.WriteLine("[Startup] Camera permission requested");
			await _viewModel.InitializeAsync(CancellationToken.None);
			Debug.WriteLine("[Startup] MainPage.OnAppearing initialize complete");
		}
		catch (Exception ex)
		{
			Debug.WriteLine($"[Startup] MainPage.OnAppearing exception: {ex.GetType().Name}");
			throw;
		}
	}

	protected override void OnDisappearing()
	{
		_viewModel.AlertRequested -= HandleAlertRequested;
		_cameraErrorNotifier.ErrorReported -= HandleAlertRequested;
		base.OnDisappearing();
	}

	private void HandleAlertRequested(object? sender, string message)
	{
		MainThread.BeginInvokeOnMainThread(async () => await DisplayAlertAsync("Aviso", message, "OK"));
	}
}
