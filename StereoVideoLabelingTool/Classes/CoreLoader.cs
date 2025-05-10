using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StereoVideoLabelingTool.Classes
{
	class Core
	{
#if MY_DEBUG
		const string DLL_NAME = "StereoVideoLabelingToolCore_d.dll";
#else
		const string DLL_NAME = "StereoVideoLabelingToolCore.dll";
#endif

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern bool Initalize();

		[DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl)]
		public static extern void Release();
	}
}
