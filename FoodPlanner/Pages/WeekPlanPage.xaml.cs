using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using AccessCommunication;
using FoodPlanner.SQLObj;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for WeekPlanPage.xaml
	/// </summary>
	public partial class WeekPlanPage : Page
	{
		public WeekPlanPage()
		{
			InitializeComponent();
		}

		private async void WeekPlanPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			if(IsEnabled)
				return;

			await Task.Delay(100);

			QueryResult currWeekPlan = await App.ExecuteQuery("select Wochentag, IDRezepte from Wochenplan");

			int[] distinctIds = currWeekPlan.ReturnedRows.Select(a => (int)a[1]).Distinct().ToArray();

			string currRecipesQuery = $"select ID, Gerichtname from Rezepte where ID = {distinctIds[0]}";
			for(int i = 1; i < distinctIds.Length; i++)
			{
				currRecipesQuery += $" or ID = {distinctIds[i]}";
			}
			QueryResult currRecipes = await App.ExecuteQuery(currRecipesQuery);
			
			var currWeekPlanObj = new WeekPlan
			{
				Monday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Montag"))?[1] ?? default(int))))?[1].ToString(),

				Tuesday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Dienstag"))?[1] ?? default(int))))?[1].ToString(),

				Wednesday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Mittwoch"))?[1] ?? default(int))))?[1].ToString(),

				Thursday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Donnerstag"))?[1] ?? default(int))))?[1].ToString(),

				Friday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Freitag"))?[1] ?? default(int))))?[1].ToString(),

				Saturday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Samstag"))?[1] ?? default(int))))?[1].ToString(),

				Sunday = currRecipes.ReturnedRows.FirstOrDefault(a => 
					a[0].Equals((int)(currWeekPlan.ReturnedRows.FirstOrDefault(b => 
						b[0].Equals("Sonntag"))?[1] ?? default(int))))?[1].ToString()
			};

			DataContext = currWeekPlanObj;

			IsEnabled = true;
		}
	}
}
