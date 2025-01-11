using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ImageLabelingTool.Classes
{
	internal class Core
	{
#if MY_DEBUG
		const string CORE_DLL_NAME = "ImageLabelingToolCore_d.dll";
#else
		const string CORE_DLL_NAME = "ImageLabelingToolCore.dll";
#endif

		public const Int64 IMG_TYPE_ERROR = -1;
		public const Int64 IMG_TYPE_NONE = 0;
		public const Int64 IMG_TYPE_GRAY = 1;
		public const Int64 IMG_TYPE_RGBA = 2;
		public const Int64 IMG_TYPE_VIDEO = 3;

		public delegate void CallbackLogPush(string logStr);

		public static string ConvertReturnCodeToString(int returnCode) {
			switch (returnCode) {
				case 0: return "RETURN_CODE_SUCCESS_OK";
				default: return "UNDEFINED_RETURN_CODE";
			}
		}

		[DllImport(CORE_DLL_NAME)] public static extern Int64 Initialize(CallbackLogPush? log_callback_func);
		[DllImport(CORE_DLL_NAME)] public static extern Int64 Release();

		[DllImport(CORE_DLL_NAME)] public static extern Int64 LoadData([MarshalAs(UnmanagedType.LPWStr)] String img_path, [MarshalAs(UnmanagedType.LPWStr)] String lab_path);
		[DllImport(CORE_DLL_NAME)] public static extern Int64 SaveData([MarshalAs(UnmanagedType.LPWStr)] String img_path, [MarshalAs(UnmanagedType.LPWStr)] String lab_path);
		[DllImport(CORE_DLL_NAME)] public static extern Int64 UnloadData();
		[DllImport(CORE_DLL_NAME)] public static extern unsafe Int64 GetDataInfo(Int64* type, Int64* w, Int64* h, Int64* d, int** img_data_ptr, byte** lab_data_ptr);
	}
}
