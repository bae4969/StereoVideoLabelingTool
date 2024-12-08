#include "pch.h"
#include "Type.h"
#include "CudaType.cuh"

namespace ImageLabelTool
{
	namespace Core
	{
		using namespace std;
		using namespace cv;
		using namespace itk;
		namespace fs = filesystem;
		namespace ch = chrono;
		using TVRaw = TV::Image::TVRaw;
		using TVRawContainer = TV::Image::TVRawContainer;



		string GetVersionString(const VersionType& ver) {
			return FORMAT_STR("{:d}.{:d}.{:d}.{:d}", get<0>(ver), get<1>(ver), get<2>(ver), get<3>(ver));
		}

		Vol16Ptr ConvertTVRawToItk(shared_ptr<TVRaw> tvRaw) {
			NEW_TMP(itk::ImportImageFilter, importFilter, USHORT, 3);
			importFilter->SetImportPointer((USHORT*)tvRaw->GetDataPointer(), tvRaw->GetBytePerPixel(), false);

			REGION3 region;
			INDEX3 index = { 0 };
			SIZE3 size;
			SPACING3 spacing;
			POINT3 origin;
			tvRaw->GetImageSize(&size[0], &size[1], &size[2]);
			tvRaw->GetSpacingWithMillimeter(&spacing[0], &spacing[1], &spacing[2]);
			//tvRaw->GetObjectRealLocationWithMillimeter(&origin[0], &origin[1], &origin[2]);
			for (int i = 0; i < 3; i++) {
				if (spacing[i] < DBL_EPSILON) spacing[i] = 1.0;
				origin[i] = size[i] * spacing[i] * -0.5;
			}

			region.SetIndex(index);
			region.SetSize(size);
			importFilter->SetOrigin(origin);
			importFilter->SetSpacing(spacing);
			importFilter->SetRegion(region);
			importFilter->Update();

			return importFilter->GetOutput();
		}
		vector<Vol16Ptr> ConvertTVRawContainerToItkList(TVRawContainer& tvRawContainer) {
			vector<Vol16Ptr> ret(tvRawContainer.GetNumberOfImages());
			if (ret.size() == 0u) return ret;

			for (size_t idx = 0; idx < ret.size(); idx++)
				ret[idx] = ConvertTVRawToItk(tvRawContainer[idx]);

			return ret;
		}

		template void ConvertItkToTVRaw<Vol8Type>(shared_ptr<TVRaw>, Vol8Type::Pointer, shared_ptr<TVRaw>);
		template void ConvertItkToTVRaw<Vol16Type>(shared_ptr<TVRaw>, Vol16Type::Pointer, shared_ptr<TVRaw>);
		template<typename TYPE>
		void ConvertItkToTVRaw(shared_ptr<TVRaw> out_tvRaw, typename TYPE::Pointer img, shared_ptr<TVRaw> in_copyInfo) {
			if (!img) return;

			auto imgPtr = img->GetBufferPointer();
			SIZE3 size = img->GetLargestPossibleRegion().GetSize();
			SPACING3 spacing = img->GetSpacing();

			if (in_copyInfo)
				out_tvRaw->CopyInfo(in_copyInfo);
			out_tvRaw->UseHostPointer(size[0], size[1], size[2], sizeof(*imgPtr) * 8ull, imgPtr, TVRaw::DATA_MANAGING::POINTING);
			out_tvRaw->SetSpacingWithMillimeter(&spacing[0], &spacing[1], &spacing[2]);
		}

		template Cuda::CudaVolume<CuVol8Type::PixelType> ConvertItkToCuda<CuVol8Type>(size_t, CuVol8Type::Pointer);
		template Cuda::CudaVolume<CuVol16Type::PixelType> ConvertItkToCuda<CuVol16Type>(size_t, CuVol16Type::Pointer);
		template Cuda::CudaVolume<CuVol32Type::PixelType> ConvertItkToCuda<CuVol32Type>(size_t, CuVol32Type::Pointer);
		template<typename TYPE>
		Cuda::CudaVolume<typename TYPE::PixelType> ConvertItkToCuda(size_t deviceID, typename TYPE::Pointer vol) {
			Cuda::CudaVolume<typename TYPE::PixelType> ret;

			cudaSetDevice(deviceID);
			CudaDataManager::Pointer imgDataManager = vol->GetCudaDataManager();
			imgDataManager->Free();
			imgDataManager->Allocate();
			imgDataManager->UpdateGPUBuffer();

			void* d_ptr = imgDataManager->GetGPUBufferPointer();
			auto size = vol->GetLargestPossibleRegion().GetSize();
			auto origin = vol->GetOrigin();
			auto spacing = vol->GetSpacing();

			ret.Import(deviceID, d_ptr, dim3(size[0], size[1], size[2]));
			ret.SetOrigin(float3(origin[0], origin[1], origin[2]));
			ret.SetSpacing(float3(spacing[0], spacing[1], spacing[2]));

			return ret;
		}

		template cv::Mat GetCvSlice<Vol8Type>(Vol8Type::Pointer, int64_t);
		template cv::Mat GetCvSlice<Vol16Type>(Vol16Type::Pointer, int64_t);
		template<typename TYPE>
		cv::Mat GetCvSlice(typename TYPE::Pointer input, int64_t slice_index) {
			if (input == nullptr || input->GetImageDimension() < 3) return cv::Mat();

			typename TYPE::SizeType size = input->GetLargestPossibleRegion().GetSize();
			if (slice_index < 0 || size[2] <= slice_index) return cv::Mat();

			int bytePerPxl = sizeof(typename TYPE::PixelType);
			int cvType;
			switch (bytePerPxl) {
			case 1: cvType = CV_8U; break;
			case 2: cvType = CV_16U; break;
			case 4: cvType = CV_32F; break;
			case 8: cvType = CV_64F; break;
			default: return cv::Mat();
			}

			typename TYPE::PixelType* t_ptr = input->GetBufferPointer();
			cv::Size cvSize(size[0], size[1]);

			return cv::Mat(cvSize, cvType, t_ptr + slice_index * cvSize.area());
		}

		template Vol8Type::Pointer ExtractNormal<Vol8Type>(Vol8Type::Pointer, TransformPtr, SIZE3);
		template Vol16Type::Pointer ExtractNormal<Vol16Type>(Vol16Type::Pointer, TransformPtr, SIZE3);
		template<typename TYPE>
		typename TYPE::Pointer ExtractNormal(typename TYPE::Pointer src, TransformPtr transform, SIZE3 size) {
			typename NEW_TMP(itk::ResampleImageFilter, resampleFilter, TYPE, TYPE);

			SPACING3 spacing = src->GetSpacing();
			POINT3 origin;
			origin[0] = -0.5 * spacing[0] * size[0];
			origin[1] = -0.5 * spacing[1] * size[1];
			origin[2] = -0.5 * spacing[2] * size[2];

			if (sizeof(typename TYPE::PixelType) == 1) {
				typename NEW_TMP(itk::NearestNeighborInterpolateImageFunction, interpolator, TYPE);
				resampleFilter->SetInterpolator(interpolator);
			}

			resampleFilter->SetInput(src);
			resampleFilter->SetDefaultPixelValue(0);
			resampleFilter->SetOutputParametersFromImage(src);
			resampleFilter->SetTransform(transform);
			resampleFilter->SetOutputOrigin(origin);
			resampleFilter->SetSize(size);
			resampleFilter->Update();

			return resampleFilter->GetOutput();
		}
		template Vol8Type::Pointer ExtractSlice<Vol8Type>(Vol8Type::Pointer, TransformPtr, SIZE3, double);
		template Vol16Type::Pointer ExtractSlice<Vol16Type>(Vol16Type::Pointer, TransformPtr, SIZE3, double);
		template<typename TYPE>
		typename TYPE::Pointer ExtractSlice(typename TYPE::Pointer src, TransformPtr transform, SIZE3 size, double zOffset) {
			typename NEW_TMP(itk::ResampleImageFilter, resampleFilter, TYPE, TYPE);

			SIZE3 dstSize = size;
			SPACING3 dstSpacing = src->GetSpacing();
			POINT3 dstOrigin = src->GetOrigin();
			dstSize[2] = 1;
			dstOrigin[0] = -0.5 * dstOrigin[0] * size[0];
			dstOrigin[1] = -0.5 * dstOrigin[1] * size[1];
			dstOrigin[2] = zOffset;

			if (sizeof(typename TYPE::PixelType) == 1) {
				typename NEW_TMP(itk::NearestNeighborInterpolateImageFunction, interpolator, TYPE);
				resampleFilter->SetInterpolator(interpolator);
			}

			resampleFilter->SetInput(src);
			resampleFilter->SetDefaultPixelValue(0);
			resampleFilter->SetOutputParametersFromImage(src);
			resampleFilter->SetTransform(transform);
			resampleFilter->SetOutputSpacing(dstSpacing);
			resampleFilter->SetOutputOrigin(dstOrigin);
			resampleFilter->SetSize(dstSize);
			resampleFilter->Update();

			return resampleFilter->GetOutput();
		}

		template bool SaveRaw<Vol8Type>(fs::path, Vol8Type::Pointer);
		template bool SaveRaw<Vol16Type>(fs::path, Vol16Type::Pointer);
		template bool SaveRaw<Vol32Type>(fs::path, Vol32Type::Pointer);
		template bool SaveRaw<Vol64Type>(fs::path, Vol64Type::Pointer);
		template<typename TYPE>
		bool SaveRaw(fs::path filePath, typename TYPE::Pointer image) {
			if (!image) return false;

			int size[3];
			double spacing[3];
			double origin[3];
			typename TYPE::SizeType itkSize = image->GetLargestPossibleRegion().GetSize();
			typename TYPE::SpacingType itkSpacing = image->GetSpacing();
			typename TYPE::PointType itkOrigin = image->GetOrigin();
			for (int i = 0; i < 3; i++) {
				size[i] = itkSize[i];
				spacing[i] = itkSpacing[i];
				origin[i] = itkOrigin[i];
			}

			TVRaw tvRaw;
			tvRaw.UseHostPointer(
				size[0],
				size[1],
				size[2],
				sizeof(typename TYPE::PixelType) * 8ull,
				image->GetBufferPointer(),
				TVRaw::DATA_MANAGING::POINTING
			);

			tvRaw.SetSpacingWithMillimeter(&spacing[0], &spacing[1], &spacing[2]);


			return tvRaw.Save(filePath.string().c_str());
		}
		template bool SaveTiff<Vol8Type>(fs::path, Vol8Type::Pointer);
		template bool SaveTiff<Vol16Type>(fs::path, Vol16Type::Pointer);
		template bool SaveTiff<Vol32Type>(fs::path, Vol32Type::Pointer);
		template bool SaveTiff<Vol64Type>(fs::path, Vol64Type::Pointer);
		template<typename TYPE>
		bool SaveTiff(fs::path filePath, typename TYPE::Pointer image) {
			if (!image) return false;

			int size[3];
			double spacing[3];
			double origin[3];
			typename TYPE::SizeType itkSize = image->GetLargestPossibleRegion().GetSize();
			typename TYPE::SpacingType itkSpacing = image->GetSpacing();
			typename TYPE::PointType itkOrigin = image->GetOrigin();
			for (int i = 0; i < 3; i++) {
				size[i] = itkSize[i];
				spacing[i] = itkSpacing[i];
				origin[i] = itkOrigin[i];
			}
			return TV::Util::SaveTiffFile(filePath.string().c_str(), sizeof(typename TYPE::PixelType), image->GetBufferPointer(), size, spacing, origin) >= 0;
		}

		size_t RemoveWithoutLastWrite(fs::path targetDir, size_t num_remain) {
			size_t ret = 0ull;
			if (!fs::exists(targetDir))
				return ret;

			using PathTimeType = pair<string, int64>;
			vector<PathTimeType> curDirList;
			for (const auto& p : fs::directory_iterator(targetDir)) {
				curDirList.push_back(PathTimeType(
					p.path().string(),
					p.last_write_time().time_since_epoch().count()
				));
			}

			// 시간 내림차순으로 경로 정렬
			sort(
				curDirList.begin(),
				curDirList.end(),
				[](PathTimeType a, PathTimeType b) { return a.second > b.second; }
			);

			// 최대 10개 샷에 대한 결과만 살려놓을 계획
			for (size_t idx = num_remain - 1; idx < curDirList.size(); idx++) {
				fs::remove_all(curDirList[idx].first);
				ret++;
			}

			return ret;
		}

		bool IsMatchVersion(VersionType& a, VersionType& b, size_t match) {
			if (match == 4)
				return get<0>(a) == get<0>(b) && get<1>(a) == get<1>(b) && get<2>(a) == get<2>(b) && get<3>(a) == get<3>(b);
			else if (match == 3)
				return get<0>(a) == get<0>(b) && get<1>(a) == get<1>(b) && get<2>(a) == get<2>(b);
			else if (match == 2)
				return get<0>(a) == get<0>(b) && get<1>(a) == get<1>(b);
			else if (match == 1)
				return get<0>(a) == get<0>(b);
			else
				return false;
		}

		size_t RemainMemoryBytes() {
			MEMORYSTATUSEX memInfo;
			memInfo.dwLength = sizeof(memInfo);
			GlobalMemoryStatusEx(&memInfo);

			return memInfo.ullAvailPhys;

			std::cout << "Total Physical Memory: " << memInfo.ullTotalPhys / (1024 * 1024) << " MB" << std::endl;
			std::cout << "Available Physical Memory: " << memInfo.ullAvailPhys / (1024 * 1024) << " MB" << std::endl;
			std::cout << "Total Virtual Memory: " << memInfo.ullTotalVirtual / (1024 * 1024) << " MB" << std::endl;
			std::cout << "Available Virtual Memory: " << memInfo.ullAvailVirtual / (1024 * 1024) << " MB" << std::endl;
			std::cout << "Memory Load: " << memInfo.dwMemoryLoad << " %" << std::endl;
		}

		string GetCurrentDateTime(bool isMili, bool isSpliter) {
			auto now = ch::system_clock::now();
			auto now_time = ch::floor<ch::seconds>(now);
			auto milli_sec = ch::duration_cast<ch::milliseconds>(now.time_since_epoch()) % 1000;
			return
				isMili ?
				isSpliter ?
				FORMAT_STR("{:%Y-%m-%d %H:%M:%S}.{:03d}", now_time, milli_sec.count()) :
				FORMAT_STR("{:%Y%m%d%H%M%S}{:03d}", now_time, milli_sec.count()) :
				isSpliter ?
				FORMAT_STR("{:%Y-%m-%d %H:%M:%S}", now_time) :
				FORMAT_STR("{:%Y%m%d%H%M%S}", now_time);
		}
	}
}

