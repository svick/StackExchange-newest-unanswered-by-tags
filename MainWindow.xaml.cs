﻿using System;
using System.Linq;
using System.Windows;
using Newest_unaswered_by_tags.Properties;
using Stacky;

namespace Newest_unaswered_by_tags
{
	public partial class MainWindow : Window
	{
		QuestionsManager manager;

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
			DataContext = manager.Questions;
			manager.QuestionsChanged += new EventHandler(manager_QuestionsChanged);
			manager.ExceptionOcurred += new EventHandler<ExceptionEventArgs>((_, e) => ShowException(e.Exception));
		}

		void manager_QuestionsChanged(object sender, EventArgs e)
		{
		    Action action =
		        () =>
		        {
		            DataContext = manager.Questions;
		        };
		    Dispatcher.Invoke(action);
		}

        private void Mark_Click(object sender, RoutedEventArgs e)
		{
			manager.Remove(questionsGrid.SelectedItems.Cast<Question>());
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
			Dispatcher.Invoke((Action)(() => StatusBar.Text = ex.Message));
		}

        private void StopOld_Click(object sender, RoutedEventArgs e)
        {
            manager.MinDate = manager.Questions.Last().CreationDate;
        }
	}
}