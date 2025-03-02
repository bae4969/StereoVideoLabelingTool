using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
	public partial class VerticalContainerControl : StereoVideoControlBase
	{
		#region DependencyProperties

		public static readonly DependencyProperty WidgetsProperty =
			DependencyProperty.Register(
				"Widgets",
				typeof(ObservableCollection<object>),
				typeof(VerticalContainerControl),
				new PropertyMetadata(null)
			);

		public ObservableCollection<object> Widgets
		{
			get => (ObservableCollection<object>)GetValue(WidgetsProperty);
			set => SetValue(WidgetsProperty, value);
		}

		#endregion

		////////////////////////////////////////////////////////////////

		protected override void Initialize() {
			try {
				foreach (var control in Widgets) {
					if (control is not StereoVideoControlBase widget) return;
					try { widget.OnInitialize(VideoInfo); }
					catch (Exception ex) {
						widget.OnRelease();
						Logger.Print(LOG_TYPE.ERROR, $"Fail to initialize widget '{widget}' [ {ex.Message} ]");
					}
				}
			}
			catch (Exception ex) {
				Release();
				Logger.Print(LOG_TYPE.WARNING, $"Fail to initialize widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Release() {
			try {
				foreach (var control in Widgets) {
					if (control is not StereoVideoControlBase widget) continue;
					try { widget.OnRelease(); }
					catch (Exception ex) {
						Logger.Print(LOG_TYPE.ERROR, $"Fail to release widget '{widget}' [ {ex.Message} ]");
					}
				}
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to release widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Update(object sender, EventArgs e) {
			try {
				foreach (var control in Widgets) {
					if (control is not StereoVideoControlBase widget) continue;
					try { widget.OnUpdate(sender, e); }
					catch (Exception ex) {
						Logger.Print(LOG_TYPE.ERROR, $"Fail to update widget '{widget}' [ {ex.Message} ]");
					}
				}
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.WARNING, $"Fail to update widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}

		////////////////////////////////////////////////////////////////

		public VerticalContainerControl()
        {
            InitializeComponent();
			Widgets = [];
		}
    }
}
