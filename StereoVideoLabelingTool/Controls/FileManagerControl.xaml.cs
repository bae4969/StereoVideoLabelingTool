using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Xml.Linq;
using Microsoft.Win32;
using Path = System.IO.Path;
using System.ComponentModel;
using StereoVideoLabelingTool.Classes;

namespace StereoVideoLabelingTool.Controls
{
	public partial class FileManagerControl : UserControl, INotifyPropertyChanged
	{
		#region DependencyProperties

		public event EventHandler<string> SelectedFIle;

		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		private string _base_path { get; set; } = string.Empty;
		public string BasePath
		{
			get => _base_path;
			set { _base_path = value; OnPropertyChanged(null); }
		}

		#endregion

		////////////////////////////////////////////////////////////////

		private void LoadTree(string? dir_path, ItemCollection base_items) {
			if (string.IsNullOrEmpty(dir_path) || !Directory.Exists(dir_path)) return;
			try {
				base_items.Clear();

				foreach (var directory in Directory.GetDirectories(dir_path)) {
					var dir_info = new DirectoryInfo(directory);
					if (dir_info.Name.Equals("$RECYCLE.BIN", StringComparison.CurrentCultureIgnoreCase) ||
						dir_info.Name.Equals("SYSTEM VOLUME INFOMATION", StringComparison.CurrentCultureIgnoreCase) ||
						dir_info.Name[0] == '.')
						continue;

					var subItem = new TreeViewItem {
						Header = dir_info.Name,
						Tag = dir_info.FullName,
					};

					subItem.Items.Add(null);
					subItem.Expanded += FileExplorer_Folder_Expanded;

					base_items.Add(subItem);
				}

				foreach (var file in Directory.GetFiles(dir_path)) {
					var file_info = new FileInfo(file);

					if (file_info.FullName.EndsWith(".label.stereo.video.xml") is not true) continue;

					var filename_only = file_info.FullName.Replace(".label.stereo.video.xml", "");

					var subItem = new TreeViewItem {
						Header = filename_only,
					};

					base_items.Add(subItem);
				}
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load 'TreeViewState' [ {ex.Message} ]");
			}
		}

		////////////////////////////////////////////////////////////////

		public FileManagerControl() {
			InitializeComponent();
		}
		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			GlobalSettingManager.GetSetting("FileExplorer", "BasePath", out string? t_base_path);
			BasePath = t_base_path??string.Empty;

			LoadTree(_base_path, FileExplorer.Items);
		}

		private void ChangeBasePathButton_Click(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog {
				Filter = "Folder|*.none",
				InitialDirectory = _base_path,
				CheckFileExists = false,
				CheckPathExists = true,
				FileName = "Select folder"
			};
			if (dialog.ShowDialog() != true) return;
			BasePath = Path.GetDirectoryName(dialog.FileName);
			GlobalSettingManager.SetSetting("FileExplorer", "BasePath", _base_path);
			LoadTree(_base_path, FileExplorer.Items);
		}
		private void FileExplorer_Folder_Expanded(object sender, RoutedEventArgs e) {
			try {
				if (sender is not TreeViewItem item ||
					item.Items[0] != null)
					return;

				LoadTree(item.Tag as string, item.Items);
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to process 'FolderExpanded' event for 'FileExplorer' control [ {ex.Message} ]");
			}
		}
		private void FileExplorer_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
			var clickedElement = e.OriginalSource as DependencyObject;
			while (clickedElement != null && clickedElement is not TreeViewItem)
				clickedElement = VisualTreeHelper.GetParent(clickedElement);

			if (clickedElement is not TreeViewItem tree_view_item) return;
			tree_view_item.IsSelected = true;
		}
		private void FileExplorer_LoadMenuItem_Click(object sender, RoutedEventArgs e) {
			if (FileExplorer.SelectedItem is not TreeViewItem tree_view_item ||
				tree_view_item.Tag is not string filename_only)
				return;

			SelectedFIle?.Invoke(this, filename_only);
		}
	}
}
