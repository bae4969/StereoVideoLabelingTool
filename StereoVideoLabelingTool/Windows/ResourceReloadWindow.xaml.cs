using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MahApps.Metro.Controls;
using StereoVideoLabelingTool.Classes;
using StereoVideoLabelingTool.Controls;
using Path = System.IO.Path;


namespace StereoVideoLabelingTool.Windows
{
	public partial class ResourceReloadWindow : Window
	{
		#region DependencyProperties

		public static readonly DependencyProperty CurrentInfoTextProperty =
			DependencyProperty.Register(
				"CurrentInfoText",
				typeof(string),
				typeof(ResourceReloadWindow),
				new PropertyMetadata("")
			);
		public static readonly DependencyProperty HistoryInfoTextProperty =
			DependencyProperty.Register(
				"HistoryInfoText",
				typeof(string),
				typeof(ResourceReloadWindow),
				new PropertyMetadata("")
			);

		public string CurrentInfoText
		{
			get => (string)GetValue(CurrentInfoTextProperty);
			set => SetValue(CurrentInfoTextProperty, value);
		}
		public string HistoryInfoText
		{
			get => (string)GetValue(HistoryInfoTextProperty);
			set => SetValue(HistoryInfoTextProperty, value);
		}

		#endregion

		private CancellationTokenSource _cancellation = null;

		////////////////////////////////////////////////////////////////



		////////////////////////////////////////////////////////////////

		public ResourceReloadWindow() {
			InitializeComponent();
		}

		private void ThemedWindow_Loaded(object sender, RoutedEventArgs e) {
			_cancellation?.Cancel();
			_cancellation = new();
			var token = _cancellation.Token;
			Dispatcher.InvokeAsync(async () => {
				try {
					// TODO
					string target_dir = "";

					List<string> target_data_list = new();
					{
						CurrentInfoText = $"Collecting resource file list ... ";
						HistoryInfoText += CurrentInfoText;

						if (!Directory.Exists(target_dir)) Directory.CreateDirectory(target_dir);

						foreach (var filepath in Directory.GetFiles(target_dir, "*.mp4", SearchOption.AllDirectories))
							target_data_list.Add(filepath);

						CurrentInfoText += $"Done";
						HistoryInfoText += $"Done\n";
					}

					Int64 total = target_data_list.Count;
					Int64 exe_cnt = 0;
					Int64 err_cnt = 0;
					Dictionary<string, string> data_name_map_new = new();
					{
						foreach (var data_path in target_data_list) {
							try {
								CurrentInfoText = $"Checking resource file ... ({exe_cnt}/{total})";
								HistoryInfoText += $"Loading {Path.GetFileName(data_path)} ... ";
								TotalProgressBarEdit.Value = 100.0 * exe_cnt / total;
								Title = $"Loading Resource ... {(int)TotalProgressBarEdit.Value}%";
								await Task.Delay(50, token);

								// TODO

								HistoryInfoText += $"Done\n";
							}
							catch (TaskCanceledException) {
								throw new TaskCanceledException();
							}
							catch (Exception ex) {
								HistoryInfoText += $"Fail [ {ex.Message} ]\n";
								err_cnt++;
							}
							finally { exe_cnt++; }
						}
					}

					TotalProgressBarEdit.Value = 100;
					Title = $"Loading Resource ... Done";
					if (err_cnt > 0) {
						CurrentInfoText = $"Complete [ Error Count : {err_cnt} ]";
						HistoryInfoText += $"Complete [ Error Count : {err_cnt} ]";
						if (MessageBox.Show(
							$"Some resources were fail to load.\n" +
							$"Do you still want to replace resources?",
							"Fail",
							MessageBoxButton.YesNoCancel,
							MessageBoxImage.Question
							) != MessageBoxResult.Yes) {
							return Task.CompletedTask;
						}
					}
					else {
						CurrentInfoText = $"Complete";
						HistoryInfoText += $"Complete";
					}

					GlobalMemoryManager.SetMemory("VideoList", data_name_map_new);

					for (int i = 10; i > 0; i--) {
						CancelButton.Content = $"Complete ({i})";
						await Task.Delay(999, token);
					}
					Close();
				}
				catch (TaskCanceledException) {
					CancelButton.Content = $"Close";
				}
				catch (Exception ex) {
					Title = $"Loading Resource ... Fail";
					CurrentInfoText = $"Fail to reload resource [ {ex.Message} ]";
					HistoryInfoText += $"Fail to reload resource [ {ex.Message} ]";
				}

				return Task.CompletedTask;

			}, DispatcherPriority.Background);
		}

		private void HistroyScrollViewer_LayoutUpdated(object sender, EventArgs e) {
			if (sender is not ScrollViewer sv) return;
			sv.ScrollToEnd();
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e) {
			_cancellation?.Cancel();
			Close();
		}
	}
}
