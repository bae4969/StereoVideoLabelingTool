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
using ImageLabelTool.Classes;
using ImageLabelTool.Controls;


namespace ImageLabelTool
{
	public partial class MainWindow : Window
	{
		private string __img_filename = string.Empty;
		private string __lab_filename = string.Empty;
		private IMG_TYPE __img_type = IMG_TYPE.NONE;
		private SIZE3 __data_size = new() { w = -1, h = -1, d = -1 };
		private unsafe int* __img_data_ptr = null;
		private unsafe byte* __lab_data_ptr = null;


		public unsafe bool OnLoadImageData() {
			try {
				if (Core.LoadData(__img_filename, __lab_filename) < 0)
					throw new Exception($"Core LoadData | {__img_filename} | {__lab_filename}");

				{
					Int64 t_img_type;
					SIZE3 t_img_size = new();
					int* t_img_data_ptr;
					byte* t_lab_data_ptr;
					if (Core.GetDataInfo(&t_img_type, &t_img_size.w, &t_img_size.h, &t_img_size.d, &t_img_data_ptr, &t_lab_data_ptr) < 0 ||
						t_img_type < 0 ||
						t_img_size.w <= 0 ||
						t_img_size.h <= 0 ||
						t_img_size.d <= 0 ||
						t_img_data_ptr == null ||
						t_lab_data_ptr == null)
						throw new Exception($"Core GetDataInfo | {__img_filename} | {__lab_filename}");

					switch (t_img_type) {
						case 0:	__img_type = IMG_TYPE.GRAY;		break;
						case 1:	__img_type = IMG_TYPE.COLOR;	break;
						default: throw new Exception($"Not Support Img Type");
					}
					__data_size = t_img_size;
					__img_data_ptr = t_img_data_ptr;
					__lab_data_ptr= t_lab_data_ptr;
				}

				{
					ImageLabelViewerControl.OnLoadImageData(__img_type, __data_size, __img_data_ptr, __lab_data_ptr);
				}


				return true;
			}
			catch (Exception ex) {
				OnUnloadImageData();
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load Imgae data [ {ex.Message} ]");
				return false;
			}
		}
		public unsafe bool OnUnloadImageData() {
			try {
				{
					ImageLabelViewerControl.OnUnloadImageData();
				}

				{
					__img_filename = string.Empty;
					__lab_filename = string.Empty;
					__img_type = IMG_TYPE.NONE;
					__data_size = new() { w = -1, h = -1, d = -1 };
					__img_data_ptr = null;
					__lab_data_ptr= null;
				}

				if (Core.UnloadData() < 0)
					throw new Exception($"Core UnloadData");


				return true;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to unload Imgae data [ {ex.Message} ]");
				return false;
			}
		}

		public MainWindow() {
			InitializeComponent();

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
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			Core.Release();
		}

		private void Button1_Click(object sender, RoutedEventArgs e) {
			__img_filename = "C:\\temp\\3D AXI\\Alpine\\Images\\Alpine_sample1_1point_1st_ReconResult.tif";
			__lab_filename = "C:\\temp\\3D AXI\\Alpine\\Labels\\Alpine_sample1_1point_1st_ReconResult.tif";
			OnLoadImageData();
			Keyboard.ClearFocus();
		}
		private void Button2_Click(object sender, RoutedEventArgs e) {
			Core.SaveData("./img.tif", "./lab.tif");
			Keyboard.ClearFocus();
		}
	}
}