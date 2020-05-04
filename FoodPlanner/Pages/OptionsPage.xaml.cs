using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FoodPlanner.AdditionalWindows;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for OptionsPage.xaml
	/// </summary>
	public partial class OptionsPage : Page
	{
		public OptionsPage()
		{
			InitializeComponent();
		}

		private void RefreshFlagImage(object sender, RoutedEventArgs e)
		{
			CultureInfo currLang = Languages.Resources.Culture;

			ImgLangFlagBtn.Source = new BitmapImage(currLang.GetFlagLocation());
		}

		private void ChangeLanguage(object sender, RoutedEventArgs e)
		{
			var win = new ChangeLangWin(App.LangHelper.GetSupportedLanguages()) {Owner = Application.Current.MainWindow};
			bool? finished = win.ShowDialog();

			if(finished != true) return;

			App.LangHelper.ChangeLanguage(win.SelectedCulture);

			NavigationService?.Refresh();

			MessageBox.Show(Languages.Resources.MsgChangedLang,
				Languages.Resources.SuccessSimple, MessageBoxButton.OK, MessageBoxImage.Information);
		}
	}
}
