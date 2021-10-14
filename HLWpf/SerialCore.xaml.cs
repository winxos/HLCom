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
using System.Threading;

namespace HLWpf
{
    /// <summary>
    /// SerialCore.xaml 的交互逻辑
    /// </summary>
    public partial class SerialCore : UserControl
    {
        SerialPort _sp=new SerialPort();
        Action<byte[]> received;
        ManualResetEvent _sp_flag = new ManualResetEvent(false);
        Queue<byte[]> _frames = new Queue<byte[]>();
        public SerialCore()
        {
            InitializeComponent();
        }
        public void add_listener(Action<byte[]> rec)
        {
            received += rec;
        }
        
        public void send_bytes(byte[] bs)
        {
            if(_sp.IsOpen)
            {
                Console.WriteLine("{0} out {1}",DateTime.Now, bs.Length);
                _sp.Write(bs, 0, bs.Length);
            }
        }
        public void send_texts(string s)
        {
            if(_sp.IsOpen)
            {
                _sp.Write(s);
            }
        }
        void update_serialport()
        {
            var sp = SerialPort.GetPortNames();
            combo_port.Items.Clear();
            foreach (string s in sp)
            {
                combo_port.Items.Add(s);
            }
            combo_port.SelectedIndex = sp.Length - 1;
        }
        void gui_init()
        {
            int[] bs = new int[] { 9600, 19200, 115200 };
            foreach (int b in bs)
            {
                combo_baud.Items.Add(b);
            }
            combo_baud.SelectedIndex = bs.Length - 1;
            update_serialport();
            btn_serial_open.Background = Brushes.Firebrick;
        }
        private void Btn_serial_open_Click(object sender, RoutedEventArgs e)
        {
            if (btn_serial_open.Content.ToString() == "打开串口")
            {
                _sp.PortName = combo_port.SelectedItem.ToString();
                _sp.BaudRate = (int)combo_baud.SelectedItem;
                _sp.Encoding = Encoding.UTF8;
                _sp.Open();
                _sp_flag.Set();
                btn_serial_open.Content = "关闭串口";
                btn_serial_open.Background = Brushes.LightGreen;
            }
            else
            {
                _sp_flag.Reset();
                _sp.Close();
                btn_serial_open.Content = "打开串口";
                btn_serial_open.Background = Brushes.Firebrick;
            }
        }
        void serial_received()
        {
            int last_received_timeout = 0;
            List<byte> frame = new List<byte>();
            bool is_ticking = false;
            const int idle_tick = 2;
            while (true)
            {
                if (_sp_flag.WaitOne())
                {
                    while (_sp.BytesToRead > 0)
                    {
                        if (is_ticking == false)
                        {
                            is_ticking = true;
                            frame.Clear();//数据上升沿
                        }
                        frame.Add((byte)_sp.ReadByte());
                        last_received_timeout = 0;
                    }
                    if (is_ticking)
                    {
                        last_received_timeout++;
                        
                        if (last_received_timeout >= idle_tick)
                        {
                            //idle callback
                            _frames.Enqueue(frame.ToArray());
                            Console.WriteLine("{0} in {1}", DateTime.Now, frame.Count);
                            is_ticking = false;
                        }
                    }
                }
                Thread.Sleep(2);
            }
        }
        void callback()
        {
            while(true)
            {
                if(_sp_flag.WaitOne())
                {
                    if (_frames.Count > 0)
                    {
                        received?.Invoke(_frames.Dequeue());
                    }
                }
                Thread.Sleep(10);
            }
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            gui_init();
            new Thread(serial_received).Start();
            new Thread(callback).Start();
        }
    }
}
