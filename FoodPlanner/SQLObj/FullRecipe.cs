using AccessCommunication;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FoodPlanner.SQLObj
{
	public class FullRecipe
	{
		public int? ID { get; set; }

		public string Name { get; set; }

		public string Preparation { get; set; }

		public ObservableCollection<IngredientInfo> LinkedIngredients { get; set; }

		public FullRecipe(int? id, string name, string preparation, IEnumerable<IngredientInfo> ingredients)
		{
			ID = id;
			Name = name;
			Preparation = preparation;
			LinkedIngredients = new ObservableCollection<IngredientInfo>(ingredients);
		}

		public FullRecipe(string name, string preparation, IEnumerable<IngredientInfo> ingredients)
		{
			Name = name;
			Preparation = preparation;
			LinkedIngredients = new ObservableCollection<IngredientInfo>(ingredients);
		}

		public FullRecipe(int recipeID)
		{
			QueryResult recipe = App.ExecuteQuery("select ID, Gerichtname, Zubereitung from Rezepte " +
			                                                $"where ID = {recipeID}").GetAwaiter().GetResult();

			QueryResult ingreds = App.ExecuteQuery(
				"select Rezepte_ID, Zutat, Menge, Notiz, Zutaten_ID from ZusammenfassungRezeptzutatenliste " +
				$"where Rezepte_ID = {recipeID}").GetAwaiter().GetResult();

			if(!recipe.Success || !ingreds.Success)
				return;

			ID = recipeID;
			Name = recipe.ReturnedRows[0][1].ToString();
			Preparation = recipe.ReturnedRows[0][2].ToString();

			LinkedIngredients = new ObservableCollection<IngredientInfo>();
			for(int i = 0; i < (ingreds.ReturnedRows?.Count ?? 0); i++)
			{
				LinkedIngredients.Add(new IngredientInfo(
					ingreds.ReturnedRows[i][4] as int?,
					ingreds.ReturnedRows[i][1].ToString(), 
					ingreds.ReturnedRows[i][2].ToString(), 
					ingreds.ReturnedRows[i][3].ToString()));
			}
		}
	}
}