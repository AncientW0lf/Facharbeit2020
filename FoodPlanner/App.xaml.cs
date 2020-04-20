using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Threading;
using BoxLib.Scripts;

namespace FoodPlanner
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			LanguageHelper.Resources = typeof(Languages.Resources);
			LanguageHelper.ChangeLanguage(CultureInfo.InstalledUICulture);
			Log.Write($"Selected language: {CultureInfo.InstalledUICulture}", 1, TraceEventType.Information);
		}

		private void App_OnExit(object sender, ExitEventArgs e)
		{
			Log.Close();
		}

		private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Write("Unhandled exception!", 1, TraceEventType.Critical, true);
			Log.Write($"Exception: {e.Exception.Message}\n{e.Exception.StackTrace}", 2, null);
			App_OnExit(sender, null);
		}
	}
}
