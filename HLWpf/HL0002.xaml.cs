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

namespace HLWpf
{
    /// <summary>
    /// HL0002.xaml 的交互逻辑
    /// </summary>
    public partial class HL0002 : UserControl
    {
        HLib.ModbusMaster mm = new HLib.ModbusMaster();
        public Action<byte[]> send_bytes
        {
            set => mm.send_bytes = value;
        }
        public void received(byte[] bs)
        {
            mm.received_bytes(bs);
        }
        public HL0002()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            mm.read_registers(Convert.ToByte(addr.Text), 0, 9,
                new Action<byte[]>((byte[] bs) => {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        string type = Encoding.Default.GetString(bs.Skip(3).Take(8).ToArray()).TrimEnd('\0');
                        string hw = String.Format("{0:X2}{1:X2}", bs[12], bs[11]);
                        string sw = String.Format("{0:X2}{1:X2}", bs[14], bs[13]);
                        string lib = String.Format("{0:X2}{1:X2}",bs[16],bs[15]);
                        string uid = BitConverter.ToString(bs.Skip(17).Take(4).ToArray()).Replace("-", "");

                        device_info.Text = String.Format("      型号：{0}\r\n硬件版本：{1}\r\n软件版本：{2}\r\n基库版本：{3}\r\n  序列号：{4}", type, hw, sw, lib, uid);
                        info.Content = "通信成功";
                    }));
                }),
                fail);
            info.Content = "通信中...";
        }
        void success(byte[] bs)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                info.Content = "通信成功";
            }));
        }
        void fail()
        {
            Dispatcher.Invoke(new Action(() =>
            {
                info.Content = "通信失败";
            }));
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x11, 0,success,fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x11, 1, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x11, 2, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x11, 3, success, fail);
            info.Content = "通信中...";
        }


        private void Slider_left_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x10, Convert.ToByte(slider_left.Value), success, fail);
            info.Content = "通信中...";
        }

        private void Slider_right_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x20, Convert.ToByte(slider_v.Value), success, fail);
            info.Content = "通信中...";
        }

        private void Slider_v_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x16, 0, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_4(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x21, 0, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_5(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x21, 1, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_6(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x21, 2, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_7(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x21, 3, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_8(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x15, 0xff00, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_9(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x15, 0x00, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_10(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x18, 0x00, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_11(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x18, 0xff00, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_12(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x28, 0x00, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_13(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x28, 0xff00, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_14(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x38, 0x00, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_15(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x38, 0xff00, success, fail);
            info.Content = "通信中...";
        }
        void send_out()
        {
            UInt16 s = 0;
            if (out1.IsChecked==true)
            {
                s |= 0x0100;
            }
            if (out2.IsChecked == true)
            {
                s |= 0x0200;
            }
            if (out3.IsChecked == true)
            {
                s |= 0x0400;
            }
            if (out4.IsChecked == true)
            {
                s |= 0x0800;
            }
            mm.write_register(Convert.ToByte(addr.Text), 0x1d, s, success, fail);
            info.Content = "通信中...";
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            send_out();
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            send_out();
        }

        private void CheckBox_Checked_2(object sender, RoutedEventArgs e)
        {
            send_out();
        }

        private void CheckBox_Checked_3(object sender, RoutedEventArgs e)
        {
            send_out();
        }
        void loop()
        {
            while(true)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if(update_sensor.IsChecked==true)
                    {
                        mm.read_registers(Convert.ToByte(addr.Text), 0x1b,1, 
                            new Action<byte[]>((byte[] bs)=> {
                                Dispatcher.Invoke(new Action(() =>{
                                    in1.IsChecked = (bs[3] & 0x01) == 0;
                                    in2.IsChecked = (bs[3] & 0x02) == 0;
                                    in3.IsChecked = (bs[3] & 0x04) == 0;
                                    in4.IsChecked = (bs[3] & 0x08) == 0;
                                }));
                            }), fail);
                        info.Content = "通信中...";
                    }
                    if(update_left_weight.IsChecked==true)
                    {
                        mm.read_registers(2, 0, 2,
                            new Action<byte[]>((byte[] bs) => {
                                byte[] bbs = { bs[4], bs[3], bs[6], bs[5] };
                                Int32 w = BitConverter.ToInt32(bbs, 0);
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    weight_left.Text = w.ToString();
                                }));
                            }), fail);
                        info.Content = "通信中...";
                    }
                    if(update_right_weight.IsChecked==true)
                    {
                        mm.read_registers(3, 0, 2,
                            new Action<byte[]>((byte[] bs) => {
                                byte[] bbs = { bs[4], bs[3], bs[6], bs[5] };
                                Int32 w = BitConverter.ToInt32(bbs, 0);
                                Dispatcher.Invoke(new Action(() =>
                                {
                                    weight_right.Text = w.ToString();
                                }));
                            }), fail);
                        info.Content = "通信中...";
                    }
                }));
                System.Threading.Thread.Sleep(1000);
            }
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            new System.Threading.Thread(loop).Start();
        }
    }
}
