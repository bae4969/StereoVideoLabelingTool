using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ImageLabelTool.Classes
{
	internal class Core
	{
#if MY_DEBUG
		const string CORE_DLL_NAME = "ImageLabelToolCore_d.dll";
#else
		const string CORE_DLL_NAME = "ImageLabelToolCore.dll";
#endif

		public const Int64 CV_8U = 0;
		public const Int64 CV_8S = 1;
		public const Int64 CV_16U = 2;
		public const Int64 CV_16S = 3;
		public const Int64 CV_32S = 4;
		public const Int64 CV_32F = 5;
		public const Int64 CV_64F = 6;
		public const Int64 CV_16F = 7;

		public delegate void CallbackLogPush(string logStr);

		public static string ConvertReturnCodeToString(int returnCode) {
			switch (returnCode) {
				case 0: return "RETURN_CODE_SUCCESS_OK";
				default: return "UNDEFINED_RETURN_CODE";
			}
		}
		public static Int64 MakeCvType(Int64 depth, Int64 channels) {
			return (depth & ((1 << 3) - 1)) + ((channels - 1) << 3);
		}

		[DllImport(CORE_DLL_NAME)] public static extern Int64 Initialize(CallbackLogPush log_callback_func);
		[DllImport(CORE_DLL_NAME)] public static extern Int64 Release();

		[DllImport(CORE_DLL_NAME)] public static extern Int64 LoadData([MarshalAs(UnmanagedType.LPWStr)] String img_path, [MarshalAs(UnmanagedType.LPWStr)] String lab_path);
		[DllImport(CORE_DLL_NAME)] public static extern Int64 SaveData([MarshalAs(UnmanagedType.LPWStr)] String img_path, [MarshalAs(UnmanagedType.LPWStr)] String lab_path);
		[DllImport(CORE_DLL_NAME)] public static extern Int64 UnloadData();
		[DllImport(CORE_DLL_NAME)] public static extern unsafe Int64 GetInfo(Int64* w, Int64* h, Int64* d);
		[DllImport(CORE_DLL_NAME)] public static extern unsafe Int64 GetData(Int64 z, byte* data_ptr);
	}
}
