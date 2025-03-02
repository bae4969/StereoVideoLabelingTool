using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StereoVideoLabelingTool.Classes
{
	public enum LOG_TYPE
	{
		INFO,
		WARNING,
		ERROR,
	}

	public class Logger
	{
		public static void Print(LOG_TYPE type, string log_str) {
			switch (type) {
				case LOG_TYPE.INFO: Trace.WriteLine($"[ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ][ INFO ] {log_str}"); break;
				case LOG_TYPE.WARNING: Trace.WriteLine($"[ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ][ WARNING ] {log_str}"); break;
				case LOG_TYPE.ERROR: Trace.WriteLine($"[ {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")} ][ ERROR ] {log_str}"); break;
			}
		}
	}
}
