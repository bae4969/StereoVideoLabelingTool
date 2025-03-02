using ControlzEx.Theming;
using StereoVideoLabelingTool.Classes;
using System.Configuration;
using System.Data;
using System.Windows;

namespace StereoVideoLabelingTool
{
	public partial class App : Application
	{
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);


			if (GlobalSettingManager.GetSetting("Theme", "Major", out string major_theme) == false)
				major_theme = "Dark";

			if (GlobalSettingManager.GetSetting("Theme", "Minor", out string minor_theme) == false)
				minor_theme = "Orange";

			ThemeManager.Current.ChangeTheme(Application.Current, $"{major_theme}.{minor_theme}");
		}
	}
}
