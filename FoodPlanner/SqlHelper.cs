using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AccessCommunication;
using FoodPlanner.AdditionalWindows;
using FoodPlanner.SQLObj;

namespace FoodPlanner
{
	/// <summary>
	/// This class provides static methods to make communicating with the <see cref="AccessComm"/> a bit easier.
	/// It also catches any exceptions that might happen when executing these queries.
	/// </summary>
	public static class SqlHelper
	{
		/// <summary>
		/// Inserts the specified recipe into the database.
		/// </summary>
		/// <param name="recipe">The recipe to insert into the database.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult[]> InsertNewRecipe(FullRecipe recipe)
		{
			List<QueryResult> allResults = null;
			try
			{
				//Inserts basic info
				allResults = new List<QueryResult>
				{
					await App.ExecuteQuery(
						"insert into Rezepte(Gerichtname, Zubereitung) " +
						$"values('{recipe.Name}', '{recipe.Preparation}')"),
					await App.ExecuteQuery("select ID from Rezepte " +
					                       $"where Gerichtname = '{recipe.Name}' " +
					                       $"and Zubereitung = '{recipe.Preparation}'")
				};

				//Inserts the ingredient links
				if(allResults[1].ReturnedRows?[0][0] is int newRecipeID)
				{
					for(int i = 0; i < recipe.LinkedIngredients.Count; i++)
					{
						allResults.Add(await App.ExecuteQuery(
							"insert into Rezeptzutatenliste(IDRezepte, IDZutaten, Menge, Notiz) " +
							$"values({newRecipeID}, " +
							$"{recipe.LinkedIngredients[i].ID}, " +
							$"'{recipe.LinkedIngredients[i].Amount}', " +
							$"'{recipe.LinkedIngredients[i].Note}')"));
					}
				}
			}
			catch(Exception)
			{
				//Ignore
			}

			return allResults?.ToArray();
		}
		
		/// <summary>
		/// Inserts a recipe specified by the user into the database.
		/// </summary>
		/// <param name="msgFinished">Enable to display a message once the queries have returned.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult[]> InsertNewRecipeInteractive(bool msgFinished)
		{
			//Opens a new window to ask the user for the recipe details
			var win = new CreateRecipeWin {Owner = App.CurrWindow};
			win.ShowDialog();

			//Returns if the dialog was canceled
			if(win.DialogResult != true)
				return null;

			//Inserts the user-specified recipe
			QueryResult[] allResults = await InsertNewRecipe(win.Recipe);

			if(!msgFinished) return allResults;

			//Shows an informational message
			if(!allResults.All(a => a.Success))
			{
				MessageBox.Show(Languages.Resources.MsgRecipeCreatedWithErrors
						.Replace("$1", allResults.Count(a => !a.Success).ToString()),
					Languages.Resources.ErrorSimple, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				MessageBox.Show(Languages.Resources.MsgRecipeCreated,
					Languages.Resources.SuccessSimple, MessageBoxButton.OK, MessageBoxImage.Information);
			}

			return allResults;
		}
		
		/// <summary>
		/// Updates the specified recipe in the database.
		/// </summary>
		/// <param name="recipe">The recipe to update in the database.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult[]> UpdateRecipe(FullRecipe recipe)
		{
			List<QueryResult> allResults = null;
			try
			{
				//Updates the basic info of the recipe & deletes all recipe links temporarily
				allResults = new List<QueryResult>
				{
					await App.ExecuteQuery(
						"update Rezepte " +
						$"set Gerichtname = '{recipe.Name}', Zubereitung = '{recipe.Preparation}' " +
						$"where ID = {recipe.ID}"),
					await App.ExecuteQuery($"delete from Rezeptzutatenliste where IDRezepte = {recipe.ID}")
				};

				//Updates the ingredient links of the recipe
				for(int i = 0; i < recipe.LinkedIngredients.Count; i++)
				{
					allResults.Add(await App.ExecuteQuery(
						"insert into Rezeptzutatenliste(IDRezepte, IDZutaten, Menge, Notiz) " +
						$"values({recipe.ID}, " +
						$"{recipe.LinkedIngredients[i].ID}, " +
						$"'{recipe.LinkedIngredients[i].Amount}', " +
						$"'{recipe.LinkedIngredients[i].Note}')"));
				}
			}
			catch(Exception)
			{
				//Ignore
			}

			return allResults?.ToArray();
		}
		
		/// <summary>
		/// Updates the specified recipe in the database by asking the user for details.
		/// </summary>
		/// <param name="recipeId">The ID of the recipe to update in the database.</param>
		/// <param name="msgFinished">Enable to display a message once the queries have returned.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult[]> UpdateRecipeInteractive(int recipeId, bool msgFinished)
		{
			//Opens a new window with already filled in infomation to ask the user for the recipe details
			var win = new CreateRecipeWin(recipeId) {Owner = App.CurrWindow};
			win.ShowDialog();

			//Returns if the dialog was canceled
			if(win.DialogResult != true)
				return null;

			//Updates the recipe with the info specified by the user
			QueryResult[] allResults = await UpdateRecipe(win.Recipe);

			if(!msgFinished) return allResults;

			//Shows an informational message
			if(!allResults.All(a => a.Success))
			{
				MessageBox.Show(Languages.Resources.MsgRecipeUpdatedWithErrors
						.Replace("$1", allResults.Count(a => !a.Success).ToString()),
					Languages.Resources.ErrorSimple, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				MessageBox.Show(Languages.Resources.MsgRecipeCreated,
					Languages.Resources.SuccessSimple, MessageBoxButton.OK, MessageBoxImage.Information);
			}

			return allResults;
		}
		
		/// <summary>
		/// Deletes the specified recipe in the database.
		/// </summary>
		/// <param name="recipeId">The ID of the recipe to delete in the database.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult[]> DeleteRecipe(int recipeId)
		{
			QueryResult result = default, result2 = default;
			try
			{
				result = await App.ExecuteQuery($"delete from Rezeptzutatenliste where IDRezepte = {recipeId}");
				result2 = await App.ExecuteQuery($"delete from Rezepte where ID = {recipeId}");
			}
			catch(Exception)
			{
				//Ignore
			}

			return new[] {result, result2};
		}
		
		/// <summary>
		/// Deletes the specified recipe in the database but asks the user beforehand.
		/// </summary>
		/// <param name="recipeName">The name of the recipe to delete in the database.</param>
		/// <param name="recipeId">The ID of the recipe to delete in the database.</param>
		/// <param name="msgFinished">Enable to display a message once the queries have returned.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult[]> DeleteRecipeInteractive(string recipeName, int recipeId, bool msgFinished)
		{
			//Asks the user for confirmation
			if(MessageBox.Show(Languages.Resources.DelRecipeConfirm.Replace("$1", recipeName),
				Languages.Resources.ConfirmSimple, MessageBoxButton.YesNo, MessageBoxImage.Question,
				MessageBoxResult.No) == MessageBoxResult.No)
				return null;

			//Deletes the recipe
			QueryResult[] results = await DeleteRecipe(recipeId);

			//Shows an informational message
			if(msgFinished)
			{
				MessageBox.Show(Languages.Resources.DelQueriesInfo
						.Replace("$1", "2")
						.Replace("$2", (results[0].Success && results[1].Success).ToString(Languages.Resources.Culture))
						.Replace("$3", (results[0].RecordsAffected + results[1].RecordsAffected).ToString()),
					Languages.Resources.InfoSimple, MessageBoxButton.OK, MessageBoxImage.Information);
			}

			return results;
		}

		/// <summary>
		/// Inserts the specified ingredient into the database.
		/// </summary>
		/// <param name="ingredientName">The ingredient to insert into the database.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult?> InsertNewIngredient(string ingredientName)
		{
			try
			{
				return await App.ExecuteQuery(
					"insert into Zutaten(Zutat) " +
					$"values('{ingredientName}')");
			}
			catch(Exception)
			{
				//Ignore
			}

			return null;
		}
		
		/// <summary>
		/// Inserts a ingredient specified by the user into the database.
		/// </summary>
		/// <param name="msgFinished">Enable to display a message once the queries have returned.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult?> InsertNewIngredientInteractive(bool msgFinished)
		{
			//Opens a new window to ask for the ingredient name
			var win = new StringInputWin("New ingredient", "Enter the name of your new ingredient:", "New ingredient")
			{
				Owner = App.CurrWindow
			};
			bool? res = win.ShowDialog();

			//Returns if the dialog was canceled
			if(res != true)
				return null;

			//Inserts the new ingredient
			QueryResult? result = await InsertNewIngredient(win.Result);

			if(!msgFinished) return result;

			//Shows an informational message
			if(!result?.Success ?? false)
			{
				MessageBox.Show(Languages.Resources.MsgIngredientCreatedWithErrors,
					Languages.Resources.ErrorSimple, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				MessageBox.Show(Languages.Resources.MsgIngredientCreated,
					Languages.Resources.SuccessSimple, MessageBoxButton.OK, MessageBoxImage.Information);
			}

			return result;
		}
		
		/// <summary>
		/// Updates the specified ingredient in the database.
		/// </summary>
		/// <param name="ingredient">The ingredient to update in the database.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult?> UpdateIngredient(IngredientInfo ingredient)
		{
			try
			{
				return await App.ExecuteQuery(
					"update Zutaten " +
					$"set Zutat = '{ingredient.Name}' " +
					$"where ID = {ingredient.ID}");
			}
			catch(Exception)
			{
				//Ignore
			}

			return null;
		}
		
		/// <summary>
		/// Updates the specified ingredient in the database by asking the user for details.
		/// </summary>
		/// <param name="ingredientId">The ID of the ingredient to update in the database.</param>
		/// <param name="oldName">An optional parameter to show the user the current ingredient name.</param>
		/// <param name="msgFinished">Enable to display a message once the queries have returned.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult?> UpdateIngredientInteractive(int ingredientId, string oldName, bool msgFinished)
		{
			//Opens a new window to ask the user of the updated ingredient name
			var win = new StringInputWin("Update ingredient", "Enter the new name of the ingredient:", oldName)
			{
				Owner = App.CurrWindow
			};
			bool? res = win.ShowDialog();

			//Returns if the dialog was canceled
			if(res != true)
				return null;

			//Updates the ingredient
			QueryResult? result = await UpdateIngredient(new IngredientInfo(ingredientId, win.Result));

			if(!msgFinished) return result;

			//Shows an informational message
			if(!result?.Success ?? false)
			{
				MessageBox.Show(Languages.Resources.MsgIngredientUpdatedWithErrors,
					Languages.Resources.ErrorSimple, MessageBoxButton.OK, MessageBoxImage.Error);
			}
			else
			{
				MessageBox.Show(Languages.Resources.MsgIngredientCreated,
					Languages.Resources.SuccessSimple, MessageBoxButton.OK, MessageBoxImage.Information);
			}

			return result;
		}
		
		/// <summary>
		/// Deletes the specified ingredient in the database.
		/// </summary>
		/// <param name="ingredientId">The ID of the ingredient to delete in the database.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult[]> DeleteIngredient(int ingredientId)
		{
			QueryResult result = default, result2 = default;
			try
			{
				result = await App.ExecuteQuery($"delete from Rezeptzutatenliste where IDZutaten = {ingredientId}");
				result2 = await App.ExecuteQuery($"delete from Zutaten where ID = {ingredientId}");
			}
			catch(Exception)
			{
				//Ignore
			}

			return new[] {result, result2};
		}
		
		/// <summary>
		/// Deletes the specified ingredient in the database but asks the user beforehand.
		/// </summary>
		/// <param name="ingredientName">The name of the ingredient to delete in the database.</param>
		/// <param name="ingredientId">The ID of the ingredient to delete in the database.</param>
		/// <param name="msgFinished">Enable to display a message once the queries have returned.</param>
		/// <returns>Returns whether or not the required queries were successful.</returns>
		public static async Task<QueryResult[]> DeleteIngredientInteractive(string ingredientName, int ingredientId, bool msgFinished)
		{
			//Asks the user for confirmation
			if(MessageBox.Show(Languages.Resources.DelIngredConfirm.Replace("$1", ingredientName),
				Languages.Resources.ConfirmSimple, MessageBoxButton.YesNo, MessageBoxImage.Question,
				MessageBoxResult.No) == MessageBoxResult.No)
				return null;

			//Deletes the ingredient
			QueryResult[] results = await DeleteIngredient(ingredientId);

			//Shows an informational message
			if(msgFinished)
			{
				MessageBox.Show(Languages.Resources.DelQueriesInfo
						.Replace("$1", "2")
						.Replace("$2", (results[0].Success && results[1].Success).ToString(Languages.Resources.Culture))
						.Replace("$3", (results[0].RecordsAffected + results[1].RecordsAffected).ToString()),
					Languages.Resources.InfoSimple, MessageBoxButton.OK, MessageBoxImage.Information);
			}

			return results;
		}
	}
}