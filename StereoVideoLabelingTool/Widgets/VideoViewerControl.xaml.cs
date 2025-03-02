using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;
using StereoVideoLabelingTool.Classes;
using StereoVideoLabelingTool.Widgets;
using static System.Collections.Specialized.BitVector32;
using Point = System.Windows.Point;

namespace StereoVideoLabelingTool.Widgets
{
	public enum DRAG_TYPE
	{
		NONE,
		TRANSLATE_IMAGE,
		ROTATE_IMAGE,
		BRUSH_LABEL,
	}

	public partial class VideoViewerControl : StereoVideoControlBase
	{
		private readonly int[] __lab_color_map = new int[256];
		private int[]? __top_view_data = null;
		private int[]? __side_view_data = null;
		private WriteableBitmap? __top_view_bitmap = null;
		private WriteableBitmap? __side_view_bitmap = null;

		private double __last_canvas_w = 0.0;
		private double __last_canvas_h = 0.0;
		private double __scale_rate = 1.0;
		private Int64 __show_z_idx = -1;
		private FLOAT3 __lut_min_val = new() { x = 0, y = 0, z = 0, };
		private FLOAT3 __lut_max_val = new() { x = 255, y = 255, z = 255, };
		private float __lab_blend_rate = 0.3f;

		private DRAG_TYPE __drag_type = DRAG_TYPE.NONE;
		private POINT3 __drag_prev_position = new();


		////////////////////////////////////////////////////////////////

		protected override void Initialize() {
			try {
				//__top_view_data = new int[__img_info.ImgSize.w * __img_info.ImgSize.h];
				//__side_view_data = new int[__img_info.ImgSize.w * 2 * __img_info.ImgSize.d];
				//__top_view_bitmap = new WriteableBitmap((int)__img_info.ImgSize.w, (int)__img_info.ImgSize.h, 96, 96, PixelFormats.Bgra32, null);
				//__side_view_bitmap = new WriteableBitmap((int)__img_info.ImgSize.w * 2, (int)__img_info.ImgSize.d, 96, 96, PixelFormats.Bgra32, null);

				//TopViewGridRow.Height = new GridLength(__img_info.ImgSize.h, GridUnitType.Star);
				//SideViewGridRow.Height = new GridLength(__img_info.ImgSize.d, GridUnitType.Star);

				//__scale_rate = 1.0;
				//__show_z_idx = __img_info.ImgSize.d / 2;
				//__lab_blend_rate = 0.3f;
				//switch (__img_info.ImgType) {
				//	case IMG_TYPE.GRAY:
				//		__lut_min_val.x = 0;
				//		__lut_min_val.y = 0;
				//		__lut_min_val.z = 0;
				//		__lut_max_val.x = 65535;
				//		__lut_max_val.y = 65535;
				//		__lut_max_val.z = 65535;
				//		break;
				//	case IMG_TYPE.COLOR:
				//		__lut_min_val.x = 0;
				//		__lut_min_val.y = 0;
				//		__lut_min_val.z = 0;
				//		__lut_max_val.x = 255;
				//		__lut_max_val.y = 255;
				//		__lut_max_val.z = 255;
				//		break;
				//}
				//TopViewImage.Source = __top_view_bitmap;
				//SideViewImage.Source = __side_view_bitmap;
				//DepthScrollBar.Maximum = __img_info.ImgSize.d - 1;
				//DepthScrollBar.ViewportSize = __img_info.ImgSize.d / 10;
				//DepthScrollBar.Value = 0;
				//DepthScrollBar.Value = __img_info.ImgSize.d / 2;

				//var isEnable = __img_info.ImgSize.d > 1 ? Visibility.Visible : Visibility.Collapsed;
				//HorizontalLine.Visibility = isEnable;
				//VerticalLine.Visibility = isEnable;
				//DepthLine.Visibility = isEnable;

				//ImageControlBase_SizeChanged(this, null);
			}
			catch (Exception ex) {
				Release();
				Logger.Print(LOG_TYPE.WARNING, $"Fail to initialize widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Release() {
			try {
				__top_view_data = null;
				__side_view_data = null;
				__top_view_bitmap = null;
				__side_view_bitmap = null;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to release widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Update(object sender, EventArgs e) {
			try {
				if (!VideoInfo.GetSetting("LUT", "MIN", out float t_min)) t_min = 0;
				if (!VideoInfo.GetSetting("LUT", "MAX", out float t_max)) t_max = 65535;
				if (!VideoInfo.GetSetting("LUT", "BLEND", out float t_blend)) t_blend = 0.3f;

				__lut_min_val.x = t_min;
				__lut_min_val.y = t_min;
				__lut_min_val.z = t_min;

				__lut_max_val.x = t_max;
				__lut_max_val.y = t_max;
				__lut_max_val.z = t_max;

				__lab_blend_rate = t_blend;

				OnRefreshViewImage();
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.WARNING, $"Fail to update widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}

		////////////////////////////////////////////////////////////////

		public VideoViewerControl() {
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
		private void ImageControlBase_SizeChanged(object sender, SizeChangedEventArgs? e) {
			if (VideoInfo == null) return;

			UpdateLayout();


			//TopViewRotation.CenterX = TopViewCanvas.ActualWidth / 2;
			//TopViewRotation.CenterY = TopViewCanvas.ActualHeight / 2;

			//if (e == null) {
			//	__scale_rate = Math.Min(TopViewCanvas.ActualWidth / __img_info.ImgSize.w, TopViewCanvas.ActualHeight / __img_info.ImgSize.h);
			//	TopViewScale.ScaleX = TopViewScale.ScaleY = SideViewScale.ScaleX = SideViewScale.ScaleY = __scale_rate;
			//	TopViewTranslate.X = (TopViewCanvas.ActualWidth - __img_info.ImgSize.w * TopViewScale.ScaleX) / 2.0;
			//	TopViewTranslate.Y = (TopViewCanvas.ActualHeight - __img_info.ImgSize.h * TopViewScale.ScaleY) / 2.0;
			//	TopViewRotation.Angle = 0.0;
			//}
			//else {
			//	TopViewTranslate.X += (TopViewCanvas.ActualWidth - __last_canvas_w) / 2;
			//	TopViewTranslate.Y += (TopViewCanvas.ActualHeight - __last_canvas_h) / 2;
			//}
			//__last_canvas_w = TopViewCanvas.ActualWidth;
			//__last_canvas_h = TopViewCanvas.ActualHeight;

			//SideViewTranslate.X = (SideViewCanvas.ActualWidth - __img_info.ImgSize.w * 2 * SideViewScale.ScaleX) / 2.0;
			//SideViewTranslate.Y = (SideViewCanvas.ActualHeight - __img_info.ImgSize.d * SideViewScale.ScaleY) / 2.0;

			//HorizontalLine.X1 = 0;
			//HorizontalLine.X2 = TopViewCanvas.ActualWidth;
			//HorizontalLine.Y1 = TopViewCanvas.ActualHeight / 2;
			//HorizontalLine.Y2 = TopViewCanvas.ActualHeight / 2;
			//VerticalLine.X1 = TopViewCanvas.ActualWidth / 2;
			//VerticalLine.X2 = TopViewCanvas.ActualWidth / 2;
			//VerticalLine.Y1 = 0;
			//VerticalLine.Y2 = TopViewCanvas.ActualHeight;
			//DepthLine.X1 = 0;
			//DepthLine.X2 = SideViewCanvas.ActualWidth;

			OnRefreshViewImage();
		}

		private void DepthScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args) {
			if (args.OldValue == args.NewValue) return;
			OnChangeDepth((Int64)Math.Round(args.NewValue));
		}

		private void ViewCanvas_MouseWheel(object sender, MouseWheelEventArgs e) {
			if (VideoInfo == null) return;

			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
				double zoomSpeed = 1.1;
				double new_scale = e.Delta > 0 ? __scale_rate * zoomSpeed : __scale_rate * (1.0 / zoomSpeed);
				OnChangeImageScale(new_scale);
			}
			else {
				Int64 new_idx = e.Delta < 0 ? __show_z_idx + 1 : __show_z_idx - 1;
				OnChangeDepth(new_idx);
			}
		}
		private void ViewCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
			if (VideoInfo == null) return;

			var canvas = (Canvas)sender;
			var cur_pos = e.GetPosition(canvas);
			POINT3 pos_3d = new() {
				x = cur_pos.X,
				y = cur_pos.Y,
				z = __show_z_idx,
			};
			__drag_prev_position = pos_3d;

			if (e.LeftButton == MouseButtonState.Pressed) {
				if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
					__drag_type = DRAG_TYPE.TRANSLATE_IMAGE;
				}
				else if (
					Keyboard.Modifiers.HasFlag(ModifierKeys.Control) &&
					canvas == TopViewCanvas
					) {
					__drag_type = DRAG_TYPE.ROTATE_IMAGE;
				}
				else {
					__drag_type = DRAG_TYPE.BRUSH_LABEL;
					OnBrushLabel(canvas, pos_3d);
				}
			}

			if (__drag_type != DRAG_TYPE.NONE) {
				canvas.CaptureMouse();
			}
		}
		private void ViewCanvas_MouseMove(object sender, MouseEventArgs? e) {
			var canvas = (Canvas)sender;
			POINT3 cur_pos_3d;
			if (e == null) {
				cur_pos_3d = __drag_prev_position;
			}
			else {
				var cur_pos = e.GetPosition(canvas);
				cur_pos_3d.x = (float)cur_pos.X;
				cur_pos_3d.y = (float)cur_pos.Y;
			}
			cur_pos_3d.z = (float)__show_z_idx;

			switch (__drag_type) {
				case DRAG_TYPE.TRANSLATE_IMAGE: OnChangeImageTranslate(canvas, cur_pos_3d); break;
				case DRAG_TYPE.BRUSH_LABEL: OnBrushLabel(canvas, cur_pos_3d); break;
				default: return;
			}

			__drag_prev_position = cur_pos_3d;
		}
		private void ViewCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
			__drag_type = DRAG_TYPE.NONE;
			((Canvas)sender).ReleaseMouseCapture();
		}

		private unsafe void OnRefreshViewImage() {
			try {
				if (VideoInfo == null ||
					__top_view_data == null ||
					__side_view_data == null ||
					__top_view_bitmap == null ||
					__side_view_bitmap == null)
					return;

				/*
				__show_z_idx = Math.Clamp(__show_z_idx, 0, __img_info.ImgSize.w - 1);
				DepthLine.Y1 = DepthLine.Y2 = SideViewTransformGroup.Transform(new Point(0, __show_z_idx)).Y;

				Matrix top_view_to_image_matrix = TopViewTransformGroup.Value;
				Matrix side_image_to_view_matrix = SideViewTransformGroup.Value;
				top_view_to_image_matrix.Invert();
				double h_loc = TopViewCanvas.ActualHeight / 2;

				if (__img_info.ImgType == IMG_TYPE.GRAY) {
					float* t_img_cast_ptr = (float*)__img_info.ImgDataPtr;
					int to_bgra(float img, int lab, float min, float max, float blend) {
						int t_img = Math.Clamp((int)((img - min) / (max - min) * 255.0f), 0, 255);
						int r = (int)(t_img * (1.0 - blend) + (((lab >> 08) & 0xFF) * blend));
						int g = (int)(t_img * (1.0 - blend) + (((lab >> 16) & 0xFF) * blend));
						int b = (int)(t_img * (1.0 - blend) + (((lab >> 24) & 0xFF) * blend));
						return (b << 0) | (g << 8) | (r << 16) | (0xFF << 24);
					}
					Parallel.For(0, __img_info.ImgSize.h, y => {
						for (Int64 x = 0; x < __img_info.ImgSize.w; x++) {
							Int64 dst_pxl_idx = x + y * __img_info.ImgOffset.w;
							Int64 src_pxl_idx = x + y * __img_info.ImgOffset.w + __show_z_idx * __img_info.ImgOffset.h;
							float img_val = t_img_cast_ptr[src_pxl_idx];
							int lab_val = __lab_color_map[__img_info.LabDataPtr[src_pxl_idx]];
							__top_view_data[dst_pxl_idx] = to_bgra(
								img_val,
								lab_val,
								__lut_min_val.x,
								__lut_max_val.x,
								__lab_blend_rate
							);
						}
					});
					Parallel.For(0, __img_info.ImgSize.d, z => {
						for (Int64 x = 0; x < __img_info.ImgSize.w * 2; x++) {
							Int64 dst_pxl_idx = x + z * __img_info.ImgSize.w * 2;

							Point pt = new(x, 0);
							pt = side_image_to_view_matrix.Transform(pt);
							pt.Y = h_loc;
							pt = top_view_to_image_matrix.Transform(pt);

							if (pt.X < 0 || __img_info.ImgSize.w <= pt.X ||
								pt.Y < 0 || __img_info.ImgSize.h <= pt.Y) {
								__side_view_data[dst_pxl_idx] = 0;
							}
							else {
								Int64 src_pxl_idx = (Int64)pt.X + (Int64)pt.Y * __img_info.ImgOffset.w + z * __img_info.ImgOffset.h;
								float img_val = t_img_cast_ptr[src_pxl_idx];
								int lab_val = __lab_color_map[__img_info.LabDataPtr[src_pxl_idx]];
								__side_view_data[dst_pxl_idx] = to_bgra(
									img_val,
									lab_val,
									__lut_min_val.x,
									__lut_max_val.x,
									__lab_blend_rate
								);
							}
						}
					});
				}
				else if (__img_info.ImgType == IMG_TYPE.COLOR) {
					int to_bgra(int img, int lab, FLOAT3 min, FLOAT3 max, float alpha) {
						int pxl_r = Math.Clamp((int)(((img >> 24 & 0xFF) - min.x) / (max.x - min.x) * 255.0f), 0, 255);
						int pxl_g = Math.Clamp((int)(((img >> 16 & 0xFF) - min.y) / (max.y - min.y) * 255.0f), 0, 255);
						int pxl_b = Math.Clamp((int)(((img >>  8 & 0xFF) - min.z) / (max.z - min.z) * 255.0f), 0, 255);
						int r = (int)(pxl_r * (1.0 - alpha) + (((lab >> 08) & 0xFF) * alpha));
						int g = (int)(pxl_g * (1.0 - alpha) + (((lab >> 16) & 0xFF) * alpha));
						int b = (int)(pxl_b * (1.0 - alpha) + (((lab >> 24) & 0xFF) * alpha));
						return (b << 0) | (g << 8) | (r << 16) | (0xFF << 24);
					}
					Parallel.For(0, __img_info.ImgSize.h, y => {
						for (Int64 x = 0; x < __img_info.ImgSize.w; x++) {
							Int64 dst_pxl_idx = x + y * __img_info.ImgOffset.w;
							Int64 src_pxl_idx = x + y * __img_info.ImgOffset.w + __show_z_idx * __img_info.ImgOffset.h;
							int img_val = __img_info.ImgDataPtr[src_pxl_idx];
							int lab_val = __lab_color_map[__img_info.LabDataPtr[src_pxl_idx]];
							__top_view_data[dst_pxl_idx] = to_bgra(
								img_val,
								lab_val,
								__lut_min_val,
								__lut_max_val,
								__lab_blend_rate
							);
						}
					});
					Parallel.For(0, __img_info.ImgSize.d, z => {
						for (Int64 x = 0; x < __img_info.ImgSize.w * 2; x++) {
							Int64 dst_pxl_idx = x + z * __img_info.ImgSize.w * 2;

							Point pt = new(x, 0);
							pt = side_image_to_view_matrix.Transform(pt);
							pt.Y = h_loc;
							pt = top_view_to_image_matrix.Transform(pt);

							if (pt.X < 0 || __img_info.ImgSize.w <= pt.X ||
								pt.Y < 0 || __img_info.ImgSize.h <= pt.Y) {
								__side_view_data[dst_pxl_idx] = 0;
							}
							else {
								Int64 src_pxl_idx = (Int64)pt.X + (Int64)pt.Y * __img_info.ImgOffset.w + z * __img_info.ImgOffset.h;
								int img_val = __img_info.ImgDataPtr[src_pxl_idx];
								int lab_val = __lab_color_map[__img_info.LabDataPtr[src_pxl_idx]];
								__side_view_data[dst_pxl_idx] = to_bgra(
									img_val,
									lab_val,
									__lut_min_val,
									__lut_max_val,
									__lab_blend_rate
								);
							}
						}
					});
				}
				else
					return;

				__top_view_bitmap.WritePixels(new Int32Rect(0, 0, (int)__img_info.ImgSize.w, (int)__img_info.ImgSize.h), __top_view_data, (int)(__img_info.ImgSize.w * 4), 0);
				__side_view_bitmap.WritePixels(new Int32Rect(0, 0, (int)__img_info.ImgSize.w * 2, (int)__img_info.ImgSize.d), __side_view_data, (int)(__img_info.ImgSize.w * 4) * 2, 0);
				*/
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to update viewer image in ImageLabelViewer Control [ {ex.Message} ]");
			}
		}
		private void OnChangeDepth(Int64 new_idx) {
			if (VideoInfo == null) return;


			//Int64 next_z_idx = Math.Clamp(new_idx, 0, __img_info.ImgSize.d - 1);
			//if (__show_z_idx == next_z_idx) return;
			//__show_z_idx = next_z_idx;
			//DepthScrollBar.Value = next_z_idx;

			//if (__drag_type == DRAG_TYPE.NONE)
			//	OnRefreshViewImage();
			//else
			//	ViewCanvas_MouseMove(TopViewCanvas, null);
		}
		private void OnChangeImageScale(double new_scale) {
			if (VideoInfo == null) return;

			//__scale_rate = new_scale;

			//Matrix rot = TopViewRotation.Value;
			//rot.Invert();
			//Vector colVec = rot.Transform(new Vector(1, 0));
			//Vector rowVec = rot.Transform(new Vector(0, 1));
			//colVec.Normalize();
			//rowVec.Normalize();

			//Point canvasCenter = new(TopViewCanvas.ActualWidth / 2.0, TopViewCanvas.ActualHeight / 2.0);

			//Matrix curMatrix = TopViewTransformGroup.Value;
			//curMatrix.Invert();
			//Point centerImage = curMatrix.Transform(canvasCenter);
			//TopViewScale.ScaleX = TopViewScale.ScaleY = SideViewScale.ScaleX = SideViewScale.ScaleY =  __scale_rate;

			//Matrix newMatrix = TopViewTransformGroup.Value;
			//Point newPosCanvas = newMatrix.Transform(centerImage);
			//double x_diff = canvasCenter.X - newPosCanvas.X;
			//double y_diff = canvasCenter.Y - newPosCanvas.Y;

			//TopViewTranslate.X += x_diff * colVec.X + y_diff * rowVec.X;
			//TopViewTranslate.Y += x_diff * colVec.Y + y_diff * rowVec.Y;
			//SideViewTranslate.X = (SideViewCanvas.ActualWidth - __img_info.ImgSize.w * 2 * SideViewScale.ScaleX) / 2.0;
			//SideViewTranslate.Y = (SideViewCanvas.ActualHeight / 2.0 - __show_z_idx * __scale_rate);

			//OnRefreshViewImage();
		}
		private void OnChangeImageTranslate(Canvas canvas, POINT3 cur_pos) {
			if (canvas == TopViewCanvas) {
				Matrix curMatrix = TopViewRotation.Value;
				curMatrix.Invert();

				Vector colVec = curMatrix.Transform(new Vector(1, 0));
				Vector rowVec = curMatrix.Transform(new Vector(0, 1));
				colVec.Normalize();
				rowVec.Normalize();

				double x_diff = cur_pos.x - __drag_prev_position.x;
				double y_diff = cur_pos.y - __drag_prev_position.y;

				TopViewTranslate.X += x_diff * colVec.X + y_diff * rowVec.X;
				TopViewTranslate.Y += x_diff * colVec.Y + y_diff * rowVec.Y;
			}
			else if (canvas == SideViewCanvas) {
				SideViewTranslate.Y +=  cur_pos.y - __drag_prev_position.y;
			}
			else return;

			OnRefreshViewImage();
		}
		private unsafe void OnBrushLabel(Canvas canvas, POINT3 cur_pos) {
			if (VideoInfo == null) return;


			//if (canvas == TopViewCanvas) {
			//	Matrix curMatrix = TopViewTransformGroup.Value;
			//	curMatrix.Invert();

			//	var t_idx_from = curMatrix.Transform(new Point(__drag_prev_position.x, __drag_prev_position.y));
			//	var t_idx_to = curMatrix.Transform(new Point(cur_pos.x, cur_pos.y));

			//	POINT3 idx_from = new() { x = (float)t_idx_from.X, y = (float)t_idx_from.Y, z = __drag_prev_position.z };
			//	POINT3 idx_to = new() { x = (float)t_idx_to.X, y = (float)t_idx_to.Y, z = cur_pos.z };

			//	Int64 radius = 3;
			//	Int64 x_from = Math.Clamp((Int64)Math.Min(idx_from.x, idx_to.x) - radius, 0, __img_info.ImgSize.w - 1);
			//	Int64 x_to = Math.Clamp((Int64)Math.Max(idx_from.x, idx_to.x) + radius, 0, __img_info.ImgSize.w - 1);
			//	Int64 y_from = Math.Clamp((Int64)Math.Min(idx_from.y, idx_to.y) - radius, 0, __img_info.ImgSize.h - 1);
			//	Int64 y_to = Math.Clamp((Int64)Math.Max(idx_from.y, idx_to.y) + radius, 0, __img_info.ImgSize.h - 1);
			//	Int64 z_from = Math.Clamp((Int64)Math.Min(idx_from.z, idx_to.z) - radius, 0, __img_info.ImgSize.d - 1);
			//	Int64 z_to = Math.Clamp((Int64)Math.Max(idx_from.z, idx_to.z) + radius, 0, __img_info.ImgSize.d - 1);

			//	POINT3 pt = new();
			//	for (pt.z = z_from; pt.z <= z_to; pt.z += 1)
			//		for (pt.y = y_from; pt.y <= y_to; pt.y += 1)
			//			for (pt.x = x_from; pt.x <= x_to; pt.x += 1) {
			//				double dist = MyMath.CalcDistance(idx_from, idx_to, pt);
			//				if (dist > radius)
			//					continue;

			//				Int64 idx = (Int64)pt.x + (Int64)pt.y * __img_info.ImgOffset.w + (Int64)pt.z * __img_info.ImgOffset.h;
			//				__img_info.LabDataPtr[idx] = (byte)(__img_info.LabDataPtr[idx] | 0b00000001);
			//			}
			//}
			//else if (canvas == SideViewCanvas) {
			//	Matrix top_view_to_image_matrix = TopViewTransformGroup.Value;
			//	Matrix side_view_to_image_matrix = SideViewTransformGroup.Value;
			//	top_view_to_image_matrix.Invert();
			//	side_view_to_image_matrix.Invert();
			//	double h_loc = TopViewCanvas.ActualHeight / 2;

			//	FLOAT3 idx_from = new();
			//	{
			//		Point t_pt = new(__drag_prev_position.x, h_loc);
			//		t_pt = top_view_to_image_matrix.Transform(t_pt);
			//		idx_from.x = (float)t_pt.X;
			//		idx_from.y = (float)t_pt.Y;

			//		t_pt.X = 0;
			//		t_pt.Y = __drag_prev_position.y;
			//		t_pt = side_view_to_image_matrix.Transform(t_pt);
			//		idx_from.z = (float)t_pt.Y;
			//	}
			//	FLOAT3 idx_to = new();
			//	{
			//		Point t_pt = new(cur_pos.x, h_loc);
			//		t_pt = top_view_to_image_matrix.Transform(t_pt);
			//		idx_to.x = (float)t_pt.X;
			//		idx_to.y = (float)t_pt.Y;

			//		t_pt.X = 0;
			//		t_pt.Y = cur_pos.y;
			//		t_pt = side_view_to_image_matrix.Transform(t_pt);
			//		idx_to.z = (float)t_pt.Y;
			//	}

			//	Int64 radius = 3;
			//	Int64 x_from = Math.Clamp((Int64)Math.Min(idx_from.x, idx_to.x) - radius, 0, __img_info.ImgSize.w - 1);
			//	Int64 x_to = Math.Clamp((Int64)Math.Max(idx_from.x, idx_to.x) + radius, 0, __img_info.ImgSize.w - 1);
			//	Int64 y_from = Math.Clamp((Int64)Math.Min(idx_from.y, idx_to.y) - radius, 0, __img_info.ImgSize.h - 1);
			//	Int64 y_to = Math.Clamp((Int64)Math.Max(idx_from.y, idx_to.y) + radius, 0, __img_info.ImgSize.h - 1);
			//	Int64 z_from = Math.Clamp((Int64)Math.Min(idx_from.z, idx_to.z) - radius, 0, __img_info.ImgSize.d - 1);
			//	Int64 z_to = Math.Clamp((Int64)Math.Max(idx_from.z, idx_to.z) + radius, 0, __img_info.ImgSize.d - 1);

			//	FLOAT3 pt = new();
			//	for (pt.z = z_from; pt.z <= z_to; pt.z += 1)
			//		for (pt.y = y_from; pt.y <= y_to; pt.y += 1)
			//			for (pt.x = x_from; pt.x <= x_to; pt.x += 1) {
			//				double dist = MyMath.CalcDistance(idx_from, idx_to, pt);
			//				if (dist > radius)
			//					continue;

			//				Int64 idx = (Int64)pt.x + (Int64)pt.y * __img_info.ImgOffset.w + (Int64)pt.z * __img_info.ImgOffset.h;
			//				__img_info.LabDataPtr[idx] = (byte)(__img_info.LabDataPtr[idx] | 0b00000001);
			//			}
			//}
			//else return;

			OnRefreshViewImage();
		}
	}
}

