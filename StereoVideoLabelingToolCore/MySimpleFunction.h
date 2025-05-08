#pragma once
#include "MyTypes.h"
#include <filesystem>

namespace Bae {
	bool SaveVolume(std::filesystem::path filename, Vol8TypePtr vol);
}