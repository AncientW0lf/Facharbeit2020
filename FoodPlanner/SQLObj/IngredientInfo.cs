using System;
using AccessCommunication;

namespace FoodPlanner.SQLObj
{
	/// <summary>
	/// Contains information for both normal and linked ingredients.
	/// </summary>
	public class IngredientInfo
	{
		/// <summary>
		/// The ID of the ingredient. Can be null if the ingredient in question hasn't been inserted into the database yet.
		/// </summary>
		public int? ID { get; set; }

		/// <summary>
		/// The name of the ingredient.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The amount needed for the linked recipe.
		/// </summary>
		public string Amount { get; set; }

		/// <summary>
		/// Can contain notes about how to prepare the ingredient, its amount, when to use it and more.
		/// </summary>
		public string Note { get; set; }

		/// <summary>
		/// Creates a new ingredient with complete manual input and no queries.
		/// </summary>
		/// <param name="id">The ID of the ingredient.</param>
		/// <param name="name">The name of the ingredient.</param>
		/// <param name="amount">The amount needed for the linked recipe.</param>
		/// <param name="note">The notes of this ingredient.</param>
		public IngredientInfo(int? id, string name, string amount = null, string note = null)
		{
			ID = id;
			Name = name;
			Amount = amount;
			Note = note;
		}

		/// <summary>
		/// Creates a new ingredient with complete manual input and no queries.
		/// </summary>
		/// <param name="name">The name of the ingredient.</param>
		/// <param name="amount">The amount needed for the linked recipe.</param>
		/// <param name="note">The notes of this ingredient.</param>
		public IngredientInfo(string name, string amount = null, string note = null)
		{
			Name = name;
			Amount = amount;
			Note = note;
		}

		/// <summary>
		/// Creates a new ingredient based on the already existing information in the database.
		/// Will not contain information as a linked ingredient.
		/// </summary>
		/// <param name="ingredientID">The ID of the already existing ingredient.</param>
		public IngredientInfo(int ingredientID)
		{
			//Gets the ingredient in the database
			QueryResult ingredient = App.ExecuteQuery(
				"select ID, Zutat from Zutaten " +
				$"where ID = {ingredientID}").GetAwaiter().GetResult();

			//Returns if an error occurred
			if(!ingredient.Success)
				return;

			//Applies the information
			ID = Convert.ToInt32(ingredient.ReturnedRows[0][0]);
			Name = ingredient.ReturnedRows[0][1].ToString();
		}

		/// <summary>
		/// Creates a new ingredient based on the already existing information in the database.
		/// </summary>
		/// <param name="ingredientID">The ID of the already existing ingredient.</param>
		/// <param name="recipeID">The linked recipe ID to use to get the link info.</param>
		public IngredientInfo(int ingredientID, int recipeID)
		{
			//Gets the ingredient in the database
			QueryResult ingredient = App.ExecuteQuery(
				"select Rezepte_ID, Zutaten_ID, Zutat, Menge, Notiz from ZusammenfassungRezeptzutatenliste " +
				$"where Zutaten_ID = {ingredientID} " +
				$"and Rezepte_ID = {recipeID}").GetAwaiter().GetResult();

			//Returns if an error occurred
			if(!ingredient.Success)
				return;

			//Applies the information
			ID = ingredientID;
			Name = ingredient.ReturnedRows[0][1].ToString();
			Amount = ingredient.ReturnedRows[0][2].ToString();
			Note = ingredient.ReturnedRows[0][3].ToString();
		}
	}
}