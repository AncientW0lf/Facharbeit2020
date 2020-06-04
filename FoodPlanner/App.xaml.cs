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

		public static MainWindow CurrWindow;

		public static RecipesPage CurrRecipePage;

		private static AccessComm _accessDB;

		public const string DBPassword = "Ywc^r72*qX45ndtK";

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

			savedLang ??= CultureInfo.InstalledUICulture;

			LangHelper.ChangeLanguage(savedLang);
			bool isSupported = LangHelper.GetSupportedLanguages()
				.FirstOrDefault(a => a.Equals(savedLang)) != null;
			Log.Write($"Selected language: {savedLang}", 1, TraceEventType.Information, true);
			Log.Write($"Supported: {isSupported}", 2, null);

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
		/// Closes the log file.
		/// </summary>
		private void App_OnExit(object sender, ExitEventArgs e)
		{
			SaveLanguageToFile(Languages.Resources.Culture);
			_accessDB.Dispose();
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

		public static async Task<QueryResult> ExecuteQuery(string query)
		{
			QueryResult queryRes = await _accessDB.ExecuteQuery(query);
			Log.Write("Executed SQL query:", 1, TraceEventType.Information, true);
			Log.Write($"Query: {queryRes.ExecutedQuery}\nSuccess: {queryRes.Success}", 2, null);

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
