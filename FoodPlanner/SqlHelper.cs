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
	public static class SqlHelper
	{
		public static async Task<QueryResult[]> InsertNewRecipe(FullRecipe recipe)
		{
			List<QueryResult> allResults = null;
			try
			{
				allResults = new List<QueryResult>
				{
					await App.ExecuteQuery(
						"insert into Rezepte(Gerichtname, Zubereitung) " +
						$"values('{recipe.Name}', '{recipe.Preparation}')"),
					await App.ExecuteQuery("select ID from Rezepte " +
					                       $"where Gerichtname = '{recipe.Name}' " +
					                       $"and Zubereitung = '{recipe.Preparation}'")
				};

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

		public static async Task<QueryResult[]> InsertNewRecipeInteractive(bool msgFinished)
		{
			var win = new CreateRecipeWin();
			win.ShowDialog();

			if(win.DialogResult != true)
				return null;

			QueryResult[] allResults = await InsertNewRecipe(win.Recipe);

			if(!msgFinished) return allResults;

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

		public static async Task<QueryResult[]> UpdateRecipe(FullRecipe recipe)
		{
			List<QueryResult> allResults = null;
			try
			{
				allResults = new List<QueryResult>
				{
					await App.ExecuteQuery(
						"update Rezepte " +
						$"set Gerichtname = '{recipe.Name}', Zubereitung = '{recipe.Preparation}' " +
						$"where ID = {recipe.ID}"),
					await App.ExecuteQuery($"delete from Rezeptzutatenliste where IDRezepte = {recipe.ID}")
				};

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

		public static async Task<QueryResult[]> UpdateRecipeInteractive(int recipeId, bool msgFinished)
		{
			var win = new CreateRecipeWin(recipeId);
			win.ShowDialog();

			if(win.DialogResult != true)
				return null;

			QueryResult[] allResults = await UpdateRecipe(win.Recipe);

			if(!msgFinished) return allResults;

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

		public static async Task<QueryResult[]> DeleteRecipeInteractive(string recipeName, int recipeId, bool msgFinished)
		{
			if(MessageBox.Show(Languages.Resources.DelRecipeConfirm.Replace("$1", recipeName),
				Languages.Resources.ConfirmSimple, MessageBoxButton.YesNo, MessageBoxImage.Question,
				MessageBoxResult.No) == MessageBoxResult.No)
				return null;

			QueryResult[] results = await DeleteRecipe(recipeId);

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

		public static async Task<QueryResult?> InsertNewIngredientInteractive(bool msgFinished)
		{
			var win = new StringInputWin("New ingredient", "Enter the name of your new ingredient:", "New ingredient");
			bool? res = win.ShowDialog();

			if(res != true)
				return null;

			QueryResult? result = await InsertNewIngredient(win.Result);

			if(!msgFinished) return result;

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

		public static async Task<QueryResult?> UpdateIngredientInteractive(int ingredientId, string oldName, bool msgFinished)
		{
			var win = new StringInputWin("Update ingredient", "Enter the new name of the ingredient:", oldName);
			bool? res = win.ShowDialog();

			if(res != true)
				return null;

			QueryResult? result = await UpdateIngredient(new IngredientInfo(ingredientId, win.Result));

			if(!msgFinished) return result;

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

		public static async Task<QueryResult[]> DeleteIngredientInteractive(string ingredientName, int ingredientId, bool msgFinished)
		{
			if(MessageBox.Show(Languages.Resources.DelIngredConfirm.Replace("$1", ingredientName),
				Languages.Resources.ConfirmSimple, MessageBoxButton.YesNo, MessageBoxImage.Question,
				MessageBoxResult.No) == MessageBoxResult.No)
				return null;

			QueryResult[] results = await DeleteIngredient(ingredientId);

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