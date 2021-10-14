using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            serial_ui.send_bytes = serial_core.send_bytes;
            serial_core.add_listener(serial_ui.received);
            serial_0401.send_bytes = serial_core.send_bytes;
            serial_core.add_listener(serial_0401.received);
            hl0601.send_bytes = serial_core.send_bytes;
            serial_core.add_listener(hl0601.received);
        }
    }
}
