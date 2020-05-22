using AccessCommunication;
using BoxLib.Scripts;
using FoodPlanner.AdditionalWindows;
using System.Diagnostics;
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
				_recipes = await App.AccessDB.ExecuteQuery(
					"select ID, Gerichtname, Zubereitung from Rezepte");
				Log.Write("Executed SQL query:", 1, TraceEventType.Information, true);
				Log.Write($"Query: {_recipes.ExecutedQuery}\nSuccess: {_recipes.Success}", 2, null);

				_ingredients = await App.AccessDB.ExecuteQuery(
					"select Rezepte_ID, Gerichtname, Zutat, Menge, Notiz from ZusammenfassungRezeptzutatenliste");
				Log.Write("Executed SQL query:", 1, TraceEventType.Information, true);
				Log.Write($"Query: {_ingredients.ExecutedQuery}\nSuccess: {_ingredients.Success}", 2, null);

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

		private void OpenNewRecipeWin(object sender, RoutedEventArgs e)
		{
			var win = new CreateRecipeWin();
			win.ShowDialog();
			win.Closed += (_, __) =>
			{
				if(win.DialogResult != true)
					return;

				//App.AccessDB.ExecuteQuery(
				//	$"insert into Rezepte(Gerichtname, Zubereitung) values('{win.Recipe.Name}', '{win.Recipe.Preparation}')");
				//TODO: Add queries for Rezeptzutatenliste and Zutaten
			};
		}

		private void OpenEditRecipeWin(object sender, RoutedEventArgs e)
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
	}
}