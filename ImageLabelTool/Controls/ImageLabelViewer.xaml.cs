using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageLabelTool.Classes;
using Point = System.Windows.Point;

namespace ImageLabelTool.Controls
{
	public enum DRAG_TYPE
	{
		NONE,
		CHANGE_INDEX,
		TRANSLATE_IMAGE,
		BRUSH_LABEL,
	}

	public partial class ImageLabelViewer : ImageControlBase
	{
		private readonly int[] __lab_color_map = new int[256];
		private int[]? __top_view_data = null;
		private int[]? __side_view_data = null;
		private WriteableBitmap? __top_view_bitmap = null;
		private WriteableBitmap? __side_view_bitmap = null;

		private IMG_TYPE __img_type = IMG_TYPE.NONE;
		private SIZE3 __data_size = new() { w = -1, h = -1, d = -1, };
		private SIZE3 __offset_table = new() { w = -1, h = -1, d = -1, };
		private unsafe int* __img_data_ptr = null;
		private unsafe byte* __lab_data_ptr = null;

		private INDEX3 __show_idx = new() { x = -1, y = -1, z = -1, };
		private FLOAT3 __lut_min_val = new() { x = 0, y = 0, z = 0, };
		private FLOAT3 __lut_max_val = new() { x = 255, y = 255, z = 255, };
		private float __lab_blend_rate = 0.3f;


		private DRAG_TYPE __drag_type = DRAG_TYPE.NONE;
		private Point __drag_prev_position = new();


		////////////////////////////////////////////////////////////////

		public override unsafe void OnLoadImageData(IMG_TYPE type, SIZE3 size, int* img_data_ptr, byte* lab_data_ptr) {
			try {
				__img_type = type;
				__data_size = size;
				__img_data_ptr = img_data_ptr;
				__lab_data_ptr = lab_data_ptr;
				__top_view_data = new int[size.w * size.h];
				__side_view_data = new int[size.w * size.d];
				__top_view_bitmap = new WriteableBitmap((int)__data_size.w, (int)__data_size.h, 96, 96, PixelFormats.Bgra32, null);
				__side_view_bitmap = new WriteableBitmap((int)__data_size.w, (int)__data_size.d, 96, 96, PixelFormats.Bgra32, null);

				TopViewGridRow.Height = new GridLength(__data_size.h, GridUnitType.Star);
				SideViewGridRow.Height = new GridLength(__data_size.d, GridUnitType.Star);
				UpdateLayout();

				__offset_table.w = __data_size.w;
				__offset_table.h = __data_size.h * __offset_table.w;
				__offset_table.d = __data_size.d * __offset_table.h;
				__show_idx.x = __data_size.w / 2;
				__show_idx.y = __data_size.h / 2;
				__show_idx.z = __data_size.d / 2;
				__lab_blend_rate = 0.3f;
				switch (__img_type) {
					case IMG_TYPE.GRAY:
						__lut_min_val.x = 0;
						__lut_min_val.y = 0;
						__lut_min_val.z = 0;
						__lut_max_val.x = 65535;
						__lut_max_val.y = 65535;
						__lut_max_val.z = 65535;
						break;
					case IMG_TYPE.COLOR:
						__lut_min_val.x = 0;
						__lut_min_val.y = 0;
						__lut_min_val.z = 0;
						__lut_max_val.x = 255;
						__lut_max_val.y = 255;
						__lut_max_val.z = 255;
						break;
				}
				TopViewImage.Source = __top_view_bitmap;
				SideViewImage.Source = __side_view_bitmap;
				DepthScrollBar.Maximum = __data_size.d - 1;
				DepthScrollBar.Value = 0;
				DepthScrollBar.Value = __data_size.d / 2;

				ViewCanvas_SizeChanged(TopViewCanvas, null);
				ViewCanvas_SizeChanged(SideViewCanvas, null);
			}
			catch (Exception ex) {
				OnUnloadImageData();
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load image in ImageLabelViewer Control [ {ex.Message} ]");
			}
		}
		public override unsafe void OnUnloadImageData() {
			try {
				__img_type = IMG_TYPE.NONE;
				__data_size = new() { w = -1, h = -1, d = -1, };
				__img_data_ptr = null;
				__lab_data_ptr = null;
				__top_view_data = null;
				__side_view_data = null;
				__top_view_bitmap = null;
				__side_view_bitmap = null;

				GC.Collect();
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to unload image in ImageLabelViewer Control [ {ex.Message} ]");
				throw new Exception($"ImageLabelViewer | {ex.Message}");
			}
		}
		public override unsafe void OnUpstreamImageData() {
			throw new NotImplementedException();
		}
		public override unsafe void OnDownstreamImageData() {
			throw new NotImplementedException();
		}

		////////////////////////////////////////////////////////////////

		public ImageLabelViewer() {
			InitializeComponent();

			Random random = new(4969);
			for (int i = 0; i < __lab_color_map.Length; i++) {
				int b = i % 3 != 0 ? random.Next(130, 230) : 0;
				int g = i % 3 != 1 ? random.Next(130, 230) : 0;
				int r = i % 3 != 2 ? random.Next(130, 230) : 0;
				__lab_color_map[i] = (int)((b << 24) | (g << 16) | (r << 8) | 0xFF);
			}
			__lab_color_map[0] = 0xFF;
		}

		private void DepthScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args) {
			INDEX3 idx = __show_idx;
			idx.z = (Int64)Math.Round(args.NewValue);
			OnRefreshViewImage(idx);
		}

		private void ViewCanvas_SizeChanged(object sender, SizeChangedEventArgs? e) {
			if (__data_size.w <= 0 ||
				__data_size.h <= 0 ||
				__data_size.d <= 0)
				return;

			if (sender == TopViewCanvas) {
				double new_scale =
					e == null ?
					Math.Min(TopViewCanvas.ActualWidth / __data_size.w, TopViewCanvas.ActualHeight / __data_size.h) :
					TopViewScale.ScaleX * e.NewSize.Width / e.PreviousSize.Width;

				TopViewScale.ScaleX = TopViewScale.ScaleY = new_scale;
				TopViewTranslate.X = (TopViewCanvas.ActualWidth  - __data_size.w * TopViewScale.ScaleX) / 2.0;
				TopViewTranslate.Y = (TopViewCanvas.ActualHeight - __data_size.h * TopViewScale.ScaleY) / 2.0;
			}
			else if (sender == SideViewCanvas) {
				double new_scale =
					e == null ?
					Math.Min(SideViewCanvas.ActualWidth / __data_size.w, SideViewCanvas.ActualHeight / __data_size.d) :
					SideViewScale.ScaleX * e.NewSize.Width / e.PreviousSize.Width;

				SideViewScale.ScaleX = SideViewScale.ScaleY = new_scale;
				SideViewTranslate.X = (SideViewCanvas.ActualWidth  - __data_size.w * SideViewScale.ScaleX) / 2;
				SideViewTranslate.Y = (SideViewCanvas.ActualHeight - __data_size.d * SideViewScale.ScaleY) / 2;
			}
		}
		private void ViewCanvas_MouseWheel(object sender, MouseWheelEventArgs e) {
			if (__data_size.w <= 0 ||
				__data_size.h <= 0 ||
				__data_size.d <= 0)
				return;

			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
				double zoomSpeed = 1.1;
				double scale = (e.Delta > 0) ? zoomSpeed : (1.0 / zoomSpeed);

				{
					Point canvasCenter = new(TopViewCanvas.ActualWidth / 2.0, TopViewCanvas.ActualHeight / 2.0);

					Matrix curMatrix = TopViewTransformGroup.Value;
					curMatrix.Invert();
					Point centerImage = curMatrix.Transform(canvasCenter);
					TopViewScale.ScaleX *= scale;
					TopViewScale.ScaleY *= scale;

					Matrix newMatrix = TopViewTransformGroup.Value;
					Point newPosCanvas = newMatrix.Transform(centerImage);
					TopViewTranslate.X += canvasCenter.X - newPosCanvas.X;
					TopViewTranslate.Y += canvasCenter.Y - newPosCanvas.Y;
				}
				{
					Point canvasCenter = new(SideViewCanvas.ActualWidth / 2.0, SideViewCanvas.ActualHeight / 2.0);

					Matrix curMatrix = SideViewTransformGroup.Value;
					curMatrix.Invert();
					Point centerImage = curMatrix.Transform(canvasCenter);
					SideViewScale.ScaleX *= scale;
					SideViewScale.ScaleY *= scale;

					Matrix newMatrix = SideViewTransformGroup.Value;
					Point newPosCanvas = newMatrix.Transform(centerImage);
					SideViewTranslate.X += canvasCenter.X - newPosCanvas.X;
					SideViewTranslate.Y += canvasCenter.Y - newPosCanvas.Y;
				}
			}
			else {
				DepthScrollBar.Value += e.Delta < 0 ? 1 : -1;
			}
		}

		private void ViewCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
			if (__data_size.w <= 0 ||
				__data_size.h <= 0 ||
				__data_size.d <= 0)
				return;

			var canvas = (Canvas)sender;
			Point cur_pos = e.GetPosition(canvas);

			if (e.LeftButton == MouseButtonState.Pressed) {
				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
					__drag_type = DRAG_TYPE.CHANGE_INDEX;
					__drag_prev_position = cur_pos;
					OnChangeShowIndex(canvas, cur_pos);
				}
				else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
					__drag_type = DRAG_TYPE.TRANSLATE_IMAGE;
					__drag_prev_position = cur_pos;
				}
				else {
					__drag_type = DRAG_TYPE.BRUSH_LABEL;
					__drag_prev_position = cur_pos;
					OnBrushLabel(canvas, cur_pos);
				}
			}

			if (__drag_type != DRAG_TYPE.NONE) {
				canvas.CaptureMouse();
			}
		}
		private void ViewCanvas_MouseMove(object sender, MouseEventArgs e) {
			var canvas = (Canvas)sender;
			var cur_pos = e.GetPosition(canvas);

			switch (__drag_type) {
				case DRAG_TYPE.CHANGE_INDEX: OnChangeShowIndex(canvas, cur_pos); break;
				case DRAG_TYPE.TRANSLATE_IMAGE: OnTranslateImage(canvas, cur_pos); break;
				case DRAG_TYPE.BRUSH_LABEL: OnBrushLabel(canvas, cur_pos); break;
				default: return;
			}

			__drag_prev_position = cur_pos;
		}
		private void ViewCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
			__drag_type = DRAG_TYPE.NONE;
			((Canvas)sender).ReleaseMouseCapture();
		}

		private unsafe void OnRefreshViewImage(INDEX3? idx = null) {
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

				INDEX3 next_idx = idx ?? __show_idx;
				__show_idx.x = Math.Clamp(next_idx.x, 0, __data_size.w);
				__show_idx.y = Math.Clamp(next_idx.y, 0, __data_size.h);
				__show_idx.z = Math.Clamp(next_idx.z, 0, __data_size.d);

				if (__img_type == IMG_TYPE.GRAY) {
					float* t_img_cast_ptr = (float*)__img_data_ptr;
					Func<float, int, float, float, float, int>? to_bgra = (float img, int lab, float min, float max, float blend) => {
						int t_img = Math.Clamp((int)((img - min) / (max - min) * 255.0f), 0, 255);
						int r = (int)(t_img * (1.0 - blend) + (((lab >> 08) & 0xFF) * blend));
						int g = (int)(t_img * (1.0 - blend) + (((lab >> 16) & 0xFF) * blend));
						int b = (int)(t_img * (1.0 - blend) + (((lab >> 24) & 0xFF) * blend));
						return (b << 0) | (g << 8) | (r << 16) | (0xFF << 24);
					};
					Parallel.For(0, __data_size.h, y => {
						for (Int64 x = 0; x < __data_size.w; x++) {
							Int64 dst_pxl_idx = x + y * __offset_table.w;
							Int64 src_pxl_idx = x + y * __offset_table.w + __show_idx.z * __offset_table.h;
							float img_val = t_img_cast_ptr[src_pxl_idx];
							int lab_val = __lab_color_map[__lab_data_ptr[src_pxl_idx]];
							__top_view_data[dst_pxl_idx] = to_bgra(
								img_val,
								lab_val,
								__lut_min_val.x,
								__lut_max_val.x,
								__lab_blend_rate
							);
						}
					});
					Parallel.For(0, __data_size.d, z => {
						for (Int64 x = 0; x < __data_size.w; x++) {
							Int64 dst_pxl_idx = x + z * __offset_table.w;
							Int64 src_pxl_idx = x + __show_idx.y * __offset_table.w + z * __offset_table.h;
							float img_val = t_img_cast_ptr[src_pxl_idx];
							int lab_val = __lab_color_map[__lab_data_ptr[src_pxl_idx]];
							__side_view_data[dst_pxl_idx] = to_bgra(
								img_val,
								lab_val,
								__lut_min_val.x,
								__lut_max_val.x,
								__lab_blend_rate
							);
						}
					});
				}
				else if (__img_type == IMG_TYPE.COLOR) {
					Func<int, int, FLOAT3, FLOAT3, float, int>? to_bgra = (int img, int lab, FLOAT3 min, FLOAT3 max, float alpha) => {
						int pxl_r = Math.Clamp((int)(((img >> 24 & 0xFF) - min.x) / (max.x - min.x) * 255.0f), 0, 255);
						int pxl_g = Math.Clamp((int)(((img >> 16 & 0xFF) - min.y) / (max.y - min.y) * 255.0f), 0, 255);
						int pxl_b = Math.Clamp((int)(((img >>  8 & 0xFF) - min.z) / (max.z - min.z) * 255.0f), 0, 255);
						int r = (int)(pxl_r * (1.0 - alpha) + (((lab >> 08) & 0xFF) * alpha));
						int g = (int)(pxl_g * (1.0 - alpha) + (((lab >> 16) & 0xFF) * alpha));
						int b = (int)(pxl_b * (1.0 - alpha) + (((lab >> 24) & 0xFF) * alpha));
						return (b << 0) | (g << 8) | (r << 16) | (0xFF << 24);
					};
					Parallel.For(0, __data_size.h, y => {
						for (Int64 x = 0; x < __data_size.w; x++) {
							Int64 dst_pxl_idx = x + y * __offset_table.w;
							Int64 src_pxl_idx = x + y * __offset_table.w + __show_idx.z * __offset_table.h;
							int img_val = __img_data_ptr[src_pxl_idx];
							int lab_val = __lab_color_map[__lab_data_ptr[src_pxl_idx]];
							__top_view_data[dst_pxl_idx] = to_bgra(
								img_val,
								lab_val,
								__lut_min_val,
								__lut_max_val,
								__lab_blend_rate
							);
						}
					});
					Parallel.For(0, __data_size.d, z => {
						for (Int64 x = 0; x < __data_size.w; x++) {
							Int64 dst_pxl_idx = x + z * __offset_table.w;
							Int64 src_pxl_idx = x + __show_idx.y * __offset_table.w + z * __offset_table.h;
							int img_val = __img_data_ptr[src_pxl_idx];
							int lab_val = __lab_color_map[__lab_data_ptr[src_pxl_idx]];
							__side_view_data[dst_pxl_idx] = to_bgra(
								img_val,
								lab_val,
								__lut_min_val,
								__lut_max_val,
								__lab_blend_rate
							);
						}
					});
				}
				else
					return;

				__top_view_bitmap.WritePixels(new Int32Rect(0, 0, (int)__data_size.w, (int)__data_size.h), __top_view_data, (int)(__data_size.w * 4), 0);
				__side_view_bitmap.WritePixels(new Int32Rect(0, 0, (int)__data_size.w, (int)__data_size.d), __side_view_data, (int)(__data_size.w * 4), 0);
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to update viewer image in ImageLabelViewer Control [ {ex.Message} ]");
			}
		}
		private void OnChangeShowIndex(Canvas canvas, Point cur_pos) {
			if (canvas == TopViewCanvas) {
				Matrix curMatrix = TopViewTransformGroup.Value;
				curMatrix.Invert();

				Point img_idx = curMatrix.Transform(cur_pos);
				INDEX3 idx = __show_idx;
				idx.x = (Int64)img_idx.X;
				idx.y = (Int64)img_idx.Y;
				if (idx.x < 0 || __data_size.w <= idx.x ||
					idx.y < 0 || __data_size.h <= idx.y)
					return;

				OnRefreshViewImage(idx);
			}
			else if (canvas == SideViewCanvas) {
				Matrix curMatrix = TopViewTransformGroup.Value;
				curMatrix.Invert();

				Point img_idx = curMatrix.Transform(cur_pos);
				INDEX3 idx = __show_idx;
				idx.x = (Int64)img_idx.X;
				idx.z = (Int64)img_idx.Y;
				if (idx.x < 0 || __data_size.w <= idx.x ||
					idx.z < 0 || __data_size.d <= idx.z)
					return;

				OnRefreshViewImage(idx);
			}
		}
		private void OnTranslateImage(Canvas canvas, Point cur_pos) {
			TopViewTranslate.X += cur_pos.X - __drag_prev_position.X;
			SideViewTranslate.X += cur_pos.X - __drag_prev_position.X;
			if (canvas == TopViewCanvas)
				TopViewTranslate.Y += cur_pos.Y - __drag_prev_position.Y;
			else if (canvas == SideViewCanvas)
				SideViewTranslate.Y += cur_pos.Y - __drag_prev_position.Y;
		}
		private unsafe void OnBrushLabel(Canvas canvas, Point cur_pos) {
			if (canvas == TopViewCanvas) {
				Matrix curMatrix = TopViewTransformGroup.Value;
				curMatrix.Invert();

				Point idx_from = curMatrix.Transform(__drag_prev_position);
				Point idx_to = curMatrix.Transform(cur_pos);

				Int64 radius = 3;
				Int64 x_from = Math.Clamp((Int64)Math.Min(idx_from.X, idx_to.X) - radius, 0, __data_size.w - 1);
				Int64 x_to = Math.Clamp((Int64)Math.Max(idx_from.X, idx_to.X) + radius, 0, __data_size.w - 1);
				Int64 y_from = Math.Clamp((Int64)Math.Min(idx_from.Y, idx_to.Y) - radius, 0, __data_size.h - 1);
				Int64 y_to = Math.Clamp((Int64)Math.Max(idx_from.Y, idx_to.Y) + radius, 0, __data_size.h - 1);

				Point pt = new();
				for (pt.Y = y_from; pt.Y <= y_to; pt.Y++) for (pt.X = x_from; pt.X <= x_to; pt.X++) {
						double dist = MY_MATH.CalcDistance(idx_from, idx_to, pt);
						if (dist > radius)
							continue;

						Int64 idx = (Int64)pt.X + (Int64)pt.Y * __offset_table.w + __show_idx.z * __offset_table.h;
						__lab_data_ptr[idx] = (byte)(__lab_data_ptr[idx] | 0b00000001);
					}

				OnRefreshViewImage();
			}
			else if (canvas == SideViewCanvas) {
				Matrix curMatrix = SideViewTransformGroup.Value;
				curMatrix.Invert();

				Point idx_from = curMatrix.Transform(__drag_prev_position);
				Point idx_to = curMatrix.Transform(cur_pos);

				Int64 radius = 3;
				Int64 x_from = Math.Clamp((Int64)Math.Min(idx_from.X, idx_to.X) - radius, 0, __data_size.w - 1);
				Int64 x_to = Math.Clamp((Int64)Math.Max(idx_from.X, idx_to.X) + radius, 0, __data_size.w - 1);
				Int64 y_from = Math.Clamp((Int64)Math.Min(idx_from.Y, idx_to.Y) - radius, 0, __data_size.d - 1);
				Int64 y_to = Math.Clamp((Int64)Math.Max(idx_from.Y, idx_to.Y) + radius, 0, __data_size.d - 1);

				Point pt = new();
				for (pt.Y = y_from; pt.Y <= y_to; pt.Y++) for (pt.X = x_from; pt.X <= x_to; pt.X++) {
						double dist = MY_MATH.CalcDistance(idx_from, idx_to, pt);
						if (dist > radius)
							continue;

						Int64 idx = (Int64)pt.X + __show_idx.y * __offset_table.w + (Int64)pt.Y * __offset_table.h;
						__lab_data_ptr[idx] = (byte)(__lab_data_ptr[idx] | 0b00000001);
					}

				OnRefreshViewImage();
			}
		}
	}
}

