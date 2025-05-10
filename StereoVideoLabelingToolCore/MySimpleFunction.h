#pragma once
#include "MyTypes.h"
#include <filesystem>

namespace Bae {
	template<typename VolType>
	bool SaveVolume(std::filesystem::path filename, typename VolType::Pointer vol);
	template<typename VolType>
	typename VolType::Pointer LoadVolume(std::filesystem::path filename);
}