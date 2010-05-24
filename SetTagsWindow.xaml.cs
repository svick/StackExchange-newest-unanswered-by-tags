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

namespace Newest_unaswered_by_tags
{
	/// <summary>
	/// Interaction logic for SetTagsWindow.xaml
	/// </summary>
	public partial class SetTagsWindow : Window
	{
		public SetTagsWindow()
		{
			InitializeComponent();
		}

		public string[] Tags
		{
			get;
			set;
		}

		private void Ok_Click(object sender, RoutedEventArgs e)
		{
			Tags = tagsTextBox.Text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			DialogResult = true;
		}

		private void Cancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (Tags != null)
				tagsTextBox.Text = string.Join(" ", Tags);
		}
	}
}