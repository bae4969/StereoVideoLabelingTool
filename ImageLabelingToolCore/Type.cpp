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
		string GetVersionString(const VersionType& ver) {
			return FORMAT_STR("{:d}.{:d}.{:d}.{:d}", get<0>(ver), get<1>(ver), get<2>(ver), get<3>(ver));
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

			for (size_t idx = num_remain - 1; idx < curDirList.size(); idx++) {
				fs::remove_all(curDirList[idx].first);
				ret++;
			}

			return ret;
		}

		tuple<IMG_MODE, VolBasePtr> LoadImageFile(fs::path filename) {
			tuple<IMG_MODE, VolBasePtr> ret = { IMG_MODE::NONE, nullptr };

			try {
				const string ext = filename.extension().string();
				if (ext == ".mhd" ||
					ext == ".tif" ||
					ext == ".tiff" ||
					ext == ".png")
				{
					ImageIOBase::Pointer io;
					if (ext == ".mhd")			io = MetaImageIO::New();
					else if (ext == ".tif")		io = TIFFImageIO::New();
					else if (ext == ".tiff")	io = TIFFImageIO::New();
					else if (ext == ".png")		io = PNGImageIO::New();
					io->SetFileName(filename.string());
					io->ReadImageInformation();


					const auto comp_type = io->GetComponentType();
					const auto comp_cannel = io->GetNumberOfComponents();

					if (comp_type == IOComponentEnum::UCHAR && comp_cannel == 1) {
						NEW_TMP(itk::ImageFileReader, readerFilter, Vol8Type);
						NEW_TMP(itk::CastImageFilter, castFilter, Vol8Type, Vol32Type);

						readerFilter->SetFileName(io->GetFileName());
						readerFilter->SetImageIO(io);
						readerFilter->Update();
						castFilter->SetInput(readerFilter->GetOutput());
						castFilter->Update();

						get<0>(ret) = IMG_MODE::GRAY;
						get<1>(ret) = castFilter->GetOutput();
						get<1>(ret)->DisconnectPipeline();
					}
					else if (comp_type == IOComponentEnum::UCHAR && comp_cannel == 3) {
						NEW_TMP(itk::ImageFileReader, readerFilter, VolRGBType);
						NEW_TMP(itk::CastImageFilter, castFilter, VolRGBType, VolRGBAType);

						readerFilter->SetFileName(io->GetFileName());
						readerFilter->SetImageIO(io);
						readerFilter->Update();
						castFilter->SetInput(readerFilter->GetOutput());
						castFilter->Update();

						get<0>(ret) = IMG_MODE::COLOR;
						get<1>(ret) = castFilter->GetOutput();
						get<1>(ret)->DisconnectPipeline();
					}
					else if (comp_type == IOComponentEnum::UCHAR && comp_cannel == 4) {
						NEW_TMP(itk::ImageFileReader, readerFilter, VolRGBAType);

						readerFilter->SetFileName(io->GetFileName());
						readerFilter->SetImageIO(io);
						readerFilter->Update();

						get<0>(ret) = IMG_MODE::COLOR;
						get<1>(ret) = readerFilter->GetOutput();
						get<1>(ret)->DisconnectPipeline();
					}
					else if (comp_type == IOComponentEnum::USHORT && comp_cannel == 1) {
						NEW_TMP(itk::ImageFileReader, readerFilter, Vol16Type);
						NEW_TMP(itk::CastImageFilter, castFilter, Vol16Type, Vol32Type);

						readerFilter->SetFileName(io->GetFileName());
						readerFilter->SetImageIO(io);
						readerFilter->Update();
						castFilter->SetInput(readerFilter->GetOutput());
						castFilter->Update();

						get<0>(ret) = IMG_MODE::GRAY;
						get<1>(ret) = castFilter->GetOutput();
						get<1>(ret)->DisconnectPipeline();
					}
					else if (comp_type == IOComponentEnum::FLOAT && comp_cannel == 1) {
						NEW_TMP(itk::ImageFileReader, readerFilter, Vol32Type);

						readerFilter->SetFileName(io->GetFileName());
						readerFilter->SetImageIO(io);
						readerFilter->Update();

						get<0>(ret) = IMG_MODE::GRAY;
						get<1>(ret) = readerFilter->GetOutput();
						get<1>(ret)->DisconnectPipeline();
					}
					else throw exception();
				}
				else {
					TV::Image::TVRaw tvRaw;
					if (!tvRaw.Load(filename.extension().string().c_str()))
						throw exception();

					SIZE3 t_size;
					SPACING3 t_spacing;
					tvRaw.GetImageSize(&t_size[0], &t_size[1], &t_size[2]);
					tvRaw.GetSpacing(&t_spacing[0], &t_spacing[1], &t_spacing[2]);

					REGION3 t_region;
					t_region.SetSize(t_size);


					if (tvRaw.GetBytePerPixel() == 1) {
						NEW_TMP(itk::ImportImageFilter, importFilter, UCHAR, 3);
						NEW_TMP(itk::CastImageFilter, castFilter, Vol8Type, Vol32Type);

						importFilter->SetRegion(t_region);
						importFilter->SetSpacing(t_spacing);
						importFilter->SetImportPointer(
							(UCHAR*)tvRaw.GetDataPointer(),
							tvRaw.GetImageOffset3D(),
							false
						);
						importFilter->Update();
						castFilter->SetInput(importFilter->GetOutput());
						castFilter->Update();

						get<0>(ret) = IMG_MODE::GRAY;
						get<1>(ret) = castFilter->GetOutput();
						get<1>(ret)->DisconnectPipeline();
					}
					else if (tvRaw.GetBytePerPixel() == 2) {
						NEW_TMP(itk::ImportImageFilter, importFilter, USHORT, 3);
						NEW_TMP(itk::CastImageFilter, castFilter, Vol16Type, Vol32Type);

						importFilter->SetRegion(t_region);
						importFilter->SetSpacing(t_spacing);
						importFilter->SetImportPointer(
							(USHORT*)tvRaw.GetDataPointer(),
							tvRaw.GetImageOffset3D(),
							false
						);
						importFilter->Update();
						castFilter->SetInput(importFilter->GetOutput());
						castFilter->Update();

						get<0>(ret) = IMG_MODE::GRAY;
						get<1>(ret) = castFilter->GetOutput();
						get<1>(ret)->DisconnectPipeline();
					}
					else if (tvRaw.GetBytePerPixel() == 4) {
						NEW_TMP(itk::ImportImageFilter, importFilter, FLOAT, 3);
						NEW_TMP(itk::CastImageFilter, castFilter, Vol32Type, Vol32Type);

						importFilter->SetRegion(t_region);
						importFilter->SetSpacing(t_spacing);
						importFilter->SetImportPointer(
							(FLOAT*)tvRaw.GetDataPointer(),
							tvRaw.GetImageOffset3D(),
							false
						);
						importFilter->Update();
						castFilter->SetInput(importFilter->GetOutput());
						castFilter->Update();

						get<0>(ret) = IMG_MODE::GRAY;
						get<1>(ret) = castFilter->GetOutput();
						get<1>(ret)->DisconnectPipeline();
					}
					else throw exception();
				}
			}
			AUTO_CATCH(PRINT_DEBUG_MSG("The image file is not supported [ {:s} ]", filename.string()););
			return ret;
		}
		Vol8Ptr LoadLabelFile(fs::path filename) {
			Vol8Ptr ret = nullptr;

			try {
				const string ext = filename.extension().string();
				if (ext == ".mhd" ||
					ext == ".tif" ||
					ext == ".tiff" ||
					ext == ".png")
				{
					ImageIOBase::Pointer io;
					if (ext == ".mhd")			io = MetaImageIO::New();
					else if (ext == ".tif")		io = TIFFImageIO::New();
					else if (ext == ".tiff")	io = TIFFImageIO::New();
					else if (ext == ".png")		io = PNGImageIO::New();
					io->SetFileName(filename.string());
					io->ReadImageInformation();


					const auto comp_type = io->GetComponentType();
					const auto comp_cannel = io->GetNumberOfComponents();

					if (comp_type == IOComponentEnum::UCHAR && comp_cannel == 1) {
						NEW_TMP(itk::ImageFileReader, readerFilter, Vol8Type);

						readerFilter->SetFileName(io->GetFileName());
						readerFilter->SetImageIO(io);
						readerFilter->Update();

						ret = readerFilter->GetOutput();
						ret->DisconnectPipeline();
					}
					else throw exception();
				}
				else {
					TV::Image::TVRaw tvRaw;
					if (!tvRaw.Load(filename.extension().string().c_str()))
						throw exception();

					SIZE3 t_size;
					SPACING3 t_spacing;
					tvRaw.GetImageSize(&t_size[0], &t_size[1], &t_size[2]);
					tvRaw.GetSpacing(&t_spacing[0], &t_spacing[1], &t_spacing[2]);

					REGION3 t_region;
					t_region.SetSize(t_size);


					if (tvRaw.GetBytePerPixel() == 1) {
						NEW_TMP(itk::ImportImageFilter, importFilter, UCHAR, 3);
						NEW_TMP(itk::CastImageFilter, castFilter, Vol8Type, Vol8Type);

						importFilter->SetRegion(t_region);
						importFilter->SetSpacing(t_spacing);
						importFilter->SetImportPointer(
							(UCHAR*)tvRaw.GetDataPointer(),
							tvRaw.GetImageOffset3D(),
							false
						);
						importFilter->Update();
						castFilter->SetInput(importFilter->GetOutput());
						castFilter->Update();

						ret = castFilter->GetOutput();
						ret->DisconnectPipeline();
					}
					else throw exception();
				}
			}
			AUTO_CATCH(PRINT_DEBUG_MSG("The image file is not supported [ {:s} ]", filename.string()););
			return ret;
		}
		bool SaveItkFile(fs::path filename, VolBasePtr vol_base) {
			bool ret = false;
			try {
				const string ext = filename.extension().string();

				ImageIOBase::Pointer io;
				if (ext == ".mhd")			io = MetaImageIO::New();
				else if (ext == ".tif")		io = TIFFImageIO::New();
				else if (ext == ".tiff")	io = TIFFImageIO::New();
				else if (ext == ".png")		io = PNGImageIO::New();
				else throw exception();

				Vol8Ptr vol8 = dynamic_cast<Vol8Type*>(vol_base.GetPointer());
				Vol32Ptr vol32 = dynamic_cast<Vol32Type*>(vol_base.GetPointer());
				VolRGBAPtr volRGBA = dynamic_cast<VolRGBAType*>(vol_base.GetPointer());

				if (vol8) {
					NEW_TMP(itk::ImageFileWriter, writerFilter, Vol8Type);
					writerFilter->SetFileName(filename.string());
					writerFilter->SetImageIO(io);
					writerFilter->SetInput(vol8);
					writerFilter->Update();

					ret = true;
				}
				else if (vol32) {
					NEW_TMP(itk::ImageFileWriter, writerFilter, Vol32Type);
					writerFilter->SetFileName(filename.string());
					writerFilter->SetImageIO(io);
					writerFilter->SetInput(vol32);
					writerFilter->Update();

					ret = true;
				}
				else if (volRGBA) {
					NEW_TMP(itk::ImageFileWriter, writerFilter, VolRGBAType);
					writerFilter->SetFileName(filename.string());
					writerFilter->SetImageIO(io);
					writerFilter->SetInput(volRGBA);
					writerFilter->Update();

					ret = true;
				}
				else throw exception();
			}
			AUTO_CATCH(PRINT_DEBUG_MSG("Fail to save image file [ {:s} ]", filename.string()););
			return ret;
		}

		template<typename TYPE>
		cv::Mat GetCvSlice(typename TYPE::Pointer input, int64_t slice_index, int64_t cv_type) {
			if (input == nullptr || input->GetImageDimension() < 3) return cv::Mat();

			typename TYPE::SizeType size = input->GetLargestPossibleRegion().GetSize();
			if (slice_index < 0 || size[2] <= slice_index) return cv::Mat();

			typename TYPE::PixelType* t_ptr = input->GetBufferPointer();
			cv::Size cvSize(size[0], size[1]);

			return cv::Mat(cvSize, cv_type, t_ptr + slice_index * cvSize.area());
		}
		template cv::Mat GetCvSlice<Vol8Type>(Vol8Type::Pointer, int64_t, int64_t);
		template cv::Mat GetCvSlice<Vol32Type>(Vol32Type::Pointer, int64_t, int64_t);
		template cv::Mat GetCvSlice<VolRGBAType>(VolRGBAType::Pointer, int64_t, int64_t);

		template<typename TYPE>
		Cuda::CudaVolume<typename TYPE::PixelType> GetCudaVolume(size_t deviceID, typename TYPE::Pointer vol) {
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
		template Cuda::CudaVolume<CuVol8Type::PixelType> GetCudaVolume<CuVol8Type>(size_t, CuVol8Type::Pointer);
		template Cuda::CudaVolume<CuVol32Type::PixelType> GetCudaVolume<CuVol32Type>(size_t, CuVol32Type::Pointer);

	}
}

