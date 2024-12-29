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
		private Int64 __img_type = -1;
		private SIZE3 __data_size = new() { w = -1, h = -1, d = -1 };
		private unsafe Int32* __img_data_ptr = null;
		private unsafe Byte* __lab_data_ptr = null;


		public MainWindow() {
			InitializeComponent();

			try {
				string before_path_env = Environment.GetEnvironmentVariable("Path")??"";
				Environment.SetEnvironmentVariable("Path", before_path_env + "./bin;");
				if (Core.Initialize(null) < 0)
					throw new Exception($"Core Initialize");
			}
			catch (Exception ex) {
				Console.WriteLine($"Fail to initialize window [ {ex.Message} ]");
				Close();
			}
		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
			Core.Release();
		}

		public unsafe bool OnLoadImageData() {
			try {
				if (Core.LoadData(__img_filename, __lab_filename) < 0)
					throw new Exception($"Core LoadData | {__img_filename} | {__lab_filename}");

				Int64 t_img_type;
				SIZE3 t_img_size = new();
				Int32* t_img_data_ptr;
				Byte* t_lab_data_ptr;
				if (Core.GetDataInfo(&t_img_type, &t_img_size.w, &t_img_size.h, &t_img_size.d, &t_img_data_ptr, &t_lab_data_ptr) < 0 ||
					t_img_type <= 0 ||
					t_img_size.w <= 0 ||
					t_img_size.h <= 0 ||
					t_img_size.d <= 0 ||
					t_img_data_ptr == null ||
					t_lab_data_ptr == null)
					throw new Exception($"Core GetDataInfo | {__img_filename} | {__lab_filename}");

				__img_type = t_img_type;
				__data_size = t_img_size;
				__img_data_ptr = t_img_data_ptr;
				__lab_data_ptr= t_lab_data_ptr;


				ImageLabelViewerControl.OnLoadImageData(__img_type, __data_size, __img_data_ptr, __lab_data_ptr);


				return true;
			}
			catch (Exception ex) {
				OnUnloadImageData();
				Console.WriteLine($"Fail to load Imgae data [ {ex.Message} ]");
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
					__img_type = -1;
					__data_size = new() { w = -1, h = -1, d = -1 };
					__img_data_ptr = null;
					__lab_data_ptr= null;
				}

				{
					if (Core.UnloadData() < 0)
						throw new Exception($"Core UnloadData");
				}


				return true;
			}
			catch (Exception ex) {
				Console.WriteLine($"Fail to unload Imgae data [ {ex.Message} ]");
				return false;
			}
		}
	}
}