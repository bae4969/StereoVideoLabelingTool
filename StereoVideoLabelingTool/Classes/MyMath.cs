using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Point = System.Windows.Point;

namespace StereoVideoLabelingTool.Classes
{
	class MyMath {
		public static Int64 CalcImageIndex(INDEX3 idx, SIZE3 size) {
			return idx.x + idx.y * size.w + idx.z * size.w * size.h;
		}

		public static double CalcDistance(Point a, Point b) {
			Point diff = new(a.X - b.X, a.Y - b.Y);
			return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
		}
		public static double CalcDistance(FLOAT3 a, FLOAT3 b) {
			FLOAT3 diff = new() {
				x = a.x - b.x,
				y = a.y - b.y,
				z = a.z - b.z,
			};
			return Math.Sqrt(diff.x * diff.x + diff.y * diff.y + diff.z * diff.z);
		}
		public static double CalcDistance(DOUBLE3 a, DOUBLE3 b) {
			DOUBLE3 diff = new() {
				x = a.x - b.x,
				y = a.y - b.y,
				z = a.z - b.z,
			};
			return Math.Sqrt(diff.x * diff.x + diff.y * diff.y + diff.z * diff.z);
		}
		public static double CalcDistance(POINT3 a, POINT3 b) {
			POINT3 diff = new() {
				x = a.x - b.x,
				y = a.y - b.y,
				z = a.z - b.z,
			};
			return Math.Sqrt(diff.x * diff.x + diff.y * diff.y + diff.z * diff.z);
		}

		public static double CalcDistance(Point line_pt_from, Point line_pt_to, Point other_pt) {
			Point line_vec = new(line_pt_to.X - line_pt_from.X, line_pt_to.Y - line_pt_from.Y);
			Point other_vec = new(other_pt.X - line_pt_from.X, other_pt.Y - line_pt_from.Y);

			double c1 = other_vec.X * line_vec.X + other_vec.Y * line_vec.Y;
			double c2 = line_vec.X * line_vec.X + line_vec.Y * line_vec.Y;

			if (c1 <= 0) { return CalcDistance(other_pt, line_pt_from); }
			else if (c2 <= c1) { return CalcDistance(other_pt, line_pt_to); }
			else {
				double b = c1 / c2;
				double projX = line_pt_from.X + b * line_vec.X;
				double projY = line_pt_from.Y + b * line_vec.Y;
				return Math.Sqrt(
					(other_pt.X - projX) * (other_pt.X - projX) +
					(other_pt.Y - projY)*(other_pt.Y - projY)
					);
			}
		}
		public static double CalcDistance(FLOAT3 line_pt_from, FLOAT3 line_pt_to, FLOAT3 other_pt) {
			Vector3 line_vec = new(line_pt_to.x - line_pt_from.x, line_pt_to.y - line_pt_from.y, line_pt_to.z - line_pt_from.z);
			Vector3 other_vec = new(other_pt.x - line_pt_from.x, other_pt.y - line_pt_from.y, other_pt.z - line_pt_from.z);

			double c1 = other_vec.X * line_vec.X + other_vec.Y * line_vec.Y + other_vec.Z * line_vec.Z;
			double c2 = line_vec.X * line_vec.X + line_vec.Y * line_vec.Y + line_vec.Z * line_vec.Z;

			if (c1 <= 0) { return CalcDistance(other_pt, line_pt_from); }
			else if (c2 <= c1) { return CalcDistance(other_pt, line_pt_to); }
			else {
				double b = c1 / c2;
				double projX = line_pt_from.x + b * line_vec.X;
				double projY = line_pt_from.y + b * line_vec.Y;
				double projZ = line_pt_from.z + b * line_vec.Z;
				return Math.Sqrt(
					(other_pt.x - projX) * (other_pt.x - projX) +
					(other_pt.y - projY) * (other_pt.y - projY) +
					(other_pt.z - projZ) * (other_pt.z - projZ)
					);
			}
		}
		public static double CalcDistance(DOUBLE3 line_pt_from, DOUBLE3 line_pt_to, DOUBLE3 other_pt) {
			Vector3 line_vec = new(
				(float)(line_pt_to.x - line_pt_from.x),
				(float)(line_pt_to.y - line_pt_from.y),
				(float)(line_pt_to.z - line_pt_from.z)
				);
			Vector3 other_vec = new(
				(float)(other_pt.x - line_pt_from.x),
				(float)(other_pt.y - line_pt_from.y),
				(float)(other_pt.z - line_pt_from.z)
				);

			double c1 = other_vec.X * line_vec.X + other_vec.Y * line_vec.Y + other_vec.Z * line_vec.Z;
			double c2 = line_vec.X * line_vec.X + line_vec.Y * line_vec.Y + line_vec.Z * line_vec.Z;

			if (c1 <= 0) { return CalcDistance(other_pt, line_pt_from); }
			else if (c2 <= c1) { return CalcDistance(other_pt, line_pt_to); }
			else {
				double b = c1 / c2;
				double projX = line_pt_from.x + b * line_vec.X;
				double projY = line_pt_from.y + b * line_vec.Y;
				double projZ = line_pt_from.z + b * line_vec.Z;
				return Math.Sqrt(
					(other_pt.x - projX) * (other_pt.x - projX) +
					(other_pt.y - projY) * (other_pt.y - projY) +
					(other_pt.z - projZ) * (other_pt.z - projZ)
					);
			}
		}
		public static double CalcDistance(POINT3 line_pt_from, POINT3 line_pt_to, POINT3 other_pt) {
			Vector3 line_vec = new(
				(float)(line_pt_to.x - line_pt_from.x),
				(float)(line_pt_to.y - line_pt_from.y),
				(float)(line_pt_to.z - line_pt_from.z)
				);
			Vector3 other_vec = new(
				(float)(other_pt.x - line_pt_from.x),
				(float)(other_pt.y - line_pt_from.y),
				(float)(other_pt.z - line_pt_from.z)
				);

			double c1 = other_vec.X * line_vec.X + other_vec.Y * line_vec.Y + other_vec.Z * line_vec.Z;
			double c2 = line_vec.X * line_vec.X + line_vec.Y * line_vec.Y + line_vec.Z * line_vec.Z;

			if (c1 <= 0) { return CalcDistance(other_pt, line_pt_from); }
			else if (c2 <= c1) { return CalcDistance(other_pt, line_pt_to); }
			else {
				double b = c1 / c2;
				double projX = line_pt_from.x + b * line_vec.X;
				double projY = line_pt_from.y + b * line_vec.Y;
				double projZ = line_pt_from.z + b * line_vec.Z;
				return Math.Sqrt(
					(other_pt.x - projX) * (other_pt.x - projX) +
					(other_pt.y - projY) * (other_pt.y - projY) +
					(other_pt.z - projZ) * (other_pt.z - projZ)
					);
			}
		}
	}
}
