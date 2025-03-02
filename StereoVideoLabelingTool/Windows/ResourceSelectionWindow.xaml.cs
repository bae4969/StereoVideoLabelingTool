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
using StereoVideoLabelingTool.Classes;


namespace StereoVideoLabelingTool.Windows
{
	public partial class ResourceSelectionWindow : Window
	{
		private Dictionary<string, object>? _res_dict = null;

		public object? SelectedResource { get; private set; } = null;

		private string SelectedResourceName = string.Empty;

		////////////////////////////////////////////////////////////////



		////////////////////////////////////////////////////////////////

		public ResourceSelectionWindow() {
			InitializeComponent();
		}
		private void ThemedWindow_Loaded(object sender, RoutedEventArgs e) {

		}

		private void TaggedItemSearchControl_SelectionChanged(object sender, RoutedEventArgs e) {
			if (sender is not ListBox listBox ||
				listBox.SelectedItem is not string item_name)
				return;

			try {
				SelectedResourceName = item_name;
				if (_res_dict != null)
					;// VersionItemSerachControl.SetVersionItem(_res_dict[item_name], (item) => { return item.Ver; });
				else
					VersionItemSerachControl.ClearVersionItem();
			}
			catch { VersionItemSerachControl.ClearVersionItem(); }
		}
		private void VersionItemSerachControl_SelectionChanged(object sender, RoutedEventArgs e) {
			if (sender is not ComboBox comboBox ||
				string.IsNullOrEmpty(SelectedResourceName) ||
				comboBox.SelectedItem is not string ver_string)
				return;

			try {
				SelectedResource = null;
				if (_res_dict != null) {

				}
				else
					ResInfoControl.ClearResourceInformation();
			}
			catch { ResInfoControl.ClearResourceInformation(); }
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e) {
			DialogResult = true;
			Close();
		}
		private void CancelButton_Click(object sender, RoutedEventArgs e) {
			SelectedResource = null;
			Close();
		}
	}
}
