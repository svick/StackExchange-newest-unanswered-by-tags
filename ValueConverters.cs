using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Data;
using Newest_unaswered_by_tags.Properties;

namespace Newest_unaswered_by_tags
{
	[ValueConversion(typeof(long), typeof(Uri))]
	public class IdToUrlConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			int id = (int)value;

			return new Uri(StackySite.GetSite(Settings.Default.Site).SiteUrl + "/questions/" + id.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}

	[ValueConversion(typeof(IEnumerable<object>), typeof(string))]
	public class EnumerableToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return string.Join(" ", ((IEnumerable<object>)value).Select(o => o.ToString()));
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			return null;
		}
	}
}