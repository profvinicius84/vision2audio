using System.Diagnostics;

namespace Vision2Audio.App;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Debug.WriteLine("[Startup] AppShell constructed");
	}
}
