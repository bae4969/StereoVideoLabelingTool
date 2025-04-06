using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StereoVideoLabelingTool.Classes
{
    public static class MyConst
	{
		public static readonly string LAST_WORK_BACKUP_PATH = "./temp/last_edit_data/backup.xml";
		public static readonly int[] LABEL_COLOR_MAP = new int[256];

		static MyConst() {
			Random random = new(4969);
			for (int i = 0; i < LABEL_COLOR_MAP.Length; i++) {
				int b = i % 3 != 0 ? random.Next(130, 230) : 0;
				int g = i % 3 != 1 ? random.Next(130, 230) : 0;
				int r = i % 3 != 2 ? random.Next(130, 230) : 0;
				LABEL_COLOR_MAP[i] = (int)((b << 24) | (g << 16) | (r << 8) | 0xFF);
			}
			LABEL_COLOR_MAP[0] = 0xFF;
		}
		public static int ToBGRA(int img, int lab, float min, float max, float blend) {
			var t_color = MyConst.LABEL_COLOR_MAP[lab];
			int t_r = Math.Clamp((int)((((img >> 08) & 0xFF) - min) / (max - min + 1) * 255.0f), 0, 255);
			int t_g = Math.Clamp((int)((((img >> 16) & 0xFF) - min) / (max - min + 1) * 255.0f), 0, 255);
			int t_b = Math.Clamp((int)((((img >> 24) & 0xFF) - min) / (max - min + 1) * 255.0f), 0, 255);
			int r = (int)(t_r * (1.0 - blend) + (((t_color >> 08) & 0xFF) * blend));
			int g = (int)(t_g * (1.0 - blend) + (((t_color >> 16) & 0xFF) * blend));
			int b = (int)(t_b * (1.0 - blend) + (((t_color >> 24) & 0xFF) * blend));
			return (b << 0) | (g << 8) | (r << 16) | (0xFF << 24);
		}
	}
}
