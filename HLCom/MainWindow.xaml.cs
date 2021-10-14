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
using System.Collections.ObjectModel;
namespace HLCom
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
        SerialPort _sp;
        Thread _sp_handler;
        ManualResetEvent _sp_flag = new ManualResetEvent(false);
        static string format_bin(byte[] bs)
        {
            string r = "";
            foreach (byte b in bs)
            {
                r += string.Format("{0:X2} ", b);
            }
            return r;
        }
        class Msg
        {
            byte[] data;
            public int ID
            {
                get;
                set;
            }
            public DateTime Time
            {
                get;
                set;
            }
            public string Data
            {
                get {
                    return format_bin(data);
                }
            }
            public string StrData
            {
                get
                {
                    string r = "";
                    foreach (byte b in data)
                    {
                        r += (char)b;
                    }
                    return r;
                }
            }
            public byte[] bData
            {
                set { data = (byte[])value.Clone(); }
            }
        }
        ObservableCollection<Msg> _recs=new ObservableCollection<Msg>();
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
        void update_serialport()
        {
            var sp= SerialPort.GetPortNames();
            combo_port.Items.Clear();
            foreach (string s in sp)
            {
                combo_port.Items.Add(s);
            }
            combo_port.SelectedIndex = sp.Length - 1;
        }
        private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            update_serialport();
        }
        void gui_init()
        {
            int[] bs = new int[] { 9600,19200, 115200 };
            foreach (int b in bs)
            {
                combo_baud.Items.Add(b);
            }
            combo_baud.SelectedIndex = bs.Length - 1;
            string[] checks = new string[] { "none", "modbus crc16", "add8" };
            foreach (string b in checks)
            {
                combo_check.Items.Add(b);
            }
            combo_check.SelectedIndex = 0;
            update_serialport();
            list_rec.DataContext = _recs;
            this.Title = "HLCom v" + Application.ResourceAssembly.GetName().Version.ToString();
        }
        void uart_deal(byte[] lb)
        {
            Dispatcher.Invoke(new Action(() => {
                _recs.Add(new Msg()
                {
                    ID = _recs.Count,
                    Time = DateTime.Now,
                    bData = lb
                }) ;
                if (list_rec.Items.Count > 0) //scroll to the last
                {
                    var border = VisualTreeHelper.GetChild(list_rec, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null) scroll.ScrollToEnd();
                    }
                }
            }));
        }
            void serial_received()
        {
            int last_received_timeout = 0;
            List<byte> frame = new List<byte>();
            bool is_ticking = false;
            const int idle_tick = 3;
            while (true)
            {
                if(_sp_flag.WaitOne())
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
                        if (last_received_timeout > idle_tick)
                        {
                            //idle callback
                            uart_deal(frame.ToArray());
                            is_ticking = false;
                        }
                    }
                }
                Thread.Sleep(2);
            }
        }
        void var_init()
        {
            _sp = new SerialPort();
            _sp_handler = new Thread(serial_received);
            _sp_handler.Start();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            gui_init();
            var_init();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(btn_serial_open.Content.ToString()=="打开串口")
            {
                _sp.PortName = combo_port.SelectedItem.ToString();
                _sp.BaudRate = (int)combo_baud.SelectedItem;
                _sp.Encoding = Encoding.UTF8;
                _sp.Open();
                _sp_flag.Set();
                btn_serial_open.Content = "关闭串口";
            }
            else
            {
                _sp_flag.Reset();
                _sp.Close();
                btn_serial_open.Content = "打开串口";
            }
            
        }
        void send_bytes(byte[] bs)
        {
            list_history.Items.Insert(0,format_bin(bs));

            if (_sp.IsOpen)
            {
                _sp.Write(bs,0,bs.Length);
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(chk_hex.IsChecked==true)
            {
                string[] ss = text_input.Text.Split();
                byte[] bs=new byte[ss.Length];
                for(int i=0;i<bs.Length;i++)
                {
                    bs[i] = byte.Parse(ss[i], System.Globalization.NumberStyles.HexNumber);
                }
                send_bytes(bs);
            }
            else
            {
                string ss = text_input.Text;
                send_bytes(ASCIIEncoding.ASCII.GetBytes(ss));
            }

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {

        }
        public void send_modbus(byte addr, byte cmd, ushort reg,ushort value)
        {
            byte[] bs = new byte[8];
            bs[0] = addr;
            bs[1] = cmd;
            bs[2] =(byte)( reg >> 8);
            bs[3] = (byte)(reg & 0xff);
            bs[4] = (byte)(value >> 8);
            bs[5] = (byte)(value & 0xff);
            send_bytes(bs);
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
        }
    }
}
