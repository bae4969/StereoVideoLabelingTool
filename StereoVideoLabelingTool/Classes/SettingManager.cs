using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace StereoVideoLabelingTool.Classes
{
	public class SettingManager
	{
		private readonly ReaderWriterLockSlim _setting_lock = new();
		private XmlDocument _xml_doc = null;
		private XmlElement _xml_root = null;

		public SettingManager() {
			_xml_doc = new();
			_xml_root = _xml_doc.FirstChild as XmlElement;
			_xml_root ??= _xml_doc.AppendChild(_xml_doc.CreateElement("ROOT")) as XmlElement;
		}
		private void _reset_settings() {
			_xml_doc = new();
			_xml_root = _xml_doc.FirstChild as XmlElement;
			_xml_root ??= _xml_doc.AppendChild(_xml_doc.CreateElement("ROOT")) as XmlElement;
		}

		public bool LoadSettings(string filename) {
			_setting_lock.EnterWriteLock();
			try {
				_xml_doc.Load(filename);
				_xml_root = _xml_doc.FirstChild as XmlElement;
				_xml_root ??= _xml_doc.AppendChild(_xml_doc.CreateElement("ROOT")) as XmlElement;
				return true;
			}
			catch { _reset_settings(); return false; }
			finally { _setting_lock.ExitWriteLock(); }
		}
		public bool SaveSettings(string filename) {
			_setting_lock.EnterReadLock();
			try {
				_xml_doc.Save(filename);
				return true;
			}
			catch { return false; }
			finally { _setting_lock.ExitReadLock(); }
		}

		public bool ImportSettings(XmlDocument xml_doc) {
			_setting_lock.EnterWriteLock();
			try {
				_xml_doc.LoadXml(xml_doc.OuterXml);
				_xml_root = _xml_doc.FirstChild as XmlElement;
				_xml_root ??= _xml_doc.AppendChild(_xml_doc.CreateElement("ROOT")) as XmlElement;
				return true;
			}
			catch { _reset_settings(); return false; }
			finally { _setting_lock.ExitWriteLock(); }
		}
		public bool ExportSettings(out XmlDocument xml_doc) {
			_setting_lock.EnterReadLock();
			try {
				xml_doc = (XmlDocument)_xml_doc.CloneNode(true);
				return true;
			}
			catch { xml_doc = null; return false; }
			finally { _setting_lock.ExitReadLock(); }
		}

		public bool GetSetting(string key, string attr, out string value) {
			_setting_lock.EnterReadLock();
			try {
				value = _xml_root.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == key)?.GetAttribute(attr);
				if (value == null) return false;
				return true;
			}
			catch { value = null; return false; }
			finally { _setting_lock.ExitReadLock(); }
		}
		public bool GetSetting(string key, string attr, out int value) {
			value = 0;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!int.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public bool GetSetting(string key, string attr, out Int64 value) {
			value = 0;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!Int64.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public bool GetSetting(string key, string attr, out float value) {
			value = 0.0f;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!float.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public bool GetSetting(string key, string attr, out double value) {
			value = 0.0;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!double.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public bool SetSetting(string key, string attr, string value) {
			_setting_lock.EnterWriteLock();
			try {
				XmlElement node = _xml_root.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == key);
				node ??= _xml_root.AppendChild(_xml_doc.CreateElement(key)) as XmlElement;
				node.SetAttribute(attr, value);
				return true;
			}
			catch { return false; }
			finally { _setting_lock.ExitWriteLock(); }
		}
		public bool SetSetting(string key, string attr, int value) {
			return SetSetting(key, attr, value.ToString());
		}
		public bool SetSetting(string key, string attr, Int64 value) {
			return SetSetting(key, attr, value.ToString());
		}
		public bool SetSetting(string key, string attr, float value) {
			return SetSetting(key, attr, value.ToString());
		}
		public bool SetSetting(string key, string attr, double value) {
			return SetSetting(key, attr, value.ToString());
		}
	}

	public static class GlobalSettingManager
	{
		private static readonly ReaderWriterLockSlim _xml_lock = new();
		private static readonly XmlDocument _xml_doc = new();
		private static readonly XmlElement _xml_root;


		static GlobalSettingManager() {
			_xml_doc.LoadXml(Properties.Settings.Default.SETTING_XML);
			_xml_root = _xml_doc.FirstChild as XmlElement;
			_xml_root ??= _xml_doc.AppendChild(_xml_doc.CreateElement("ROOT")) as XmlElement;
		}

		public static bool GetSetting(string key, string attr, out string value) {
			_xml_lock.EnterReadLock();
			try {
				value = _xml_root.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == key)?.GetAttribute(attr);
				if (value == null) {
					value = string.Empty;
					return false;
				}
				return true;
			}
			catch { value = string.Empty; return false; }
			finally { _xml_lock.ExitReadLock(); }
		}
		public static bool GetSetting(string key, string attr, out bool value) {
			value = false;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!bool.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public static bool GetSetting(string key, string attr, out int value) {
			value = 0;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!int.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public static bool GetSetting(string key, string attr, out Int64 value) {
			value = 0;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!Int64.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public static bool GetSetting(string key, string attr, out float value) {
			value = 0.0f;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!float.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public static bool GetSetting(string key, string attr, out double value) {
			value = 0.0;
			try {
				if (!GetSetting(key, attr, out string str_value) ||
					!double.TryParse(str_value, out value))
					return false;

				return true;
			}
			catch { return false; }
		}
		public static bool SetSetting(string key, string attr, string value) {
			_xml_lock.EnterWriteLock();
			try {
				XmlElement node = _xml_root.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == key);
				node ??= _xml_root.AppendChild(_xml_doc.CreateElement(key)) as XmlElement;
				node.SetAttribute(attr, value);
				Properties.Settings.Default.SETTING_XML = _xml_doc.OuterXml;
				Properties.Settings.Default.Save();
				return true;
			}
			catch { return false; }
			finally { _xml_lock.ExitWriteLock(); }
		}
		public static bool SetSetting(string key, string attr, bool value) {
			return SetSetting(key, attr, value.ToString());
		}
		public static bool SetSetting(string key, string attr, int value) {
			return SetSetting(key, attr, value.ToString());
		}
		public static bool SetSetting(string key, string attr, Int64 value) {
			return SetSetting(key, attr, value.ToString());
		}
		public static bool SetSetting(string key, string attr, float value) {
			return SetSetting(key, attr, value.ToString());
		}
		public static bool SetSetting(string key, string attr, double value) {
			return SetSetting(key, attr, value.ToString());
		}
	}

	public static class GlobalMemoryManager
	{
		private static readonly ReaderWriterLockSlim _memory_lock = new();
		private static readonly Dictionary<string, object> _memory = new();

		public static bool GetMemory(string key, out object value) {
			_memory_lock.EnterReadLock();
			try {
				if (!_memory.ContainsKey(key)) {
					value = null;
					return false;
				}
				value = _memory[key];
				return true;
			}
			catch { value = null; return false; }
			finally { _memory_lock.ExitReadLock(); }
		}
		public static bool SetMemory(string key, object value) {
			_memory_lock.EnterWriteLock();
			try {
				_memory[key] = value;
				return true;
			}
			catch { return false; }
			finally { _memory_lock.ExitWriteLock(); }
		}
		public static bool RemoveMemory(string key) {
			_memory_lock.EnterWriteLock();
			try {
				if (!_memory.ContainsKey(key))
					return false;

				_memory.Remove(key);
				return true;
			}
			catch { return false; }
			finally { _memory_lock.ExitWriteLock(); }
		}
	}
}
