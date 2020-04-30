using BoxLib.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using CultureInfo = System.Globalization.CultureInfo;

namespace FoodPlanner.AdditionalWindows
{
	/// <summary>
	/// Interaction logic for ChangeLangWin.xaml
	/// </summary>
	public partial class ChangeLangWin : Window
	{
		/// <summary>
		/// The culture that was selected by the user.
		/// </summary>
		public CultureInfo SelectedCulture { get; private set; }

		/// <summary>
		/// Initializes a new window and retrieves all supported languages.
		/// </summary>
		public ChangeLangWin(IReadOnlyList<CultureInfo> languages)
		{
			InitializeComponent();

			//Adds the languages
			for(int i = 0; i < languages.Count; i++)
			{
				LangBox.Items.Add(languages[i]);
			}

			LangBox.SelectedIndex = 0;

			Log.Write($"Initialized {nameof(ChangeLangWin)} window.", 1, TraceEventType.Information);
		}

		/// <summary>
		/// Closes the window and saves the selected language to <see cref="SelectedCulture"/>.
		/// </summary>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			//Cancels if nothing was chosen
			if(string.IsNullOrWhiteSpace(LangBox.Text))
				return;

			SelectedCulture = LangBox.SelectedItem as CultureInfo;
			DialogResult = true;
			Close();
		}

		private void ChangeFlag(object sender, SelectionChangedEventArgs e)
		{
			var selectedItem = e.AddedItems[0] as CultureInfo;

			if(selectedItem.Equals(CultureInfo.GetCultureInfo("en-us")))
				FlagImg.Source = new BitmapImage(new Uri(@"Resources\Flags\united-states.png"));
			else if(selectedItem.Equals(CultureInfo.GetCultureInfo("de-de")))
				FlagImg.Source = new BitmapImage(new Uri(@"Resources\Flags\germany.png"));
		}
	}
}
