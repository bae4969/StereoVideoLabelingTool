using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
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
using ImageLabelTool.Classes;

namespace ImageLabelTool.Controls
{
	public partial class ImageLabelViewer : ImageControlBase
	{
		private readonly Int32[] __lab_color_map = new Int32[256];
		private Int32[]? __top_view_data = null;
		private Int32[]? __side_view_data = null;
		private WriteableBitmap? __top_view_bitmap = null;
		private WriteableBitmap? __side_view_bitmap = null;

		private Int64 __img_type = -1;
		private SIZE3 __data_size = new() { w = -1, h = -1, d = -1, };
		private unsafe Int32* __img_data_ptr = null;
		private unsafe Byte* __lab_data_ptr = null;


		public ImageLabelViewer() {
			InitializeComponent();

			Random random = new(4969);
			for (int i = 0; i < __lab_color_map.Length; i++) {
				int b = i % 3 != 0 ? random.Next(130, 230) : 0;
				int g = i % 3 != 1 ? random.Next(130, 230) : 0;
				int r = i % 3 != 2 ? random.Next(130, 230) : 0;
				__lab_color_map[i] = (Int32)((b << 24) | (g << 16) | (r << 8) | 0xFF);
			}
			__lab_color_map[0] = 0xFF;
		}

		public override unsafe void OnLoadImageData(Int64 type, SIZE3 size, Int32* img_data_ptr, Byte* lab_data_ptr) {
			try {
				__img_type = type;
				__data_size = size;
				__img_data_ptr = img_data_ptr;
				__lab_data_ptr = lab_data_ptr;
				__top_view_data = new Int32[size.w * size.h];
				__side_view_data = new Int32[size.w * size.d];
				__top_view_bitmap = new WriteableBitmap((int)__data_size.w, (int)__data_size.h, 96, 96, PixelFormats.Bgra32, null);
				__side_view_bitmap = new WriteableBitmap((int)__data_size.w, (int)__data_size.d, 96, 96, PixelFormats.Bgra32, null);

				TopViewImage.Source = __top_view_bitmap;
				SideViewImage.Source = __side_view_bitmap;
				DepthScrollBar.Maximum = size.d - 1;
				DepthScrollBar.Value = size.d / 2;
			}
			catch (Exception e) {
				OnUnloadImageData();
				Console.WriteLine($"Fail to load image in ImageLabelViewer Control [ {e.Message} ]");
			}
		}
		public override unsafe void OnUnloadImageData() {
			try {
				__img_type = -1;
				__data_size = new() { w = -1, h = -1, d = -1, };
				__img_data_ptr = null;
				__lab_data_ptr = null;
				__top_view_data = null;
				__side_view_data = null;
				__top_view_bitmap = null;
				__side_view_bitmap = null;

				GC.Collect();
			}
			catch (Exception e) {
				Console.WriteLine($"Fail to unload image in ImageLabelViewer Control [ {e.Message} ]");
				throw new Exception($"ImageLabelViewer | {e.Message}");
			}
		}
		public override unsafe void OnUpstreamImageData() {
			throw new NotImplementedException();
		}
		public override unsafe void OnDownstreamImageData() {
			throw new NotImplementedException();
		}

		private unsafe void DepthScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args) {
			try {
				if (__img_type <= 0 ||
					__data_size.w <= 0 ||
					__data_size.h <= 0 ||
					__data_size.d <= 0 ||
					__img_data_ptr == null ||
					__lab_data_ptr == null ||
					__top_view_data == null ||
					__side_view_data == null ||
					__top_view_bitmap == null ||
					__side_view_bitmap == null)
					return;


				Int64 zIdx = (Int64)Math.Round(args.NewValue);
				double gray_min = 0;
				double gray_max = 65535;
				double blend_rate = 30 * 0.01;

				if (__img_type == 1) {
					float* t_img_cast_ptr = (float*)__img_data_ptr;
					Parallel.For(0, __top_view_data.Length, i => {
						Int64 pxl_idx = i + zIdx * __top_view_data.Length;
						double img_val = t_img_cast_ptr[pxl_idx];
						double t_gray = ((img_val - gray_min) / (gray_max - gray_min) * 255.0);
						Int32 gray_8 = t_gray < 0 ? 0 : t_gray > 255 ? 255 : (Int32)t_gray;
						Int32 lab = __lab_color_map[__lab_data_ptr[pxl_idx]];
						Int32 r = (Int32)(gray_8 * (1.0 - blend_rate)) + (Int32)(((lab >> 08) & 0xFF) * blend_rate);
						Int32 g = (Int32)(gray_8 * (1.0 - blend_rate)) + (Int32)(((lab >> 16) & 0xFF) * blend_rate);
						Int32 b = (Int32)(gray_8 * (1.0 - blend_rate)) + (Int32)(((lab >> 24) & 0xFF) * blend_rate);
						__top_view_data[i] = (b << 0) | (g << 8) | (r << 16) | (0xFF << 24);
					});
				}
				else if (__img_type == 2) {
					Parallel.For(0, __top_view_data.Length, i => {
						Int64 pxl_idx = i + zIdx * __top_view_data.Length;
						Int32 img_val = __img_data_ptr[pxl_idx];
						Int32 lab = __lab_color_map[__lab_data_ptr[pxl_idx]];
						Int32 r = (Int32)((img_val >> 24 & 0xFF) * (1.0 - blend_rate)) + (Int32)(((lab >> 08) & 0xFF) * blend_rate);
						Int32 g = (Int32)((img_val >> 16 & 0xFF) * (1.0 - blend_rate)) + (Int32)(((lab >> 16) & 0xFF) * blend_rate);
						Int32 b = (Int32)((img_val >>  8 & 0xFF) * (1.0 - blend_rate)) + (Int32)(((lab >> 24) & 0xFF) * blend_rate);
						__top_view_data[i] = (b << 0) | (g << 8) | (r << 16) | (0xFF << 24);
					});
				}

				__top_view_bitmap.WritePixels(new Int32Rect(0, 0, (int)__data_size.w, (int)__data_size.h), __top_view_data, (int)(__data_size.w * 4), 0);
				__side_view_bitmap.WritePixels(new Int32Rect(0, 0, (int)__data_size.w, (int)__data_size.d), __side_view_data, (int)(__data_size.w * 4), 0);
			}
			catch (Exception e) {
				Console.WriteLine($"Fail to update viewer image in ImageLabelViewer Control [ {e.Message} ]");
			}

		}
		private void ViewCanvas_MouseWheel(object sender, MouseWheelEventArgs e) {
			DepthScrollBar.Value += e.Delta < 0 ? 1 : -1;
		}
	}
}
