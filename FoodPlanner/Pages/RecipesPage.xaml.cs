using System.Linq;
using System.Threading.Tasks;
using AccessCommunication;
using System.Windows.Controls;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for RecipesPage.xaml
	/// </summary>
	public partial class RecipesPage : Page
	{
		private QueryResult _recipes;

		private QueryResult _ingredients;

		/// <summary>
		/// Initialize this page.
		/// </summary>
		public RecipesPage()
		{
			InitializeComponent();

			Task.Run(async() =>
			{
				_recipes = await App.AccessDB.ExecuteQuery("select Gerichtname, Zubereitung from Rezepte");
				_ingredients = await App.AccessDB.ExecuteQuery("select * from Zutatenmengen");

				await Dispatcher.InvokeAsync(() =>
				{
					ListRecipes.Items.Clear();
					for(int i = 0; i < _recipes.ReturnedRows.Count; i++)
					{
						ListRecipes.Items.Add(_recipes.ReturnedRows[i][0]);
					}
				});
			});
		}

		private void ListRecipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			object[] selected = _recipes.ReturnedRows.FirstOrDefault(a => a[0].Equals(e.AddedItems[0]));
			int selectedIndex = _recipes.ReturnedRows.IndexOf(selected);

			if(selected == null)
				return;

			TxtBoxRecipeName.Text = _recipes.ReturnedRows[selectedIndex][0].ToString() ?? "";
			TxtBoxRecipeDesc.Text = _recipes.ReturnedRows[selectedIndex][1].ToString() ?? "";

			ListIngreds.Items.Clear();
			for(int i = 1; i < _ingredients.ColumnNames.Length; i++)
			{
				ListIngreds.Items.Add($"{_ingredients.ReturnedRows[selectedIndex][i]} {_ingredients.ColumnNames[i]}");
			}
		}
	}
}
