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
using System.IO.Ports;
namespace HL0401View
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class HL0401View : UserControl
    {
        public HL0401View()
        {
            InitializeComponent();
        }
        public Action<byte[]> send_bytes;
        public byte addr;
        System.Windows.Threading.DispatcherTimer dtimer;
        Ellipse[] io_out;
        Ellipse[] io_in;
        Ellipse draw_io(string id)
        {
            Ellipse e = new Ellipse();
            e.Uid = id;
            return e;
        }
        public void get_modbus(byte[] bs)
        {

        }
        public byte[] build_modbus(byte addr, byte cmd, ushort reg, ushort value)
        {
            byte[] bs = new byte[8];
            bs[0] = addr;
            bs[1] = cmd;
            bs[2] = (byte)(reg >> 8);
            bs[3] = (byte)(reg & 0xff);
            bs[4] = (byte)(value >> 8);
            bs[5] = (byte)(value & 0xff);
            return bs;
        }
        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse ee = (Ellipse)sender;
            if (ee.Fill == Brushes.Firebrick)
            {
                ee.Fill = Brushes.LightGreen;
                send_bytes(build_modbus(addr, 6, (ushort)(0x20 + int.Parse(ee.Uid)), 0xff00));
            }
            else
            {
                ee.Fill = Brushes.Firebrick;
                send_bytes(build_modbus(addr, 6, (ushort)(0x20 + int.Parse(ee.Uid)), 0x0000));
            }
        }
        void input_update(object sender, EventArgs e)
        {
            if (in_update.IsChecked == true)
            {
                send_bytes(build_modbus(addr, 3, 0x30, 6));
            }

        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            dtimer = new System.Windows.Threading.DispatcherTimer();
            dtimer.Interval = TimeSpan.FromMilliseconds(1000);
            dtimer.Tick += input_update;
            dtimer.Start();
            io_out = new Ellipse[4];
            for (int i = io_out.Length - 1; i >= 0; i--)
            {
                Label t = new Label();
                t.Content = "OUT" + (1 + i).ToString();
                t.SetValue(Grid.ColumnProperty, io_out.Length - 1 - i);
                t.SetValue(Grid.RowProperty, 0);
                io_grid.Children.Add(t);
                io_out[i] = draw_io((io_out.Length - 1 - i).ToString());
                io_grid.Children.Add(io_out[i]);
                io_out[i].StrokeThickness = 10;
                io_out[i].Fill = Brushes.Firebrick;
                io_out[i].Stroke = Brushes.LightBlue;
                io_out[i].SetValue(Grid.RowProperty, 1);
                io_out[i].SetValue(Grid.ColumnProperty, i);
                io_out[i].MouseDown += Ellipse_MouseDown;
            }
            io_in = new Ellipse[6];
            for (int i = 0; i < 6; i++)
            {
                Label t = new Label();
                t.Content = "IN" + (1 + i).ToString();
                t.SetValue(Grid.ColumnProperty, i + 4);
                t.SetValue(Grid.RowProperty, 0);
                io_grid.Children.Add(t);
                io_in[i] = draw_io(i.ToString());
                io_grid.Children.Add(io_in[i]);
                io_in[i].StrokeThickness = 10;
                io_in[i].Fill = Brushes.Firebrick;
                io_in[i].Stroke = Brushes.LightBlue;
                io_in[i].SetValue(Grid.RowProperty, 1);
                io_in[i].SetValue(Grid.ColumnProperty, i + 4);
            }
        }
    }
}
