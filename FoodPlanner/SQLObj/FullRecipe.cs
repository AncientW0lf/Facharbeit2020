using AccessCommunication;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FoodPlanner.SQLObj
{
	/// <summary>
	/// This class contains the information for a full recipe with linked ingredients.
	/// </summary>
	public class FullRecipe
	{
		/// <summary>
		/// The ID of the recipe. Can be null if the recipe in question hasn't been inserted into the database yet.
		/// </summary>
		public int? ID { get; set; }

		/// <summary>
		/// The full name of the recipe.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The full text on how to prepare the dish.
		/// </summary>
		public string Preparation { get; set; }

		/// <summary>
		/// A list of all ingredients which are included and should be bought for this recipe.
		/// </summary>
		public ObservableCollection<IngredientInfo> LinkedIngredients { get; set; }

		/// <summary>
		/// Creates a new recipe with complete manual input and no queries.
		/// </summary>
		/// <param name="id">The ID of the recipe.</param>
		/// <param name="name">The name of the recipe.</param>
		/// <param name="preparation">The full text on how to prepare the dish.</param>
		/// <param name="ingredients">An enumerable of linked ingredients.</param>
		public FullRecipe(int? id, string name, string preparation, IEnumerable<IngredientInfo> ingredients)
		{
			ID = id;
			Name = name;
			Preparation = preparation;
			LinkedIngredients = new ObservableCollection<IngredientInfo>(ingredients);
		}

		/// <summary>
		/// Creates a new recipe with complete manual input and no queries.
		/// </summary>
		/// <param name="name">The name of the recipe.</param>
		/// <param name="preparation">The full text on how to prepare the dish.</param>
		/// <param name="ingredients">An enumerable of linked ingredients.</param>
		public FullRecipe(string name, string preparation, IEnumerable<IngredientInfo> ingredients)
		{
			Name = name;
			Preparation = preparation;
			LinkedIngredients = new ObservableCollection<IngredientInfo>(ingredients);
		}

		/// <summary>
		/// Creates a new recipe based on the info already present in the database.
		/// </summary>
		/// <param name="recipeID">The ID of the already present recipe.</param>
		public FullRecipe(int recipeID)
		{
			//Gets the specified recipe
			QueryResult recipe = App.ExecuteQuery("select ID, Gerichtname, Zubereitung from Rezepte " +
			                                                $"where ID = {recipeID}").GetAwaiter().GetResult();

			//Gets the linked ingredients of the recipe
			QueryResult ingreds = App.ExecuteQuery(
				"select Rezepte_ID, Zutat, Menge, Notiz, Zutaten_ID from ZusammenfassungRezeptzutatenliste " +
				$"where Rezepte_ID = {recipeID}").GetAwaiter().GetResult();

			//Returns if an error occurred
			if(!recipe.Success || !ingreds.Success)
				return;

			//Applies the fetched information
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