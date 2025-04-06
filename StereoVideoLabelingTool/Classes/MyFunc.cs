using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StereoVideoLabelingTool.Classes
{
    class MyFunc
	{
		public static (List<string>, string) GetExtensionList(string path) {
			var ret = new List<string>();
			var dir = Path.GetDirectoryName(path);
			var ext = Path.GetExtension(path);
			while (string.IsNullOrWhiteSpace(ext) == false) {
				ret.Add(ext);
				path = Path.GetFileNameWithoutExtension(path);
				ext = Path.GetExtension(path);
			}

			if (dir.Length > 0)
				dir += '\\';

			return (ret, $"{dir}{path}");
		}
	}
}
