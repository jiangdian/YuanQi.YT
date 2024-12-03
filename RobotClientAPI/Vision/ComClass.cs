using System.IO.Ports;

namespace RobotClientAPI.Vision
{
    public class ComClass
    {
        private static ComClass instance;

        private static readonly object lockobj = new object();

        IConfiguration configuration;

        SerialPort port1;
        SerialPort port2;

        private ComClass(IConfiguration _configuration, Parity parity, StopBits stopBits, Handshake handshake)
        {
            configuration = _configuration;
            Initialize(parity, stopBits, handshake);
        }

        public static ComClass GetInstance(IConfiguration _configuration, Parity parity, StopBits stopBits, Handshake handshake)
        {
            if (instance == null)
            {
                lock (lockobj)
                {
                    if (instance == null)
                        instance = new ComClass(_configuration, parity, stopBits, handshake);
                }
            }
            return instance;
        }

        private void Initialize(Parity parity, StopBits stopBits, Handshake handshake)
        {
            string[] comPorts = ["com1", "com2"];
            string[] baudRates = ["baudRate1", "baudRate2"];
            string[] dataBitsPorts = ["dataBits1", "dataBits2"];

            for (int i = 0; i < comPorts.Length; i++)
            {
                SerialPort port = new(configuration[comPorts[i]])
                {
                    BaudRate = Convert.ToInt32(configuration[baudRates[i]]),
                    DataBits = Convert.ToInt32(configuration[dataBitsPorts[i]]),
                    Parity = parity,
                    StopBits = stopBits,
                    Handshake = handshake
                };

                switch (i)
                {
                    case 0:
                        port1 = port;
                        break;
                    case 1:
                        port2 = port;
                        break;
                }

                try
                {
                    port.Open();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public void StartRead(out string result1, out string result2)
        {
            byte[] send = [0x16, 0x54, 0x0D];
            WriteToPorts(port1,send);
            try
            {
                if (port1 != null && port1.IsOpen)
                {
                    Thread.Sleep(500);
                    port1.ReadTimeout = 10000;
                    byte[] buffer = new byte[1024]; // 根据需要调整缓冲区大小
                    int bytesRead = port1.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        // 将收到的数据转换为字符串
                        result1 = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    }
                    else result1 = "";
                }
                else result1 = "";
            }
            catch (Exception ex)
            {
                result1 = "";
            }
            finally
            {
                EndRead1();
            }

            WriteToPorts(port2, send);
            try
            {
                if (port2 != null && port2.IsOpen)
                {
                    Thread.Sleep(500);
                    port2.ReadTimeout = 10000;
                    byte[] buffer = new byte[1024]; // 根据需要调整缓冲区大小
                    int bytesRead = port2.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        // 将收到的数据转换为字符串
                        result2 = System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    }
                    else result2 = "";
                }
                else result2 = "";
            }
            catch (Exception ex)
            {
                result2 = "";
            }
            finally
            {
                EndRead2();
            }
        }

        private void EndRead1()
        {
            byte[] send = [0x16, 0x55, 0x0D];
            if (port1 != null && port1.IsOpen) port1.Write(send, 0, send.Length);
        }

        private void EndRead2()
        {
            byte[] send = [0x16, 0x55, 0x0D];
            if (port2 != null && port2.IsOpen) port2.Write(send, 0, send.Length);
        }

        private void WriteToPorts(SerialPort port, byte[] data)
        {
            if (port != null && port.IsOpen) port.Write(data, 0, data.Length);
        }
    }
}