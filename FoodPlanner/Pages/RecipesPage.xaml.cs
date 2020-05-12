using AccessCommunication;
using System.Windows.Controls;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for RecipesPage.xaml
	/// </summary>
	public partial class RecipesPage : Page
	{
		/// <summary>
		/// Initialize this page.
		/// </summary>
		public RecipesPage()
		{
			InitializeComponent();

			Dispatcher.InvokeAsync(async() =>
			{
				QueryResult recipes = await App.AccessDB.ExecuteQuery("select Gerichtname, Zubereitung from Rezepte");

				ListRecipes.Items.Clear();
				for(int i = 0; i < recipes.ReturnedRows.Count; i++)
				{
					ListRecipes.Items.Add(recipes.ReturnedRows[i][0]);
				}

				TxtBoxRecipeName.Text = recipes.ReturnedRows[0][0].ToString() ?? "";
				TxtBoxRecipeDesc.Text = recipes.ReturnedRows[0][1].ToString() ?? "";

				QueryResult ingredients = await App.AccessDB.ExecuteQuery("select * from Zutatenmengen");
				ListIngreds.Items.Clear();
				for(int i = 1; i < ingredients.ColumnNames.Length; i++)
				{
					ListIngreds.Items.Add($"{ingredients.ReturnedRows[0][i]} {ingredients.ColumnNames[i]}");
				}
			});
		}
	}
}
