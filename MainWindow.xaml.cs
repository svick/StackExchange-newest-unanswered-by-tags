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

namespace Newest_unaswered_by_tags
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		const string fileName = "tags.txt";

		string[] tags;
		string[] Tags
		{
			get
			{
				return tags;
			}
			set
			{
				tags = value;
				File.WriteAllLines(fileName, tags);
			}
		}

		List<Question> questions = new List<Question>(10);
		StackOverflowClient client;
		int nextPage = 1;

		public MainWindow()
		{
			InitializeComponent();

			if (File.Exists(fileName))
				Tags = File.ReadAllLines(fileName);

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
			window.Tags = Tags;
			if (window.ShowDialog() == true)
			{
				Tags = window.Tags;
				Reload();
			}
		}

		void LoadNextPage()
		{
			questions.AddRange(getQuestions(nextPage++));
			questionsGrid.Items.Refresh();
		}

		void Reload()
		{
			questions.Clear();
			nextPage = 1;
			LoadNextPage();
		}

		IEnumerable<Question> getQuestions(int page)
		{
			return client.GetQuestions(
				sortBy: QuestionSort.UnansweredNewest,
				pageSize: 100,
				page: page).Where(q => q.Tags.Any(tag => Tags.Contains(tag)));
		}
	}
}
