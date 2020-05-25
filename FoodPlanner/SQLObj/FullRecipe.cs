using AccessCommunication;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
			QueryResult recipe = App.ExecuteQuery("select ID, Gerichtname, Zubereitung from Rezepte " +
			                                                $"where ID = {recipeID}").GetAwaiter().GetResult();

			QueryResult ingreds = App.ExecuteQuery(
				"select Rezepte_ID, Zutat, Menge, Notiz from ZusammenfassungRezeptzutatenliste " +
				$"where Rezepte_ID = {recipeID}").GetAwaiter().GetResult();

			if(!recipe.Success || !ingreds.Success)
				return;

			Name = recipe.ReturnedRows[0][1].ToString();
			Preparation = recipe.ReturnedRows[0][2].ToString();

			LinkedIngredients = new ObservableCollection<IngredientInfo>();
			for(int i = 0; i < ingreds.ReturnedRows.Count; i++)
			{
				LinkedIngredients.Add(new IngredientInfo(
					ingreds.ReturnedRows[i][1].ToString(), 
					ingreds.ReturnedRows[i][2].ToString(), 
					ingreds.ReturnedRows[i][3].ToString()));
			}
		}
	}
}