#pragma once
#include "Macro.h"
#include "Type.h"
#include <tinyxml2.h>

namespace ImageLabelTool
{
	namespace Core
	{
		class SwitcherType {
		private:
			struct GrayInfoType {
				float Min;
				float Max;
				float Mean;
				float Variance;

				GrayInfoType(Vol32Ptr vol32);
			};

			std::mutex __mutex;
			TV::LOG::CallbackLogString* __log_callback_func = NULL;

			IMG_MODE __img_mode;
			VolBasePtr __img_vol;
			Vol8Ptr __lab_vol;
			cv::Mat __out_img;
			std::shared_ptr<GrayInfoType> __gray_info;
			std::shared_ptr<tinyxml2::XMLDocument> __profile;


		public:
			SwitcherType(TV::LOG::CallbackLogString callback_func);
			~SwitcherType();

			RETURN_CODE Initialize(TV::LOG::CallbackLogString callback_func);
			RETURN_CODE Release();

			RETURN_CODE LoadData(std::filesystem::path img_path, std::filesystem::path lab_path);
			RETURN_CODE SaveData(std::filesystem::path img_path, std::filesystem::path lab_path);
			RETURN_CODE UnloadData();
			RETURN_CODE GetInfo(int64_t* w, int64_t* h, int64_t* d);
			RETURN_CODE GetData(int64_t z, uchar* data_ptr);
		};
	}
}
