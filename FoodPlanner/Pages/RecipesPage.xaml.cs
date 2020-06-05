using AccessCommunication;
using FoodPlanner.SQLObj;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for RecipesPage.xaml
	/// </summary>
	public partial class RecipesPage : Page
	{
		/// <summary>
		/// Contains a list of all recipes.
		/// </summary>
		private QueryResult _recipes;

		/// <summary>
		/// Contains a list of all linked ingredients.
		/// </summary>
		private QueryResult _ingredients;

		/// <summary>
		/// Initialize this page and adds a reference to <see cref="App.CurrRecipePage"/>.
		/// </summary>
		public RecipesPage()
		{
			InitializeComponent();

			App.CurrRecipePage = this;
		}

		/// <summary>
		/// Fetches all recipes and linked ingredients and enables this page.
		/// </summary>
		private async void RecipesPage_OnLoaded(object sender, RoutedEventArgs e)
		{
			//Returns if the page is already filled with information
			if(IsEnabled)
				return;

			//Wait for the window to display this page
			await Task.Delay(100);

			//Gets all recipes
			_recipes = await App.ExecuteQuery(
				"select ID, Gerichtname, Zubereitung from Rezepte");

			//Gets all linked ingredients
			_ingredients = await App.ExecuteQuery(
				"select Rezepte_ID, Gerichtname, Zutat, Menge, Notiz from ZusammenfassungRezeptzutatenliste");

			//Adds all fetched recipes to an array as new objects
			ListRecipes.Items.Clear();
			var rawArray = new FullRecipe[_recipes.ReturnedRows.Count];
			for(int i = 0; i < _recipes.ReturnedRows.Count; i++)
			{
				rawArray[i] = new FullRecipe(
					_recipes.ReturnedRows[i][0] as int?,
					_recipes.ReturnedRows[i][1].ToString(),
					_recipes.ReturnedRows[i][2].ToString(),
					new IngredientInfo[0]);
			}

			//Orders the recipes and adds them to the displayed list
			FullRecipe[] orderedArray = rawArray.OrderBy(a => a.Name).ToArray();
			for(int i = 0; i < orderedArray.Length; i++)
			{
				ListRecipes.Items.Add(orderedArray[i]);
			}

			//Enables the page
			IsEnabled = true;
		}

		/// <summary>
		/// Updates the displayed information based on what recipe is selected.
		/// </summary>
		private void ListRecipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			//Gets the currently selected recipe
			object[] selected =
				_recipes.ReturnedRows.FirstOrDefault(a => a[0].Equals(((FullRecipe)e.AddedItems[0])?.ID));

			//Gets all linked ingredients for the currently selected recipe
			object[][] selectedIngreds = _ingredients.ReturnedRows.Where(a => a[0].Equals(selected?[0])).ToArray();

			//Returns if no recipe is selected
			if(selected == null)
				return;

			//Updates the displayed information
			TxtBoxRecipeName.Text = selected[1].ToString() ?? "";
			TxtBoxRecipeDesc.Text = selected[2].ToString() ?? "";

			ListIngreds.Items.Clear();
			for(int i = 0; i < selectedIngreds.Length; i++)
			{
				ListIngreds.Items.Add(new IngredientInfo(
					selectedIngreds[i][2].ToString(),
					selectedIngreds[i][3].ToString(),
					selectedIngreds[i][4].ToString()));
			}
		}

		/// <summary>
		/// Opens a new window to create a recipe and refreshes this page afterwards.
		/// </summary>
		private async void OpenNewRecipeWin(object sender, RoutedEventArgs e)
		{
			await SqlHelper.InsertNewRecipeInteractive(true);

			NavigationService?.Refresh();
		}

		/// <summary>
		/// Opens a new window to edit the currently selected recipe and refreshes the page afterwards.
		/// </summary>
		private async void OpenEditRecipeWin(object sender, RoutedEventArgs e)
		{
			//Gets the currently selected recipe
			object[] selected =
				_recipes.ReturnedRows.FirstOrDefault(a => a[0].Equals(((FullRecipe)ListRecipes.SelectedItem)?.ID));

			//Returns if no recipe is selected
			if(selected == null)
				return;

			//Updates the recipe info
			await SqlHelper.UpdateRecipeInteractive((int)selected[0], true);

			//Refreshes the page
			NavigationService?.Refresh();
		}

		/// <summary>
		/// Deletes the currently selected recipe.
		/// </summary>
		private async void DeleteRecipe(object sender, RoutedEventArgs e)
		{
			//Gets the currently selected recipe
			object[] selected =
				_recipes.ReturnedRows.FirstOrDefault(a => a[0].Equals(((FullRecipe)ListRecipes.SelectedItem)?.ID));

			//Returns if no recipe is selected
			if(selected == null)
				return;

			//Deletes the recipe
			await SqlHelper.DeleteRecipeInteractive(selected[1].ToString(), (int)selected[0], true);

			//Refreshes the page
			NavigationService?.Refresh();
		}
	}
}