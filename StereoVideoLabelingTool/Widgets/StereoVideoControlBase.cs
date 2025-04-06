using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using OpenCvSharp;
using StereoVideoLabelingTool.Classes;

namespace StereoVideoLabelingTool.Widgets
{
	public abstract class StereoVideoControlBase : UserControl
	{
		public event EventHandler UpdateAllWidget;
		protected void UpdateAllWidgetEvent(object sender, EventArgs e) {
			UpdateAllWidget?.Invoke(sender, e);
		}

		private StereoVideoInfoType _video_info = null;
		protected StereoVideoInfoType VideoInfo => _video_info;


		public void OnInitialize(StereoVideoInfoType video_info) {
			_video_info = video_info;
			if (_video_info == null) return;
			if (!Dispatcher.CheckAccess()) Dispatcher.Invoke(() => Initialize());
			else { Initialize(); }
		}
		public void OnRelease() {
			_video_info = null;
			if (!Dispatcher.CheckAccess()) Dispatcher.Invoke(() => Release());
			else { Release(); }
		}
		public void OnUpdate(object sender, EventArgs e) {
			if (_video_info == null || sender == this) return;
			if (!Dispatcher.CheckAccess()) Dispatcher.Invoke(() => Update(sender, e));
			else { Update(sender, e); }
		}
		protected void RiseUpdateEvent(object sender, EventArgs e) {
			UpdateAllWidget?.Invoke(sender, e);
		}

		protected abstract void Initialize();
		protected abstract void Release();
		protected abstract void Update(object sender, EventArgs e);

#if CHILD_SAMPLE_CODE
		protected override void Initialize()
		{
			try
			{
				
			}
			catch (Exception ex)
			{
				Release();
				Logger.Print(LOG_TYPE.WARNING, $"Fail to initialize widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Release()
		{
			try
			{
				
			}
			catch (Exception ex)
			{
				Logger.Print(LOG_TYPE.ERROR, $"Fail to release widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
		protected override void Update(object sender, EventArgs e)
		{
			try
			{

			}
			catch (Exception ex)
			{
				Logger.Print(LOG_TYPE.WARNING, $"Fail to update widget control [ {this.GetType().Name} | {ex.Message} ]");
			}
		}
#endif
	}
}
