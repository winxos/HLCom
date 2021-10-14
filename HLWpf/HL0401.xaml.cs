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
using System.Threading;
namespace HLWpf
{
    /// <summary>
    /// HL0401.xaml 的交互逻辑
    /// </summary>
    public partial class HL0401 : UserControl
    {
        public Action<byte[]> send_bytes
        {
            set=>mm.send_bytes=value;
        }
        Ellipse[] io_out;
        Ellipse[] io_in;
        HLib.ModbusMaster mm = new HLib.ModbusMaster();
        ManualResetEvent mre = new ManualResetEvent(false);
        Ellipse draw_io(string id)
        {
            Ellipse e = new Ellipse();
            e.Uid = id;
            return e;
        }
        public void received(byte[] bs)
        {
            mm.received_bytes(bs);
        }
        public HL0401()
        {
            InitializeComponent();
        }
        void timeout()
        {
            Console.WriteLine("timeout");
        }
        private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Ellipse ee = (Ellipse)sender;
            if (ee.Fill == Brushes.Firebrick)
            {
                ee.Fill = Brushes.LightGreen;
                mm.write_register(Convert.ToByte(addr.Text), (ushort)(0x30 + int.Parse(ee.Uid)), 0x0000, null, timeout);
            }
            else
            {
                ee.Fill = Brushes.Firebrick;
                mm.write_register(Convert.ToByte(addr.Text), (ushort)(0x30 + int.Parse(ee.Uid)), 0xff00, null, timeout);
            }
        }
        void input_update()
        {
            while(true)
            {
                if(mre.WaitOne())
                {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        mm.read_registers(Convert.ToByte(addr.Text), 0x20, 4,
                            new Action<byte[]>((byte[] bs) => {
                            Dispatcher.Invoke(new Action(() => {
                                for(int i=0;i<4;i++)
                                {
                                    if ((bs[3+2*i]) == 0)
                                    {
                                        io_in[i].Fill = Brushes.LightGreen;
                                    }
                                    else
                                    {
                                        io_in[i].Fill = Brushes.Firebrick;
                                    }
                                }
                            }));
                        }), timeout);
                    }));
                }
                System.Threading.Thread.Sleep(500);
            }
        }
        private void In_update_Click(object sender, RoutedEventArgs e)
        {
            if (in_update.IsChecked == true)
            {
                mre.Set();
            }
            else
            {
                mre.Reset();
            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            new System.Threading.Thread(input_update).Start();
            Console.WriteLine("{0} loaded {1}", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
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
            io_in = new Ellipse[4];
            for (int i = 0; i < 4; i++)
            {
                Label t = new Label();
                t.Content = "IN" + (1 + i).ToString();
                t.SetValue(Grid.ColumnProperty, i + io_out.Length);
                t.SetValue(Grid.RowProperty, 0);
                io_grid.Children.Add(t);
                io_in[i] = draw_io(i.ToString());
                io_grid.Children.Add(io_in[i]);
                io_in[i].StrokeThickness = 10;
                io_in[i].Fill = Brushes.Firebrick;
                io_in[i].Stroke = Brushes.LightBlue;
                io_in[i].SetValue(Grid.RowProperty, 1);
                io_in[i].SetValue(Grid.ColumnProperty, i + io_out.Length);
            }
        }
    }
}
