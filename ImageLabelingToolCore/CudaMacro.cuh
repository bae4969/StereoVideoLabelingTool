#pragma once


#define MY_PI					3.1415926535897932384626433832795
#define MY_NATURE				2.7182818284590452353602874713527
#define PTR_SWAP(ptr1, ptr2)	{ auto t_ptr = ptr1; ptr1 = ptr2; ptr2 = t_ptr; }
#define CUDA_KERNEL_END()		CHECK_CUDA_ERROR(cudaDeviceSynchronize());

#ifdef __INTELLISENSE__
#define CUDA_KERNEL_2D_LINEAR_FUNCTION(func, ...)	func
#define CUDA_KERNEL_3D_LINEAR_FUNCTION(func, ...)	func
#define CUDA_KERNEL_3D_KERNEL_FUNCTION(func, ...)	func
#else
#define CUDA_KERNEL_2D_LINEAR_FUNCTION(func, ...)	func<<<MACRO_CUDA_2D_LINEAR_GRID_VAR, MACRO_CUDA_THREAD_VAR>>>(__VA_ARGS__)
#define CUDA_KERNEL_3D_LINEAR_FUNCTION(func, ...)	func<<<MACRO_CUDA_3D_LINEAR_GRID_VAR, MACRO_CUDA_THREAD_VAR>>>(__VA_ARGS__)
#define CUDA_KERNEL_3D_KERNEL_FUNCTION(func, ...)	func<<<MACRO_CUDA_3D_KERNEL_GRID_VAR, MACRO_CUDA_THREAD_VAR>>>(__VA_ARGS__)
#endif

#define CHECK_CUDA_ERROR(call) \
    { \
        const cudaError_t error = call; \
        if (error != cudaSuccess) { \
            std::cout << "Cuda Error: " << __FILE__ << ":" << __LINE__ << ", code: " << error << ", reason: " << cudaGetErrorString(error) << std::endl; \
            throw error; \
        } \
    }
#define CHECK_CUFFT_ERROR(call) \
    { \
        const cufftResult error = call; \
        if (error != CUFFT_SUCCESS) { \
            std::cout << "Cuda Error: " << __FILE__ << ":" << __LINE__ << ", code: " << error << std::endl; \
            throw error; \
        } \
    }
#define CHECK_NPP_ERROR(call) \
    { \
        const NppStatus error = call; \
        if (error != NppStatus::NPP_SUCCESS) { \
            std::cout << "Cuda Error: " << __FILE__ << ":" << __LINE__ << ", code: " << error << std::endl; \
            throw error; \
        } \
    }

#define CUDA_INIT_DEVICE(device_index)																		\
	dim3 MACRO_CUDA_THREAD_VAR; {																			\
		cudaDeviceProp prop{};																				\
		CHECK_CUDA_ERROR(cudaSetDevice(device_index));														\
		CHECK_CUDA_ERROR(cudaGetDeviceProperties(&prop, device_index));										\
		int blockSize = sqrt(prop.maxThreadsPerBlock);														\
		MACRO_CUDA_THREAD_VAR.x = blockSize;																\
		MACRO_CUDA_THREAD_VAR.y = blockSize;																\
		MACRO_CUDA_THREAD_VAR.z = 1;																		\
	}
#define CUDA_INIT_2D_LINEAR_GRID(device_index, img_size)													\
	dim3 MACRO_CUDA_2D_LINEAR_GRID_VAR {																	\
		(img_size.x + MACRO_CUDA_THREAD_VAR.x - 1) / MACRO_CUDA_THREAD_VAR.x,								\
		(img_size.y + MACRO_CUDA_THREAD_VAR.y - 1) / MACRO_CUDA_THREAD_VAR.y,								\
		1																									\
	}
#define CUDA_INIT_3D_LINEAR_GRID(device_index, img_size)													\
	dim3 MACRO_CUDA_3D_LINEAR_GRID_VAR {																	\
		(img_size.x + MACRO_CUDA_THREAD_VAR.x - 1) / MACRO_CUDA_THREAD_VAR.x,								\
		(img_size.y + MACRO_CUDA_THREAD_VAR.y - 1) / MACRO_CUDA_THREAD_VAR.y,								\
		(img_size.z + MACRO_CUDA_THREAD_VAR.z - 1) / MACRO_CUDA_THREAD_VAR.z								\
	}
#define CUDA_INIT_3D_KERNEL_GRID(device_index, img_size, kernel_size)										\
	dim3 MACRO_CUDA_3D_KERNEL_GRID_VAR {																	\
		(img_size.x / kernel_size.x + MACRO_CUDA_THREAD_VAR.x - 1) / MACRO_CUDA_THREAD_VAR.x,				\
		(img_size.y / kernel_size.y + MACRO_CUDA_THREAD_VAR.y - 1) / MACRO_CUDA_THREAD_VAR.y,				\
		(img_size.z / kernel_size.z + MACRO_CUDA_THREAD_VAR.z - 1) / MACRO_CUDA_THREAD_VAR.z				\
	}


