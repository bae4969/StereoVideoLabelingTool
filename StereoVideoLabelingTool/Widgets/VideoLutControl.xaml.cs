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
using StereoVideoLabelingTool.Classes;
using StereoVideoLabelingTool.Widgets;

namespace StereoVideoLabelingTool.Widgets
{
	public partial class VideoLutControl : StereoVideoControlBase
	{
		////////////////////////////////////////////////////////////////
		
		protected override void Initialize() {
			try {
				UnregistValueChangeEvent();

				MinValueSlider.Minimum = MaxValueSlider.Minimum = 0;
				MinValueSlider.Maximum = MaxValueSlider.Maximum = 255;
				MinValueSlider.Value = 0;
				MaxValueSlider.Value = 255;
				BlendValueSlider.Value = 0.3;

				VideoInfo.SetSetting("LUT", "MIN", MinValueSlider.Value);
				VideoInfo.SetSetting("LUT", "MAX", MaxValueSlider.Value);
				VideoInfo.SetSetting("LUT", "BLEND", BlendValueSlider.Value);
			}
			catch (Exception ex) {
				Release();
				Logger.Print(LOG_TYPE.WARNING, $"Fail to initialize widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
			finally { RegistValueChangeEvent(); }
		}
		protected override void Release() {
			try {

			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to release widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Update(object sender, EventArgs e) {
			try {
				UnregistValueChangeEvent();

				if (VideoInfo.GetSetting("LUT", "MIN", out double t_min)) MinValueSlider.Value = t_min;
				if (VideoInfo.GetSetting("LUT", "MAX", out double t_max)) MaxValueSlider.Value = t_max;
				if (VideoInfo.GetSetting("LUT", "BLEND", out double t_blend)) BlendValueSlider.Value = t_blend;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.WARNING, $"Fail to update widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
			finally { RegistValueChangeEvent(); }
		}

		////////////////////////////////////////////////////////////////

		public VideoLutControl() {
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
			if (VideoInfo == null ||
				e.OldValue == e.NewValue ||
				sender is not Slider slider)
				return;

			try {
				UnregistValueChangeEvent();

				if (MinValueSlider.Value > MaxValueSlider.Value) {
					if (slider == MinValueSlider) {
						MaxValueSlider.Value = MinValueSlider.Value;
					}
					else if (slider == MaxValueSlider) {
						MinValueSlider.Value = MaxValueSlider.Value;
					}
				}

				VideoInfo.SetSetting("LUT", "MIN", MinValueSlider.Value);
				VideoInfo.SetSetting("LUT", "MAX", MaxValueSlider.Value);
				VideoInfo.SetSetting("LUT", "BLEND", BlendValueSlider.Value);

				RiseUpdateEvent(this, null);
			}
			finally { RegistValueChangeEvent(); }
		}
	}
}
