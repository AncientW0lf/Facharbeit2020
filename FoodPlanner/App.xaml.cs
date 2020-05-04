using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

			CultureInfo savedLang = null;
			try
			{
				savedLang = LoadLanguageFromFile();
			}
			catch(Exception)
			{
				//Ignore
			}

			LangHelper.ChangeLanguage(savedLang ?? CultureInfo.InstalledUICulture);
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
			SaveLanguageToFile(Languages.Resources.Culture);
			Log.Close();
		}

		private void SaveLanguageToFile(CultureInfo language)
		{
			Directory.CreateDirectory("config");

			using var stream = new FileStream(@"config\language.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
			using var writer = new StreamWriter(stream);

			writer.WriteLine(language.ToString());
		}

		private CultureInfo LoadLanguageFromFile()
		{
			using var stream = new FileStream(@"config\language.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new StreamReader(stream);

			string languageStr = reader.ReadLine();

			return languageStr != null ? new CultureInfo(languageStr) : null;
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
