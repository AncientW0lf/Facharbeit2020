using System;
using System.Windows;
using System.Windows.Input;

namespace FoodPlanner.AdditionalWindows
{
	/// <summary>
	/// Interaction logic for StringInputWin.xaml
	/// </summary>
	public partial class StringInputWin : Window
	{
		/// <summary>
		/// The input of the user after the window is closed.
		/// </summary>
		public string Result { get; set; }

		/// <summary>
		/// The action that will be executed when pressing the custom button.
		/// </summary>
		private readonly Action<StringInputWin> _customBtn;

		/// <summary>
		/// Ctor to initialize a new <see cref="StringInputWin"/>.
		/// </summary>
		/// <param name="title">The window title.</param>
		/// <param name="content">The instructions given to the user.</param>
		/// <param name="defaultResult">A default value to enter into the textbox.</param>
		/// <param name="customButtonAction">An action that will be executed when pressing the custom button. The custom button will be invisible if this is null.</param>
		/// <param name="customButtonText">The label of the custom button.</param>
		public StringInputWin(string title, string content, string defaultResult = null, Action<StringInputWin> customButtonAction = null, string customButtonText = null)
		{
			InitializeComponent();

			Title = title;
			TxtBlockContent.Text = content;
			TxtBoxInput.Text = defaultResult ?? string.Empty;

			if(customButtonAction == null) return;

			BtnCustom.Content = customButtonText;
			BtnCustom.Visibility = Visibility.Visible;
			_customBtn = customButtonAction;
		}

		/// <summary>
		/// Confirm button.
		/// </summary>
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Result = TxtBoxInput.Text;
			DialogResult = true;
			Close();
		}

		/// <summary>
		/// Cancel button.
		/// </summary>
		private void Button2_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		/// <summary>
		/// Custom button.
		/// </summary>
		private void ButtonCustom_Click(object sender, RoutedEventArgs e)
		{
			_customBtn.Invoke(this);
		}

		/// <summary>
		/// Automatically sets the cursor focus to the textbox upon loading.
		/// </summary>
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			FocusManager.SetFocusedElement(this, TxtBoxInput);
		}
	}
}
