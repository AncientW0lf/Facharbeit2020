using AccessCommunication;
using FoodPlanner.SQLObj;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for IngredsPage.xaml
	/// </summary>
	public partial class IngredsPage : Page
	{
		public IngredsPage()
		{
			InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			if(IsEnabled)
				return;

			await Task.Delay(100);

			QueryResult rawIngreds = await App.ExecuteQuery("select ID, Zutat from Zutaten");

			var polishedIngreds = new List<IngredientInfo>();
			polishedIngreds.AddRange(rawIngreds.ReturnedRows.Select(a => new IngredientInfo((int)a[0], a[1].ToString())));
			DataContext = new ObservableCollection<IngredientInfo>(polishedIngreds);

			IsEnabled = true;
		}
	}
}
