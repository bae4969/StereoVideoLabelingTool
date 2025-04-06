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
			return idx.X + idx.Y * size.W + idx.Z * size.W * size.H;
		}

		public static double CalcDistance(Point a, Point b) {
			Point diff = new(a.X - b.X, a.Y - b.Y);
			return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
		}
		public static double CalcDistance(FLOAT3 a, FLOAT3 b) {
			FLOAT3 diff = new() {
				X = a.X - b.X,
				Y = a.Y - b.Y,
				Z = a.Z - b.Z,
			};
			return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y + diff.Z * diff.Z);
		}
		public static double CalcDistance(DOUBLE3 a, DOUBLE3 b) {
			DOUBLE3 diff = new() {
				X = a.X - b.X,
				Y = a.Y - b.Y,
				Z = a.Z - b.Z,
			};
			return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y + diff.Z * diff.Z);
		}
		public static double CalcDistance(POINT3 a, POINT3 b) {
			POINT3 diff = new() {
				X = a.X - b.X,
				Y = a.Y - b.Y,
				Z = a.Z - b.Z,
			};
			return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y + diff.Z * diff.Z);
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
			Vector3 line_vec = new(line_pt_to.X - line_pt_from.X, line_pt_to.Y - line_pt_from.Y, line_pt_to.Z - line_pt_from.Z);
			Vector3 other_vec = new(other_pt.X - line_pt_from.X, other_pt.Y - line_pt_from.Y, other_pt.Z - line_pt_from.Z);

			double c1 = other_vec.X * line_vec.X + other_vec.Y * line_vec.Y + other_vec.Z * line_vec.Z;
			double c2 = line_vec.X * line_vec.X + line_vec.Y * line_vec.Y + line_vec.Z * line_vec.Z;

			if (c1 <= 0) { return CalcDistance(other_pt, line_pt_from); }
			else if (c2 <= c1) { return CalcDistance(other_pt, line_pt_to); }
			else {
				double b = c1 / c2;
				double projX = line_pt_from.X + b * line_vec.X;
				double projY = line_pt_from.Y + b * line_vec.Y;
				double projZ = line_pt_from.Z + b * line_vec.Z;
				return Math.Sqrt(
					(other_pt.X - projX) * (other_pt.X - projX) +
					(other_pt.Y - projY) * (other_pt.Y - projY) +
					(other_pt.Z - projZ) * (other_pt.Z - projZ)
					);
			}
		}
		public static double CalcDistance(DOUBLE3 line_pt_from, DOUBLE3 line_pt_to, DOUBLE3 other_pt) {
			Vector3 line_vec = new(
				(float)(line_pt_to.X - line_pt_from.X),
				(float)(line_pt_to.Y - line_pt_from.Y),
				(float)(line_pt_to.Z - line_pt_from.Z)
				);
			Vector3 other_vec = new(
				(float)(other_pt.X - line_pt_from.X),
				(float)(other_pt.Y - line_pt_from.Y),
				(float)(other_pt.Z - line_pt_from.Z)
				);

			double c1 = other_vec.X * line_vec.X + other_vec.Y * line_vec.Y + other_vec.Z * line_vec.Z;
			double c2 = line_vec.X * line_vec.X + line_vec.Y * line_vec.Y + line_vec.Z * line_vec.Z;

			if (c1 <= 0) { return CalcDistance(other_pt, line_pt_from); }
			else if (c2 <= c1) { return CalcDistance(other_pt, line_pt_to); }
			else {
				double b = c1 / c2;
				double projX = line_pt_from.X + b * line_vec.X;
				double projY = line_pt_from.Y + b * line_vec.Y;
				double projZ = line_pt_from.Z + b * line_vec.Z;
				return Math.Sqrt(
					(other_pt.X - projX) * (other_pt.X - projX) +
					(other_pt.Y - projY) * (other_pt.Y - projY) +
					(other_pt.Z - projZ) * (other_pt.Z - projZ)
					);
			}
		}
		public static double CalcDistance(POINT3 line_pt_from, POINT3 line_pt_to, POINT3 other_pt) {
			Vector3 line_vec = new(
				(float)(line_pt_to.X - line_pt_from.X),
				(float)(line_pt_to.Y - line_pt_from.Y),
				(float)(line_pt_to.Z - line_pt_from.Z)
				);
			Vector3 other_vec = new(
				(float)(other_pt.X - line_pt_from.X),
				(float)(other_pt.Y - line_pt_from.Y),
				(float)(other_pt.Z - line_pt_from.Z)
				);

			double c1 = other_vec.X * line_vec.X + other_vec.Y * line_vec.Y + other_vec.Z * line_vec.Z;
			double c2 = line_vec.X * line_vec.X + line_vec.Y * line_vec.Y + line_vec.Z * line_vec.Z;

			if (c1 <= 0) { return CalcDistance(other_pt, line_pt_from); }
			else if (c2 <= c1) { return CalcDistance(other_pt, line_pt_to); }
			else {
				double b = c1 / c2;
				double projX = line_pt_from.X + b * line_vec.X;
				double projY = line_pt_from.Y + b * line_vec.Y;
				double projZ = line_pt_from.Z + b * line_vec.Z;
				return Math.Sqrt(
					(other_pt.X - projX) * (other_pt.X - projX) +
					(other_pt.Y - projY) * (other_pt.Y - projY) +
					(other_pt.Z - projZ) * (other_pt.Z - projZ)
					);
			}
		}
	}
}
