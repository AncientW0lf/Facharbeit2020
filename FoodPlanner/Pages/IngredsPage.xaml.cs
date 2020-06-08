using System;
using System.Collections;
using AccessCommunication;
using FoodPlanner.SQLObj;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FoodPlanner.Pages
{
	/// <summary>
	/// Interaction logic for IngredsPage.xaml
	/// </summary>
	public partial class IngredsPage : Page
	{
		/// <summary>
		/// A list of ingredients that are currently hidden by the search.
		/// </summary>
		private readonly List<IngredientInfo> _hiddenIngreds = new List<IngredientInfo>();

		/// <summary>
		/// Initializes this page.
		/// </summary>
		public IngredsPage()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Loads the full list of ingredients and enables the page afterwards.
		/// </summary>
		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			//Returns if the page is already filled with information
			if(IsEnabled)
				return;

			//Waits for the window to display the page
			await Task.Delay(100);

			//Gets the list of ingredients
			QueryResult rawIngreds = await App.ExecuteQuery("select ID, Zutat from Zutaten");

			//Orders the list of ingredients and adds it to the DataContext
			var polishedIngreds = new List<IngredientInfo>();
			polishedIngreds.AddRange(rawIngreds.ReturnedRows.Select(a => new IngredientInfo((int)a[0], a[1].ToString())));
			DataContext = new ObservableCollection<IngredientInfo>(polishedIngreds.OrderBy(a => a.Name));

			//Enables the page
			IsEnabled = true;
		}

		/// <summary>
		/// Updates the list of visible ingredients based on the current search term in <see cref="TxtBoxSearch"/>.
		/// Gets called every time something is typed into the textbox.
		/// </summary>
		private void UpdateSearch(object sender, KeyEventArgs e)
		{
			//Gets the current DataContext
			var currItems = (ObservableCollection<IngredientInfo>)DataContext;
			
			//Gets two lists of ingredients that match the search term
			IngredientInfo[] foundItems =
				currItems.Where(a => a.Name.ToLowerInvariant().Contains(TxtBoxSearch.Text.ToLowerInvariant())).ToArray();
			IngredientInfo[] foundHiddenItems =
				_hiddenIngreds.Where(a => a.Name.ToLowerInvariant().Contains(TxtBoxSearch.Text.ToLowerInvariant())).ToArray();

			//Iterates through all ingredients in the displayed list
			for(int i = 0; i < currItems.Count; i++)
			{
				//Removes the ingredient from the list if it doesn't match the search term
				if(!foundItems.Contains(currItems[i]))
				{
					_hiddenIngreds.Add(currItems[i]);
					currItems.RemoveAt(i);
					i--;
				}
				//Removes the ingredient from the list of hidden ingredients if it does match the search term
				else
				{
					_hiddenIngreds.Remove(currItems[i]);
				}
			}

			//Iterates through all hidden ingredients
			for(int i = 0; i < _hiddenIngreds.Count; i++)
			{
				//Ignores the ingredient if it doesn't match the search term
				if(!currItems.Contains(_hiddenIngreds[i]) 
				   && !foundItems.Contains(_hiddenIngreds[i]) 
				   && !foundHiddenItems.Contains(_hiddenIngreds[i])) 
					continue;

				//Adds the ingredient back to the displayed list
				currItems.Add(_hiddenIngreds[i]);
				_hiddenIngreds.RemoveAt(i);
				i--;
			}

			//Orders the ingredients
			DataContext = new ObservableCollection<IngredientInfo>(currItems.OrderBy(a => a.Name));
		}

		/// <summary>
		/// Inserts a new ingredient into the database and refreshes the page afterwards.
		/// </summary>
		private async void AddIngredient(object sender, RoutedEventArgs e)
		{
			await SqlHelper.InsertNewIngredientInteractive(true);

			NavigationService?.Refresh();
		}

		/// <summary>
		/// Inserts a new ingredient into the database and refreshes the page afterwards.
		/// </summary>
		private async void EditIngredient(object sender, RoutedEventArgs e)
		{
			var selected = (IngredientInfo)LBoxIngreds.SelectedItem;

			await SqlHelper.UpdateIngredientInteractive(
				selected.ID 
				?? throw new ArgumentException($"ID of ingredient \"{selected.Name}\" not present!"), 
				selected.Name, 
				true);

			NavigationService?.Refresh();
		}

		/// <summary>
		/// Removes all selected ingredients in the database and refreshes the page afterwards.
		/// </summary>
		private async void RemoveIngredient(object sender, RoutedEventArgs e)
		{
			IList selected = LBoxIngreds.SelectedItems;

			for(int i = 0; i < selected.Count; i++)
			{
				await SqlHelper.DeleteIngredientInteractive(
					((IngredientInfo)selected[i])?.Name,
					((IngredientInfo)selected[i])?.ID 
					?? throw new ArgumentException(
						$"ID of ingredient \"{((IngredientInfo)selected[i])?.Name}\" not present!"), 
					true);
			}

			NavigationService?.Refresh();
		}
	}
}
