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
		public static LanguageHelper LangHelper;

		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			LangHelper = new LanguageHelper(typeof(Languages.Resources), CultureInfo.GetCultureInfo("en-us"));
			LangHelper.ChangeLanguage(CultureInfo.InstalledUICulture);
			bool isSupported = LangHelper.GetSupportedLanguages()
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
