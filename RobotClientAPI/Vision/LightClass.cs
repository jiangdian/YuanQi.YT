using System.IO.Ports;

namespace RobotClientAPI.Vision
{
    public class LightClass
    {
        private static LightClass instance;

        private static readonly object lockobj = new object();//线程锁
        IConfiguration configuration;
        SerialPort port;

        private LightClass(IConfiguration _configuration, Parity parity, StopBits stopBits, Handshake handshake)
        {
            configuration = _configuration;
            Initialize(parity, stopBits, handshake);
        }

        public static LightClass GetInstance(IConfiguration _configuration, Parity parity, StopBits stopBits, Handshake handshake)
        {
            if (instance == null)
            {
                lock (lockobj)
                {
                    if (instance == null)
                        instance = new LightClass(_configuration, parity, stopBits, handshake);
                }
            }
            return instance;
        }

        private void Initialize(Parity parity, StopBits stopBits, Handshake handshake)
        {
            port = new SerialPort(configuration["lightCom"])
            {
                BaudRate = Convert.ToInt32(configuration["lightBaudRate"]),
                DataBits = Convert.ToInt32(configuration["lightDataBits"]),
                Parity = parity,
                StopBits = stopBits,
                Handshake = handshake
            };

            try
            {
                port.Open();
            }
            catch (Exception ex)
            {
                // 处理打开串口时的异常，例如记录日志
                Console.WriteLine($"Error opening port {port.PortName}: {ex.Message}");
            }
        }

        public void OpenLight()
        {
            byte[] openLight = [0xA5, 0x01, 0x11, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x00, 0xff, 0x6E, 0xB1];
            WriteToPort(openLight);
        }

        public void OpenLightScan()
        {
            byte[] openLight = [0xA5, 0x01, 0x11, 0x00, 0xff, 0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xE6, 0xAC];
            WriteToPort(openLight);
        }

        public void OpenLightRecord()
        {
            byte[] openLight = [0xA5, 0x01, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0xff, 0x00, 0xff, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xBB, 0x31];
            WriteToPort(openLight);
        }

        public void CloseLight()
        {
            byte[] closeLight = [0xA5, 0x01, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x58, 0x47];
            WriteToPort(closeLight);
        }

        public void SetLightBright(int bright)
        {
            byte[] headLine = [0xA5, 0x01, 0x11];
            byte[] body = [ 0x00, Convert.ToByte(bright.ToString("X2"), 16), 0x00, Convert.ToByte(bright.ToString("X2"), 16), 0x00, Convert.ToByte(bright.ToString("X2"), 16),
                0x00, Convert.ToByte(bright.ToString("X2"), 16), 0x00, Convert.ToByte(bright.ToString("X2"), 16), 0x00, Convert.ToByte(bright.ToString("X2"), 16),
                0x00, Convert.ToByte(bright.ToString("X2"), 16), 0x00, Convert.ToByte(bright.ToString("X2"), 16) ];
            byte[] message = new byte[19];
            Array.Copy(headLine, 0, message, 0, headLine.Length);
            Array.Copy(body, 0, message, 3, body.Length);
            byte[] crc16 = Crc16.CRCCalc(message);
            byte[] send = new byte[21];
            Array.Copy(message, 0, send, 0, message.Length);
            Array.Copy(crc16, 0, send, 19, crc16.Length);
            WriteToPort(send);
        }

        public void SwitchRealTimeMode()
        {
            byte[] switchMode = [0xA5, 0x01, 0x0F, 0x00, 0x00, 0x01, 0xFA, 0xE7];
            WriteToPort(switchMode);
        }

        public void SwitchWorkMode()
        {
            byte[] switchMode = [0xA5, 0x01, 0x0F, 0x00, 0x00, 0x00, 0x3A, 0x26];
            WriteToPort(switchMode);
        }

        private void WriteToPort(byte[] data)
        {
            if (port != null && port.IsOpen)
            {
                port.Write(data, 0, data.Length);
            }
            else
            {
                Console.WriteLine("Port is not open.");
            }
        }
    }
}