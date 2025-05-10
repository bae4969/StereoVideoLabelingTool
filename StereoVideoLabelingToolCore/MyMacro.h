#pragma once

#ifdef STEREOVIDEOLABELINGTOOLCORE_EXPORTS
#define STEREOVIDEOLABELINGTOOLCORE_EXPORT extern "C" __declspec(dllexport) 
#else
#define STEREOVIDEOLABELINGTOOLCORE_EXPORT extern "C" __declspec(dllimport)
#endif


#define FORMAT_STR(FORMAT_STR, ...) std::format(FORMAT_STR, ##__VA_ARGS__)
#define DATETIME_STR(WITH_SPLITER) WITH_SPLITER ? std::format("{:%Y-%m-%d %H:%M:%S}", std::chrono::system_clock::now()) : std::format("{:%Y%m%d%H%M%S}", std::chrono::system_clock::now())


#define PRINT_LOG_COMMON(FORMAT, TYPE, ...) OutputDebugStringA((FORMAT_STR("[ {:%Y-%m-%d %H:%M:%S} ][ {:s} ] ", std::chrono::system_clock::now(),TYPE) + FORMAT_STR(FORMAT, ##__VA_ARGS__) + FORMAT_STR(" ( {:s} | {:d} )\n", std::filesystem::path(__FILE__).filename().string(), __LINE__)).c_str());
#define PRINT_LOG_N(FORMAT, ...)  PRINT_LOG_COMMON(FORMAT, "INFO", ##__VA_ARGS__)
#define PRINT_LOG_D(FORMAT, ...)  PRINT_LOG_COMMON(FORMAT, "DEBUG", ##__VA_ARGS__)
#define PRINT_LOG_W(FORMAT, ...)  PRINT_LOG_COMMON(FORMAT, "WARN", ##__VA_ARGS__)
#define PRINT_LOG_E(FORMAT, ...)  PRINT_LOG_COMMON(FORMAT, "ERROR", ##__VA_ARGS__)
#define PRINT_LOG_F(FORMAT, ...)  PRINT_LOG_COMMON(FORMAT, "FATAL", ##__VA_ARGS__)


#define DEGREE_TO_RADIAN(deg) (deg) * 0.0174532925199432957692369076849
#define RADIAN_TO_DEGREE(rad) (rad) * 57.295779513082320876798154814105


#define ITK_NEW(TYPE, VAR)	TYPE::Pointer VAR = TYPE::New()
#define PCL_NEW(TYPE, VAR)	TYPE::Ptr VAR(new TYPE)


#define THROW_STR(FORMAT, ...) throw std::exception(FORMAT_STR(FORMAT, ##__VA_ARGS__).c_str())
#define AUTO_CATCH(FUNC)																							\
catch (cv::Exception ex) { std::string EX_MSG = FORMAT_STR("OpenCV | {:s}", ex.what()); FUNC }					\
catch (itk::ExceptionObject ex) { std::string EX_MSG = FORMAT_STR("ITK | {:s}", ex.what()); FUNC }				\
catch (pcl::PCLException ex) { std::string EX_MSG = FORMAT_STR("PCL | {:s}", ex.what()); FUNC }					\
catch (std::bad_alloc ex) { std::string EX_MSG = FORMAT_STR("Bad Alloc | {:s}", ex.what()); FUNC }				\
catch (std::out_of_range ex) { std::string EX_MSG = FORMAT_STR("Out Of Range | {:s}", ex.what()); FUNC }		\
catch (std::invalid_argument ex) { std::string EX_MSG = FORMAT_STR("Invalid Argu | {:s}", ex.what()); FUNC }	\
catch (std::length_error ex) { std::string EX_MSG = FORMAT_STR("Length | {:s}", ex.what()); FUNC }				\
catch (std::range_error ex) { std::string EX_MSG = FORMAT_STR("Range | {:s}", ex.what()); FUNC }				\
catch (std::exception ex) { std::string EX_MSG = FORMAT_STR("Common | {:s}", ex.what()); FUNC }					\
catch (...) { std::string EX_MSG = FORMAT_STR("Common | ..."); FUNC }	


#define MY_DEF(TYPE, VAR)	protected: TYPE m_##VAR;
#define MY_GET(TYPE, VAR)	public: TYPE Get##VAR() { return m_##VAR; }
#define MY_SET(TYPE, VAR)	public: void Set##VAR(TYPE _##VAR) { m_##VAR = _##VAR; }

#define MY_CLASS_GET_SET(TYPE, VAR)	\
MY_DEF(TYPE, VAR)					\
MY_GET(TYPE, VAR)					\
MY_SET(TYPE, VAR)

#define MY_CLASS_GET(TYPE, VAR)		\
MY_DEF(TYPE, VAR)					\
MY_GET(TYPE, VAR)

#define MY_CLASS_SET(TYPE, VAR)		\
MY_DEF(TYPE, VAR)					\
MY_SET(TYPE, VAR)


