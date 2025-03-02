using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StereoVideoLabelingTool.Controls
{
	public partial class TaggedItemSearchControl : UserControl
	{
		#region DependencyProperties

		public event RoutedEventHandler SelectionChanged;

		#endregion

		private readonly List<string> _keys = new();
		private readonly Dictionary<string, List<string>> _tag_to_key_dict = new();

		private readonly List<ToggleButton> _toggle_buttons = new();
		private readonly HashSet<string> _selected_tags = new();

		private CancellationTokenSource _cancel_token_source = null;

		////////////////////////////////////////////////////////////////

		public void SetTaggedItem<ItemType>(Dictionary<string, List<ItemType>> item_dict, Func<ItemType, List<string>> tag_func) {
			_keys.Clear();
			_tag_to_key_dict.Clear();
			_toggle_buttons.Clear();

			foreach (var key_value in item_dict) {
				List<string> tags = new();
				foreach (var item in key_value.Value)
					tags.AddRange(tag_func(item));
				tags = tags.Distinct().ToList();

				_keys.Add(key_value.Key);
				foreach (var tag in tags) {
					if (_tag_to_key_dict.ContainsKey(tag) == false) {
						var btn = new ToggleButton {
							Content = tag,
							Margin = new(0, 0, 5, 0),
							IsChecked = false
						};
						btn.Checked += TagToggleButton_Changed;
						btn.Unchecked += TagToggleButton_Changed;

						_toggle_buttons.Add(btn);
						_tag_to_key_dict.Add(tag, new());
					}
					_tag_to_key_dict[tag].Add(key_value.Key);
				}
			}

			UpdateTagToggles();
			UpdateFilteredResults();
		}

		////////////////////////////////////////////////////////////////

		public TaggedItemSearchControl() {
			InitializeComponent();
		}

		private void ItemSearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
			UpdateFilteredResults();
		}
		private void TagSearchTextBox_TextChanged(object sender, TextChangedEventArgs e) {
			UpdateTagToggles();
		}
		private void TagToggleButton_Changed(object sender, RoutedEventArgs e) {
			if (sender is not ToggleButton tBtn) return;

			var tag = tBtn.Content.ToString();
			if (tBtn.IsChecked == true) {
				_selected_tags.Add(tag);
			}
			else {
				_selected_tags.Remove(tag);
			}

			UpdateTagToggles();
			UpdateFilteredResults();
		}
		private void ResultsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
			SelectionChanged?.Invoke(sender, e);
		}

		private void UpdateTagToggles() {
			string tag_search_text = TagSearchTextBox.Text;

			// 1) 태그 검색 (_tag_serach_string)에 맞게 필터
			var filtered = _toggle_buttons.Where(tb =>
				string.IsNullOrEmpty(tag_search_text) ||
				tb.Content.ToString().IndexOf(tag_search_text, StringComparison.OrdinalIgnoreCase) >= 0
			);

			// 2) 정렬
			//    - IsChecked = true(체크됨) 우선
			//    - 같은 그룹 내에서는 _tag_map[tag].Count 내림차순
			filtered = filtered
				.OrderByDescending(tb => tb.IsChecked == true)
				.ThenByDescending(tb => _tag_to_key_dict[tb.Content.ToString()].Count);

			// 3) ItemsControl에 표시
			var final_list = filtered.ToList();
			TagsPanel.ItemsSource = null;
			TagsPanel.ItemsSource = final_list;
		}
		private async void UpdateFilteredResults() {
			_cancel_token_source?.Cancel();
			_cancel_token_source = new();
			var token = _cancel_token_source.Token;
			try {
				await Task.Delay(1000, token);

				string item_search_text = ItemSearchTextBox.Text?.Trim().ToLower();

				// 1) 아이템 검색어 필터
				IEnumerable<string> filtered = _keys;
				if (!string.IsNullOrEmpty(item_search_text)) {
					filtered = filtered.Where(name =>
						name.ToLower().Contains(item_search_text));
				}

				// 2) 태그 필터 (AND 조건)
				if (_selected_tags.Count > 0) {
					var filtered_list = filtered.ToList();
					foreach (var tag in _selected_tags) {
						if (_tag_to_key_dict.TryGetValue(tag, out var itemList)) {
							filtered_list = filtered_list.Intersect(itemList).ToList();
						}
						else {
							filtered_list.Clear();
							break;
						}
					}
					filtered = filtered_list;
				}

				// 결과 정렬 후 표시
				ResultsListBox.ItemsSource = filtered.OrderBy(name => name).ToList();
			}
			catch { }
		}

	}
}
