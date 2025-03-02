using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
	public enum PATH_SELECTION_MODE
	{
		OEPN_FOLDER,
		OPEN_FILE,
		SAVE_FILE,
	}
	public partial class PathSelectionControl : UserControl
	{
		#region DependencyProperties

		public static readonly DependencyProperty ControlNameProperty =
			DependencyProperty.Register(
				"ControlName",
				typeof(string),
				typeof(PathSelectionControl),
				new PropertyMetadata("Path")
			);
		public static readonly DependencyProperty ControlNameWidthProperty =
			DependencyProperty.Register(
				"ControlNameWidth",
				typeof(double),
				typeof(PathSelectionControl),
				new PropertyMetadata(100.0)
			);
		public static readonly DependencyProperty SelectionModeProperty =
			DependencyProperty.Register(
				"SelectionMode",
				typeof(PATH_SELECTION_MODE),
				typeof(PathSelectionControl),
				new PropertyMetadata(PATH_SELECTION_MODE.OEPN_FOLDER)
			);
		public static readonly DependencyProperty PathStringProperty =
			DependencyProperty.Register(
				"PathString",
				typeof(string),
				typeof(PathSelectionControl),
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
		public PATH_SELECTION_MODE SelectionMode
		{
			get => (PATH_SELECTION_MODE)GetValue(SelectionModeProperty);
			set => SetValue(SelectionModeProperty, value);
		}
		public string PathString
		{
			get => (string)GetValue(PathStringProperty);
			set => SetValue(PathStringProperty, value);
		}

		#endregion


		public PathSelectionControl() {
			InitializeComponent();
		}

		private void SelectButton_Click(object sender, RoutedEventArgs e) {
			switch (SelectionMode) {
				case PATH_SELECTION_MODE.OEPN_FOLDER:
					var open_folder_dialog = new OpenFolderDialog();
					if (open_folder_dialog.ShowDialog() == true) {
						PathString = open_folder_dialog.FolderName;
					}
					break;
				case PATH_SELECTION_MODE.OPEN_FILE:
					var open_file_dialog = new OpenFileDialog();
					if (open_file_dialog.ShowDialog() == true) {
						PathString = open_file_dialog.FileName;
					}
					break;
				case PATH_SELECTION_MODE.SAVE_FILE:
					var save_file_dialog = new SaveFileDialog();
					if (save_file_dialog.ShowDialog() == true) {
						PathString = save_file_dialog.FileName;
					}
					break;
			}
		}
	}
}
