#pragma once
#include "CudaMacro.cuh"
#include <vector_types.h>

namespace ImageLabelTool
{
	namespace Core
	{
		namespace Cuda
		{
			template <typename PixelType>
			struct CudaVolume {
				int m_deviceId = -1;
				bool m_isImported = false;
				PixelType* m_devicePtr = nullptr;
				dim3 m_size = { 0, 0, 0 };
				float3 m_spacing = { 1.0f, 1.0f, 1.0f };
				float3 m_origin = { 0.0f, 0.0f, 0.0f };
				ulonglong3 m_offset = { 0, 0, 0 };
				ulonglong3 m_byteSize = { 0, 0, 0 };

				~CudaVolume();
				int GetDeviceID();
				PixelType* GetDevicePtr();
				dim3 GetSize();
				float3 GetSpacing();
				float3 GetOrigin();
				ulonglong3 GetOffset();
				ulonglong3 GetByteSize();
				void SetSpacing(float3 spacing);
				void SetOrigin(float3 origin);
				void Import(int deviceID, void* d_ptr, dim3 size);
				void Allocate(int deviceID, dim3 size);
				void Free();
			};
		}
	}
}
