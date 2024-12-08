#include "pch.h"
#include "ImageLabelToolCore.h"
#include "Switcher.h"

namespace ImageLabelTool
{
	namespace Core
	{
		using namespace std;


		SwitcherType g_Switcher;


		int64_t Initialize(TV::LOG::CallbackLogString log_callback_func) {
			int64_t ret_code = RETURN_CODE_NORMAL_EXCEPTION;
			try { ret_code = g_Switcher.Initialize(log_callback_func);}
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
		int64_t Release() {
			int64_t ret_code = RETURN_CODE_NORMAL_EXCEPTION;
			try { ret_code = g_Switcher.Release(); }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}

		int64_t LoadResourceImage(const wchar_t* resource_key, const wchar_t* file_name, int64_t w, int64_t h, int64_t d) {
			int64_t ret_code = RETURN_CODE_NORMAL_EXCEPTION;
			try { ret_code = RETURN_CODE_SUCCESS; }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
		int64_t SaveResourceImage(const wchar_t* resource_key, const wchar_t* file_name) {
			int64_t ret_code = RETURN_CODE_NORMAL_EXCEPTION;
			try { ret_code = RETURN_CODE_SUCCESS; }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
	}
}




