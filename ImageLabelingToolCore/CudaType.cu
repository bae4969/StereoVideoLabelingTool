#include "CudaType.cuh"
#include <iostream>
#include <cuda_runtime_api.h>

namespace ImageLabelTool
{
	namespace Core
	{
		namespace Cuda
		{
			using namespace std;

			template struct CudaVolume<unsigned char>;
			template struct CudaVolume<unsigned short>;
			template struct CudaVolume<float>;
			template <typename PixelType> CudaVolume<PixelType>::~CudaVolume() {
				Free();
			}
			template <typename PixelType> int CudaVolume<PixelType>::GetDeviceID() {
				return m_deviceId;
			}
			template <typename PixelType> PixelType* CudaVolume<PixelType>::GetDevicePtr() {
				return *(PixelType**)m_devicePtr;
			}
			template <typename PixelType> dim3 CudaVolume<PixelType>::GetSize() {
				return m_size;
			}
			template <typename PixelType> float3 CudaVolume<PixelType>::GetSpacing() {
				return m_spacing;
			}
			template <typename PixelType> float3 CudaVolume<PixelType>::GetOrigin() {
				return m_origin;
			}
			template <typename PixelType> ulonglong3 CudaVolume<PixelType>::GetOffset() {
				return m_offset;
			}
			template <typename PixelType> ulonglong3 CudaVolume<PixelType>::GetByteSize() {
				return m_byteSize;
			}
			template <typename PixelType> void CudaVolume<PixelType>::SetSpacing(float3 spacing) {
				m_spacing = spacing;
			}
			template <typename PixelType> void CudaVolume<PixelType>::SetOrigin(float3 origin) {
				m_origin = origin;
			}
			template <typename PixelType> void CudaVolume<PixelType>::Import(int deviceID, void* d_ptr, dim3 size) {
				Free();
				m_deviceId = deviceID;
				m_isImported = true;
				m_devicePtr = (PixelType*)d_ptr;
				m_size = size;
				m_offset.x = size.x;
				m_offset.y = m_offset.x * size.y;
				m_offset.z = m_offset.y * size.z;
				m_byteSize.x = m_offset.x * sizeof(PixelType);
				m_byteSize.y = m_offset.y * sizeof(PixelType);
				m_byteSize.z = m_offset.z * sizeof(PixelType);
			}
			template <typename PixelType> void CudaVolume<PixelType>::Allocate(int deviceID, dim3 size) {
				Free();
				m_size = size;
				m_offset.x = size.x;
				m_offset.y = m_offset.x * size.y;
				m_offset.z = m_offset.y * size.z;
				m_byteSize.x = m_offset.x * sizeof(PixelType);
				m_byteSize.y = m_offset.y * sizeof(PixelType);
				m_byteSize.z = m_offset.z * sizeof(PixelType);
				CHECK_CUDA_ERROR(cudaSetDevice(deviceID));
				CHECK_CUDA_ERROR(cudaMalloc((void**)&m_devicePtr, m_byteSize.z));
				m_deviceId = deviceID;
				m_isImported = false;
			}
			template <typename PixelType> void CudaVolume<PixelType>::Free() {
				if (!m_isImported && m_devicePtr)
					CHECK_CUDA_ERROR(cudaFree(m_devicePtr));
				m_devicePtr = NULL;
			}
		}
	}
}
