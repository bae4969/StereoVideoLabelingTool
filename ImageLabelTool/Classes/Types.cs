using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Windows.Point;

namespace ImageLabelTool.Classes
{
	public enum IMG_TYPE
	{
		NONE,
		GRAY,
		COLOR,
	}
	public enum LOG_TYPE
	{
		INFO,
		WARNING,
		ERROR,
	}

	class Logger{
		public static void Print(LOG_TYPE type, string log_str) {
			switch (type) {
				case LOG_TYPE.INFO: Trace.WriteLine($"[ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ][ INFO ] {log_str}"); break;
				case LOG_TYPE.WARNING: Trace.WriteLine($"[ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ][ WARNING ] {log_str}"); break;
				case LOG_TYPE.ERROR: Trace.WriteLine($"[ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ][ ERROR ] {log_str}"); break;
			}
		}
	}

	class MY_MATH {
		public static Int64 CalcImageIndex(INDEX3 idx, SIZE3 size) {
			return idx.x + idx.y * size.w + idx.z * size.w * size.h;
		}

		public static double CalcDistance(Point a, Point b) {
			Point diff = new(a.X - b.X, a.Y - b.Y);
			return Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y);
		}

		public static double CalcDistance(Point line_pt_from, Point line_pt_to, Point other_pt) {
			Point line_vec = new(line_pt_to.X - line_pt_from.X, line_pt_to.Y - line_pt_from.Y);
			Point other_vec = new(other_pt.X - line_pt_from.X, other_pt.Y - line_pt_from.Y);

			// 2. 내적 계산
			double c1 = other_vec.X * line_vec.X + other_vec.Y * line_vec.Y;
			double c2 = line_vec.X * line_vec.X + line_vec.Y * line_vec.Y;

			// 3. 세 가지 경우로 분기
			if (c1 <= 0) {
				// P1과의 거리
				return CalcDistance(other_pt, line_pt_from);
			}
			else if (c2 <= c1) {
				// P2와의 거리
				return CalcDistance(other_pt, line_pt_to);
			}
			else {
				// 선분 중간 지점에 수선이 내려가는 경우
				double b = c1 / c2;
				double projX = line_pt_from.X + b * line_vec.X;
				double projY = line_pt_from.Y + b * line_vec.Y;
				return Math.Sqrt((other_pt.X - projX)*(other_pt.X - projX) + (other_pt.Y - projY)*(other_pt.Y - projY));
			}
		}
	}

	public struct INDEX3 { public Int64 x, y, z; };
	public struct SIZE3 { public Int64 w, h, d; }

	public struct FLOAT3 { public float x, y, z; };
}
