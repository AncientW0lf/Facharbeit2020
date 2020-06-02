using System;
using AccessCommunication;

namespace FoodPlanner.SQLObj
{
	public class IngredientInfo
	{
		public int? ID { get; set; }

		public string Name { get; set; }

		public string Amount { get; set; }

		public string Note { get; set; }

		public IngredientInfo(int? id, string name, string amount = null, string note = null)
		{
			ID = id;
			Name = name;
			Amount = amount;
			Note = note;
		}

		public IngredientInfo(string name, string amount = null, string note = null)
		{
			Name = name;
			Amount = amount;
			Note = note;
		}

		public IngredientInfo(int ingredientID)
		{
			QueryResult ingredient = App.ExecuteQuery(
				"select ID, Zutat from Zutaten " +
				$"where ID = {ingredientID}").GetAwaiter().GetResult();

			if(!ingredient.Success)
				return;

			ID = Convert.ToInt32(ingredient.ReturnedRows[0][0]);
			Name = ingredient.ReturnedRows[0][1].ToString();
		}

		public IngredientInfo(int ingredientID, int recipeID)
		{
			QueryResult ingredient = App.ExecuteQuery(
				"select Rezepte_ID, Zutaten_ID, Zutat, Menge, Notiz from ZusammenfassungRezeptzutatenliste " +
				$"where Zutaten_ID = {ingredientID} " +
				$"and Rezepte_ID = {recipeID}").GetAwaiter().GetResult();

			if(!ingredient.Success)
				return;

			ID = ingredientID;
			Name = ingredient.ReturnedRows[0][1].ToString();
			Amount = ingredient.ReturnedRows[0][2].ToString();
			Note = ingredient.ReturnedRows[0][3].ToString();
		}
	}
}