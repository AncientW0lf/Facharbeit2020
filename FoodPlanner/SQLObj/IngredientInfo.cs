using AccessCommunication;
using BoxLib.Scripts;
using System.Diagnostics;

namespace FoodPlanner.SQLObj
{
	public class IngredientInfo
	{
		public string Name { get; set; }

		public string Amount { get; set; }

		public string Note { get; set; }

		public IngredientInfo(string name, string amount = null, string note = null)
		{
			Name = name;
			Amount = amount;
			Note = note;
		}

		public IngredientInfo(int ingredientID)
		{
			QueryResult ingredient = App.AccessDB.ExecuteQuery(
				"select ID, Zutat from Zutaten " +
				$"where ID = {ingredientID}").GetAwaiter().GetResult();
			Log.Write("Executed SQL query:", 1, TraceEventType.Information, true);
			Log.Write($"Query: {ingredient.ExecutedQuery}\nSuccess: {ingredient.Success}", 2, null);

			if(!ingredient.Success)
				return;

			Name = ingredient.ReturnedRows[0][1].ToString();
		}

		public IngredientInfo(int ingredientID, int recipeID)
		{
			QueryResult ingredient = App.AccessDB.ExecuteQuery(
				"select Rezepte_ID, Zutaten_ID, Zutat, Menge, Notiz from ZusammenfassungRezeptzutatenliste " +
				$"where Zutaten_ID = {ingredientID} " +
				$"and Rezepte_ID = {recipeID}").GetAwaiter().GetResult();
			Log.Write("Executed SQL query:", 1, TraceEventType.Information, true);
			Log.Write($"Query: {ingredient.ExecutedQuery}\nSuccess: {ingredient.Success}", 2, null);

			if(!ingredient.Success)
				return;

			Name = ingredient.ReturnedRows[0][1].ToString();
			Amount = ingredient.ReturnedRows[0][2].ToString();
			Note = ingredient.ReturnedRows[0][3].ToString();
		}
	}
}