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
using System.Windows.Shapes;
using System.Collections.Specialized;

namespace Newest_unaswered_by_tags
{
	/// <summary>
	/// Interaction logic for SetTagsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{
		public SettingsWindow()
		{
			InitializeComponent();
		}

		public StackOverflow.HostSite Site
		{
			get;
			set;
		}
		public StringCollection Tags
		{
			get;
			set;
		}
		public int MaxPagesToLoad
		{
			get;
			set;
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			ok();
		}

		void ok()
		{
			Tags.Clear();
			Tags.AddRange(tagsTextBox.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

			Site = (StackOverflow.HostSite)siteComboBox.SelectedItem;

			int pages;
			if (int.TryParse(maxPagesToLoadTextBox.Text, out pages))
				MaxPagesToLoad = pages;

			DialogResult = true;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			siteComboBox.ItemsSource = Enum.GetValues(typeof(StackOverflow.HostSite));
			siteComboBox.SelectedItem = Site;

			tagsTextBox.Text = string.Join(" ", Tags.Cast<string>());

			maxPagesToLoadTextBox.Text = MaxPagesToLoad.ToString();
		}
	}
}