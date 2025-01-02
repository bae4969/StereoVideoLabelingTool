#include "pch.h"
#include "Switcher.h"
#include <itkImageBase.h>
#include <itkSmartPointer.h>


using namespace std;
using namespace cv;
namespace fs = std::filesystem;
namespace xml = tinyxml2;


namespace ImageLabelTool
{
	namespace Core
	{
		SwitcherType::GrayInfoType::GrayInfoType(Vol32Ptr vol32) {
			NEW_TMP(itk::StatisticsImageFilter, statFilter, Vol32Type);
			statFilter->SetInput(vol32);
			statFilter->Update();

			Min = statFilter->GetMinimum();
			Max = statFilter->GetMaximum();
			Mean = statFilter->GetMean();
			Variance = statFilter->GetVariance();
		}


		////////////////////////////////////////////////////////////////


		SwitcherType::SwitcherType(TV::LOG::CallbackLogString log_callback_func) {
			if (Initialize(log_callback_func) < 0) throw "Fail to initialize switcher";
		}
		SwitcherType::~SwitcherType() {
			Release();
		}

		RETURN_CODE SwitcherType::Initialize(TV::LOG::CallbackLogString log_callback_func) {
			Release();

			std::unique_lock lock(__mutex);
			TV::LOG::SetSetting("log");
			TV::LOG::SetCallback(log_callback_func);

			return RETURN_CODE_SUCCESS;
		}
		RETURN_CODE SwitcherType::Release() {
			UnloadData();

			std::unique_lock lock(__mutex);
			__log_callback_func = NULL;
			TV::LOG::SetCallback(NULL);

			return RETURN_CODE_SUCCESS;
		}

		RETURN_CODE SwitcherType::LoadData(fs::path img_path, fs::path lab_path) {
			UnloadData();

			std::unique_lock lock(__mutex);

			auto [img_mode, img_base] = LoadImageFile(img_path);
			auto vol8 = LoadLabelFile(lab_path);
			if (!img_base)
				return RETURN_CODE_FILE_IO;

			Vol32Ptr castedImage = dynamic_cast<Vol32Type*>(img_base.GetPointer());

			auto imgRegion = img_base->GetLargestPossibleRegion();
			if (vol8) {
				auto labRegion = vol8->GetLargestPossibleRegion();
				if (imgRegion != labRegion)
					return RETURN_CODE_MISMATCH_IMAGE_AND_LABEL;
			}
			else {
				vol8 = Vol8Type::New();
				vol8->SetRegions(imgRegion);
				vol8->CopyInformation(img_base);
				vol8->Allocate(true);
			}

			__img_mode = img_mode;
			__img_vol = img_base;
			__lab_vol = vol8;
			__out_data = Mat::zeros(imgRegion.GetSize(1), imgRegion.GetSize(0), CV_8UC4);
			if (img_mode == IMG_MODE::GRAY)
				__gray_info = make_shared<GrayInfoType>(dynamic_cast<Vol32Type*>(img_base.GetPointer()));

			return RETURN_CODE_SUCCESS;
		}
		RETURN_CODE SwitcherType::SaveData(fs::path img_path, fs::path lab_path) {
			std::unique_lock lock(__mutex);

			if (lab_path.empty()) {
				lab_path = img_path;
				lab_path.replace_filename(lab_path.stem().string() + "_L" + lab_path.extension().string());
			}

			bool is_good =
				SaveItkFile(img_path, __img_vol) &&
				SaveItkFile(lab_path, __lab_vol);

			return RETURN_CODE_SUCCESS;
		}
		RETURN_CODE SwitcherType::UnloadData() {
			std::unique_lock lock(__mutex);
			__img_mode = IMG_MODE::NONE;
			__img_vol = NULL;
			__lab_vol = NULL;
			__out_data = Mat();
			__profile = NULL;

			return RETURN_CODE_SUCCESS;
		}
		RETURN_CODE SwitcherType::GetDataInfo(int64_t* type, int64_t* w, int64_t* h, int64_t* d, void** img_data_ptr, void** lab_data_ptr) {
			std::unique_lock lock(__mutex);
			if (!__img_vol) return RETURN_CODE_EMPTY_RESOURCE;

			auto size = __img_vol->GetLargestPossibleRegion().GetSize();
			if (type) {
				switch (__img_mode) {
				case IMG_MODE::GRAY:	*type = 0;	break;
				case IMG_MODE::COLOR:	*type = 1;	break;
				case IMG_MODE::VIDEO:	*type = 2;	break;
				default:				*type = -1;	break;
				}
			}
			if (w) *w = size[0];
			if (h) *h = size[1];
			if (d) *d = size[2];
			if (img_data_ptr) {
				switch (__img_mode) {
				case IMG_MODE::GRAY:	*img_data_ptr = dynamic_cast<Vol32Type*>(__img_vol.GetPointer())->GetBufferPointer();	break;
				case IMG_MODE::COLOR:	*img_data_ptr = dynamic_cast<VolRGBAType*>(__img_vol.GetPointer())->GetBufferPointer();	break;
				case IMG_MODE::VIDEO:
				default:				*type = -1;	break;
				}
			}
			if (lab_data_ptr)
				*lab_data_ptr = __lab_vol->GetBufferPointer();

			return RETURN_CODE_SUCCESS;
		}
	}
}

