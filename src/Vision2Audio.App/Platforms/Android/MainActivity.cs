using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Android.Util;
using Microsoft.Extensions.DependencyInjection;
using Vision2Audio.App.Services;

namespace Vision2Audio.App;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle? savedInstanceState)
	{
		Log.Debug("Vision2Audio", "[Startup] MainActivity.OnCreate enter");
		base.OnCreate(savedInstanceState);
		Log.Debug("Vision2Audio", "[Startup] MainActivity.OnCreate exit");
	}

	protected override void OnStart()
	{
		Log.Debug("Vision2Audio", "[Startup] MainActivity.OnStart");
		base.OnStart();
	}

	protected override void OnResume()
	{
		Log.Debug("Vision2Audio", "[Startup] MainActivity.OnResume");
		base.OnResume();
	}

	public override bool OnKeyDown(Keycode keyCode, KeyEvent? e)
	{
		if (keyCode is Keycode.Enter or Keycode.DpadCenter or Keycode.Space or Keycode.ButtonA or Keycode.MediaPlay)
		{
			var triggerHub = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService<TriggerHub>();
			triggerHub?.SignalTriggered();
			return true;
		}

		return base.OnKeyDown(keyCode, e);
	}
}
