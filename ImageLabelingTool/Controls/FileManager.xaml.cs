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
	public partial class FileManager : UserControl
	{
		public ObservableCollection<string> __file_names { get; set; } = [];
		public string __base_path { get; set; } = string.Empty;


		public FileManager() {
			InitializeComponent();
			DataContext = this;
		}
		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			SettingManager.GetSetting("FileExplorerBasePath", out string? t_base_path);
			BasePathTextBlock.Text = __base_path = t_base_path??string.Empty;

			LoadTree(__base_path, FileExplorer.Items);
		}

		private void FileList_ContextMenuOpening(object sender, ContextMenuEventArgs e) {
			//var contextMenu = new ContextMenu();

			//var loadMenuItem = new MenuItem { Header = "로드" };
			//loadMenuItem.Click += (s, args) => LoadFile(((ListBox)sender).SelectedItem as string);

			//var removeMenuItem = new MenuItem { Header = "목록에서 제외하기" };
			//removeMenuItem.Click += (s, args) => __file_names.Remove(((ListBox)sender).SelectedItem as string);

			//contextMenu.Items.Add(loadMenuItem);
			//contextMenu.Items.Add(removeMenuItem);

			//((ListBox)sender).ContextMenu = contextMenu;
		}

		private void ChangeBasePathButton_Click(object sender, RoutedEventArgs e) {
			var dialog = new OpenFileDialog {
				Filter = "폴더|*.none",
				InitialDirectory = __base_path,
				CheckFileExists = false,
				CheckPathExists = true,
				FileName = "폴더 선택"
			};
			if (dialog.ShowDialog() != true) return;
			BasePathTextBlock.Text = __base_path = Path.GetDirectoryName(dialog.FileName);
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
		private void FileExplorer_File_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			try {
				if (e.ChangedButton != MouseButton.Left ||
					sender is not TreeViewItem item ||
					item.Tag is not string file_path ||
					file_path == null)
					return;

				if (__file_names.Contains(file_path)) {
					MessageBox.Show("이미 추가된 파일입니다.", "파일 추가", MessageBoxButton.OK);
					return;
				}

				var result = MessageBox.Show("추가하시겠습니까?", "파일 추가", MessageBoxButton.YesNo);
				if (result != MessageBoxResult.Yes) return;

				__file_names.Add(file_path);
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to process 'MouseDoubleClick' event for 'FileExplorer' control [ {ex.Message} ]");
			}
		}


		private void LoadTree(string? dir_path, ItemCollection base_items) {
			try {
				base_items.Clear();
				if(string.IsNullOrEmpty(dir_path) || !Directory.Exists(dir_path)) return;

				foreach (var directory in Directory.GetDirectories(dir_path)) {
					var dir_info = new DirectoryInfo(directory);
					if (dir_info.Name.Equals("$RECYCLE.BIN", StringComparison.CurrentCultureIgnoreCase) ||
						dir_info.Name.Equals("SYSTEM VOLUME INFOMATION", StringComparison.CurrentCultureIgnoreCase) ||
						dir_info.Name[0] == '.')
						continue;

					var subItem = new TreeViewItem {
						Header = dir_info.Name,
						Tag = directory
					};

					subItem.Items.Add(null);
					subItem.Expanded += FileExplorer_Folder_Expanded;

					base_items.Add(subItem);
				}

				foreach (var file in Directory.GetFiles(dir_path)) {
					var file_info = new FileInfo(file);
					if (!(
							file_info.Extension.Equals(".MHD", StringComparison.CurrentCultureIgnoreCase) ||
							file_info.Extension.Equals(".RAW", StringComparison.CurrentCultureIgnoreCase) ||
							file_info.Extension.Equals(".PNG", StringComparison.CurrentCultureIgnoreCase) ||
							file_info.Extension.Equals(".TIF", StringComparison.CurrentCultureIgnoreCase) ||
							file_info.Extension.Equals(".TIFF", StringComparison.CurrentCultureIgnoreCase)
						) ||
						file_info.Name[0] == '.')
						continue;

					var subItem = new TreeViewItem {
						Header = file_info.Name,
						Tag = file
					};

					subItem.MouseDoubleClick += FileExplorer_File_MouseDoubleClick;

					base_items.Add(subItem);
				}
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load 'TreeViewState' [ {ex.Message} ]");
			}
		}
	}
}
