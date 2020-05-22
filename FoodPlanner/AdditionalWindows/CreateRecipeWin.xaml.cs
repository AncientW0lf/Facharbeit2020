using AccessCommunication;
using FoodPlanner.SQLObj;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;

namespace FoodPlanner.AdditionalWindows
{
	/// <summary>
	/// Interaction logic for CreateRecipeWin.xaml
	/// </summary>
	public partial class CreateRecipeWin : Window
	{
		public FullRecipe Recipe { get; private set; }

		private readonly QueryResult _allIngreds;

		private readonly ObservableCollection<string> _allIngredsString;

		public CreateRecipeWin(int? recipeID = null)
		{
			InitializeComponent();
			DataContext = recipeID != null 
				? new FullRecipe(recipeID.Value) 
				: new FullRecipe(Languages.Resources.NewRecipeSimple, string.Empty, new IngredientInfo[0]);

			((FullRecipe)DataContext).LinkedIngredients.CollectionChanged += UpdateIngredientsToBeAddedList;

			_allIngreds = App.AccessDB.ExecuteQuery("select ID, Zutat from Zutaten").GetAwaiter().GetResult();
			_allIngredsString = new ObservableCollection<string>(_allIngreds.ReturnedRows
				.Select(a => a[1].ToString())
				.OrderBy(b => b));
		}

		private void ConfirmRecipe(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Recipe = (FullRecipe)DataContext;
			Close();
		}

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

		private void RemoveLinkedIngredient(object sender, RoutedEventArgs e)
		{
			for(int i = 0; i < IngredsList.SelectedItems.Count; i++)
			{
				((FullRecipe)DataContext).LinkedIngredients.Remove((IngredientInfo)IngredsList.SelectedItems[i]);
			}
		}

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
