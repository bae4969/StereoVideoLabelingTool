#pragma once

#ifdef IMAGELABELINGTOOLCORE_EXPORTS
#define CORE_FUNC_EXPORT extern "C" __declspec(dllexport)
#endif

#define CLASS_VAR_TEMPLATE(TYPE, VAR) protected: TYPE m_##VAR;
#define CLASS_GET_TEMPLATE(TYPE, VAR) public: virtual TYPE Get##VAR() { return m_##VAR; }
#define CLASS_SET_TEMPLATE(TYPE, VAR) public: virtual void Set##VAR(TYPE var) { m_##VAR = var; }

#define CLASS_VAR_GET_SET_TEMPLATE(TYPE, VAR)	\
	CLASS_VAR_TEMPLATE(TYPE, VAR)				\
	CLASS_GET_TEMPLATE(TYPE, VAR)				\
	CLASS_SET_TEMPLATE(TYPE, VAR)

#define CLASS_VAR_GET_TEMPLATE(TYPE, VAR)		\
	CLASS_VAR_TEMPLATE(TYPE, VAR)				\
	CLASS_GET_TEMPLATE(TYPE, VAR)

#define MY_PI										3.1415926535897932384626433832795
#define MY_PI_INV									0.31830988618379067154 // 1 / PI
#define MY_NATURE									2.7182818284590452353602874713527
#define MY_BASE_LOG(BASE, VAL)						log(VAL) / log(BASE)
#define MY_MIN(X, Y)								X < Y ? X : Y
#define MY_MAX(X, Y)								X > Y ? X : Y
#define MY_DEGREE_TO_RADIAN(degree)					degree * 0.01745329251994329576923690768489
#define MY_RADIAN_TO_DEGREE(radian)					rad * 57.295779513082320876798154814105

#define NEW_SIG(TYPE, VAR)							TYPE::Pointer VAR = TYPE::New()
#define NEW_TMP(TYPE, VAR, ...)						TYPE<##__VA_ARGS__>::Pointer VAR = TYPE<##__VA_ARGS__>::New()
#define FORMAT_STR(format_str, ...)					std::format(format_str, __VA_ARGS__).c_str()
#define THROW_EXCEPTION_STR(format_str, ...)		throw std::exception(std::format(format_str, __VA_ARGS__).c_str())

#define AUTO_CATCH(code)																\
catch (cv::Exception ex) { std::string ERROR_MSG = ex.what(); code}						\
catch (itk::ExceptionObject ex) { std::string ERROR_MSG = ex.GetDescription(); code}	\
catch (pcl::PCLException ex) { std::string ERROR_MSG = ex.detailedMessage(); code}		\
catch (std::exception ex) { std::string ERROR_MSG = ex.what(); code}					\
catch (...) { std::string ERROR_MSG = "..."; code}

#define PRINT_DEBUG_MSG(format_str, ...) {																		\
	auto now = std::chrono::system_clock::now();																\
	auto now_time = std::chrono::floor<std::chrono::seconds>(now);												\
	auto milli_sec = std::chrono::duration_cast<std::chrono::milliseconds>(now.time_since_epoch()) % 1000;		\
	auto msg = std::format(format_str, __VA_ARGS__);															\
	auto filename = std::filesystem::path(__FILE__).filename().string();															\
	OutputDebugStringA(std::format("[ MY ][ {:%Y-%m-%d %H:%M:%S}.{:03d} | AlgorithmDLL ] {:s} ( {:s}:{:d} | {:s} )\n", now_time, milli_sec.count(), msg, __func__, __LINE__, filename).c_str());	\
}

#define LOG_CODE_STR(code, str)	TV_LOG(code, str);
#ifdef MY_DEBUG
#define LOG_N(format_str, ...)		LOG_CODE_STR(TV::LOG::CODE_PRINT_CONSOLE | TV::LOG::CODE_NORMAL, std::format(format_str, __VA_ARGS__).c_str())
#define LOG_W(format_str, ...)		LOG_CODE_STR(TV::LOG::CODE_PRINT_CONSOLE | TV::LOG::CODE_WARNING | TV::LOG::CODE_PRINT_LOCATION, std::format(format_str, __VA_ARGS__).c_str())
#define LOG_E(format_str, ...)		LOG_CODE_STR(TV::LOG::CODE_PRINT_CONSOLE | TV::LOG::CODE_ERROR | TV::LOG::CODE_PRINT_LOCATION, std::format(format_str, __VA_ARGS__).c_str())
#define LOG_D(format_str, ...)		LOG_CODE_STR(TV::LOG::CODE_PRINT_CONSOLE | TV::LOG::CODE_DEBUG | TV::LOG::CODE_PRINT_LOCATION, std::format(format_str, __VA_ARGS__).c_str())
#else
#define LOG_N(format_str, ...)		LOG_CODE_STR(TV::LOG::CODE_NORMAL, std::format(format_str, __VA_ARGS__).c_str())
#define LOG_W(format_str, ...)		LOG_CODE_STR(TV::LOG::CODE_WARNING | TV::LOG::CODE_PRINT_LOCATION, std::format(format_str, __VA_ARGS__).c_str())
#define LOG_E(format_str, ...)		LOG_CODE_STR(TV::LOG::CODE_ERROR | TV::LOG::CODE_PRINT_LOCATION, std::format(format_str, __VA_ARGS__).c_str())
#define LOG_D(format_str, ...)
#endif

