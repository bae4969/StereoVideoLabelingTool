using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageLabelingTool.Classes
{
	public struct StringPairType
	{
		public string Outter { get; set; }
		public object Inner { get; set; }

		public StringPairType() {
			Outter = string.Empty;
			Inner = string.Empty;
		}
	}
}
