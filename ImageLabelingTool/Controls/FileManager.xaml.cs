using ImageLabelingTool.Classes;
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

namespace ImageLabelingTool.Controls
{
	public partial class FileManager : UserControl, INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;
		public void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

		private string __base_path { get; set; } = string.Empty;
		public string BasePath
		{
			get => __base_path;
			set { __base_path = value; OnPropertyChanged(null); }
		}

		public Action<string?, string?>? LoadFileCallbackFunc = null;

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

					var t_filename = Path.GetFileNameWithoutExtension(file_info.FullName);
					var ext_1 = Path.GetExtension(t_filename);
					var ext_2 = Path.GetExtension(file_info.FullName);
					if (!ext_1.ToLower().EndsWith(".image") ||
						!(
							ext_2.ToLower().EndsWith(".mhd") ||
							ext_2.ToLower().EndsWith(".raw") ||
							ext_2.ToLower().EndsWith(".png") ||
							ext_2.ToLower().EndsWith(".tif") ||
							ext_2.ToLower().EndsWith(".tiff")
						) ||
						file_info.Name[0] == '.')
						continue;

					var img_filename = file_info.FullName;
					var lab_filename = img_filename.Replace(".image", ".label");
					if (!File.Exists(lab_filename)) lab_filename = string.Empty;

					var subItem = new TreeViewItem {
						Header = file_info.Name,
						Background = new SolidColorBrush(string.IsNullOrEmpty(lab_filename) ? Color.FromRgb(0x2F, 0x1F, 0x1F) : Color.FromRgb(0x1F, 0x2F, 0x1F)),
						Tag = new Tuple<string, string>(img_filename, lab_filename)
					};

					base_items.Add(subItem);
				}
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load 'TreeViewState' [ {ex.Message} ]");
			}
		}

		////////////////////////////////////////////////////////////////

		public FileManager() {
			InitializeComponent();
			DataContext = this;
		}
		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			SettingManager.GetSetting("FileExplorerBasePath", out string? t_base_path);
			BasePath = t_base_path??string.Empty;

			LoadTree(__base_path, FileExplorer.Items);
		}

		private void ChangeBasePathButton_Click(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog {
				Filter = "Folder|*.none",
				InitialDirectory = __base_path,
				CheckFileExists = false,
				CheckPathExists = true,
				FileName = "Select folder"
			};
			if (dialog.ShowDialog() != true) return;
			BasePath = Path.GetDirectoryName(dialog.FileName);
			SettingManager.SetSetting("FileExplorerBasePath", __base_path);
			LoadTree(__base_path, FileExplorer.Items);
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
				tree_view_item.Tag  is not Tuple<string, string> img_file_path)
				return;

			if (LoadFileCallbackFunc == null) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load image file [ __file_load_func is null ]");
				MessageBox.Show("Fail to load image file", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			LoadFileCallbackFunc(img_file_path.Item1, img_file_path.Item2);
		}
	}
}
