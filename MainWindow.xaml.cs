using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Newest_unaswered_by_tags.Properties;
using Stacky;

namespace Newest_unaswered_by_tags
{
	public partial class MainWindow : Window
	{
		QuestionsManager manager;
		Question[] questions = new Question[0];

		public MainWindow()
		{
			InitializeComponent();

			if (Settings.Default.Tags == null)
				Settings.Default.Tags = new System.Collections.Specialized.StringCollection();

			Site site = StackySite.GetSite(Settings.Default.Site);
			if (site == null)
			{
				site = Sites.StackOverflow;
				Settings.Default.Site = site.Name;
			}

			manager = new ParallelQuestionsManager(site, Settings.Default.Tags.Cast<string>());
			DataContext = questions;
			manager.QuestionsChanged += new EventHandler(manager_QuestionsChanged);
			manager.ExceptionOcurred += new EventHandler<ExceptionEventArgs>((_, e) => ShowException(e.Exception));
		}

		void manager_QuestionsChanged(object sender, EventArgs e)
		{
			questions = manager.Questions.ToArray();
			Dispatcher.Invoke((Action)(()  =>
			{
				DataContext = questions;
			}));
		}

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
		}

		private void More_Click(object sender, RoutedEventArgs e)
		{
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow window = new SettingsWindow();
			window.Tags = Settings.Default.Tags;
			window.Site = StackySite.GetSite(Settings.Default.Site);
			window.MaxPagesToLoad = Settings.Default.MaxPagesToLoad;
			if (window.ShowDialog() == true)
			{
				Settings.Default.Tags = window.Tags;
				Settings.Default.Site = window.Site.Name;
				Settings.Default.MaxPagesToLoad = window.MaxPagesToLoad;
				Settings.Default.Save();
				manager.Tags = window.Tags.Cast<string>();
				manager.Site = window.Site;
			}
		}

		public void ShowException(Exception ex)
		{
			while (ex.InnerException != null)
				ex = ex.InnerException;
			StatusBar.Text = ex.Message;
		}
	}
}