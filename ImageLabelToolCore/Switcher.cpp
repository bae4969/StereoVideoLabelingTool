#include "pch.h"
#include "Switcher.h"


using namespace std;

namespace ImageLabelTool
{
	namespace Core
	{
		int64_t SwitcherType::Initialize(TV::LOG::CallbackLogString log_callback_func) {
			std::unique_lock lock(__mutex);
			__image_map.clear();
			__log_callback_func = log_callback_func;
			TV::LOG::SetSetting("log");
			TV::LOG::SetCallback(log_callback_func);

			return RETURN_CODE_SUCCESS;
		}
		int64_t SwitcherType::Release() {
			std::unique_lock lock(__mutex);
			__image_map.clear();
			__log_callback_func = NULL;
			TV::LOG::SetCallback(NULL);

			return RETURN_CODE_SUCCESS;
		}
	}
}
