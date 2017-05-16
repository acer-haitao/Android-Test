using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DIH.CommunicationProtocol.Utils.Communication
{
    public class SerialPortCommunication
    {
        private static SerialPortCommunication instance = new SerialPortCommunication();
        public static SerialPortCommunication Instance
        {
            get { return instance; }
        }

        public SerialPortCommunication() 
        {; }

        static AutoResetEvent CheckSerialPortEvent = new AutoResetEvent(false);
        public bool initialSerialPort(SerialPort serialPort, string portName, int BaudRate)
        {
            try
            {
                if (!serialPort.IsOpen)
                {
                    serialPort.PortName = portName;
                    serialPort.BaudRate = BaudRate;
                    serialPort.Open();
                    serialPort.DataReceived += serialPort_DataReceived;
                }
                //发生查询心跳指令，如果500毫秒没有返回，则表示设备连接失败
                if (GetUsedSerialPort(serialPort))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            CheckSerialPortEvent.Set();
        }
        public bool GetUsedSerialPort(SerialPort serialPort)
        {
            CheckSerialPort(serialPort);
            AutoResetEvent[] evs = new AutoResetEvent[1];
            evs[0] = CheckSerialPortEvent;
            return WaitHandle.WaitAll(evs, 500);
        }
        public void CheckSerialPort(SerialPort serialPort)
        {
            CheckSerialPortEvent.Reset();
            SendData(CommunicationTools.Instance.GetSendData(SystemUtils.QueryHeartbeat,new byte[1]{0}), serialPort);
        }

        private void SendData(byte[] bytes, SerialPort serialPort)
        {
            serialPort.Write(bytes, 0, bytes.Length);
        }
    }
}
