#include "pch.h"
#include "ImageLabelingToolCore.h"
#include "Switcher.h"

namespace ImageLabelTool
{
	namespace Core
	{
		using namespace std;


		shared_ptr<SwitcherType> g_Switcher;


		int64_t Initialize(TV::LOG::CallbackLogString log_callback_func) {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try {
				g_Switcher = make_shared<SwitcherType>(log_callback_func);
				ret_code = RETURN_CODE_SUCCESS;
			}
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
		int64_t Release() {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try {
				itk::ObjectFactoryBase::UnRegisterAllFactories();
				itk::MultiThreaderBase::SetGlobalDefaultNumberOfThreads(1);
				g_Switcher = NULL;
				ret_code = RETURN_CODE_SUCCESS;
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
		int64_t GetDataInfo(int64_t* type, int64_t* w, int64_t* h, int64_t* d, void** img_data_ptr, void** lab_data_ptr) {
			int64_t ret_code = RETURN_CODE_UNEXPECTED_EXCEPTION;
			try { ret_code = g_Switcher->GetDataInfo(type, w, h, d, img_data_ptr, lab_data_ptr); }
			AUTO_CATCH(LOG_E("{:s}", ERROR_MSG));
			return ret_code;
		}
	}
}




