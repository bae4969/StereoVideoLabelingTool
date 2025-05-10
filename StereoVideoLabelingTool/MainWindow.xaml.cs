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
using Path = System.IO.Path;
using Window = System.Windows.Window;


namespace StereoVideoLabelingTool
{
	public partial class MainWindow : Window
	{
		#region EventHandler

		private event EventHandler<Tuple<string, string, string, string, string>> CreateVideoInfoEvent;      // 생성자에서 초기화 됨
		private event EventHandler<string> LoadVideoInfoEvent;       // 생성자에서 초기화 됨
		private event EventHandler<string> SaveVideoInfoEvent;       // 생성자에서 초기화 됨
		private event EventHandler UnloadVideoInfoEvent;             // 생성자에서 초기화 됨
		private event EventHandler SaveBackupVideoInfoEvent;         // 생성자에서 초기화 됨

		#endregion

		private readonly StereoVideoInfoType __stereo_video_info = new();
		private string _current_filename = string.Empty;

		////////////////////////////////////////////////////////////////

		private void ShowWaitIndicator(bool is_show) {
			// TODO
			//MainWaitIndicator.DeferedVisibility = is_show;
			this.IsEnabled = !is_show;
		}

		private bool OnCreateVideoInfo(Tuple<string, string, string, string, string> new_info) {
			try {
				if (__stereo_video_info.IsLoaded() == true)
					throw new Exception($"Already Loaded");

				// TODO

				//if (__stereo_video_info.Create(filename) == false)
				//	throw new Exception($"Load TV_LIB");

				//if (string.IsNullOrWhiteSpace(filename) == false &&
				//	__stereo_video_info.Load(filename) == false)
				//	throw new Exception($"Load Image");

				return true;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"라벨 모델 생성 실패 [ {ex.Message} ]");
				OnUnloadVideoInfo();

				return false;
			}
		}
		private bool OnLoadVideoInfo(string filename) {
			try {
				if (__stereo_video_info.IsLoaded() == true)
					throw new Exception($"Already Loaded");

				if (__stereo_video_info.Load(filename) == false)
					throw new Exception($"Load TV_LIB");

				return true;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"라벨 모델 불러오기 실패 [ {filename} | {ex.Message} ]");
				OnUnloadVideoInfo();

				return false;
			}
		}
		private bool OnSaveVideoInfo(string filename = null) {
			try {
				if (string.IsNullOrWhiteSpace(filename)) {
					GlobalSettingManager.GetSetting("Path", "LabelDir", out string label_dir); ;
					filename = $"{label_dir}\\{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.xml";
				}
				
				if (__stereo_video_info.Save(filename) == false)
					throw new Exception($"저장 실패");

				return true;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"라벨 모델 저장 실패 [ {filename} | {ex.Message} ]");
				return false;
			}
		}
		private void OnUnloadVideoInfo() {
			__stereo_video_info.Unload();

			GC.Collect();
		}
		private void OnSaveBackupVideoInfo() {
			try {
				Directory.CreateDirectory(Path.GetDirectoryName(MyConst.LAST_WORK_BACKUP_PATH));
				if (__stereo_video_info.Save(MyConst.LAST_WORK_BACKUP_PATH) == false)
					throw new Exception($"Fail to Backup");
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.WARNING, $"Fail to save backup [ {ex.Message} ]");
			}
		}

		private void OnInitializeWidgets() {
			StreoVideoViewer.OnInitialize(__stereo_video_info);
			RightTopContainer.OnInitialize(__stereo_video_info);
			RightBotContainer.OnInitialize(__stereo_video_info);
		}
		private void OnReleaseWidgets() {
			StreoVideoViewer.OnRelease();
			RightTopContainer.OnRelease();
			RightBotContainer.OnRelease();
		}
		private void OnUpdateWidgets(object sender, EventArgs e) {
			StreoVideoViewer.OnUpdate(sender, e);
			RightTopContainer.OnUpdate(sender, e);
			RightBotContainer.OnUpdate(sender, e);
		}

		private void OnRestartApp() {
			if (Environment.ProcessPath == null) {
				MessageBox.Show(
					"프로그램 재시작을 실패했습니다.\n" +
					"수동으로 재시작하시길 바랍니다.",
					"에러",
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
				Directory.CreateDirectory("./temp");
				Directory.CreateDirectory("./temp/debug");

				CreateVideoInfoEvent += async (s, e) => {
					bool is_good = true;
					try {
						ShowWaitIndicator(true);
						await Task.Run(() => {
							//WaitStateMessage = "Releasing Data";
							OnReleaseWidgets();
							OnUnloadVideoInfo();
							//WaitStateMessage = "Creating Data";
							if (OnCreateVideoInfo(e) == false) throw new Exception();
							//WaitStateMessage = "Initializing Window";
							OnInitializeWidgets();
						});
					}
					catch { is_good = false; }
					finally { ShowWaitIndicator(false); }
					if (is_good == true) return;

					MessageBox.Show("Fail to create parts library", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				};

				LoadVideoInfoEvent += async (s, e) => {
					bool is_good = true;
					try {
						ShowWaitIndicator(true);
						await Task.Run(() => {
							//WaitStateMessage = "Releasing Data";
							OnReleaseWidgets();
							OnUnloadVideoInfo();
							//WaitStateMessage = "Loading Data";
							if (OnLoadVideoInfo(e) == false) throw new Exception();
							//WaitStateMessage = "Initializing Window";
							OnInitializeWidgets();
						});
					}
					catch { is_good = false; }
					finally { ShowWaitIndicator(false); }
					if (is_good == true) return;

					MessageBox.Show("Fail to load parts library", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				};

				SaveVideoInfoEvent += async (s, e) => {
					bool is_good = true;
					try {
						StreoVideoViewer.Focus();
						ShowWaitIndicator(true);
						await Task.Run(() => {
							//WaitStateMessage = "Saving Data";
							if (OnSaveVideoInfo(e) == false) throw new Exception();
						});
					}
					catch { is_good = false; }
					finally { ShowWaitIndicator(false); }
					if (is_good == true) return;

					MessageBox.Show("Fail to save parts library", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				};

				UnloadVideoInfoEvent += async (s, e) => {
					bool is_good = true;
					try {
						ShowWaitIndicator(true);
						await Task.Run(() => {
							//WaitStateMessage = "Releasing Data";
							OnReleaseWidgets();
							OnUnloadVideoInfo();
						});
					}
					catch { is_good = false; }
					finally { ShowWaitIndicator(false); }
					if (is_good == true) return;

					MessageBox.Show("Fail to unload parts library", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				};

				SaveBackupVideoInfoEvent += async (s, e) => {
					try {
						await Task.Run(() => {
							OnSaveBackupVideoInfo();
						});
					}
					catch { }
				};
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to initialize window [ {ex.Message} ]");
				Close();
			}
		}
		private void Window_Loaded(object sender, RoutedEventArgs e) {
#if MY_DEBUG
			DevTestLoad.Visibility = Visibility.Visible;
			DevTestSave.Visibility = Visibility.Visible;
#endif

			Core.Initalize();
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
			UnloadVideoInfoEvent(sender, e);
		}
		private void SaveMenuItem_Click(object sender, RoutedEventArgs e) {
			if (!__stereo_video_info.IsLoaded()) return;
			SaveVideoInfoEvent.Invoke(sender, null);
		}
		private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e) {
			if (!__stereo_video_info.IsLoaded()) {
				MessageBox.Show("로딩된 라벨이 없음", "에러", MessageBoxButton.OK, MessageBoxImage.Error);
				return;
			}

			// TODO : 파일 선택 루틴
		}
		private void RestartMenuItem_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
					"프로그램을 재시작 하시겠습니까?",
					"재시작",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question
					)
				!= MessageBoxResult.Yes)
				return;

			OnRestartApp();
		}
		private void ExitMenuItem_Click(object sender, RoutedEventArgs e) {
			if (MessageBox.Show(
					"프로그램을 종료하시겠습니까?",
					"종료",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question
				)
				!= MessageBoxResult.Yes)
				return;

			UnloadVideoInfoEvent(sender, e);
			System.Windows.Application.Current.Shutdown();
		}

		private void PreferencesMenuItem_Click(object sender, RoutedEventArgs e) {
			PreferencesWindow pref_window = new() {
				Owner = Window.GetWindow(this),
			};
			if (pref_window.ShowDialog() != true) return;

			OnRestartApp();
		}
		private void ReloadResourceMenuItem_Click(object sender, RoutedEventArgs e) {
			// 로딩된 상태이면 저장할 건지 물어보기
			if (__stereo_video_info.IsLoaded()) {
				var ret = MessageBox.Show(
					"현재 열린 데이터의 변화값들은 모두 버려지게 됩니다.\n" +
					"그래도 종료하시겠습니까?",
					"경고",
					MessageBoxButton.YesNoCancel,
					MessageBoxImage.Question);

				switch (ret) {
					case MessageBoxResult.Yes:
						try {
							SaveVideoInfoEvent.Invoke(sender, null);
						}
						catch {
							MessageBox.Show("저장 실패", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
							return;
						}
						break;
					case MessageBoxResult.No: break;
					default: return;
				}

				UnloadVideoInfoEvent(sender, e);
			}

			// TODO
		}

		private void DevTestLoad_Click(object sender, RoutedEventArgs e) {
			LoadVideoInfoEvent.Invoke(sender, "./temp/debug/origin/20250406_202319.xml");
		}
		private void DevTestSave_Click(object sender, RoutedEventArgs e) {
			SaveVideoInfoEvent.Invoke(sender, "./temp/debug/delta/20250406_202319.xml");
		}
		private void DevTestExec_Click(object sender, RoutedEventArgs e) {
		}

		private void LabelFileManagerControl_SelectedFIle(object sender, string e) {

		}
	}
}