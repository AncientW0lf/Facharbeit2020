using System;
using System.Globalization;

namespace FoodPlanner
{
	/// <summary>
	/// A static class to expand on the methods of other types and objects.
	/// </summary>
	public static class Extensions
	{
		/// <summary>
		/// Retrieves the path to the corresponding flag image.
		/// </summary>
		/// <param name="culture">The culture to get a flag from.</param>
		public static Uri GetFlagLocation(this CultureInfo culture)
		{
			if(culture.Equals(CultureInfo.GetCultureInfo("en-us")))
				return new Uri(@"\Resources\Flags\united-states.png", UriKind.Relative);

			if(culture.Equals(CultureInfo.GetCultureInfo("de-de")))
				return new Uri(@"\Resources\Flags\germany.png", UriKind.Relative);

			return new Uri(@"\Resources\Flags\unknown.png", UriKind.Relative);
		}
	}
}
