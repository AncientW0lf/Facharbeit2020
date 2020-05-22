using System.Windows;
using FoodPlanner.SQLObj;

namespace FoodPlanner.AdditionalWindows
{
	/// <summary>
	/// Interaction logic for CreateRecipeWin.xaml
	/// </summary>
	public partial class CreateRecipeWin : Window
	{
		public FullRecipe Recipe { get; private set; }

		public CreateRecipeWin(int? recipeID = null)
		{
			InitializeComponent();
			DataContext = recipeID != null 
				? new FullRecipe(recipeID.Value) 
				: new FullRecipe(Languages.Resources.NewRecipeSimple, string.Empty, new IngredientInfo[0]);
		}

		private void ConfirmRecipe(object sender, RoutedEventArgs e)
		{
			DialogResult = true;
			Recipe = (FullRecipe)DataContext;
			Close();
		}
	}
}
