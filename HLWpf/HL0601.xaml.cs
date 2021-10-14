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
    public partial class HL0601 : UserControl
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
        public HL0601()
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
            mm.write_register(Convert.ToByte(addr.Text), 0x31, 0,success,fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x31, 1, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x31, 2, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x31, 3, success, fail);
            info.Content = "通信中...";
        }


        private void Slider_1_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x30, Convert.ToByte(slider_1.Value), success, fail);
            info.Content = "通信中...";
        }

        private void Slider_2_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x32, Convert.ToByte(slider_2.Value), success, fail);
            info.Content = "通信中...";
        }

        private void Slider_3_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x34, Convert.ToByte(slider_3.Value), success, fail);
            info.Content = "通信中...";
        }
        private void Slider_4_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x36, Convert.ToByte(slider_4.Value), success, fail);
            info.Content = "通信中...";
        }
        private void Slider_5_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x38, Convert.ToByte(slider_5.Value), success, fail);
            info.Content = "通信中...";
        }
        private void Slider_6_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x3a, Convert.ToByte(slider_6.Value), success, fail);
            info.Content = "通信中...";
        }
        private void RadioButton_Checked_4(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x33, 0, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_5(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x33, 1, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_6(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x33, 2, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_7(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x33, 3, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_8(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x35, 0, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_9(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x35, 1, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_10(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x35, 2, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_11(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x35, 3, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_12(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x37, 0, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_13(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x37, 1, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_14(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x37, 2, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_15(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x37, 3, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_16(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x39, 0, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_17(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x39, 1, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_18(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x39, 2, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_19(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x39, 3, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_20(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x3b, 0, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_21(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x3b, 1, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_22(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x3b, 2, success, fail);
            info.Content = "通信中...";
        }

        private void RadioButton_Checked_23(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x3b, 3, success, fail);
            info.Content = "通信中...";
        }
        void loop()
        {
            while(true)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    if(update_sensor.IsChecked==true)
                    {
                        mm.read_registers(Convert.ToByte(addr.Text), 0x20,4, 
                            new Action<byte[]>((byte[] bs)=> {
                                Dispatcher.Invoke(new Action(() =>{
                                    in1.IsChecked = (bs[3]) == 0;
                                    in2.IsChecked = (bs[5]) == 0;
                                    in3.IsChecked = (bs[7]) == 0;
                                    in4.IsChecked = (bs[9]) == 0;
                                }));
                            }), fail);
                    }
                    if (update_slot.IsChecked == true)
                    {
                        mm.read_registers(Convert.ToByte(addr.Text), 0x40, 3,
                            new Action<byte[]>((byte[] bs) => {
                                Dispatcher.Invoke(new Action(() => {
                                    slot1.IsChecked = (bs[3]) == 0;
                                    slot2.IsChecked = (bs[5]) == 0;
                                    slot3.IsChecked = (bs[7]) == 0;
                                }));
                            }), fail);
                    }
                }));
                System.Threading.Thread.Sleep(1000);
            }
        }
        private void UserControl_Initialized(object sender, EventArgs e)
        {
            new System.Threading.Thread(loop).Start();
        }

        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            mm.write_register(Convert.ToByte(addr.Text), 0x11, Convert.ToByte(newaddr.Text),
                new Action<byte[]>((byte[] bs) => {
                    Dispatcher.Invoke(new Action(() =>
                    {
                        info.Content = "修改成功";
                    }));
                }),
                fail);
            info.Content = "通信中...";
        }
    }
}
