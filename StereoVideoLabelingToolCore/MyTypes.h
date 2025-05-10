#pragma once
#include "MyMacro.h"
#include <itkImage.h>
#include <itkMesh.h>
#include <itkShapeLabelObject.h>
#include <pcl/point_cloud.h>
#include <pcl/point_types.h>
#include <pcl/io/pcd_io.h>

namespace Bae {
	using Vol8Type = itk::Image<unsigned char, 3>;
	using Vol16Type = itk::Image<unsigned short, 3>;
	using Vol32Type = itk::Image<float, 3>;
	using Vol64Type = itk::Image<double, 3>;
	using Vol8Ptr = Vol8Type::Pointer;
	using Vol16Ptr = Vol16Type::Pointer;
	using Vol32Ptr = Vol32Type::Pointer;
	using Vol64Ptr = Vol64Type::Pointer;

	using Mesh8Type = itk::Mesh<unsigned char, 3>;
	using Mesh16Type = itk::Mesh<unsigned short, 3>;
	using Mesh32Type = itk::Mesh<float, 3>;
	using Mesh64Type = itk::Mesh<double, 3>;
	using Mesh8Ptr = Mesh8Type::Pointer;
	using Mesh16Ptr = Mesh16Type::Pointer;
	using Mesh32Ptr = Mesh32Type::Pointer;
	using Mesh64Ptr = Mesh64Type::Pointer;

	using LabelType = itk::ShapeLabelObject<unsigned char, 3>;
	using LabelPtr = LabelType::Pointer;

	using PointType = pcl::PointXYZRGB;
	using PointCloudType = pcl::PointCloud<PointType>;
	using MeshType = pcl::PolygonMesh;
}
