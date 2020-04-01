using System.Windows;
using BoxLib.Scripts;

namespace FoodPlanner
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private void App_OnStartup(object sender, StartupEventArgs e)
		{
			LanguageHelper.Resources = typeof(Resources);
		}
	}
}
