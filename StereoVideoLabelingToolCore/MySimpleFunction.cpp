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


	bool SaveVolume(fs::path filename, Vol8TypePtr vol) {
		using namespace itk;

		if (vol.IsNull()) {
			DEBUG_PRINT("Volume is null");
			return false;
		}

		if (filename.has_extension() == false)
			filename.replace_extension(".nii");

		auto ext_str = filename.extension().string();

		ImageIOBase::Pointer io;
		if (ext_str == ".nii") {
			typename itk::NiftiImageIO::Pointer t_io = itk::NiftiImageIO::New();
			t_io->SetUseCompression(false);
			t_io->SetUseStreamedReading(false);
			t_io->SetUseStreamedWriting(false);
			io = t_io;
		}
		else if (ext_str == ".mha" || ext_str == ".m4d") {
			typename itk::MetaImageIO::Pointer t_io = itk::MetaImageIO::New();
			io = t_io;
		}
		else if (ext_str == ".h5" || ext_str == ".hdf5") {
			typename itk::HDF5ImageIO::Pointer t_io = itk::HDF5ImageIO::New();
			io = t_io;
		}
		else if (ext_str == ".nrrd") {
			typename itk::NrrdImageIO::Pointer t_io = itk::NrrdImageIO::New();
			io = t_io;
		}
		else if (ext_str == ".tiff" || ext_str == ".tif") {
			typename itk::TIFFImageIO::Pointer t_io = itk::TIFFImageIO::New();
			io = t_io;
		}

		typename itk::ImageFileWriter<Vol8Type>::Pointer writer = itk::ImageFileWriter<Vol8Type>::New();
		writer->SetInput(vol);
		writer->SetImageIO(io);
		writer->SetFileName(filename.string());
		try { writer->Update(); }
		AUTO_CATCH(return false;);

		return true;
	}
}
