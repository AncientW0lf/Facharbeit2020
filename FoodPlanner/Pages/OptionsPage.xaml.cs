using System.Diagnostics;
using FoodPlanner.AdditionalWindows;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BoxLib.Scripts;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for OptionsPage.xaml
	/// </summary>
	public partial class OptionsPage : Page
	{
		/// <summary>
		/// Initializes the page.
		/// </summary>
		public OptionsPage()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Refreshes the image displayed in the change language button to reflect the current language.
		/// </summary>
		private void RefreshFlagImage(object sender, RoutedEventArgs e)
		{
			CultureInfo currLang = Languages.Resources.Culture;

			ImgLangFlagBtn.Source = new BitmapImage(currLang.GetFlagLocation());
		}

		/// <summary>
		/// Opens a new window to change the current language.
		/// </summary>
		private void ChangeLanguage(object sender, RoutedEventArgs e)
		{
			//Opens the window
			var win = new ChangeLangWin(App.LangHelper.GetSupportedLanguages()) {Owner = Application.Current.MainWindow};
			bool? finished = win.ShowDialog();

			//Returns if the user cancelled the dialog
			if(finished != true) return;

			//Changes the language
			App.LangHelper.ChangeLanguage(win.SelectedCulture);

			Log.Write($"Changed language to {Languages.Resources.Culture}.", 1, TraceEventType.Information);

			//Refreshes the current page to show the new language
			NavigationService?.Refresh();

			MessageBox.Show(Languages.Resources.MsgChangedLang,
				Languages.Resources.SuccessSimple, MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
