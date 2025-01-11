using ImageLabelTool.Classes;
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

namespace ImageLabelTool.Controls
{
    public partial class VerticalImageControlContainer : ImageControlBase
	{
		public static readonly DependencyProperty ItemsProperty =
			DependencyProperty.Register(
				"Items",
				typeof(ObservableCollection<object>),
				typeof(VerticalImageControlContainer),
				new PropertyMetadata(null)
			);

		public ObservableCollection<object> Items
		{
			get => (ObservableCollection<object>)GetValue(ItemsProperty);
			set => SetValue(ItemsProperty, value);
		}

		////////////////////////////////////////////////////////////////

		public override unsafe void OnLoadImageInfo(ImageInfoType img_info, Action on_update_image_info) {
			base.SetImageInfo(img_info, on_update_image_info);
			foreach(var obj in Items) {
				if (obj is not ImageControlBase control) return;
				try { control.OnLoadImageInfo(img_info, on_update_image_info); }
				catch (Exception ex) {
					control.OnUnloadImageInfo();
					Logger.Print(LOG_TYPE.ERROR, $"Fail to load image info in '{obj}' Control [ {ex.Message} ]");
				}
			}
		}
		public override unsafe void OnUnloadImageInfo() {
			base.SetImageInfo(null, null);
			foreach (var obj in Items) {
				if (obj is not ImageControlBase control) return;
				try { control.OnUnloadImageInfo(); }
				catch (Exception ex) {
					Logger.Print(LOG_TYPE.ERROR, $"Fail to unload image info in '{obj}' Control [ {ex.Message} ]");
				}
			}
		}
		public override unsafe void OnUpdateImageInfo() {
			foreach (var obj in Items) {
				if (obj is not ImageControlBase control) return;
				try { control.OnUpdateImageInfo(); }
				catch (Exception ex) {
					Logger.Print(LOG_TYPE.ERROR, $"Fail to update image info in '{obj}' Control [ {ex.Message} ]");
				}
			}
		}

		////////////////////////////////////////////////////////////////

		public VerticalImageControlContainer()
        {
            InitializeComponent();
			Items = [];
		}
    }
}
