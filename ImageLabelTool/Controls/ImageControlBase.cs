using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using ImageLabelTool.Classes;

namespace ImageLabelTool.Controls
{
    public abstract class ImageControlBase : UserControl
	{
		public abstract unsafe void OnLoadImageData(Int64 type, SIZE3 size, Int32* img_data_ptr, Byte* lab_data_ptr);
        public abstract unsafe void OnUnloadImageData();

        public abstract unsafe void OnUpstreamImageData();
        public abstract unsafe void OnDownstreamImageData();
	}
}
