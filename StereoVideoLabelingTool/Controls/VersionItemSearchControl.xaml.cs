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
using StereoVideoLabelingTool.Classes;
using UserControl = System.Windows.Controls.UserControl;

namespace StereoVideoLabelingTool.Controls
{
	public partial class VersionItemSearchControl : UserControl
	{
		#region DependencyProperties

		public event RoutedEventHandler SelectionChanged;

		#endregion

		private readonly List<string> _keys = new();

		////////////////////////////////////////////////////////////////

		public void SetVersionItem<ItemType>(List<ItemType> item_list, Func<ItemType, VersionType> ver_func) {
			VersionSearchComboBox.ItemsSource = null;
			_keys.Clear();

			if (item_list != null && item_list.Count > 0) {
				foreach (var item in item_list) {
					var version = ver_func(item);
					var key = version.ToVerString();
					_keys.Add(key);
				}
				_keys.Sort((a, b) => { return -a.CompareTo(b); });
			}

			VersionSearchComboBox.ItemsSource = _keys;
			if (_keys.Count > 0)
				VersionSearchComboBox.SelectedIndex = 0;
		}
		public void ClearVersionItem() {
			VersionSearchComboBox.ItemsSource = null;
			_keys.Clear();
			VersionSearchComboBox.ItemsSource = _keys;
		}

		////////////////////////////////////////////////////////////////

		public VersionItemSearchControl() {
			InitializeComponent();
		}

		private void VersionSearchComboBox_SelectedIndexChanged(object sender, RoutedEventArgs e) {
			SelectionChanged?.Invoke(sender, e);
		}
	}
}
