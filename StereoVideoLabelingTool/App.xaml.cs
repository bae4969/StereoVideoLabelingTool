using ControlzEx.Theming;
using StereoVideoLabelingTool.Classes;
using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace StereoVideoLabelingTool
{
	public partial class App : Application
	{
		static void check_settings() {
			GlobalSettingManager.GetSetting("Path", "LabelDir", out string label_dir);
			if (string.IsNullOrWhiteSpace(label_dir) ||
				!Path.Exists(label_dir)) {
				label_dir = "D:\\res\\STEREO_VIDEO";
				GlobalSettingManager.SetSetting("Path", "LabelDir", label_dir);
			}
		}
		static void check_theme() {
			if (GlobalSettingManager.GetSetting("Theme", "Major", out string major_theme) == false)
				major_theme = "Dark";

			if (GlobalSettingManager.GetSetting("Theme", "Minor", out string minor_theme) == false)
				minor_theme = "Orange";

			ThemeManager.Current.ChangeTheme(Application.Current, $"{major_theme}.{minor_theme}");
		}
		static void set_env_path() {
			string currentPath = Environment.GetEnvironmentVariable("PATH");
			string newPath = currentPath + @";./bin";
			Environment.SetEnvironmentVariable("PATH", newPath, EnvironmentVariableTarget.Process);
		}

		protected override void OnStartup(StartupEventArgs e) {
			check_settings();
			check_theme();
			set_env_path();
		}
	}
}
