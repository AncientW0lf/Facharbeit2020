using System;
using AccessCommunication;
using FoodPlanner.AdditionalWindows;
using System.Collections.Generic;
using System.Globalization;
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
						ListRecipes.Items.Add(_recipes.ReturnedRows[i][1]);
					}
				});
			});
		}

		private void ListRecipes_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			object[] selected = _recipes.ReturnedRows.FirstOrDefault(a => a[1].Equals(e.AddedItems[0]));
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
			var win = new CreateRecipeWin();
			win.ShowDialog();

			if(win.DialogResult != true)
				return;

			var allResults = new List<QueryResult>
			{
				await App.ExecuteQuery(
					"insert into Rezepte(Gerichtname, Zubereitung) " +
					$"values('{win.Recipe.Name}', '{win.Recipe.Preparation}')"),
				await App.ExecuteQuery("select ID from Rezepte " +
				                       $"where Gerichtname = '{win.Recipe.Name}' " +
				                       $"and Zubereitung = '{win.Recipe.Preparation}'")
			};

			if(allResults[1].ReturnedRows?[0][0] is int newRecipeID)
			{
				for (int i = 0; i < win.Recipe.LinkedIngredients.Count; i++)
				{
					allResults.Add(await App.ExecuteQuery(
						"insert into Rezeptzutatenliste(IDRezepte, IDZutaten, Menge, Notiz) " +
						$"values({newRecipeID}, " +
						$"{win.Recipe.LinkedIngredients[i].ID}, " +
						$"'{win.Recipe.LinkedIngredients[i].Amount}', " +
						$"'{win.Recipe.LinkedIngredients[i].Note}')"));
				}
			}

			if(!allResults.All(a => a.Success))
			{
				MessageBox.Show(Languages.Resources.MsgRecipeCreatedWithErrors.Replace("$1", allResults.Count(a => !a.Success).ToString()), 
					Languages.Resources.ErrorSimple, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				MessageBox.Show(Languages.Resources.MsgRecipeCreated, 
					Languages.Resources.SuccessSimple, MessageBoxButton.OK, MessageBoxImage.Information);
			}

			NavigationService?.Refresh();
		}

		private async void OpenEditRecipeWin(object sender, RoutedEventArgs e)
		{			
			object[] selected = _recipes.ReturnedRows.FirstOrDefault(a => a[1].Equals(ListRecipes.SelectedItem?.ToString()));

			if(selected == null) 
				return;

			var win = new CreateRecipeWin((int)selected[0]);
			win.ShowDialog();

			if(win.DialogResult != true)
				return;

			//TODO: Add queries for Rezepte, Rezeptzutatenliste and Zutaten
		}

		private async void DeleteRecipe(object sender, RoutedEventArgs e)
		{
			object[] selected = _recipes.ReturnedRows.FirstOrDefault(a => a[1].Equals(ListRecipes.SelectedItem?.ToString()));

			if(selected == null || MessageBox.Show(Languages.Resources.DelRecipeConfirm.Replace("$1", selected[1].ToString()),
				Languages.Resources.ConfirmSimple, MessageBoxButton.YesNo, MessageBoxImage.Question,
				MessageBoxResult.No) == MessageBoxResult.No)
				return;

			QueryResult result = default, result2 = default;
			try
			{
				result = await App.ExecuteQuery($"delete from Rezeptzutatenliste where IDRezepte = {selected[0]}");
				result2 = await App.ExecuteQuery($"delete from Rezepte where ID = {selected[0]}");
			}
			catch(Exception)
			{
				//Ignore
			}

			MessageBox.Show(Languages.Resources.DelQueriesInfo
					.Replace("$1", "2")
					.Replace("$2", (result.Success && result2.Success).ToString(Languages.Resources.Culture))
					.Replace("$3", (result.RecordsAffected + result2.RecordsAffected).ToString()),
				Languages.Resources.InfoSimple, MessageBoxButton.OK, MessageBoxImage.Information);

			NavigationService?.Refresh();
		}
	}
}