using System.Diagnostics;
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
using System.Xml;
using ImageLabelingTool.Classes;
using ImageLabelingTool.Controls;


namespace ImageLabelingTool
{
	public partial class MainWindow : Window
	{
		private readonly ImageInfoType __img_info = new();
		private string __img_filename = string.Empty;
		private string __lab_filename = string.Empty;

		////////////////////////////////////////////////////////////////

		public void LoadFile(string? img_path, string? lab_path) {
			OnUnloadImageInfo();
			__img_filename = img_path??string.Empty;
			__lab_filename = lab_path??string.Empty;
			OnLoadImageInfo();
		}
		public unsafe bool OnLoadImageInfo() {
			try {
				if (Core.LoadData(__img_filename, __lab_filename) < 0)
					throw new Exception($"Core LoadData | {__img_filename} | {__lab_filename}");

				{
					Int64 t_img_int_type;
					SIZE3 t_img_size = new();
					int* t_img_data_ptr;
					byte* t_lab_data_ptr;
					if (Core.GetDataInfo(&t_img_int_type, &t_img_size.w, &t_img_size.h, &t_img_size.d, &t_img_data_ptr, &t_lab_data_ptr) < 0 ||
						t_img_int_type < 0 ||
						t_img_size.w <= 0 ||
						t_img_size.h <= 0 ||
						t_img_size.d <= 0 ||
						t_img_data_ptr == null ||
						t_lab_data_ptr == null)
						throw new Exception($"Core GetDataInfo | {__img_filename} | {__lab_filename}");

					IMG_TYPE t_img_type = t_img_int_type switch {
						0 => IMG_TYPE.GRAY,
						1 => IMG_TYPE.COLOR,
						_ => throw new Exception($"Not Support Img Type"),
					};
					__img_info.SetImage(t_img_type, t_img_size, t_img_data_ptr, t_lab_data_ptr);
				}

				{
					ImageLabelViewerControl.OnLoadImageInfo(__img_info, OnUpdateImageInfo);
					//LeftTopContainer.OnLoadImageInfo(__img_info, OnUpdateImageInfo);
					RightTopContainer.OnLoadImageInfo(__img_info, OnUpdateImageInfo);
					RightBotContainer.OnLoadImageInfo(__img_info, OnUpdateImageInfo);
				}


				return true;
			}
			catch (Exception ex) {
				OnUnloadImageInfo();
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load Imgae data [ {ex.Message} ]");
				return false;
			}
		}
		public unsafe bool OnUnloadImageInfo() {
			try {
				{
					ImageLabelViewerControl.OnUnloadImageInfo();
					//LeftTopContainer.OnUnloadImageInfo();
					RightTopContainer.OnUnloadImageInfo();
					RightBotContainer.OnUnloadImageInfo();
				}

				{
					__img_info.ClearImage();
					__img_info.ClearNodeAll();
					__img_filename = string.Empty;
					__lab_filename = string.Empty;
				}

				if (Core.UnloadData() < 0)
					throw new Exception($"Core UnloadData");


				return true;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to unload Imgae data [ {ex.Message} ]");
				return false;
			}
			finally { GC.Collect(); }
		}
		public unsafe void OnUpdateImageInfo() {
			ImageLabelViewerControl.OnUpdateImageInfo();
			//LeftTopContainer.OnUpdateImageInfo();
			RightTopContainer.OnUpdateImageInfo();
			RightBotContainer.OnUpdateImageInfo();
		}

		////////////////////////////////////////////////////////////////

		public MainWindow() {
			InitializeComponent();
			FileManagerControl.LoadFileCallbackFunc = LoadFile;

			try {
				string before_path_env = Environment.GetEnvironmentVariable("Path")??"";
				Environment.SetEnvironmentVariable("Path", before_path_env + "./bin;");
				if (Core.Initialize(null) < 0)
					throw new Exception($"Core Initialize");
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to initialize window [ {ex.Message} ]");
				Close();
			}
		}
		private void Window_Loaded(object sender, RoutedEventArgs e) {
		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			Core.Release();
		}

		private void Button1_Click(object sender, RoutedEventArgs e) {
			OnLoadImageInfo();
			Keyboard.ClearFocus();
		}
		private void Button2_Click(object sender, RoutedEventArgs e) {
			OnUnloadImageInfo();
			Keyboard.ClearFocus();
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e) {
			if (sender is not MenuItem item) return;

			switch (item.Tag) {
				case "Load": break;
				case "Save": break;
				case "Exit":
					if (MessageBox.Show(
						"Do you want to exit?",
						"Exit",
						MessageBoxButton.YesNo,
						MessageBoxImage.Question
						) == MessageBoxResult.Yes)
						Close();
					break;
				case "Preferences": break;
				case "Test1": break;
				case "Test2": break;
				default: return;
			}
		}
	}
}