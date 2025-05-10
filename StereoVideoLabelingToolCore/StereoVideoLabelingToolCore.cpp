#include "pch.h"
#include "StereoVideoLabelingToolCore.h"
#include "MyTypes.h"
#include "MySimpleFunction.h"

namespace Bae::Stereo::Core {
	

	bool Initalize() {
		try {

			PRINT_LOG_N("Success to initialize StereoVideoLabelingToolCore");
			return true;
		}
		AUTO_CATCH(
			PRINT_LOG_E("Failed to initialize StereoVideoLabelingToolCore [ {:s} ]", EX_MSG);
			return false;
		);
	}
	void Release() {
		try {

			PRINT_LOG_N("Success to release StereoVideoLabelingToolCore");
		}
		AUTO_CATCH(
			PRINT_LOG_E("Failed to release StereoVideoLabelingToolCore [ {:s} ]", EX_MSG);
		);
	}
}
