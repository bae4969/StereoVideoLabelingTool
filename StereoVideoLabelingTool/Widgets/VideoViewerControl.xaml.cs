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
using System.Xml.Linq;
using OpenCvSharp;
using StereoVideoLabelingTool.Classes;
using StereoVideoLabelingTool.Widgets;
using static System.Collections.Specialized.BitVector32;
using Point = System.Windows.Point;

namespace StereoVideoLabelingTool.Widgets
{
	public enum TOOL_TYPE
	{
		NONE,
		TRANSLATE_IMAGE,
		ROTATE_IMAGE,
		BRUSH_LABEL,
	}
	public enum DRAG_TYPE
	{
		NONE,
		TRANSLATE_IMAGE,
		INDEXING_IMAGE,
		BRUSH_LABEL,
	}

	public partial class VideoViewerControl : StereoVideoControlBase
	{
		private int[]? _left_view_data = null;
		private int[]? _right_view_data = null;
		private WriteableBitmap? _left_view_bitmap = null;
		private WriteableBitmap? _right_view_bitmap = null;

		private SIZE3 _img_size = new(0, 0, 0);
		private INDEX3 _img_index = new(0, 0, 0);

		private float _lut_min_val = 0;
		private float _lut_max_val = 255;
		private float _lab_blend_rate = 0.3f;

		private TOOL_TYPE _tool_type = TOOL_TYPE.NONE;
		private DRAG_TYPE _drag_type = DRAG_TYPE.NONE;
		private INDEX3 _left_prev_drag_position = new();
		private INDEX3 _right_prev_drag_position = new();


		////////////////////////////////////////////////////////////////

		protected override void Initialize() {
			try {
				_img_size.W = VideoInfo.ImgSize.W;
				_img_size.H = VideoInfo.ImgSize.H;
				_img_size.D = VideoInfo.ImgSize.D;
				_img_index.X = VideoInfo.GetSetting("VIDEO_LOC", "X", out long t_idx_x) ? t_idx_x : VideoInfo.ImgSize.W / 2;
				_img_index.Y = VideoInfo.GetSetting("VIDEO_LOC", "Y", out long t_idx_y) ? t_idx_y : VideoInfo.ImgSize.H / 2;
				_img_index.Z = VideoInfo.GetSetting("VIDEO_LOC", "Z", out long t_idx_z) ? t_idx_z : 0;
				_img_index.X = Math.Clamp(_img_index.X, 0, _img_size.W - 1);
				_img_index.Y = Math.Clamp(_img_index.Y, 0, _img_size.H - 1);
				_img_index.Z = Math.Clamp(_img_index.Z, 0, _img_size.D - 1);

				_left_view_data = new int[_img_size.W * _img_size.H];
				_right_view_data = new int[_img_size.W * _img_size.H];
				_left_view_bitmap = new WriteableBitmap((int)_img_size.W, (int)_img_size.H, 96, 96, PixelFormats.Bgr24, null);
				_right_view_bitmap = new WriteableBitmap((int)_img_size.W, (int)_img_size.H, 96, 96, PixelFormats.Bgr24, null);

				_lut_min_val = 0;
				_lut_max_val = 255;
				_lab_blend_rate = 0.3f;

				LeftViewImage.Source = _left_view_bitmap;
				RightViewImage.Source = _right_view_bitmap;
				TimePointScrollBar.Maximum = _img_size.D - 1;
				TimePointScrollBar.ViewportSize = _img_size.D / 10;
				TimePointScrollBar.Value = _img_index.Z;

				_drag_type = DRAG_TYPE.NONE;
				_tool_type = TOOL_TYPE.NONE;

				ImageControlBase_SizeChanged(this, null);
			}
			catch (Exception ex) {
				Release();
				Logger.Print(LOG_TYPE.WARNING, $"Fail to initialize widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Release() {
			try {
				_drag_type = DRAG_TYPE.NONE;
				_tool_type = TOOL_TYPE.NONE;
				_left_view_data = null;
				_right_view_data = null;
				_left_view_bitmap = null;
				_right_view_bitmap = null;
				_img_index.X = 0;
				_img_index.Y = 0;
				_img_index.Z = 0;
				_img_size.W = 0;
				_img_size.H = 0;
				_img_size.D = 0;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to release widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Update(object sender, EventArgs e) {
			try {
				_lut_min_val = VideoInfo.GetSetting("LUT", "MIN", out float t_min) ? t_min : 0;
				_lut_max_val = VideoInfo.GetSetting("LUT", "MAX", out float t_max) ? t_max : 255;
				_lab_blend_rate = VideoInfo.GetSetting("LUT", "BLEND", out float t_blend) ? t_blend : 0.3f;

				InvalidateVisual();
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.WARNING, $"Fail to update widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}

		////////////////////////////////////////////////////////////////

		public VideoViewerControl() {
			InitializeComponent();
		}
		private void ImageControlBase_SizeChanged(object sender, SizeChangedEventArgs? e) {
			if (VideoInfo == null) return;

			UpdateLayout();

			double new_scale = 1.0;
			if (e == null) {
				new_scale = Math.Min(LeftViewCanvas.ActualWidth / _img_size.W, LeftViewCanvas.ActualHeight / _img_size.H);
			}
			else {
				new_scale = LeftViewScale.ScaleX;
			}

			using (Dispatcher.DisableProcessing()) {
				LeftViewScale.ScaleX = LeftViewScale.ScaleY = RightViewScale.ScaleX = RightViewScale.ScaleY = new_scale;
				LeftViewTranslate.X = RightViewTranslate.X = (LeftViewCanvas.ActualWidth  - _img_size.W * new_scale) / 2.0;
				LeftViewTranslate.Y = RightViewTranslate.Y = (LeftViewCanvas.ActualHeight - _img_size.H * new_scale) / 2.0;
			}

			InvalidateVisual();
		}
		protected unsafe override void OnRender(DrawingContext dx) {
			base.OnRender(dx);

			if (VideoInfo == null ||
				_left_view_data == null ||
				_right_view_data == null ||
				_left_view_bitmap == null ||
				_right_view_bitmap == null)
				return;


			// 이미지 그리기
			{
				VideoInfo.LeftVideoCapture.Set(VideoCaptureProperties.PosFrames, _img_index.Z);
				VideoInfo.RightVideoCapture.Set(VideoCaptureProperties.PosFrames, _img_index.Z);

				var t_left = new Mat();
				var t_right = new Mat();
				VideoInfo.LeftVideoCapture.Read(t_left);
				VideoInfo.RightVideoCapture.Read(t_right);

				var t_left_ptr = t_left.DataPointer;
				var t_right_ptr = t_right.DataPointer;

				_left_view_bitmap.WritePixels(new Int32Rect(0, 0, (int)_img_size.W, (int)_img_size.H), (nint)t_left_ptr, (int)(_img_size.W * _img_size.H * 3), (int)(_img_size.W * 3));
				_right_view_bitmap.WritePixels(new Int32Rect(0, 0, (int)_img_size.W, (int)_img_size.H), (nint)t_right_ptr, (int)(_img_size.W * _img_size.H * 3), (int)(_img_size.W * 3));
			}

			// cross line 정렬
			{
				var left_cross_center = LeftViewTransformGroup.Transform(new(_img_index.X, _img_index.Y));
				var right_cross_center = RightViewTransformGroup.Transform(new(_img_index.X, _img_index.Y));

				LeftHorizontalLine.X1 = 0;
				LeftHorizontalLine.X2 = LeftViewCanvas.RenderSize.Width;
				LeftHorizontalLine.Y1 = LeftHorizontalLine.Y2 = left_cross_center.Y;
				LeftVerticalLine.X1 = LeftVerticalLine.X2 = left_cross_center.X;
				LeftVerticalLine.Y1 = 0;
				LeftVerticalLine.Y2 = LeftViewCanvas.RenderSize.Height;

				RightHorizontalLine.X1 = 0;
				RightHorizontalLine.X2 = RightViewCanvas.RenderSize.Width;
				RightHorizontalLine.Y1 = RightHorizontalLine.Y2 = right_cross_center.Y;
				RightVerticalLine.X1 = RightVerticalLine.X2 = right_cross_center.X;
				RightVerticalLine.Y1 = 0;
				RightVerticalLine.Y2 = RightViewCanvas.RenderSize.Height;
			}
		}

		private void TimePointScrollBar_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args) {
			if (VideoInfo == null ||
				args.OldValue == args.NewValue)
				return;

			var old_z_idx = _img_index.Z;
			var new_z_idx = (long)Math.Round(args.NewValue);
			_img_index.Z = Math.Clamp(new_z_idx, 0, Math.Max(0, _img_size.D - 1));
			if (_img_index.Z != old_z_idx)
				InvalidateVisual();
		}

		private void ViewCanvas_MouseWheel(object sender, MouseWheelEventArgs e) {
			if (VideoInfo == null ||
				sender is not Canvas canvas)
				return;

			if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) {
				double zoomSpeed = 1.1;
				double new_scale = e.Delta > 0 ? zoomSpeed : (1.0 / zoomSpeed);
				OnChangeImageScale(canvas, new_scale);
			}
			else {
				TimePointScrollBar.Value = e.Delta < 0 ? _img_index.Z + 1 : _img_index.Z -1;
			}
		}
		private void ViewCanvas_MouseDown(object sender, MouseButtonEventArgs e) {
			if (VideoInfo == null ||
				sender is not Canvas canvas)
				return;

			try {
				var ui_pos = e.GetPosition(canvas);
				var pxl_pos = canvas.RenderTransform.Inverse.Transform(ui_pos);

				if (e.ChangedButton == MouseButton.Left) {
					_left_prev_drag_position.X = (long)Math.Round(pxl_pos.X);
					_left_prev_drag_position.Y = (long)Math.Round(pxl_pos.Y);
					_left_prev_drag_position.Z = (long)Math.Round(TimePointScrollBar.Value);

					if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) {
						_drag_type = DRAG_TYPE.TRANSLATE_IMAGE;
					}
					else {
						_drag_type = DRAG_TYPE.BRUSH_LABEL;
						OnBrushLabel(canvas, _left_prev_drag_position);
					}
				}
				else if (e.ChangedButton == MouseButton.Right) {
					_right_prev_drag_position.X = (long)Math.Round(pxl_pos.X);
					_right_prev_drag_position.Y = (long)Math.Round(pxl_pos.Y);
					_right_prev_drag_position.Z = (long)Math.Round(TimePointScrollBar.Value);

				}

				if (_drag_type != DRAG_TYPE.NONE) {
					canvas.CaptureMouse();
				}
			}
			catch (Exception ex) { Logger.Print(LOG_TYPE.ERROR, $"비디오 뷰어 컨트롤에서 마우스 down 이벤트 처리 오류 [ {ex.Message} ]"); }
			finally { canvas.ReleaseMouseCapture(); }
		}
		private void ViewCanvas_MouseMove(object sender, MouseEventArgs e) {
			if (sender is not Canvas canvas) return;

			try {
				var ui_pos = e.GetPosition(canvas);
				var pxl_pos = canvas.RenderTransform.Inverse.Transform(ui_pos);
				var pxl_pos_3 = new INDEX3((long)Math.Round(pxl_pos.X), (long)Math.Round(pxl_pos.Y), (long)Math.Round(TimePointScrollBar.Value));

				switch (_drag_type) {
					case DRAG_TYPE.TRANSLATE_IMAGE: OnChangeImageTranslate(canvas, pxl_pos_3); break;
					case DRAG_TYPE.BRUSH_LABEL: OnBrushLabel(canvas, pxl_pos_3); break;
					default: return;
				}

				switch (_drag_type) {
					case DRAG_TYPE.TRANSLATE_IMAGE:
					case DRAG_TYPE.BRUSH_LABEL: _left_prev_drag_position = pxl_pos_3; break;
					default: return;
				}
			}
			catch (Exception ex) { Logger.Print(LOG_TYPE.ERROR, $"비디오 뷰어 컨트롤에서 마우스 move 이벤트 처리 오류 [ {ex.Message} ]"); }
		}
		private void ViewCanvas_MouseUp(object sender, MouseButtonEventArgs e) {
			if (sender is not Canvas canvas) return;
			try {

			}
			finally {
				_drag_type = DRAG_TYPE.NONE;
				canvas.ReleaseMouseCapture();
			}
		}

		private void OnChangeImageScale(Canvas canvas, double scale_diff) {
			if (VideoInfo == null) return;

			if (canvas == LeftViewCanvas) {
				Point canvasCenter = new(LeftViewCanvas.ActualWidth / 2.0, LeftViewCanvas.ActualHeight / 2.0);

				Matrix curMatrix = LeftViewTransformGroup.Value;
				curMatrix.Invert();
				Point centerImage = curMatrix.Transform(canvasCenter);
				LeftViewScale.ScaleX *= scale_diff;
				LeftViewScale.ScaleY *= scale_diff;

				Matrix newMatrix = LeftViewTransformGroup.Value;
				Point newPosCanvas = newMatrix.Transform(centerImage);
				LeftViewTranslate.X += canvasCenter.X - newPosCanvas.X;
				LeftViewTranslate.Y += canvasCenter.Y - newPosCanvas.Y;
			}
			else if (canvas == RightViewCanvas) {
				Point canvasCenter = new(RightViewCanvas.ActualWidth / 2.0, RightViewCanvas.ActualHeight / 2.0);

				Matrix curMatrix = RightViewTransformGroup.Value;
				curMatrix.Invert();
				Point centerImage = curMatrix.Transform(canvasCenter);
				RightViewScale.ScaleX *= scale_diff;
				RightViewScale.ScaleY *= scale_diff;

				Matrix newMatrix = RightViewTransformGroup.Value;
				Point newPosCanvas = newMatrix.Transform(centerImage);
				RightViewTranslate.X += canvasCenter.X - newPosCanvas.X;
				RightViewTranslate.Y += canvasCenter.Y - newPosCanvas.Y;
			}
			else return;
		}
		private void OnChangeImageTranslate(Canvas canvas, INDEX3 pxl_pos) {
			if (VideoInfo == null) return;

			if (canvas == LeftViewCanvas) {
				var from_loc = LeftViewTransformGroup.Transform(new(_left_prev_drag_position.X, _left_prev_drag_position.Y));
				var to_loc = LeftViewTransformGroup.Transform(new(pxl_pos.X, pxl_pos.Y));

				LeftViewTranslate.X += to_loc.X - from_loc.X;
				LeftViewTranslate.Y += to_loc.Y - from_loc.Y;
				RightViewTranslate.X += to_loc.X - from_loc.X;
				RightViewTranslate.Y += to_loc.Y - from_loc.Y;
			}
			else if (canvas == RightViewCanvas) {
				var from_loc = RightViewTransformGroup.Transform(new(_left_prev_drag_position.X, _left_prev_drag_position.Y));
				var to_loc = RightViewTransformGroup.Transform(new(pxl_pos.X, pxl_pos.Y));

				LeftViewTranslate.X += to_loc.X - from_loc.X;
				LeftViewTranslate.Y += to_loc.Y - from_loc.Y;
				RightViewTranslate.X += to_loc.X - from_loc.X;
				RightViewTranslate.Y += to_loc.Y - from_loc.Y;
			}
			else return;

			InvalidateVisual();
		}
		private unsafe void OnBrushLabel(Canvas canvas, INDEX3 pxl_pos) {
			if (VideoInfo == null) return;


			//if (canvas == TopViewCanvas) {
			//	Matrix curMatrix = TopViewTransformGroup.Value;
			//	curMatrix.Invert();

			//	var t_idx_from = curMatrix.Transform(new Point(_drag_prev_position.x, _drag_prev_position.y));
			//	var t_idx_to = curMatrix.Transform(new Point(cur_pos.x, cur_pos.y));

			//	POINT3 idx_from = new() { x = (float)t_idx_from.X, y = (float)t_idx_from.Y, z = _drag_prev_position.z };
			//	POINT3 idx_to = new() { x = (float)t_idx_to.X, y = (float)t_idx_to.Y, z = cur_pos.z };

			//	Int64 radius = 3;
			//	Int64 x_from = Math.Clamp((Int64)Math.Min(idx_from.x, idx_to.x) - radius, 0, _img_info.ImgSize.w - 1);
			//	Int64 x_to = Math.Clamp((Int64)Math.Max(idx_from.x, idx_to.x) + radius, 0, _img_info.ImgSize.w - 1);
			//	Int64 y_from = Math.Clamp((Int64)Math.Min(idx_from.y, idx_to.y) - radius, 0, _img_info.ImgSize.h - 1);
			//	Int64 y_to = Math.Clamp((Int64)Math.Max(idx_from.y, idx_to.y) + radius, 0, _img_info.ImgSize.h - 1);
			//	Int64 z_from = Math.Clamp((Int64)Math.Min(idx_from.z, idx_to.z) - radius, 0, _img_info.ImgSize.d - 1);
			//	Int64 z_to = Math.Clamp((Int64)Math.Max(idx_from.z, idx_to.z) + radius, 0, _img_info.ImgSize.d - 1);

			//	POINT3 pt = new();
			//	for (pt.z = z_from; pt.z <= z_to; pt.z += 1)
			//		for (pt.y = y_from; pt.y <= y_to; pt.y += 1)
			//			for (pt.x = x_from; pt.x <= x_to; pt.x += 1) {
			//				double dist = MyMath.CalcDistance(idx_from, idx_to, pt);
			//				if (dist > radius)
			//					continue;

			//				Int64 idx = (Int64)pt.x + (Int64)pt.y * _img_info.ImgOffset.w + (Int64)pt.z * _img_info.ImgOffset.h;
			//				_img_info.LabDataPtr[idx] = (byte)(_img_info.LabDataPtr[idx] | 0b00000001);
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
			//		Point t_pt = new(_drag_prev_position.x, h_loc);
			//		t_pt = top_view_to_image_matrix.Transform(t_pt);
			//		idx_from.x = (float)t_pt.X;
			//		idx_from.y = (float)t_pt.Y;

			//		t_pt.X = 0;
			//		t_pt.Y = _drag_prev_position.y;
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
			//	Int64 x_from = Math.Clamp((Int64)Math.Min(idx_from.x, idx_to.x) - radius, 0, _img_info.ImgSize.w - 1);
			//	Int64 x_to = Math.Clamp((Int64)Math.Max(idx_from.x, idx_to.x) + radius, 0, _img_info.ImgSize.w - 1);
			//	Int64 y_from = Math.Clamp((Int64)Math.Min(idx_from.y, idx_to.y) - radius, 0, _img_info.ImgSize.h - 1);
			//	Int64 y_to = Math.Clamp((Int64)Math.Max(idx_from.y, idx_to.y) + radius, 0, _img_info.ImgSize.h - 1);
			//	Int64 z_from = Math.Clamp((Int64)Math.Min(idx_from.z, idx_to.z) - radius, 0, _img_info.ImgSize.d - 1);
			//	Int64 z_to = Math.Clamp((Int64)Math.Max(idx_from.z, idx_to.z) + radius, 0, _img_info.ImgSize.d - 1);

			//	FLOAT3 pt = new();
			//	for (pt.z = z_from; pt.z <= z_to; pt.z += 1)
			//		for (pt.y = y_from; pt.y <= y_to; pt.y += 1)
			//			for (pt.x = x_from; pt.x <= x_to; pt.x += 1) {
			//				double dist = MyMath.CalcDistance(idx_from, idx_to, pt);
			//				if (dist > radius)
			//					continue;

			//				Int64 idx = (Int64)pt.x + (Int64)pt.y * _img_info.ImgOffset.w + (Int64)pt.z * _img_info.ImgOffset.h;
			//				_img_info.LabDataPtr[idx] = (byte)(_img_info.LabDataPtr[idx] | 0b00000001);
			//			}
			//}
			//else return;

			InvalidateVisual();
		}
	}
}

