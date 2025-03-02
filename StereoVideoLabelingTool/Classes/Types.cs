using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StereoVideoLabelingTool.Classes
{
	public struct INDEX3 { public Int64 x, y, z; };
	public struct SIZE3 { public Int64 w, h, d; }
	public struct FLOAT3 { public float x, y, z; };
	public struct DOUBLE3 { public double x, y, z; };
	public struct SPACING3 { public double x, y, z; };
	public struct POINT3 { public double x, y, z; };

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

}
