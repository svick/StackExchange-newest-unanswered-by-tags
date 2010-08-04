using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;
using System.Windows.Threading;

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
}