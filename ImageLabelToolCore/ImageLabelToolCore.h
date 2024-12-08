#pragma once
#include "Macro.h"
#include "Type.h"

namespace ImageLabelTool
{
	namespace Core
	{
		IMAGELABELTOOLCORE_EXPORTS int64_t Initialize(TV::LOG::CallbackLogString log_callback_func);
		IMAGELABELTOOLCORE_EXPORTS int64_t Release();

		IMAGELABELTOOLCORE_EXPORTS int64_t LoadResourceImage(const wchar_t* resource_key, const wchar_t* file_name, int64_t w = -1, int64_t h = -1, int64_t d = -1);
		IMAGELABELTOOLCORE_EXPORTS int64_t SaveResourceImage(const wchar_t* resource_key, const wchar_t* file_name);
	}
}





