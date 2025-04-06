using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Xml.Serialization;

namespace StereoVideoLabelingTool.Classes
{
	public class INDEX3 : LOC_TYPE_3<long> { public INDEX3(long _x = default, long _y = default, long _z = default) : base(_x, _y, _z) { } };
	public class SIZE3 : SIZE_TYPE_3<long> { public SIZE3(long _x = default, long _y = default, long _z = default) : base(_x, _y, _z) { } };
	public class SPACING3 : LOC_TYPE_3<double> { public SPACING3(double _x = default, double _y = default, double _z = default) : base(_x, _y, _z) { } };
	public class POINT3 : LOC_TYPE_3<double> { public POINT3(double _x = default, double _y = default, double _z = default) : base(_x, _y, _z) { } };
	public class INT3 : LOC_TYPE_3<int> { public INT3(int _x = default, int _y = default, int _z = default) : base(_x, _y, _z) { } };
	public class INT643 : LOC_TYPE_3<long> { public INT643(long _x = default, long _y = default, long _z = default) : base(_x, _y, _z) { } };
	public class FLOAT3 : LOC_TYPE_3<float> { public FLOAT3(float _x = default, float _y = default, float _z = default) : base(_x, _y, _z) { } };
	public class DOUBLE3 : LOC_TYPE_3<double> { public DOUBLE3(double _x = default, double _y = default, double _z = default) : base(_x, _y, _z) { } };
	public class LOC_TYPE_3<T>
	{
		public T X, Y, Z;

		public LOC_TYPE_3(T _x = default, T _y = default, T _z = default) { X = _x; Y = _y; Z = _z; }

		#region key값 사용을 위한 함수
		public bool Equals(LOC_TYPE_3<T> other) {
			return
				X.Equals(other.X) &&
				Y.Equals(other.Y) &&
				Z.Equals(other.Z);
		}
		public override bool Equals(object obj) {
			return obj is LOC_TYPE_3<T> other && Equals(other);
		}
		public override int GetHashCode() {
			return HashCode.Combine(X, Y, Z);
		}
		public static bool operator ==(LOC_TYPE_3<T> left, LOC_TYPE_3<T> right) {
			return left.Equals(right);
		}
		public static bool operator !=(LOC_TYPE_3<T> left, LOC_TYPE_3<T> right) {
			return !(left == right);
		}
		#endregion
	};
	public class SIZE_TYPE_3<T>
	{
		public T W, H, D;

		public SIZE_TYPE_3(T _w = default, T _h = default, T _d = default) { W = _w; H = _h; D = _d; }

		#region key값 사용을 위한 함수
		public bool Equals(SIZE_TYPE_3<T> other) {
			return
				W.Equals(other.W) &&
				H.Equals(other.H) &&
				D.Equals(other.D);
		}
		public override bool Equals(object obj) {
			return obj is SIZE_TYPE_3<T> other && Equals(other);
		}
		public override int GetHashCode() {
			return HashCode.Combine(W, H, D);
		}
		public static bool operator ==(SIZE_TYPE_3<T> left, SIZE_TYPE_3<T> right) {
			return left.Equals(right);
		}
		public static bool operator !=(SIZE_TYPE_3<T> left, SIZE_TYPE_3<T> right) {
			return !(left == right);
		}
		#endregion
	};


	public struct StringPairType
	{
		public string Outter { get; set; }
		public object Inner { get; set; }

		public StringPairType() {
			Outter = string.Empty;
			Inner = string.Empty;
		}
	}

	public struct VersionType
	{
		public int Major { get; }
		public int Minor { get; }
		public int Patch { get; }
		public int Build { get; }

		public VersionType(int major, int minor, int patch, int build) {
			Major = major;
			Minor = minor;
			Patch = patch;
			Build = build;
		}
		public VersionType(string version) {
			string[] parts = version.Split('.');
			if (parts.Length != 4)
				throw new ArgumentException("Invalid version format");
			Major = int.Parse(parts[0]);
			Minor = int.Parse(parts[1]);
			Patch = int.Parse(parts[2]);
			Build = int.Parse(parts[3]);
		}
		public VersionType Clone() {
			return new(Major, Minor, Patch, Build);
		}

		public string ToDateString(bool spliter = false) {
			return
				spliter ?
				$"{Major:D4}-{Minor:D2}-{Patch:D2}-{Build:D2}" :
				$"{Major:D4}{Minor:D2}{Patch:D2}{Build:D2}";
		}
		public string ToVerString(bool spliter = false) {
			return
				spliter ?
				$"{Major}.{Minor}.{Patch}.{Build}" :
				$"{Major}{Minor}{Patch}{Build}";
		}

		public int MatchVersion(VersionType other) {
			bool major = Major == other.Major;
			bool minor = Minor == other.Minor;
			bool patch = Patch == other.Patch;
			bool build = Build == other.Build;
			if (major & minor & patch & build)
				return 4;
			else if (major & minor & patch)
				return 3;
			else if (major & minor)
				return 2;
			else if (major)
				return 1;
			else
				return 0;
		}

		#region key값 사용을 위한 함수
		public bool Equals(VersionType other) {
			return
				Major == other.Major &&
				Minor == other.Minor &&
				Patch == other.Patch &&
				Build == other.Build;
		}
		public override bool Equals(object obj) {
			return obj is VersionType other && Equals(other);
		}
		public override int GetHashCode() {
			return HashCode.Combine(Major, Minor, Patch, Build);
		}
		public static bool operator ==(VersionType left, VersionType right) {
			return left.Equals(right);
		}
		public static bool operator !=(VersionType left, VersionType right) {
			return !(left == right);
		}
		#endregion
	}

	#region XML Paser

	[XmlRoot("STREO_VIDEO_LABEL_INFO")]
	public class StereoVideoLabelInfoType
	{
		[XmlElement("RESOURCE_INFO")] public VideoInfoType VideoInfo { get; set; } = new();
		[XmlElement("LABEL_INFO")] public List<LabelInfoType> LabelInfoList { get; set; } = new();
	}

	public class VideoInfoType
	{
		[XmlAttribute("W")] public Int64 W { get; set; } = 0;
		[XmlAttribute("H")] public Int64 H { get; set; } = 0;
		[XmlAttribute("D")] public Int64 D { get; set; } = 0;
		[XmlAttribute("FPS")] public double Fps { get; set; } = 0.0;
		[XmlIgnore] public Int64 Offset { get => W * H; }
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
		[XmlAttribute("TILT")] public double Tilt { get; set; } = 0.0;
		[XmlAttribute("YAW")] public double Yaw { get; set; } = 0.0;
		[XmlAttribute("ROLL")] public double Roll { get; set; } = 0.0;
	}

	#endregion

	public class StereoVideoInfoType : SettingManager
	{
		private readonly ReaderWriterLockSlim __info_lock = new();
		public StereoVideoLabelInfoType StereoVideoLabelInfo = null;

		public VideoCapture LeftVideoCapture = null;
		public VideoCapture RightVideoCapture = null;

		public SIZE3 ImgSize
		{
			get => new SIZE3 {
				W = StereoVideoLabelInfo?.VideoInfo.W ?? -1,
				H = StereoVideoLabelInfo?.VideoInfo.H ?? -1,
				D = StereoVideoLabelInfo?.VideoInfo.D ?? -1,
			};
		}
		public Int64 ImgOffset
		{
			get => StereoVideoLabelInfo?.VideoInfo.Offset ?? -1;
		}


		public bool Load(string filename) {
			__info_lock.EnterWriteLock();
			try {
				var (ext_list, prefix) = MyFunc.GetExtensionList(filename);
				var label_path = $"{prefix}.label.stereo.video.xml";
				var left_path = $"{prefix}.left.stereo.video.mp4";
				var right_path = $"{prefix}.right.stereo.video.mp4";
				var setting_path = $"{prefix}.setting.stereo.video.xml";

				LeftVideoCapture = new VideoCapture(left_path);
				if (!LeftVideoCapture.IsOpened()) throw new Exception($"Open Left Video | {left_path}");

				RightVideoCapture = new VideoCapture(right_path);
				if (!RightVideoCapture.IsOpened()) throw new Exception($"Open Right Video | {right_path}");

				if (LeftVideoCapture.FrameWidth != RightVideoCapture.FrameWidth ||
					LeftVideoCapture.FrameHeight != RightVideoCapture.FrameHeight)
					throw new Exception($"좌우 이미지 크기 | {LeftVideoCapture.FrameWidth}x{LeftVideoCapture.FrameHeight} | {RightVideoCapture.FrameWidth}x{RightVideoCapture.FrameHeight}");

				if (LeftVideoCapture.Fps != RightVideoCapture.Fps ||
					LeftVideoCapture.FrameCount != RightVideoCapture.FrameCount)
					throw new Exception($"좌우 이미지 FPS | {LeftVideoCapture.Fps}x{LeftVideoCapture.FrameCount} | {RightVideoCapture.Fps}x{RightVideoCapture.FrameCount}");

				try {
					var serializer = new XmlSerializer(typeof(StereoVideoLabelInfoType));
					var reader = new StreamReader(label_path);
					StereoVideoLabelInfo = (StereoVideoLabelInfoType)serializer.Deserialize(reader);
				}
				catch (Exception ex) {
					StereoVideoLabelInfo = new();
					Logger.Print(LOG_TYPE.INFO, $"새로운 라벨파일 생성됨 [ {label_path} ]");
				}

				StereoVideoLabelInfo.VideoInfo.W = LeftVideoCapture.FrameWidth;
				StereoVideoLabelInfo.VideoInfo.H = LeftVideoCapture.FrameHeight;
				StereoVideoLabelInfo.VideoInfo.D = LeftVideoCapture.FrameCount;
				StereoVideoLabelInfo.VideoInfo.Fps = LeftVideoCapture.Fps;

				LoadSettings(setting_path);

				return true;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"데이터 불러오기 실패 [ {ex.Message} ]");
				Unload();
				return false;
			}
			finally { __info_lock.ExitWriteLock(); }
		}
		public bool Save(string filename) {
			__info_lock.EnterReadLock();
			try {
				if (StereoVideoLabelInfo == null) return false;

				var (ext_list, prefix) = MyFunc.GetExtensionList(filename);
				var label_path = $"{prefix}.label.stereo.video.xml";
				var left_path = $"{prefix}.left.stereo.video.mp4";
				var right_path = $"{prefix}.right.stereo.video.mp4";
				var setting_path = $"{prefix}.setting.stereo.video.xml";

				using (var writer = new VideoWriter(
					left_path,
					FourCC.H264,
					LeftVideoCapture.Fps,
					new Size(LeftVideoCapture.FrameWidth, LeftVideoCapture.FrameHeight)
				)) {
					using var frame = new Mat();
					while (LeftVideoCapture.Read(frame) && !frame.Empty())
						writer.Write(frame);
				}
				using (var writer = new VideoWriter(
					left_path,
					FourCC.H264,
					RightVideoCapture.Fps,
					new Size(RightVideoCapture.FrameWidth, RightVideoCapture.FrameHeight)
				)) {
					using var frame = new Mat();
					while (RightVideoCapture.Read(frame) && !frame.Empty())
						writer.Write(frame);
				}

				try {
					var serializer = new XmlSerializer(typeof(StereoVideoLabelInfoType));
					var writer = new StreamWriter(label_path);
					var ns = new XmlSerializerNamespaces();
					ns.Add("", "");
					serializer.Serialize(writer, StereoVideoLabelInfo, ns);
					writer.Close();
				}
				catch (Exception ex) { throw new Exception($"라벨 저장 | {label_path} | {ex.Message}"); }

				SaveSettings(setting_path);

				return true;
			}
			catch (Exception ex) {
				Logger.Print(LOG_TYPE.ERROR, $"데이터 저장하기 실패 [ {ex.Message} ]");
				return false;
			}
			finally {
				__info_lock.ExitReadLock();
			}
		}
		public void Unload() {
			__info_lock.EnterWriteLock();
			try {
				StereoVideoLabelInfo = null;
				LeftVideoCapture = null;
				RightVideoCapture = null;
			}
			finally { __info_lock.ExitWriteLock(); }
		}
		public bool IsLoaded() {
			__info_lock.EnterReadLock();
			try {
				return StereoVideoLabelInfo != null;
			}
			finally { __info_lock.ExitReadLock(); }
		}
	}
}
