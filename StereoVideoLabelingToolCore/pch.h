#ifndef PCH_H
#define PCH_H

#include <thread>
#include <shared_mutex>
#include <filesystem>
#include <chrono>

#include <tinyxml2.h>
#include <zmq.hpp>
#include <zmq_addon.hpp>

#include <opencv2/opencv.hpp>

#include <itkImage.h>
#include <itkMesh.h>
#include <itkImageFileReader.h>
#include <itkImageFileWriter.h>

#include <pcl/point_cloud.h>
#include <pcl/point_types.h>

#include <vtkSmartPointer.h>
#include <vtkPolyData.h>
#include <vtkRenderer.h>
#include <vtkRenderWindow.h>
#include <vtkRenderWindowInteractor.h>
#include <vtkActor.h>

#endif
