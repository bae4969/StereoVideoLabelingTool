#include "pch.h"
#include "MySimpleFunction.h"
#include <itkNiftiImageIO.h>
#include <itkTIFFImageIO.h>
#include <itkMetaImageIO.h>
#include <itkHDF5ImageIO.h>
#include <itkNrrdImageIO.h>

namespace Bae {
	using namespace std;
	namespace fs = std::filesystem;


	template<typename VolType>
	bool SaveVolume(fs::path filename, typename VolType::Pointer vol) {
		using namespace itk;
		try {
			if (filename.empty()) THROW_STR("Empty Filename");
			if (vol.IsNull()) THROW_STR("Null Volume");

			if (filename.has_extension() == false) {
				filename.replace_extension(".nii");
				PRINT_LOG_W("Extension is empty, set default extention [ {:s} ]", filename.string());
			}

			auto ext_str = filename.extension().string();

			ImageIOBase::Pointer io;
			if (ext_str == ".nii") {
				itk::NiftiImageIO::Pointer t_io = itk::NiftiImageIO::New();
				t_io->SetUseCompression(false);
				t_io->SetUseStreamedReading(false);
				t_io->SetUseStreamedWriting(false);
				io = t_io;
			}
			else if (ext_str == ".mha" || ext_str == ".m4d")
				io = itk::MetaImageIO::New();
			else if (ext_str == ".h5" || ext_str == ".hdf5")
				io = itk::HDF5ImageIO::New();
			else if (ext_str == ".nrrd")
				io = itk::NrrdImageIO::New();
			else if (ext_str == ".tiff" || ext_str == ".tif")
				io = itk::TIFFImageIO::New();
			else
				THROW_STR("Invalid Ext | '{:s}'", ext_str);

			typename itk::ImageFileWriter<VolType>::Pointer writer = itk::ImageFileWriter<VolType>::New();
			writer->SetInput(vol);
			writer->SetImageIO(io);
			writer->SetFileName(filename.string());
			writer->Update();

			return true;
		}
		AUTO_CATCH(
			PRINT_LOG_E("Failed to save volume [ {:s} | {:s} ]", filename.string(), EX_MSG)
			return false;
		);
	}
	template bool SaveVolume<Vol8Type>(fs::path filename, Vol8Type::Pointer vol);
	template bool SaveVolume<Vol16Type>(fs::path filename, Vol16Type::Pointer vol);
	template bool SaveVolume<Vol32Type>(fs::path filename, Vol32Type::Pointer vol);
	template bool SaveVolume<Vol64Type>(fs::path filename, Vol64Type::Pointer vol);

	template<typename VolType>
	typename VolType::Pointer LoadVolume(fs::path filename) {
		using namespace itk;
		try {
			if (filename.empty()) THROW_STR("Empty Filename");

			if (filename.has_extension() == false) {
				filename.replace_extension(".nii");
				PRINT_LOG_W("Extension is empty, set default extention [ {:s} ]", filename.string());
			}

			auto ext_str = filename.extension().string();

			ImageIOBase::Pointer io;
			if (ext_str == ".nii") {
				itk::NiftiImageIO::Pointer t_io = itk::NiftiImageIO::New();
				t_io->SetUseCompression(false);
				t_io->SetUseStreamedReading(false);
				t_io->SetUseStreamedWriting(false);
				io = t_io;
			}
			else if (ext_str == ".mha" || ext_str == ".m4d")
				io = itk::MetaImageIO::New();
			else if (ext_str == ".h5" || ext_str == ".hdf5")
				io = itk::HDF5ImageIO::New();
			else if (ext_str == ".nrrd")
				io = itk::NrrdImageIO::New();
			else if (ext_str == ".tiff" || ext_str == ".tif")
				io = itk::TIFFImageIO::New();
			else
				THROW_STR("Invalid Ext | '{:s}'", ext_str);

			typename itk::ImageFileReader<VolType>::Pointer reader = itk::ImageFileReader<VolType>::New();
			reader->SetFileName(filename.string());
			reader->SetImageIO(io);
			reader->Update();

			return reader->GetOutput();
		}
		AUTO_CATCH(
			PRINT_LOG_E("Failed to load volume [ {:s} | {:s} ]", filename.string(), EX_MSG)
			return nullptr;
		);
	}
	template Vol8Type::Pointer LoadVolume<Vol8Type>(fs::path filename);
	template Vol16Type::Pointer LoadVolume<Vol16Type>(fs::path filename);
	template Vol32Type::Pointer LoadVolume<Vol32Type>(fs::path filename);
	template Vol64Type::Pointer LoadVolume<Vol64Type>(fs::path filename);
}
