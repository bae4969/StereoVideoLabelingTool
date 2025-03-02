using System;
using System.Collections.Generic;
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

namespace StereoVideoLabelingTool.Controls
{
	public partial class TagAndEditableAddControl : UserControl
	{
		#region DependencyProperties

		public static readonly DependencyProperty TagNameProperty =
			DependencyProperty.Register(
				"TagName",
				typeof(string),
				typeof(TagAndEditableAddControl),
				new PropertyMetadata("")
			);
		public static new readonly DependencyProperty BackgroundProperty =
			DependencyProperty.Register(
				"Background",
				typeof(SolidColorBrush),
				typeof(TagAndEditableAddControl),
				new PropertyMetadata(null)
			);
		public static readonly DependencyProperty BorderColorProperty =
			DependencyProperty.Register(
				"BorderColor",
				typeof(SolidColorBrush),
				typeof(TagAndEditableAddControl),
				new PropertyMetadata(null)
			);
		public static new readonly DependencyProperty BorderThicknessProperty =
			DependencyProperty.Register(
				"BorderThickness",
				typeof(double),
				typeof(TagAndEditableAddControl),
				new PropertyMetadata(null)
			);

		public string TagName
		{
			get => (string)GetValue(TagNameProperty);
			set => SetValue(TagNameProperty, value);
		}
		public new SolidColorBrush Background
		{
			get => (SolidColorBrush)GetValue(BackgroundProperty);
			set => SetValue(BackgroundProperty, value);
		}
		public SolidColorBrush BorderColor
		{
			get => (SolidColorBrush)GetValue(BorderColorProperty);
			set => SetValue(BorderColorProperty, value);
		}
		public new double BorderThickness
		{
			get => (double)GetValue(BorderThicknessProperty);
			set => SetValue(BorderThicknessProperty, value);
		}

		public EventHandler<string> AddTag;

		#endregion


		public TagAndEditableAddControl()
		{
			InitializeComponent();
		}

		private void BackgroundRect_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			TagTextBlock.Padding = new(BackgroundRect.ActualHeight / 4, 3, BackgroundRect.ActualHeight / 4, 3);
			AddButton.Margin = new(-BackgroundRect.ActualHeight / 4, 1, 3, 1);
			BackgroundRect.RadiusX = BackgroundRect.RadiusY = BackgroundRect.ActualHeight / 2;
		}
		private void AddButton_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			AddButton.Width = AddButton.ActualHeight;
		}
		private void AddButton_Click(object sender, RoutedEventArgs e)
		{
			AddTag?.Invoke(this, TagName);
		}
	}
}
