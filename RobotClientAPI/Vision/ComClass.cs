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
            WriteToPorts(send);
            if (port1 != null && port1.IsOpen) result1 = port1.ReadLine();
            else result1 = "";
            if (port2 != null && port2.IsOpen) result2 = port2.ReadLine();
            else result2 = "";
        }

        public void EndRead()
        {
            byte[] send = [0x16, 0x55, 0x0D];
            WriteToPorts(send);
        }

        private void WriteToPorts(byte[] data)
        {
            if (port1 != null && port1.IsOpen) port1.Write(data, 0, data.Length);
            if (port2 != null && port2.IsOpen) port2.Write(data, 0, data.Length);
        }
    }
}