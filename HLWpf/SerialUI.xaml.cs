using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Threading;

namespace HLWpf
{
    /// <summary>
    /// SerialUI.xaml 的交互逻辑
    /// </summary>
    public partial class SerialUI : UserControl
    {
        public Action<byte[]> send_bytes;
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
            public string BinData
            {
                get
                {
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
        ObservableCollection<Msg> _recs = new ObservableCollection<Msg>();
        public SerialUI()
        {
            InitializeComponent();
        }

        public void received(byte[] bs)
        {
            Dispatcher.Invoke(new Action(() => {
                _recs.Add(new Msg()
                {
                    ID = _recs.Count,
                    Time = DateTime.Now,
                    bData = bs
                });
                if (list_rec.Items.Count > 0) //scroll to the last
                {
                    if(VisualTreeHelper.GetChildrenCount(list_rec)>0)
                    {
                        var border = VisualTreeHelper.GetChild(list_rec, 0) as Decorator;
                        if (border != null)
                        {
                            var scroll = border.Child as ScrollViewer;
                            if (scroll != null) scroll.ScrollToEnd();
                        }
                    }
                }
            }));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            byte[] bs;
            if (chk_hex.IsChecked == true)
            {
                string[] ss = text_input.Text.Split();
                bs = new byte[ss.Length];
                try
                {
                    for (int i = 0; i < bs.Length; i++)
                    {
                        bs[i] = byte.Parse(ss[i], System.Globalization.NumberStyles.HexNumber);
                    }
                }
                catch(Exception ee)
                {
                    MessageBox.Show(ee.Message);
                }
                send_bytes?.Invoke(bs);
            }
            else
            {
                string ss = text_input.Text;
                bs = ASCIIEncoding.ASCII.GetBytes(ss);
                send_bytes?.Invoke(bs);
            }
            list_history.Items.Insert(0, format_bin(bs));
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Chkbox_hex_show_Click(object sender, RoutedEventArgs e)
        {
            if(chkbox_hex_show.IsChecked==true)
            {
           
            }
            else
            {

            }
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            string[] checks = new string[] { "none", "modbus crc16", "add8" };
            combo_check.Items.Clear();
            foreach (string b in checks)
            {
                combo_check.Items.Add(b);
            }
            combo_check.SelectedIndex = 0;
            list_rec.DataContext = _recs;
        }

        private void UserControl_Loaded_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
