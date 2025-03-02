using System.Diagnostics;
using System.IO;
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
using System.Xml;
using ControlzEx.Theming;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using OpenCvSharp;
using StereoVideoLabelingTool.Classes;
using StereoVideoLabelingTool.Controls;
using StereoVideoLabelingTool.Widgets;
using StereoVideoLabelingTool.Windows;
using Window = System.Windows.Window;


namespace StereoVideoLabelingTool
{
	public partial class MainWindow : Window
	{
		private readonly StereoVideoInfoType _stereo_video_info = new();
		private string _current_filename = string.Empty;

		////////////////////////////////////////////////////////////////

		private void ShowWaitIndicator(bool is_show) {
			// TODO
			//MainWaitIndicator.DeferedVisibility = is_show;
			this.IsEnabled = !is_show;
		}

		private async Task<bool> OnLoadData(string filename) {
			ShowWaitIndicator(true);
			var ret = await Task<bool>.Run(() => {
				try {
					int load_result = _stereo_video_info.Load(filename);
					if (load_result < 0)
						throw new Exception($"{load_result}");

					_current_filename = filename;

					return true;
				}
				catch (Exception ex) {
					Logger.Print(LOG_TYPE.ERROR, $"Fail to load label info [ {filename} | {ex.Message} ]");
					OnUnloadData();

					return false;
				}
			});
			ShowWaitIndicator(false);
			return ret;
		}
		private async Task<bool> OnSaveData(string filename) {
			ShowWaitIndicator(true);
			var ret = await Task<bool>.Run(() => {
				try {
					bool save_result = _stereo_video_info.Save(filename);
					if (save_result == false)
						throw new Exception($"{save_result}");

					_current_filename = filename;

					return true;
				}
				catch (Exception ex) {
					Logger.Print(LOG_TYPE.ERROR, $"Fail to save label info [ {filename} | {ex.Message} ]");
					return false;
				}
			});
			ShowWaitIndicator(false);
			return ret;
		}
		private void OnUnloadData() {
			_stereo_video_info.Unload();
			_current_filename = string.Empty;

			GC.Collect();
		}

		private void OnInitializeWidgets() {
			StreoVideoViewer.OnInitialize(_stereo_video_info);
			//LeftTopContainer.OnInitialize(_stereo_video_info);
			RightTopContainer.OnInitialize(_stereo_video_info);
			RightBotContainer.OnInitialize(_stereo_video_info);
		}
		private void OnReleaseWidgets() {
			StreoVideoViewer.OnRelease();
			//LeftTopContainer.OnRelease();
			RightTopContainer.OnRelease();
			RightBotContainer.OnRelease();
		}
		private void OnUpdateWidgets(object sender, EventArgs e) {
			StreoVideoViewer.OnUpdate(sender, e);
			//LeftTopContainer.OnUpdate(sender, e);
			RightTopContainer.OnUpdate(sender, e);
			RightBotContainer.OnUpdate(sender, e);
		}

		private void OnRestartApp() {
			if (Environment.ProcessPath == null) {
				MessageBox.Show(
					"Fail to restart the application\n" +
					"Please restart manually",
					"Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error
				);
				return;
			}

			Process.Start(Environment.ProcessPath);
			System.Windows.Application.Current.Shutdown();
		}

		////////////////////////////////////////////////////////////////

		public MainWindow() {
			InitializeComponent();

			try {
				string before_path_env = Environment.GetEnvironmentVariable("Path")??"";
				Environment.SetEnvironmentVariable("Path", before_path_env + "./bin;");
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to initialize window [ {ex.Message} ]");
				Close();
			}
		}
		private void Window_Loaded(object sender, RoutedEventArgs e) {

		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {

		}

		private void NewMenuItem_Click(object sender, RoutedEventArgs e) {
			// TODO : 새파일 생성 루틴을 생성해야함
		}
		private void OpenMenuItem_Click(object sender, RoutedEventArgs e) {
			// TODO : 파일 선택 루틴
		}
		private void CloseMenuItem_Click(object sender, RoutedEventArgs e) {
			try {
				OnReleaseWidgets();
				OnUnloadData();
			}
			catch {
				MessageBox.Show("Fail to close label info", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private async void SaveMenuItem_Click(object sender, RoutedEventArgs e) {
			try {
				if (_stereo_video_info.IsLoaded() &&
					!await OnSaveData(_current_filename))
					throw new Exception();
			}
			catch {
				MessageBox.Show("Fail to save label info", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e) {
			// TODO : 파일 선택 루틴
		}
		private void RestartMenuItem_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
					"Do you want to restart the application?",
					"Restart",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question
					)
				!= MessageBoxResult.Yes)
				return;

			OnRestartApp();
		}
		private void ExitMenuItem_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
					"Do you want to exit the application?",
					"Exit",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question
				)
				!= MessageBoxResult.Yes)
				return;

			OnReleaseWidgets();
			OnUnloadData();
			System.Windows.Application.Current.Shutdown();
		}

		private void PreferencesMenuItem_Click(object sender, RoutedEventArgs e) {
			PreferencesWindow pref_window = new() {
				Owner = Window.GetWindow(this),
			};
			if (pref_window.ShowDialog() != true) return;

			OnRestartApp();
		}
		private async void ReloadResourceMenuItem_Click(object sender, RoutedEventArgs e) {
			// 로딩된 상태이면 저장할 건지 물어보기
			if (_stereo_video_info.IsLoaded()) {
				var ret = MessageBox.Show(
					"Changing of current opened data will be discarded.\n" +
					"Would you like to save the data and proceed?",
					"Warning",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Question);

				switch (ret) {
					case MessageBoxResult.Yes:
						try {
							if (!await OnSaveData(_current_filename)) throw new Exception();
						}
						catch {
							MessageBox.Show("Fail to save label info", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							return;
						}
						break;
					case MessageBoxResult.No: break;
					default: return;
				}

				OnReleaseWidgets();
				OnUnloadData();
			}

			// TODO
		}

		private async void DevTestLoad_Click(object sender, RoutedEventArgs e) {
			try {
				OnUnloadData();
				await OnLoadData("test");
				OnInitializeWidgets();
			}
			catch {
				MessageBox.Show("Fail to load label info", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private async void DevTestSave_Click(object sender, RoutedEventArgs e) {
			try {
				await OnSaveData("./test/test.data");
			}
			catch {
				MessageBox.Show("Fail to save label info", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}
		private void DevTest2_Click(object sender, RoutedEventArgs e) {
		}
		private void DevTest3_Click(object sender, RoutedEventArgs e) {

		}

		private void LabelFileManagerControl_SelectedFIle(object sender, string e) {

		}
	}
}