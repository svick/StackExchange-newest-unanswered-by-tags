﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using StackOverflow;
using System.Threading.Tasks;
using Newest_unaswered_by_tags.Properties;

namespace Newest_unaswered_by_tags
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		List<Question> questions = new List<Question>(10);
		StackOverflowClient client;
		int nextPage = 1;

		public MainWindow()
		{
			InitializeComponent();

			if (Settings.Default.Tags == null)
			{
				Settings.Default.Tags = new System.Collections.Specialized.StringCollection();
				Settings.Default.Save();
			}

			IUrlClient urlClient = new UrlClient();
			IProtocol protocol = new JsonProtocol();

			DataContext = questions;

			client = createNewClient(Settings.Default.Site);
			LoadNextPage();
		}

		StackOverflowClient createNewClient(HostSite site)
		{
			return new StackOverflowClient("0.8", null, site, new UrlClient(), new JsonProtocol());
		}

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
			Reload();
		}

		private void More_Click(object sender, RoutedEventArgs e)
		{
			LoadNextPage();
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow window = new SettingsWindow();
			window.Tags = Settings.Default.Tags;
			window.Site = Settings.Default.Site;
			window.MaxPagesToLoad = Settings.Default.MaxPagesToLoad;
			if (window.ShowDialog() == true)
			{
				Settings.Default.Tags = window.Tags;
				Settings.Default.Site = window.Site;
				Settings.Default.MaxPagesToLoad = window.MaxPagesToLoad;
				Settings.Default.Save();
				client = createNewClient(Settings.Default.Site);
				Reload();
			}
		}

		void LoadNextPage()
		{
			StatusBar.Text = "Loading…";
			Task task = new Task(loadNextPageTask);
			task.ContinueWith(handleTaskException, TaskContinuationOptions.OnlyOnFaulted);
			task.ContinueWith(t => Dispatcher.Invoke((Action)loadNextPageFinished), TaskContinuationOptions.NotOnFaulted);
			task.Start();
		}

		void handleTaskException(Task task)
		{
			Dispatcher.Invoke((Action<Exception>)ShowException, task.Exception);
		}

		void loadNextPageTask()
		{
			int oldCount = questions.Count;
			int i = 0;
			do
			{
				questions.AddRange(getQuestions(nextPage++));
				if (++i >= Settings.Default.MaxPagesToLoad)
					break;
			} while (questions.Count == oldCount);
		}

		void loadNextPageFinished()
		{
			questionsGrid.Items.Refresh();
			StatusBar.Text = "";
		}

		void Reload()
		{
			questions.Clear();
			nextPage = 1;
			LoadNextPage();
		}

		IEnumerable<Question> getQuestions(int page)
		{
			IEnumerable<Question> questions = client.GetQuestions(
				sortBy: QuestionSort.UnansweredNewest,
				pageSize: 100,
				page: page);

			var tags = Settings.Default.Tags.Cast<string>().ToArray();

			if (tags.Any())
				questions = questions.Where(q => q.Tags.Any(tag => tags.Contains(tag)));

			return questions;
		}

		public void ShowException(Exception ex)
		{
			while (ex.InnerException != null)
				ex = ex.InnerException;
			StatusBar.Text = ex.Message;
		}
	}
}
