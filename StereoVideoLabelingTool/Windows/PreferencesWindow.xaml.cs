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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using StereoVideoLabelingTool.Classes;
using StereoVideoLabelingTool.Controls;


namespace StereoVideoLabelingTool.Windows
{
	public partial class PreferencesWindow : Window
	{
		public PreferencesWindow()
		{
			InitializeComponent();
		}
		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			string t_str = string.Empty;

			SourceBasePathSelectionControl.PathString =
				GlobalSettingManager.GetSetting("Path", "SourceBaseDir", out t_str) ?
				t_str : "";

			MajorThemeSelectionControl.SelectedString =
				GlobalSettingManager.GetSetting("Theme", "Major", out t_str) ?
				t_str : "Dark";

			MinorThemeSelectionControl.SelectedString =
				GlobalSettingManager.GetSetting("Theme", "Minor", out t_str) ?
				t_str : "Orange";
		}

		private void ConfirmButton_Click(object sender, RoutedEventArgs e)
		{
			bool is_changed = false;
			string t_str = string.Empty;

			GlobalSettingManager.GetSetting("Path", "SourceBaseDir", out t_str);
			is_changed |= t_str != SourceBasePathSelectionControl.PathString;

			GlobalSettingManager.GetSetting("Theme", "Major", out t_str);
			is_changed |= t_str != MajorThemeSelectionControl.SelectedString;

			GlobalSettingManager.GetSetting("Theme", "Minor", out t_str);
			is_changed |= t_str != MinorThemeSelectionControl.SelectedString;

			if (!is_changed)
			{
				Close();
				return;
			}

			if (MessageBox.Show(
				"This program will reboot.\n" +
				"Do you want to save the changes?",
				"Preferences",
				MessageBoxButton.YesNoCancel,
				MessageBoxImage.Question
				) != MessageBoxResult.Yes)
			{
				return;
			}

			GlobalSettingManager.SetSetting("Path", "SourceBaseDir", SourceBasePathSelectionControl.PathString);
			GlobalSettingManager.SetSetting("Theme", "Major", MajorThemeSelectionControl.SelectedString);
			GlobalSettingManager.SetSetting("Theme", "Minor", MinorThemeSelectionControl.SelectedString);

			DialogResult = true;
			Close();
		}
		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
