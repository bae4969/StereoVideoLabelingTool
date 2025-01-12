using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ImageLabelingTool.Classes
{
	public static class SettingManager
	{
		private static readonly ReaderWriterLockSlim __xml_lock = new();
		private static readonly XmlDocument __xml_doc = new();
		private static readonly XmlElement __xml_root;


		static SettingManager() {
			__xml_doc.LoadXml(Properties.Settings.Default.SETTING_XML);
			__xml_root = __xml_doc.FirstChild as XmlElement;
			__xml_root ??= __xml_doc.AppendChild(__xml_doc.CreateElement("ROOT")) as XmlElement;
		}

		public static bool GetSetting(string key, out string? value) {
			__xml_lock.EnterReadLock();
			try {
				value = __xml_root.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == key)?.GetAttribute("VALUE");
				if(value == null) return false;
				return true;
			}
			catch {
				value = null;
				return false;
			}
			finally {
				__xml_lock.ExitReadLock();
			}
		}
		public static bool SetSetting(string key, string value) {
			__xml_lock.EnterWriteLock();
			try {
				XmlElement? node = __xml_root.ChildNodes.Cast<XmlElement>().FirstOrDefault(x => x.Name == key);
				node ??= __xml_root.AppendChild(__xml_doc.CreateElement(key)) as XmlElement;
				node.SetAttribute("VALUE", value);
				Properties.Settings.Default.SETTING_XML = __xml_doc.OuterXml;
				Properties.Settings.Default.Save();
				return true;
			}
			catch {
				return false;
			}
			finally {
				__xml_lock.ExitWriteLock();
			}
		}
	}
}
