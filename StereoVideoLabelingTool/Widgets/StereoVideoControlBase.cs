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
	#region XML Paser

	[XmlRoot("STREO_VIDEO_LABEL_INFO")]
	public class StereoVideoLabelInfoType
	{
		[XmlElement("RESOURCE_INFO")] public VideoInfoType VideoInfo { get; set; } = new();
		[XmlElement("LABEL_INFO")] public List<LabelInfoType> LabelInfoList { get; set; } = new();
	}

	public class VideoInfoType
	{
		[XmlAttribute("LEFT")] public string LeftFileName { get; set; } = string.Empty;
		[XmlAttribute("RIGHT")] public string RightFileName { get; set; } = string.Empty;
		[XmlAttribute("DATETIME")] public DateTime FileDateTime { get; set; } = DateTime.MinValue;
		[XmlAttribute("W")] public Int64 W { get; set; } = 0;
		[XmlAttribute("H")] public Int64 H { get; set; } = 0;
		[XmlAttribute("D")] public Int64 D { get; set; } = 0;
		[XmlAttribute("FPS")] public double Fps { get; set; } = 0.0;
		public Int64 Offset { get => W * H; }
	}

	public class LabelInfoType
	{
		[XmlAttribute("START")] public TimeSpan StartPoint { get; set; } = TimeSpan.MinValue;
		[XmlAttribute("END")] public TimeSpan EndPoint { get; set; } = TimeSpan.MinValue;
		[XmlElement("REGION")] public List<LabelRegionType> LabelRegionList { get; set; } = new();
	}

	public class LabelRegionType
	{
		[XmlAttribute("X")] public double X { get; set; } = 0.0;
		[XmlAttribute("Y")] public double Y { get; set; } = 0.0;
		[XmlAttribute("Z")] public double Z { get; set; } = 0.0;
		[XmlAttribute("W")] public double W { get; set; } = 0.0;
		[XmlAttribute("H")] public double H { get; set; } = 0.0;
		[XmlAttribute("D")] public double D { get; set; } = 0.0;
		[XmlAttribute("T")] public double Tilt { get; set; } = 0.0;
		[XmlAttribute("Y")] public double Yaw { get; set; } = 0.0;
		[XmlAttribute("R")] public double Roll { get; set; } = 0.0;
	}

	#endregion


	public class StereoVideoInfoType : SettingManager
	{
		private readonly ReaderWriterLockSlim _info_lock = new();
		public StereoVideoLabelInfoType StereoVideoLabelInfo = new();

		private VideoCapture _left_video_capture = null;
		private VideoCapture _right_video_capture = null;

		public SIZE3 ImgSize
		{
			get => new SIZE3 {
				w = StereoVideoLabelInfo?.VideoInfo.W ?? -1,
				h = StereoVideoLabelInfo?.VideoInfo.H ?? -1,
				d = StereoVideoLabelInfo?.VideoInfo.D ?? -1,
			};
		}
		public Int64 ImgOffset
		{
			get => StereoVideoLabelInfo?.VideoInfo.Offset ?? -1;
		}


		public int Load(string filename) {
			_info_lock.EnterWriteLock();
			try {
				var label_path = $"{filename}.label.stereo.video.xml";
				var left_path = $"{filename}.left.stereo.video.mp4";
				var right_path = $"{filename}.right.stereo.video.mp4";
				var setting_path = $"{filename}.setting.stereo.video.xml";

				_left_video_capture = new VideoCapture(left_path);
				if (!_left_video_capture.IsOpened()) throw new Exception($"Open Left Video | {left_path}");

				_right_video_capture = new VideoCapture(right_path);
				if (!_right_video_capture.IsOpened()) throw new Exception($"Open Right Video | {right_path}");

				StereoVideoLabelInfo = new();
				try {
					XmlSerializer serializer = new(typeof(StereoVideoLabelInfoType));
					StreamReader reader = new(label_path);
					StereoVideoLabelInfo = (StereoVideoLabelInfoType)serializer.Deserialize(reader);
				}
				catch (Exception ex) { throw new Exception($"{label_path} | Parse File | {ex.Message}"); }

				LoadSettings(setting_path);

				return 0;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to load data [ {ex.Message} ]");
				Unload();
				return -1;
			}
			finally { _info_lock.ExitWriteLock(); }
		}
		public bool Save(string filename) {
			_info_lock.EnterReadLock();
			try {
				if (StereoVideoLabelInfo == null) return false;

				var label_path = $"{filename}.label.stereo.video.xml";
				var left_path = $"{filename}.left.stereo.video.mp4";
				var right_path = $"{filename}.right.stereo.video.mp4";
				var setting_path = $"{filename}.setting.stereo.video.xml";

				try {
					XmlSerializer serializer = new(typeof(StereoVideoLabelInfoType));
					StreamWriter writer = new(label_path);
					XmlSerializerNamespaces ns = new();
					ns.Add("", "");
					serializer.Serialize(writer, StereoVideoLabelInfo, ns);
					writer.Close();
				}
				catch (Exception ex) { throw new Exception($"Save File | {label_path} | {ex.Message}"); }

				SaveSettings(setting_path);

				return true;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"Fail to save data [ {ex.Message} ]");
				return false;
			}
			finally {
				_info_lock.ExitReadLock();
			}
		}
		public void Unload() {
			_info_lock.EnterWriteLock();
			try {
				StereoVideoLabelInfo = null;
				_left_video_capture = null;
				_right_video_capture = null;
			}
			finally { _info_lock.ExitWriteLock(); }
		}
		public bool IsLoaded() {
			_info_lock.EnterReadLock();
			try {
				return StereoVideoLabelInfo != null;
			}
			finally { _info_lock.ExitReadLock(); }
		}
	}

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
