using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using AccessCommunication;
using BoxLib.Scripts;

namespace FoodPlanner.SQLObj
{
	public class FullRecipe
	{
		public string Name { get; set; }

		public string Preparation { get; set; }

		public ObservableCollection<IngredientInfo> LinkedIngredients { get; set; }

		public FullRecipe(string name, string preparation, IEnumerable<IngredientInfo> ingredients)
		{
			Name = name;
			Preparation = preparation;
			LinkedIngredients = new ObservableCollection<IngredientInfo>(ingredients);
		}

		public FullRecipe(int recipeID)
		{
			QueryResult recipe = App.AccessDB.ExecuteQuery("select ID, Gerichtname, Zubereitung from Rezepte " +
			                                                $"where ID = {recipeID}").GetAwaiter().GetResult();
			Log.Write("Executed SQL query:", 1, TraceEventType.Information, true);
			Log.Write($"Query: {recipe.ExecutedQuery}\nSuccess: {recipe.Success}", 2, null);

			QueryResult ingreds = App.AccessDB.ExecuteQuery(
				"select Rezepte_ID, Zutat, Menge, Notiz from ZusammenfassungRezeptzutatenliste " +
				$"where Rezepte_ID = {recipeID}").GetAwaiter().GetResult();
			Log.Write("Executed SQL query:", 1, TraceEventType.Information, true);
			Log.Write($"Query: {ingreds.ExecutedQuery}\nSuccess: {ingreds.Success}", 2, null);

			if(!recipe.Success || !ingreds.Success)
				return;

			Name = recipe.ReturnedRows[0][1].ToString();
			Preparation = recipe.ReturnedRows[0][2].ToString();

			LinkedIngredients = new ObservableCollection<IngredientInfo>();
			for(int i = 0; i < ingreds.ReturnedRows.Count; i++)
			{
				LinkedIngredients.Add(new IngredientInfo
				{
					Name = ingreds.ReturnedRows[i][1].ToString(),
					Amount = ingreds.ReturnedRows[i][2].ToString(),
					Note = ingreds.ReturnedRows[i][3].ToString()
				});
			}
		}
	}
}