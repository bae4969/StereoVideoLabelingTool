using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Xml;
using ImageLabelTool.Classes;

namespace ImageLabelTool.Controls
{
	public enum IMG_TYPE
	{
		NONE,
		GRAY,
		COLOR,
	}

	public class ImageInfoType
	{
		private readonly ReaderWriterLockSlim __xml_lock = new();
		private readonly XmlDocument __xml_doc = new();
		private IMG_TYPE __img_type = IMG_TYPE.NONE;
		private SIZE3 __img_size = new() { w = -1, h = -1, d = -1 };
		private SIZE3 __img_offset = new() { w = -1, h = -1, d = -1 };
		private unsafe int* __img_data_ptr = null;
		private unsafe byte* __lab_data_ptr = null;

		public IMG_TYPE ImgType { get { return __img_type; } }
		public SIZE3 ImgSize { get { return __img_size; } }
		public SIZE3 ImgOffset { get { return __img_offset; } }
		public unsafe int* ImgDataPtr { get { return __img_data_ptr; } }
		public unsafe byte* LabDataPtr { get { return __lab_data_ptr; } }


		public ImageInfoType() {
			__xml_doc.AppendChild(__xml_doc.CreateElement("Root"));
		}

		public unsafe void SetImage(IMG_TYPE type, SIZE3 size, int* img_data_ptr, byte* lab_data_ptr) {
			__img_type = type;
			__img_size = size;
			__img_offset.w = size.w;
			__img_offset.h = size.h * __img_offset.w;
			__img_offset.d = size.d * __img_offset.h;
			__img_data_ptr = img_data_ptr;
			__lab_data_ptr = lab_data_ptr;
		}
		public unsafe void ClearImage() {
			__img_type = IMG_TYPE.NONE;
			__img_size = new() { w = -1, h = -1, d = -1 };
			__img_offset = new() { w = -1, h = -1, d = -1 };
			__img_data_ptr = null;
			__lab_data_ptr = null;
		}

		public void SetXmlValue(string node_name, string attr_name, string val) {
			__xml_lock.EnterWriteLock();
			try {
				XmlElement root = __xml_doc.FirstChild as XmlElement;

				XmlElement? ele = root.SelectSingleNode(node_name) as XmlElement;
				ele ??= root.AppendChild(__xml_doc.CreateElement(node_name)) as XmlElement;

				ele?.SetAttribute(attr_name, val);
			}
			finally { __xml_lock.ExitWriteLock(); }
		}
		public void SetXmlValue(string node_name, string attr_name, float val) {
			SetXmlValue(node_name, attr_name, val.ToString());
		}
		public void SetXmlValue(string node_name, string attr_name, double val) {
			SetXmlValue(node_name, attr_name, val.ToString());
		}
		public bool GetXmlValue(string node_name, string attr_name, out string val) {
			val = "";

			__xml_lock.EnterReadLock();
			try {
				XmlElement root = __xml_doc.FirstChild as XmlElement;
				if (root.SelectSingleNode(node_name) is not XmlElement ele) return false;

				XmlAttribute? attr = ele.GetAttributeNode(attr_name);
				if (attr == null) return false;

				val = attr.Value;
				return true;
			}
			finally { __xml_lock.ExitReadLock(); }
		}
		public bool GetXmlValue(string node_name, string attr_name, out float val) {
			val = 0;
			return
				GetXmlValue(node_name, attr_name, out string val_str) &&
				float.TryParse(val_str, out val);
		}
		public bool GetXmlValue(string node_name, string attr_name, out double val) {
			val = 0;
			return
				GetXmlValue(node_name, attr_name, out string val_str) &&
				double.TryParse(val_str, out val);
		}
		public void ClearNode(string node_name) {
			__xml_lock.EnterWriteLock();
			try {
				XmlElement root = __xml_doc.FirstChild as XmlElement;

				XmlNode? node = root.SelectSingleNode(node_name);
				if (node == null) return;
				root.RemoveChild(node);
			}
			finally { __xml_lock.ExitWriteLock(); }
		}
		public void ClearNodeAll() {
			__xml_lock.EnterWriteLock();
			try {
				XmlElement root = __xml_doc.FirstChild as XmlElement;

				foreach (XmlNode node in root.ChildNodes)
					root.RemoveChild(node);
			}
			finally { __xml_lock.ExitWriteLock(); }
		}
	}

	public abstract class ImageControlBase : UserControl
	{
		protected ImageInfoType? __img_info = null;
		protected Action? __on_update_img_info = null;

		protected void SetImageInfo(ImageInfoType? img_info, Action? on_update_image_info) {
			__img_info = img_info;
			__on_update_img_info = on_update_image_info;
		}

		////////////////////////////////////////////////////////////////
		
		public abstract unsafe void OnLoadImageInfo(ImageInfoType img_info, Action on_update_image_info);
		public abstract unsafe void OnUnloadImageInfo();
		public abstract unsafe void OnUpdateImageInfo();
	}
}
