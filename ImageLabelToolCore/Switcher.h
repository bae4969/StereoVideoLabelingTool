#pragma once
#include "Macro.h"
#include "Type.h"

namespace ImageLabelTool
{
	namespace Core
	{
		class SwitcherType {
		private:
			TV::LOG::CallbackLogString* __log_callback_func = NULL;

			std::mutex __mutex;
			std::map<std::wstring, std::tuple<Vol16Ptr, Vol8Ptr>> __image_map;


		public:
			int64_t Initialize(TV::LOG::CallbackLogString callback_func);
			int64_t Release();

		};
	}
}
