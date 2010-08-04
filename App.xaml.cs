using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;
using Newest_unaswered_by_tags.Properties;

namespace Newest_unaswered_by_tags
{
	public partial class App : Application
	{
		public App()
		{
			FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
		}

		void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(e.Uri.ToString());
		}

		private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			Exception ex = e.Exception;
			((MainWindow)MainWindow).ShowException(ex);
		}
	}

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