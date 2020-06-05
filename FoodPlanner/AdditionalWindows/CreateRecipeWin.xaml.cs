using AccessCommunication;
using FoodPlanner.SQLObj;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;

namespace FoodPlanner.AdditionalWindows
{
	/// <summary>
	/// Interaction logic for CreateRecipeWin.xaml
	/// </summary>
	public partial class CreateRecipeWin : Window
	{
		/// <summary>
		/// The finished recipe after the window closes.
		/// </summary>
		public FullRecipe Recipe { get; private set; }

		/// <summary>
		/// A list of all available ingredients for this recipe.
		/// </summary>
		private readonly QueryResult _allIngreds;
		
		/// <summary>
		/// A list of all available ingredients for this recipe as <see cref="string"/>.
		/// </summary>
		private readonly ObservableCollection<string> _allIngredsString;

		/// <summary>
		/// Initializes the window. Updates the textboxes with content if a <see cref="recipeID"/> is specified.
		/// </summary>
		/// <param name="recipeID">An optional id to edit the info for an existing recipe.</param>
		public CreateRecipeWin(int? recipeID = null)
		{
			InitializeComponent();

			//Updates the DataContext to be able to create/edit a recipe
			DataContext = recipeID != null 
				? new FullRecipe(recipeID.Value) 
				: new FullRecipe(Languages.Resources.NewRecipeSimple, string.Empty, new IngredientInfo[0]);

			//Adds an event listener to update the ingredient list
			((FullRecipe)DataContext).LinkedIngredients.CollectionChanged += UpdateIngredientsToBeAddedList;

			//Fetches all ingredients available
			_allIngreds = App.ExecuteQuery("select ID, Zutat from Zutaten").GetAwaiter().GetResult();
			_allIngredsString = new ObservableCollection<string>(_allIngreds.ReturnedRows
				.Select(a => a[1].ToString())
				.OrderBy(b => b));

			//Updates the ingredient list so it shows the fetched ingredients
			UpdateIngredientsToBeAddedList(null, null);
		}

		/// <summary>
		/// Close the window and save the recipe into <see cref="Recipe"/> for external use.
		/// </summary>
		private void ConfirmRecipe(object sender, RoutedEventArgs e)
		{
			//Creates a new regex expression to check against illegal SQL query characters
			var matcher = new Regex(".*(\\\"|').*");

			//Checks if the recipe info contains illegal characters
			if(matcher.IsMatch(((FullRecipe)DataContext).Name)
			   || matcher.IsMatch(((FullRecipe)DataContext).Preparation)
			   || ((FullRecipe)DataContext).LinkedIngredients.Any(a => matcher.IsMatch(a.Name ?? string.Empty)
			                                                           || matcher.IsMatch(a.Amount ?? string.Empty)
			                                                           || matcher.IsMatch(a.Note ?? string.Empty)))
			{
				//Warns the user of illegal characters
				MessageBox.Show(Languages.Resources.MsgInvalidRecipeContent, Languages.Resources.ErrorSimple,
					MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			//Saves the recipe and closes the window successfully
			DialogResult = true;
			Recipe = (FullRecipe)DataContext;
			Close();
		}

		/// <summary>
		/// Adds a new linked ingredient based on what is currently selected in <see cref="CBoxAllIngreds"/>.
		/// </summary>
		private void AddLinkedIngredient(object sender, RoutedEventArgs e)
		{
			if(string.IsNullOrWhiteSpace(CBoxAllIngreds.Text))
				return;

			((FullRecipe)DataContext).LinkedIngredients.Add(
				new IngredientInfo(
					(int)_allIngreds.ReturnedRows
						.First(a => 
							a[1].Equals(CBoxAllIngreds.Text)
							)
						[0]
					)
				);
		}

		/// <summary>
		/// Removes linked ingredients based on what is currently selected in <see cref="IngredsList"/>.
		/// </summary>
		private void RemoveLinkedIngredient(object sender, RoutedEventArgs e)
		{
			for(int i = 0; i < IngredsList.SelectedItems.Count; i++)
			{
				((FullRecipe)DataContext).LinkedIngredients.Remove((IngredientInfo)IngredsList.SelectedItems[i]);
			}
		}

		/// <summary>
		/// Updates the list of available (not linked) ingredients based on the current <see cref="Window.DataContext"/>.
		/// </summary>
		private void UpdateIngredientsToBeAddedList(object sender, NotifyCollectionChangedEventArgs e)
		{
			CBoxAllIngreds.ItemsSource = new ObservableCollection<string>();

			IEnumerable<string> addedIngreds = ((FullRecipe)DataContext).LinkedIngredients.Select(a => a.Name).ToArray();
			for(int i = 0; i < _allIngredsString.Count; i++)
			{
				if(!addedIngreds.Contains(_allIngredsString[i]))
				{
					((ObservableCollection<string>)CBoxAllIngreds.ItemsSource).Add(_allIngredsString[i]);
				}
			}
		}
	}
}
