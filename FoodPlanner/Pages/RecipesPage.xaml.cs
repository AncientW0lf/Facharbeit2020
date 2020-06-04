using AccessCommunication;
using FoodPlanner.AdditionalWindows;
using FoodPlanner.SQLObj;
using System;
using System.Collections.Generic;
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
		private QueryResult _recipes;

		private QueryResult _ingredients;

		/// <summary>
		/// Initialize this page.
		/// </summary>
		public RecipesPage()
		{
			InitializeComponent();

			Task.Run(async () =>
			{
				_recipes = await App.ExecuteQuery(
					"select ID, Gerichtname, Zubereitung from Rezepte");

				_ingredients = await App.ExecuteQuery(
					"select Rezepte_ID, Gerichtname, Zutat, Menge, Notiz from ZusammenfassungRezeptzutatenliste");

				await Dispatcher.InvokeAsync(() =>
				{
					ListRecipes.Items.Clear();
					for(int i = 0; i < _recipes.ReturnedRows.Count; i++)
					{
						ListRecipes.Items.Add(new FullRecipe(
							_recipes.ReturnedRows[i][0] as int?, 
							_recipes.ReturnedRows[i][1].ToString(), 
							_recipes.ReturnedRows[i][2].ToString(),
							new IngredientInfo[0]));
					}
				});
			});
		}

		private void ListRecipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			object[] selected = _recipes.ReturnedRows.FirstOrDefault(a => a[0].Equals(((FullRecipe)e.AddedItems[0])?.ID));
			object[][] selectedIngreds = _ingredients.ReturnedRows.Where(a => a[0].Equals(selected?[0])).ToArray();

			if(selected == null)
				return;

			TxtBoxRecipeName.Text = selected[1].ToString() ?? "";
			TxtBoxRecipeDesc.Text = selected[2].ToString() ?? "";

			ListIngreds.Items.Clear();
			for(int i = 0; i < selectedIngreds.Length; i++)
			{
				ListIngreds.Items.Add(new ListBoxItem
				{
					Content = $"{selectedIngreds[i][3]} {selectedIngreds[i][2]}"
				});

				if(!string.IsNullOrWhiteSpace(selectedIngreds[i][4].ToString()))
					((ListBoxItem)ListIngreds.Items[^1]).ToolTip = selectedIngreds[i][4];
			}
		}

		private async void OpenNewRecipeWin(object sender, RoutedEventArgs e)
		{
			await SqlHelper.InsertNewRecipeInteractive(true);

			NavigationService?.Refresh();
		}

		private async void OpenEditRecipeWin(object sender, RoutedEventArgs e)
		{			
			object[] selected = _recipes.ReturnedRows.FirstOrDefault(a => a[0].Equals(((FullRecipe)ListRecipes.SelectedItem)?.ID));

			if(selected == null) 
				return;

			await SqlHelper.UpdateRecipeInteractive((int)selected[0], true);

			NavigationService?.Refresh();
		}

		private async void DeleteRecipe(object sender, RoutedEventArgs e)
		{
			object[] selected = _recipes.ReturnedRows.FirstOrDefault(a => a[0].Equals(((FullRecipe)ListRecipes.SelectedItem)?.ID));

			if(selected == null)
				return;

			await SqlHelper.DeleteRecipeInteractive(selected[1].ToString(), (int)selected[0], true);

			NavigationService?.Refresh();
		}
	}
}