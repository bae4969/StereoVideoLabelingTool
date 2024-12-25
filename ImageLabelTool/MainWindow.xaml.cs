using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ImageLabelTool.Classes;
using ImageLabelTool.Controls;


namespace ImageLabelTool
{
	public partial class MainWindow : Window
	{
		public MainWindow() {
			InitializeComponent();

			string before_path_env = Environment.GetEnvironmentVariable("Path")??"";
			Environment.SetEnvironmentVariable("Path", before_path_env + "./bin;");
			Core.Initialize(null);
		}
	}
}