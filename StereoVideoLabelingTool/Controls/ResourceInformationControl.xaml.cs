using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
using System.Xml.Serialization;
using StereoVideoLabelingTool.Classes;

namespace StereoVideoLabelingTool.Controls
{
	public partial class ResourceInformationControl : UserControl
	{
		#region DependencyProperties

		public static readonly DependencyProperty InfoTextProperty =
			DependencyProperty.Register(
				"InfoText",
				typeof(string),
				typeof(ResourceInformationControl),
				new PropertyMetadata("")
			);
		public string InfoText
		{
			get => (string)GetValue(InfoTextProperty);
			set => SetValue(InfoTextProperty, value);
		}

		#endregion

		////////////////////////////////////////////////////////////////

		public void LoadResourceInformation<T>(T res_obj, Func<T, string> res_text_func) {
			InfoText = res_text_func(res_obj);
		}
		public void ClearResourceInformation() {
			InfoText = "";
		}

		////////////////////////////////////////////////////////////////

		public ResourceInformationControl() {
			InitializeComponent();
		}
	}
}
