using ImageLabelTool.Classes;
using System;
using System.Collections.Generic;
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
using System.Xml;

namespace ImageLabelTool.Controls
{
	public partial class ImageLutController : ImageControlBase
	{
		public override unsafe void OnLoadImageInfo(ImageInfoType img_info, Action on_update_image_info) {
			base.SetImageInfo(img_info, on_update_image_info);
			if (__img_info == null) return;

			if (Application.Current.Dispatcher.CheckAccess() == false)
				Application.Current.Dispatcher.Invoke(() => { OnUpdateImageInfo(); });
			else {
				UnregistValueChangeEvent();
				try {
					switch (__img_info.ImgType) {
						case IMG_TYPE.GRAY:
							MinValueSlider.Minimum = MaxValueSlider.Minimum = 0;
							MinValueSlider.Maximum = MaxValueSlider.Maximum = 65535;
							MinValueSlider.Value = 0;
							MaxValueSlider.Value = 65535;
							break;
						case IMG_TYPE.COLOR:
							MinValueSlider.Minimum = MaxValueSlider.Minimum = 0;
							MinValueSlider.Maximum = MaxValueSlider.Maximum = 255;
							MinValueSlider.Value = 0;
							MaxValueSlider.Value = 255;
							break;
						default: return;
					}
					BlendValueSlider.Value = 0.3;

					__img_info.SetXmlValue("LUT", "MIN", MinValueSlider.Value);
					__img_info.SetXmlValue("LUT", "MAX", MaxValueSlider.Value);
					__img_info.SetXmlValue("LUT", "BLEND", BlendValueSlider.Value);
				}
				catch (Exception ex) {
					OnUnloadImageInfo();
					Logger.Print(LOG_TYPE.ERROR, $"Fail to load image in ImageLabelViewer Control [ {ex.Message} ]");
				}
				finally { RegistValueChangeEvent(); }
			}
		}
		public override unsafe void OnUnloadImageInfo() {
			base.SetImageInfo(null, null);

			if (Application.Current.Dispatcher.CheckAccess() == false)
				Application.Current.Dispatcher.Invoke(() => { OnUpdateImageInfo(); });
			else {
				UnregistValueChangeEvent();
				try {

				}
				catch (Exception ex) {
					Logger.Print(LOG_TYPE.ERROR, $"Fail to unload image in ImageLabelViewer Control [ {ex.Message} ]");
					throw new Exception($"ImageLabelViewer | {ex.Message}");
				}
				finally { RegistValueChangeEvent(); }
			}
		}
		public override unsafe void OnUpdateImageInfo() {
			if (__img_info == null) return;

			if (Application.Current.Dispatcher.CheckAccess() == false)
				Application.Current.Dispatcher.Invoke(() => { OnUpdateImageInfo(); });
			else {
				UnregistValueChangeEvent();
				try {
					if (__img_info.GetXmlValue("LUT", "MIN", out double t_min)) MinValueSlider.Value = t_min;
					if (__img_info.GetXmlValue("LUT", "MAX", out double t_max)) MaxValueSlider.Value = t_max;
					if (__img_info.GetXmlValue("LUT", "BLEND", out double t_blend)) BlendValueSlider.Value = t_blend;
				}
				finally { RegistValueChangeEvent(); }
			}
		}

		////////////////////////////////////////////////////////////////

		public ImageLutController() {
			InitializeComponent();
		}
		private void RegistValueChangeEvent() {
			MinValueSlider.ValueChanged += ValueSlider_ValueChanged;
			MaxValueSlider.ValueChanged += ValueSlider_ValueChanged;
		}
		private void UnregistValueChangeEvent() {
			MinValueSlider.ValueChanged -= ValueSlider_ValueChanged;
			MaxValueSlider.ValueChanged -= ValueSlider_ValueChanged;
		}

		private void ValueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
			if (__img_info == null ||
				__on_update_img_info == null ||
				e.OldValue == e.NewValue ||
				sender is not Slider slider)
				return;

			if (MinValueSlider.Value > MaxValueSlider.Value) {
				if (slider == MinValueSlider) {
					MaxValueSlider.Value = MinValueSlider.Value;
				}
				else if (slider == MaxValueSlider) {
					MinValueSlider.Value = MaxValueSlider.Value;
				}
			}

			__img_info.SetXmlValue("LUT", "MIN", MinValueSlider.Value);
			__img_info.SetXmlValue("LUT", "MAX", MaxValueSlider.Value);
			__img_info.SetXmlValue("LUT", "BLEND", BlendValueSlider.Value);
			__on_update_img_info();
		}
	}
}
