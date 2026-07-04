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
	private const long TriggerDebounceMilliseconds = 500;
	private static long _lastTriggerEventTimeMilliseconds;

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
		if (keyCode is Keycode.Enter or Keycode.DpadCenter or Keycode.Space or Keycode.ButtonA or Keycode.MediaPlay or Keycode.VolumeUp)
		{
			var triggerHub = Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services.GetService<TriggerHub>();
			if (triggerHub is null)
			{
				return base.OnKeyDown(keyCode, e);
			}

			if (e?.RepeatCount > 0)
			{
				return true;
			}

			var eventTime = e?.EventTime ?? Java.Lang.JavaSystem.CurrentTimeMillis();
			if (eventTime - _lastTriggerEventTimeMilliseconds < TriggerDebounceMilliseconds)
			{
				return true;
			}

			_lastTriggerEventTimeMilliseconds = eventTime;
			triggerHub.SignalTriggered();
			return true;
		}

		return base.OnKeyDown(keyCode, e);
	}
}
