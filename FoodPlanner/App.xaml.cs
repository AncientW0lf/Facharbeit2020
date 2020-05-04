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
		/// <summary>
		/// The helper class to access methods to manipulate and get resource languages.
		/// </summary>
		public static LanguageHelper LangHelper;

		/// <summary>
		/// Creates a new <see cref="LangHelper"/> and logs the selected language.
		/// </summary>
		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			LangHelper = new LanguageHelper(typeof(Languages.Resources), CultureInfo.GetCultureInfo("en-us"));
			LangHelper.ChangeLanguage(CultureInfo.InstalledUICulture);
			bool isSupported = LangHelper.GetSupportedLanguages()
				.FirstOrDefault(a => a.Equals(CultureInfo.InstalledUICulture)) != null;
			Log.Write($"Selected language: {CultureInfo.InstalledUICulture}", 1, TraceEventType.Information, true);
			Log.Write($"Supported: {isSupported}", 2, null);
		}

		/// <summary>
		/// Closes the log file.
		/// </summary>
		private void App_OnExit(object sender, ExitEventArgs e)
		{
			Log.Close();
		}

		/// <summary>
		/// Handler for unhandled exceptions. Should not replace normal try..catch commands.
		/// </summary>
		private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Log.Write("Unhandled exception!", 1, TraceEventType.Critical, true);
			Log.Write($"Exception: {e.Exception.Message}\n{e.Exception.StackTrace}", 2, null);
			App_OnExit(sender, null);
		}
	}
}
