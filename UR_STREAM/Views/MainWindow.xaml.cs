using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UR_STREAM.ViewModels;
namespace UR_STREAM
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        private readonly MainWindowViewModel MV = null;

        public MainWindow()
        {
            InitializeComponent();
            MV = new MainWindowViewModel();
            this.DataContext = MV;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton)
            {
                MV.CanConnect = false;
            }
        }
    }
}
