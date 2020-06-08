using AccessCommunication;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BoxLib.Objects;
using BoxLib.Static;
using FoodPlanner.Pages;

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
		/// The current main window. May be null if it isn't initialized yet.
		/// </summary>
		public static MainWindow CurrWindow;

		/// <summary>
		/// The recipe page. May be null if it isn't initialized yet.
		/// </summary>
		public static RecipesPage CurrRecipePage;

		/// <summary>
		/// The underlying MS Access communicator. Used for all queries in this application.
		/// </summary>
		private static AccessComm _accessDB;

		/// <summary>
		/// The password used for the database.
		/// </summary>
		public const string DBPassword = "Ywc^r72*qX45ndtK";

		/// <summary>
		/// Initializes all necessary objects.
		/// </summary>
		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			//Creates a new language helper to change languages easily
			LangHelper = new LanguageHelper(typeof(Languages.Resources), CultureInfo.GetCultureInfo("en-us"));

			//Tries to load the saved language from a config file
			CultureInfo savedLang = null;
			try
			{
				savedLang = LoadLanguageFromFile();
			}
			catch(Exception)
			{
				//Ignore
			}

			savedLang ??= CultureInfo.InstalledUICulture;

			//Applies the default/loaded language
			LangHelper.ChangeLanguage(savedLang);

			//Logs the applied language
			bool isSupported = LangHelper.GetSupportedLanguages()
				.FirstOrDefault(a => a.Equals(savedLang)) != null;
			Log.Write($"Selected language: {savedLang}", 1, TraceEventType.Information, true);
			Log.Write($"Supported: {isSupported}", 2, null);

			//Tries to open the database
			try
			{
				var secure = new SecureString();
				for(int i = 0; i < DBPassword.Length; i++)
				{
					secure.AppendChar(DBPassword[i]);
				}

				_accessDB = new AccessComm("database.accdb", secure);
			}
			catch(FileNotFoundException exc)
			{
				Log.Write("Could not find database!", 1, TraceEventType.Error, true);
				Log.Write($"Exception: {exc}", 2, null);
				MessageBox.Show(Languages.Resources.MsgDBNotFound, Languages.Resources.ErrorSimple,
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		/// <summary>
		/// Closes the log file and database and saves the current language to a config file.
		/// </summary>
		private void App_OnExit(object sender, ExitEventArgs e)
		{
			SaveLanguageToFile(Languages.Resources.Culture);
			_accessDB.Dispose();
			Log.Close();
		}

		/// <summary>
		/// Saves the specified language to a config file.
		/// </summary>
		/// <param name="language">The language to save to a file.</param>
		private void SaveLanguageToFile(CultureInfo language)
		{
			Directory.CreateDirectory("config");

			using var stream = new FileStream(@"config\language.txt", FileMode.Create, FileAccess.Write, FileShare.Read);
			using var writer = new StreamWriter(stream);

			writer.WriteLine(language.ToString());
		}

		/// <summary>
		/// Loads and returns a language that was saved to a config file.
		/// </summary>
		/// <returns>The language that was loaded from a file.</returns>
		private CultureInfo LoadLanguageFromFile()
		{
			using var stream = new FileStream(@"config\language.txt", FileMode.Open, FileAccess.Read, FileShare.Read);
			using var reader = new StreamReader(stream);

			string languageStr = reader.ReadLine();

			return languageStr != null ? new CultureInfo(languageStr) : null;
		}

		/// <summary>
		/// Wrapper method to execute and log a SQL query.
		/// </summary>
		/// <param name="query">The query that should be executed.</param>
		/// <returns>The query results.</returns>
		public static async Task<QueryResult> ExecuteQuery(string query)
		{
			QueryResult queryRes = await _accessDB.ExecuteQuery(query);
			lock(DBPassword)
			{
				Log.Write("Executed SQL query:", 1, TraceEventType.Information, true);
				Log.Write($"Query: {queryRes.ExecutedQuery}\nSuccess: {queryRes.Success}", 2, null);
			}

			return queryRes;
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
