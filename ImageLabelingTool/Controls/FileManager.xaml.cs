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
		public ObservableCollection<TupleStringType> __file_names { get; set; } = [];
		private string __base_path { get; set; } = string.Empty;
		private Action<string?, string?>? __file_load_func = null;

		////////////////////////////////////////////////////////////////

		public void SetFileLoadFunc(Action<string?, string?> func) {
			__file_load_func = func;
		}

		////////////////////////////////////////////////////////////////

		public FileManager() {
			InitializeComponent();
			DataContext = this;
		}
		private void UserControl_Loaded(object sender, RoutedEventArgs e) {
			SettingManager.GetSetting("FileExplorerBasePath", out string? t_base_path);
			BasePathTextBlock.Text = __base_path = t_base_path??string.Empty;

			LoadTree(__base_path, FileExplorer.Items);
		}

		private void FileList_MouseRightButtonUp(object sender, MouseButtonEventArgs e) {
			var listBox = sender as ListBox;
			var clickedItem = listBox?.InputHitTest(e.GetPosition(FileList)) as FrameworkElement;
			if (clickedItem?.DataContext is not TupleStringType item) return;

			FileList.SelectedItem = item;
			ListBoxContextMenu.IsOpen = true;
		}
		private void FileList_LoadMenuItem_Click(object sender, RoutedEventArgs e) {
			if (FileList.SelectedItem is not TupleStringType item) return;

			if(__file_load_func == null) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load image file [ __file_load_func is null ]");
				MessageBox.Show("이미지 파일을 로드할 수 없습니다.", "이미지 로드", MessageBoxButton.OK);
				return;
			}

			var img_file_path = __file_names.FirstOrDefault(x => x.Outter == item.Outter)?.Inner;
			var ext = Path.GetExtension(img_file_path);
			var lab_file_path = img_file_path?.Replace($".image{ext}", $".label{ext}");
			__file_load_func(img_file_path, lab_file_path);
		}
		private void FileList_DeleteMenuItem_Click(object sender, RoutedEventArgs e) {
			if (FileList.SelectedItem is not TupleStringType item) return;
			__file_names.RemoveAt(__file_names.IndexOf(__file_names.FirstOrDefault(x => x.Outter == item.Outter)));
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
					item.Header is not string file_name ||
					item.Tag is not string file_path ||
					file_path == null)
					return;

				var t_item = new TupleStringType() { Outter = file_name, Inner = file_path };

				if (__file_names.Contains(t_item)) {
					MessageBox.Show("이미 추가된 파일입니다.", "파일 추가", MessageBoxButton.OK);
					return;
				}

				var result = MessageBox.Show("추가하시겠습니까?", "파일 추가", MessageBoxButton.YesNo);
				if (result != MessageBoxResult.Yes) return;

				__file_names.Add(t_item);
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
							file_info.Name.ToUpper().EndsWith(".IMAGE.MHD") ||
							file_info.Name.ToUpper().EndsWith(".IMAGE.RAW") ||
							file_info.Name.ToUpper().EndsWith(".IMAGE.PNG") ||
							file_info.Name.ToUpper().EndsWith(".IMAGE.TIF") ||
							file_info.Name.ToUpper().EndsWith(".IMAGE.TIFF")
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
