#pragma once

#ifdef STEREOVIDEOLABELINGTOOLCORE_EXPORTS
#define STEREOVIDEOLABELINGTOOLCORE_EXPORT extern "C" __declspec(dllexport) 
#else
#define STEREOVIDEOLABELINGTOOLCORE_EXPORT extern "C" __declspec(dllimport)
#endif

#define DEBUG_PRINT(FORMAT_STR, ...) {								\
std::string output_str = std::format(FORMAT_STR, ##__VA_ARGS__);	\
output_str += '\n';													\
OutputDebugStringA(output_str.c_str());}

#define DEGREE_TO_RADIAN(deg) deg * 0.0174532925199432957692369076849
#define RADIAN_TO_DEGREE(rad) rad * 57.295779513082320876798154814105



#define ITK_NEW(TYPE, VAR)	TYPE::Pointer VAR = TYPE::New()
#define PCL_NEW(TYPE, VAR)	TYPE::Ptr VAR(new TYPE)

#define AUTO_CATCH(FUNC)													\
catch (cv::Exception ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ OpenCV ] {:s}", EX_MSG); FUNC }					\
catch (itk::ExceptionObject ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ ITK ] {:s}", EX_MSG); FUNC }				\
catch (pcl::PCLException ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ PCL ] {:s}", EX_MSG); FUNC }				\
catch (std::bad_alloc ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ Bad Alloc ] {:s}", EX_MSG); FUNC }				\
catch (std::out_of_range ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ Out Of Range ] {:s}", EX_MSG); FUNC }		\
catch (std::invalid_argument ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ Invalid Argu ] {:s}", EX_MSG); FUNC }	\
catch (std::length_error ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ Length ] {:s}", EX_MSG); FUNC }				\
catch (std::logic_error ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ Logic ] {:s}", EX_MSG); FUNC }				\
catch (std::domain_error ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ Domain ] {:s}", EX_MSG); FUNC }				\
catch (std::range_error ex) { std::string EX_MSG = ex.what(); DEBUG_PRINT("[ Error ] [ Range ] {:s}", EX_MSG); FUNC }				\
catch (std::exception ex) { std::string EX_MSG = ex.what();	DEBUG_PRINT("[ Error ] [ Common ] {:s}", EX_MSG); FUNC }				\
catch (...) { std::string EX_MSG = "..."; }	


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


