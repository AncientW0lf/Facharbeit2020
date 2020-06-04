﻿using System;
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
		private readonly List<IngredientInfo> _hiddenIngreds = new List<IngredientInfo>();

		public IngredsPage()
		{
			InitializeComponent();
		}

		private async void Page_Loaded(object sender, RoutedEventArgs e)
		{
			if(IsEnabled)
				return;

			await Task.Delay(100);

			QueryResult rawIngreds = await App.ExecuteQuery("select ID, Zutat from Zutaten");

			var polishedIngreds = new List<IngredientInfo>();
			polishedIngreds.AddRange(rawIngreds.ReturnedRows.Select(a => new IngredientInfo((int)a[0], a[1].ToString())));
			DataContext = new ObservableCollection<IngredientInfo>(polishedIngreds.OrderBy(a => a.Name));

			IsEnabled = true;
		}

		private void UpdateSearch(object sender, KeyEventArgs e)
		{
			var currItems = (ObservableCollection<IngredientInfo>)DataContext;
			
			IngredientInfo[] foundItems =
				currItems.Where(a => a.Name.ToLowerInvariant().Contains(TxtBoxSearch.Text.ToLowerInvariant())).ToArray();
			IngredientInfo[] foundHiddenItems =
				_hiddenIngreds.Where(a => a.Name.ToLowerInvariant().Contains(TxtBoxSearch.Text.ToLowerInvariant())).ToArray();

			for(int i = 0; i < currItems.Count; i++)
			{
				if(!foundItems.Contains(currItems[i]))
				{
					_hiddenIngreds.Add(currItems[i]);
					currItems.RemoveAt(i);
					i--;
				}
				else
				{
					_hiddenIngreds.Remove(currItems[i]);
				}
			}

			for(int i = 0; i < _hiddenIngreds.Count; i++)
			{
				if(!currItems.Contains(_hiddenIngreds[i]) 
				   && !foundItems.Contains(_hiddenIngreds[i]) 
				   && !foundHiddenItems.Contains(_hiddenIngreds[i])) 
					continue;

				currItems.Add(_hiddenIngreds[i]);
				_hiddenIngreds.RemoveAt(i);
				i--;
			}

			DataContext = new ObservableCollection<IngredientInfo>(currItems.OrderBy(a => a.Name));
		}

		private async void AddIngredient(object sender, RoutedEventArgs e)
		{
			await SqlHelper.InsertNewIngredientInteractive(true);

			NavigationService?.Refresh();
		}

		private async void RemoveIngredient(object sender, RoutedEventArgs e)
		{
			IngredientInfo[] selected = ((IEnumerable<IngredientInfo>)LBoxIngreds.SelectedItems).ToArray();

			for(int i = 0; i < selected.Length; i++)
			{
				await SqlHelper.DeleteIngredientInteractive(
					selected[i].Name,
					selected[i].ID 
					?? throw new ArgumentException($"ID of ingredient \"{selected[i].Name}\" not present!"), 
					true);
			}

			NavigationService?.Refresh();
		}
	}
}
