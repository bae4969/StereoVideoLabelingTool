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
		public abstract unsafe void OnLoadImageData(IMG_TYPE type, SIZE3 size, int* img_data_ptr, byte* lab_data_ptr);
        public abstract unsafe void OnUnloadImageData();

        public abstract unsafe void OnUpstreamImageData();
        public abstract unsafe void OnDownstreamImageData();
	}
}
