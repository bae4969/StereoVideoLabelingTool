#include "pch.h"
#include "ImageLabelToolCore.h"
#include "Switcher.h"

namespace ImageLabelTool
{
	namespace Core
	{
		using namespace std;


		shared_ptr<SwitcherType> g_Switcher;


		int64_t Initialize(TV::LOG::CallbackLogString log_callback_func) {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try { g_Switcher = make_shared<SwitcherType>(log_callback_func); }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
		int64_t Release() {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try {
				g_Switcher = NULL;
				itk::ObjectFactoryBase::UnRegisterAllFactories();
				itk::MultiThreaderBase::SetGlobalDefaultNumberOfThreads(1);
			}
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}

		int64_t LoadData(const wchar_t* img_path, const wchar_t* lab_path) {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try { ret_code = g_Switcher->LoadData(img_path, lab_path == nullptr ? L"" : lab_path); }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
		int64_t SaveData(const wchar_t* img_path, const wchar_t* lab_path) {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try { ret_code = g_Switcher->SaveData(img_path, lab_path == nullptr ? L"" : lab_path); }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
		int64_t UnloadData() {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try { ret_code = g_Switcher->UnloadData(); }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
		int64_t GetInfo(int64_t* w, int64_t* h, int64_t* d) {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try { ret_code = g_Switcher->GetInfo(w, h, d); }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
		int64_t GetData(int64_t z, uchar* data_ptr) {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try { ret_code = g_Switcher->GetData(z, data_ptr); }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
	}
}




