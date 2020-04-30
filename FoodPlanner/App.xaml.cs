using System.Diagnostics;
using System.Globalization;
using System.Linq;
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
		private LanguageHelper _langHelper;

		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			_langHelper = new LanguageHelper(typeof(Languages.Resources));
			_langHelper.ChangeLanguage(CultureInfo.InstalledUICulture);
			bool isSupported = _langHelper.GetSupportedLanguages()
				.FirstOrDefault(a => a.Equals(CultureInfo.InstalledUICulture)) != null;
			Log.Write($"Selected language: {CultureInfo.InstalledUICulture}", 1, TraceEventType.Information, true);
			Log.Write($"Supported: {isSupported}", 2, null);
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
