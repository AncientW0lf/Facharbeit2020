using System;
using System.Globalization;

namespace FoodPlanner
{
	public static class Extensions
	{
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
