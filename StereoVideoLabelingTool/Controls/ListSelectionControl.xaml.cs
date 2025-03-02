using StereoVideoLabelingTool.Widgets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StereoVideoLabelingTool.Controls
{
	public partial class ListSelectionControl : UserControl
	{
		#region DependencyProperties

		public static readonly DependencyProperty ControlNameProperty =
			DependencyProperty.Register(
				"ControlName",
				typeof(string),
				typeof(ListSelectionControl),
				new PropertyMetadata("List")
			);
		public static readonly DependencyProperty ControlNameWidthProperty =
			DependencyProperty.Register(
				"ControlNameWidth",
				typeof(double),
				typeof(ListSelectionControl),
				new PropertyMetadata(100.0)
			);
		public static readonly DependencyProperty ItemsProperty =
			DependencyProperty.Register(
				"Items",
				typeof(ObservableCollection<string>),
				typeof(ListSelectionControl),
				new PropertyMetadata(null)
			);
		public string ControlName
		{
			get => (string)GetValue(ControlNameProperty);
			set => SetValue(ControlNameProperty, value);
		}
		public double ControlNameWidth
		{
			get => (double)GetValue(ControlNameWidthProperty);
			set => SetValue(ControlNameWidthProperty, value);
		}
		public ObservableCollection<string> Items
		{
			get => (ObservableCollection<string>)GetValue(ItemsProperty);
			set => SetValue(ItemsProperty, value);
		}
		public string SelectedString
		{
			get => _selected_string;
			set => ItemListBox.SelectedIndex = Items.IndexOf(value);
		}

		#endregion

		////////////////////////////////////////////////////////////////

		private string _selected_string = string.Empty;

		////////////////////////////////////////////////////////////////

		public ListSelectionControl() {
			InitializeComponent();
			Items ??= [];
		}

		private void ItemListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			if (ItemListBox.SelectedIndex < 0)
				return;

			_selected_string = ItemListBox.SelectedItem.ToString()??string.Empty;
		}
	}
}
