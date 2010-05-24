using System;
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

			client = new StackOverflowClient("0.8", null, HostSite.StackOverflow, urlClient, protocol);
			LoadNextPage();
		}

		private void Refresh_Click(object sender, RoutedEventArgs e)
		{
			Reload();
		}

		private void More_Click(object sender, RoutedEventArgs e)
		{
			LoadNextPage();
		}

		private void SetTags_Click(object sender, RoutedEventArgs e)
		{
			SetTagsWindow window = new SetTagsWindow();
			window.Tags = Settings.Default.Tags;
			if (window.ShowDialog() == true)
			{
				Settings.Default.Tags = window.Tags;
				Settings.Default.Save();
				Reload();
			}
		}

		void LoadNextPage()
		{
			StatusBar.Text = "Loading…";
			Task task = new Task(loadNextPageTask);
			task.ContinueWith(t => Dispatcher.Invoke((Action)loadNextPageFinished));
			task.Start();
		}

		void loadNextPageTask()
		{
			int oldCount = questions.Count;
			do
			{
				questions.AddRange(getQuestions(nextPage++));
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
			var questions = client.GetQuestions(
				sortBy: QuestionSort.UnansweredNewest,
				pageSize: 100,
				page: page);

			var tags = Settings.Default.Tags.Cast<string>().ToArray();

			if (tags.Any())
				questions = questions.Where(q => q.Tags.Any(tag => tags.Contains(tag)));

			return questions;
		}
	}
}
