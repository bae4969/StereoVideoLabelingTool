#include "pch.h"
#include "StereoVideoLabelingToolCore.h"
#include "MyTypes.h"
#include "MySimpleFunction.h"

namespace Bae::Stereo::Core {
	

	bool Initalize() {
		try {

		}
		AUTO_CATCH(
			DEBUG_PRINT("Failed to initialize StereoVideoLabelingToolCore");
			return false;
		);
	}
	void Release() {
		try {

		}
		AUTO_CATCH(
			DEBUG_PRINT("Failed to release StereoVideoLabelingToolCore");
		);
	}
}
