using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Threading;

namespace HLWpf
{
    public class HLib
    {
        public class IAPMaster
        {
            public Action<byte[]> send_bytes;//
            public Action<byte,byte> progress;//ui callback
            class FirmWare
            {
                public byte[] data;
                public byte block_size;
                public byte sending_index;
            }
            FirmWare _fw;
            enum IapCmd
            {
                INFO = 0xf0, DATA, TIMEOUT, FINISHED
            }
            enum IapState
            {
                IDLE, SEND_INFO, SENDING, WAITING, TIMEOUT, FINISHED
            }
            IapState _iapstate = IapState.IDLE;
            const int TICK_TIMEOUT = 1000000;
            public IAPMaster(byte[] data,byte block_size)
            {
                _fw = new FirmWare();
                _fw.data = data;
                _fw.block_size = block_size;
                _fw.sending_index = 0;
                new Thread(iap_loop).Start();
            }
            public void send_info()
            {
                byte[] tx = new byte[4];
                tx[0] = _fw.block_size;
                int t = _fw.data.Length;
                tx[3] = (byte)(t & 0xff);
                t = t >> 8;
                tx[2] = (byte)(t & 0xff);
                t = t >> 8;
                tx[1] = (byte)(t & 0xff);
                byte[] b = _build_frame((byte)IapCmd.INFO, tx);
                send_bytes(b);
            }
            public void send_finished()
            {
                byte[] b = _build_frame((byte)IapCmd.FINISHED, null);
                send_bytes(b);
            }
            public void received_bytes(byte[] data)
            {
                switch((IapCmd)data[0])
                {
                    case IapCmd.INFO:
                        _iapstate = IapState.SEND_INFO;
                        break;
                    case IapCmd.DATA:
                        if(_iapstate==IapState.WAITING)
                        {
                            _iapstate = IapState.SENDING;
                        }
                        break;
                }
            }
            byte[] _build_frame(byte cmd, byte[] data)
            {
                byte[] _tx_buf;
                if (data == null)
                {
                    _tx_buf = new byte[2];
                }
                else
                {
                    _tx_buf = new byte[data.Length + 2];
                    data.CopyTo(_tx_buf, 1);
                }
                _tx_buf[0] = cmd;
                _tx_buf[_tx_buf.Length - 1] = add_inv_calc(_tx_buf, _tx_buf.Length - 1);
                return _tx_buf;
            }
            void iap_loop()
            {
                byte[] tx;
                int tick = 0;
                while (true)
                {
                    switch (_iapstate)
                    {
                        case IapState.IDLE:

                            break;
                        case IapState.SEND_INFO:
                            send_info();
                            _fw.sending_index = 0;
                            _iapstate = IapState.WAITING;
                            break;
                        case IapState.SENDING:
                            if (_fw.sending_index == (_fw.data.Length / _fw.block_size))/*last frame, may not full*/
                            {
                                int remain = _fw.data.Length - _fw.block_size * _fw.sending_index;
                                tx = new byte[remain];
                                Array.Copy(_fw.data, _fw.sending_index * _fw.block_size, tx, 0, remain);
                                _iapstate = IapState.FINISHED;
                            }
                            else
                            {
                                tx = new byte[_fw.block_size];
                                Array.Copy(_fw.data, _fw.sending_index * _fw.block_size, tx, 0, _fw.block_size);
                                _iapstate = IapState.WAITING;
                            }
                            byte[] b = _build_frame(0xf1, tx);
                            send_bytes(b);
                            _fw.sending_index++;
                            tick = 0;
                            progress((byte)IapState.SENDING, _fw.sending_index);
                            break;
                        case IapState.WAITING:
                            tick++;
                            if (tick > TICK_TIMEOUT)
                            {
                                _iapstate = IapState.TIMEOUT;
                            }
                            break;
                        case IapState.TIMEOUT:
                            progress((byte)IapState.TIMEOUT, 0);
                            break;
                        case IapState.FINISHED:
                            Thread.Sleep(100);
                            progress((byte)IapState.FINISHED, 0);
                            send_finished();
                            _iapstate = IapState.IDLE;
                            break;
                    }
                    Thread.Sleep(10);
                }
            }
        }
        public class ModbusMaster
        {
            const int frame_timeout_ms = 100;
            const int frame_sending_loop_ms = 10;
            public Action<byte[]> send_bytes;
            class FrameSending
            {
                public byte[] Data
                {
                    get;
                    set;
                }
                public Action<byte[]> Callback
                {
                    get;
                    set;
                }
                public bool IsAck
                {
                    get;
                    set;
                }
                public Action Timeout
                {
                    get;
                    set;
                }
                public FrameSending(byte[] data,Action<byte[]> callback, Action timeout)
                {
                    Data = data;
                    Callback = callback;
                    Timeout = timeout;
                    IsAck = false;
                }
            }
            Queue<FrameSending> _qsf;
            
            FrameSending _current_sending;
            public ModbusMaster()
            {
                _qsf = new Queue<FrameSending>();
                new Thread(loop).Start();
            }
            byte[] build_frame(byte addr, byte func, UInt16 p1, UInt16 p2)
            {
                byte[] _modbus_tx_buf = new byte[8];
                _modbus_tx_buf[0] = addr;
                _modbus_tx_buf[1] = func;
                _modbus_tx_buf[2] = (byte)(p1 / 256);
                _modbus_tx_buf[3] = (byte)(p1 % 256);
                _modbus_tx_buf[4] = (byte)(p2 / 256);
                _modbus_tx_buf[5] = (byte)(p2 % 256);
                UInt16 crc16 = modbus_crc_calc(_modbus_tx_buf, 6);
                _modbus_tx_buf[6] = (byte)(crc16 % 256);
                _modbus_tx_buf[7] = (byte)(crc16 / 256);
                return _modbus_tx_buf;
            }
            public void read_registers(byte addr, UInt16 p1, UInt16 p2, Action<byte[]> callback,Action timeout)
            {
                byte[] d = build_frame(addr, 0x03, p1, p2);
                _qsf.Enqueue(new FrameSending(d, callback,timeout));
            }
            public void write_register(byte addr, UInt16 p1, UInt16 p2, Action<byte[]> callback, Action timeout)
            {
                byte[] d = build_frame(addr, 0x06, p1, p2);
                _qsf.Enqueue(new FrameSending(d, callback, timeout));
            }
            public void received_bytes(byte[] data)
            {
                if(_current_sending==null)
                {
                    return;
                }
                if(!(_current_sending.Data[0] == data[0] && _current_sending.Data[1] == data[1]))
                {
                    return;
                }
                _current_sending.IsAck = true;
                _current_sending.Data = data;
            }

            public void loop()
            {
                long _sending_ticks = 0;
                while(true)
                {
                    if (_current_sending == null)
                    {
                        if (_qsf.Count > 0)
                        {
                            _current_sending = _qsf.Dequeue();
                            send_bytes?.Invoke(_current_sending.Data);
                            _sending_ticks = 0;
                        }
                    }
                    else
                    {
                        if(_current_sending.IsAck)
                        {
                            _current_sending.Callback?.Invoke(_current_sending.Data);//received
                            _current_sending = null;
                        }
                        else
                        {
                            _sending_ticks++;
                            if (_sending_ticks > (frame_timeout_ms / frame_sending_loop_ms))
                            {
                                _current_sending.Timeout?.Invoke();
                                _current_sending = null;
                            }
                        }
                    }
                    Thread.Sleep(frame_sending_loop_ms);
                }
            }
        }
        public static byte[] get_file_bytes(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            BinaryReader binReader = new BinaryReader(fs);
            byte[] r = new byte[fs.Length];
            binReader.Read(r, 0, (int)fs.Length);
            binReader.Close();
            fs.Close();
            return r;
        }
        public static UInt16 modbus_crc_calc(byte[] buffer, int size)
        {
            UInt16[] modbus_crc_table =
            {
                0x0000, 0xc0c1, 0xc181, 0x0140, 0xc301, 0x03c0, 0x0280, 0xc241, 0xc601, 0x06c0, 0x0780, 0xc741, 0x0500, 0xc5c1,
                0xc481, 0x0440, 0xcc01, 0x0cc0, 0x0d80, 0xcd41, 0x0f00, 0xcfc1, 0xce81, 0x0e40, 0x0a00, 0xcac1, 0xcb81, 0x0b40,
                0xc901, 0x09c0, 0x0880, 0xc841, 0xd801, 0x18c0, 0x1980, 0xd941, 0x1b00, 0xdbc1, 0xda81, 0x1a40, 0x1e00, 0xdec1,
                0xdf81, 0x1f40, 0xdd01, 0x1dc0, 0x1c80, 0xdc41, 0x1400, 0xd4c1, 0xd581, 0x1540, 0xd701, 0x17c0, 0x1680, 0xd641,
                0xd201, 0x12c0, 0x1380, 0xd341, 0x1100, 0xd1c1, 0xd081, 0x1040, 0xf001, 0x30c0, 0x3180, 0xf141, 0x3300, 0xf3c1,
                0xf281, 0x3240, 0x3600, 0xf6c1, 0xf781, 0x3740, 0xf501, 0x35c0, 0x3480, 0xf441, 0x3c00, 0xfcc1, 0xfd81, 0x3d40,
                0xff01, 0x3fc0, 0x3e80, 0xfe41, 0xfa01, 0x3ac0, 0x3b80, 0xfb41, 0x3900, 0xf9c1, 0xf881, 0x3840, 0x2800, 0xe8c1,
                0xe981, 0x2940, 0xeb01, 0x2bc0, 0x2a80, 0xea41, 0xee01, 0x2ec0, 0x2f80, 0xef41, 0x2d00, 0xedc1, 0xec81, 0x2c40,
                0xe401, 0x24c0, 0x2580, 0xe541, 0x2700, 0xe7c1, 0xe681, 0x2640, 0x2200, 0xe2c1, 0xe381, 0x2340, 0xe101, 0x21c0,
                0x2080, 0xe041, 0xa001, 0x60c0, 0x6180, 0xa141, 0x6300, 0xa3c1, 0xa281, 0x6240, 0x6600, 0xa6c1, 0xa781, 0x6740,
                0xa501, 0x65c0, 0x6480, 0xa441, 0x6c00, 0xacc1, 0xad81, 0x6d40, 0xaf01, 0x6fc0, 0x6e80, 0xae41, 0xaa01, 0x6ac0,
                0x6b80, 0xab41, 0x6900, 0xa9c1, 0xa881, 0x6840, 0x7800, 0xb8c1, 0xb981, 0x7940, 0xbb01, 0x7bc0, 0x7a80, 0xba41,
                0xbe01, 0x7ec0, 0x7f80, 0xbf41, 0x7d00, 0xbdc1, 0xbc81, 0x7c40, 0xb401, 0x74c0, 0x7580, 0xb541, 0x7700, 0xb7c1,
                0xb681, 0x7640, 0x7200, 0xb2c1, 0xb381, 0x7340, 0xb101, 0x71c0, 0x7080, 0xb041, 0x5000, 0x90c1, 0x9181, 0x5140,
                0x9301, 0x53c0, 0x5280, 0x9241, 0x9601, 0x56c0, 0x5780, 0x9741, 0x5500, 0x95c1, 0x9481, 0x5440, 0x9c01, 0x5cc0,
                0x5d80, 0x9d41, 0x5f00, 0x9fc1, 0x9e81, 0x5e40, 0x5a00, 0x9ac1, 0x9b81, 0x5b40, 0x9901, 0x59c0, 0x5880, 0x9841,
                0x8801, 0x48c0, 0x4980, 0x8941, 0x4b00, 0x8bc1, 0x8a81, 0x4a40, 0x4e00, 0x8ec1, 0x8f81, 0x4f40, 0x8d01, 0x4dc0,
                0x4c80, 0x8c41, 0x4400, 0x84c1, 0x8581, 0x4540, 0x8701, 0x47c0, 0x4680, 0x8641, 0x8201, 0x42c0, 0x4380, 0x8341,
                0x4100, 0x81c1, 0x8081, 0x4040
            };
            UInt16 crc = 0xFFFF;
            byte nTemp;

            for (int i = 0; i < size; i++)
            {
                nTemp = (byte)(buffer[i] ^ crc);
                crc >>= 8;
                crc ^= modbus_crc_table[(nTemp & 0xFFU)];
            }
            return (crc);
        }
        public static byte crc8_calc(byte[] b, int len)
        {
            /*crc8 maxim*/
            byte[] crc8_table =
            {
                0x00, 0x5e, 0xbc, 0xe2, 0x61, 0x3f, 0xdd, 0x83, 0xc2, 0x9c, 0x7e, 0x20, 0xa3, 0xfd, 0x1f, 0x41,
                0x9d, 0xc3, 0x21, 0x7f, 0xfc, 0xa2, 0x40, 0x1e, 0x5f, 0x01, 0xe3, 0xbd, 0x3e, 0x60, 0x82, 0xdc,
                0x23, 0x7d, 0x9f, 0xc1, 0x42, 0x1c, 0xfe, 0xa0, 0xe1, 0xbf, 0x5d, 0x03, 0x80, 0xde, 0x3c, 0x62,
                0xbe, 0xe0, 0x02, 0x5c, 0xdf, 0x81, 0x63, 0x3d, 0x7c, 0x22, 0xc0, 0x9e, 0x1d, 0x43, 0xa1, 0xff,
                0x46, 0x18, 0xfa, 0xa4, 0x27, 0x79, 0x9b, 0xc5, 0x84, 0xda, 0x38, 0x66, 0xe5, 0xbb, 0x59, 0x07,
                0xdb, 0x85, 0x67, 0x39, 0xba, 0xe4, 0x06, 0x58, 0x19, 0x47, 0xa5, 0xfb, 0x78, 0x26, 0xc4, 0x9a,
                0x65, 0x3b, 0xd9, 0x87, 0x04, 0x5a, 0xb8, 0xe6, 0xa7, 0xf9, 0x1b, 0x45, 0xc6, 0x98, 0x7a, 0x24,
                0xf8, 0xa6, 0x44, 0x1a, 0x99, 0xc7, 0x25, 0x7b, 0x3a, 0x64, 0x86, 0xd8, 0x5b, 0x05, 0xe7, 0xb9,
                0x8c, 0xd2, 0x30, 0x6e, 0xed, 0xb3, 0x51, 0x0f, 0x4e, 0x10, 0xf2, 0xac, 0x2f, 0x71, 0x93, 0xcd,
                0x11, 0x4f, 0xad, 0xf3, 0x70, 0x2e, 0xcc, 0x92, 0xd3, 0x8d, 0x6f, 0x31, 0xb2, 0xec, 0x0e, 0x50,
                0xaf, 0xf1, 0x13, 0x4d, 0xce, 0x90, 0x72, 0x2c, 0x6d, 0x33, 0xd1, 0x8f, 0x0c, 0x52, 0xb0, 0xee,
                0x32, 0x6c, 0x8e, 0xd0, 0x53, 0x0d, 0xef, 0xb1, 0xf0, 0xae, 0x4c, 0x12, 0x91, 0xcf, 0x2d, 0x73,
                0xca, 0x94, 0x76, 0x28, 0xab, 0xf5, 0x17, 0x49, 0x08, 0x56, 0xb4, 0xea, 0x69, 0x37, 0xd5, 0x8b,
                0x57, 0x09, 0xeb, 0xb5, 0x36, 0x68, 0x8a, 0xd4, 0x95, 0xcb, 0x29, 0x77, 0xf4, 0xaa, 0x48, 0x16,
                0xe9, 0xb7, 0x55, 0x0b, 0x88, 0xd6, 0x34, 0x6a, 0x2b, 0x75, 0x97, 0xc9, 0x4a, 0x14, 0xf6, 0xa8,
                0x74, 0x2a, 0xc8, 0x96, 0x15, 0x4b, 0xa9, 0xf7, 0xb6, 0xe8, 0x0a, 0x54, 0xd7, 0x89, 0x6b, 0x35,
            };
            byte crc = 0x00;
            for (int i = 0; i < len; i++)
            {
                crc = crc8_table[crc ^ b[i]];
            }
            return (crc);
        }
        public static byte add_inv_calc(byte[] bs,int len)
        {
            byte s = 0;
            for (int i = 0; i < len; i++)
            {
                s += bs[i];
            }
            return (byte)~s;
        }
        public static dynamic get_web_json(string url)
        {
            dynamic ret;
            try
            {
                WebClient client = new WebClient();
                Stream stream = client.OpenRead(url);
                StreamReader reader = new StreamReader(stream);
                String content = reader.ReadToEnd();
                var jss = new JavaScriptSerializer();
                ret = jss.Deserialize<dynamic>(content);
            }
            catch (Exception)
            {
                ret = null;
            }
            return ret;
        }
        public static bool save_web_file(string url, string fn)
        {
            WebResponse response = null;
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                response = request.GetResponse();
            }
            catch (Exception)
            {
                return false;
            }
            bool ret = true;
            byte[] buffer = new byte[1024];
            try
            {
                if (File.Exists(fn))
                {
                    File.Delete(fn);
                }
                Stream outStream = File.Create(fn);
                Stream inStream = response.GetResponseStream();
                int len;
                do
                {
                    len = inStream.Read(buffer, 0, buffer.Length);
                    if (len > 0)
                    {
                        outStream.Write(buffer, 0, len);
                    }
                }
                while (len > 0);
                outStream.Close();
                inStream.Close();
            }
            catch (Exception)
            {
                ret = false;
            }
            return ret;
        }
    }
}
