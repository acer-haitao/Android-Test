using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;


namespace iValuableTestCommonCode
{
    public class ConnectionMode : IDisposable
    {
        private static ConnectionMode instance = new ConnectionMode();
        public static ConnectionMode Instance
        {
            get { return instance; }
        }
        public void Dispose()
        {
            Udp_Rec_Thread_Abort();
        }
        public ConnectionMode() { ; }

        [DllImport("kernel32.dll",
            CallingConvention = CallingConvention.Winapi)]
        extern static int GetTickCount();

        //COM IP
        private static ArrayList COMList = new ArrayList();
        private static ArrayList LocalIPList = new ArrayList();
        private enum EnumConnectionType
        {
            Null,
            Serial,
            TCP,
        }
        private static EnumConnectionType ConnectionType = EnumConnectionType.Null;
        //serial
        private static SerialPort serialPort = new SerialPort();
        private static List<byte> serialReceiveBuffer = new List<byte>(4096);//默认分配1页内存，并始终限制不允许超过   
        private static bool serialIsReceiving = false;
        private static bool serialIsTryToClosePort = false;
        //Tcp
        private static TcpListener TcpListener = null;
        private static IAsyncResult SocketRecIAs = null;
        private static Socket TcpSocketClient = null;
        public static List<Socket> TcpSocketList = new List<Socket>();
        private const int TcpBufferSize = 4096;
        private static byte[] TcpReceivebuffer = new byte[TcpBufferSize];
        //Tcp Data Receive
        public delegate void My_Delegate_Data_Receive(byte[] data);
        public static My_Delegate_Data_Receive my_Data_ReceiveCommand = null;
        public delegate void My_Delegate_Tcp_AcceptCallback(List<Socket> SocketList, Socket client);
        public static My_Delegate_Tcp_AcceptCallback my_Tcp_AcceptCallback = null;
        //Udp
        public static UdpClient udpClient = null;
        public static Thread udpReceiveThread;
        private static List<byte> UdpReceivebuffer = new List<byte>(4096);
        public delegate void My_Delegate_Udp_Data_Receive(string ipString, int port, byte[] data);
        public static My_Delegate_Udp_Data_Receive my_Udp_Data_ReceiveCommand = null;

        public static void Serial_Scan()
        {
            for (int i = COMList.Count - 1; i >= 0 ; i--)
            {                
                if (COMList[i].ToString().Substring(0, 3) == "COM")
                {
                    COMList.RemoveAt(i);
                }
            }
            foreach (string vPortName in SerialPort.GetPortNames())
            {
                COMList.Add(vPortName);
            }
            COMList.Sort();
        }
        public static void LocalIP_Scan()
        {
            for (int i = LocalIPList.Count - 1; i >= 0 ; i--)
            {
                if (LocalIPList[i].ToString().Substring(0, 3) != "COM")
                {
                    LocalIPList.RemoveAt(i);
                }
            }

            string hostName = Dns.GetHostName();//本机名   
            IPAddress[] addressList = Dns.GetHostAddresses(hostName);//会返回所有地址，包括IPv4和IPv6   
            foreach (IPAddress ip in addressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                    LocalIPList.Add(ip.ToString());
            }
        }
        public static ArrayList Get_COMList()
        {
            return COMList;
        }
        public static ArrayList Get_LocalIPList()
        {
            return LocalIPList;
        }
        public static bool Serial_Open(string portName)
        {
            Serial_Close();
            Tcp_Close();
            try
            {
                serialPort.PortName = portName;
                serialPort.BaudRate = 115200;
                serialPort.ReadTimeout = 500;
                serialPort.WriteTimeout = 500;
                serialPort.Open();
                serialPort.NewLine = "/r/n";
                serialPort.DataReceived += serial_DataReceived;
                ConnectionType = EnumConnectionType.Serial;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "串口打开错误");
                return false;
            }
        }
        public static bool Serial_Close()
        {
            if (ConnectionType == EnumConnectionType.Serial)
            {
                ConnectionType = EnumConnectionType.Null;  
                serialIsTryToClosePort = true;
                int start = GetTickCount();
                while (serialIsReceiving && (GetTickCount() - start < 1000*60*5))
                {
                    System.Windows.Forms.Application.DoEvents();
                }
                serialPort.Close();
                serialIsTryToClosePort = false;             
            }
            return true;
        }
        public static void serial_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (serialIsTryToClosePort)
                return;

            serialIsReceiving = true;
            try
            {
                byte[] buffer = new byte[serialPort.BytesToRead];
                int readCount = serialPort.Read(buffer, 0, buffer.Length);
                Data_ReceiveCheck(buffer);
                serialIsReceiving = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "串口接收错误");
            }
            finally 
            {
                serialIsReceiving = false;
            }
        }
        public static bool Tcp_Open(IPAddress localaddr, int port)
        {
            Serial_Close();
            Tcp_Close();
            try
            {
                ConnectionType = EnumConnectionType.TCP;
                TcpListener = new TcpListener(localaddr, port); 
                TcpListener.Start();//开启监听
                TcpListener.BeginAcceptSocket(Tcp_AcceptCallback, TcpListener);
                return true;
            }
            catch (SocketException ex)
            {
                ExceptionSolver(ex, "Tcp_Open错误");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_Open失败");
                return false;
            }
        }
        public static bool Tcp_Close()
        {
            if (ConnectionType == EnumConnectionType.TCP)
            {
                ConnectionType = EnumConnectionType.Null;
                TcpListener.Stop();
                if (TcpSocketClient != null)
                {
                    TcpSocketClient.Close();
                }
                TcpSocketList.Clear();
                TcpSocketClient = null;
            }
            return true;
        }
        public static bool Connection_DataSend(byte[] data)
        {
            if (ConnectionType == EnumConnectionType.Serial)
            {
                serialPort.Write(data, 0, data.Length);
                return true;
            }
            else if (ConnectionType == EnumConnectionType.TCP && TcpSocketClient != null)
            {
                Tcp_SendData(data);
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void Tcp_AcceptCallback(IAsyncResult ar)
        {
            if (ConnectionType != EnumConnectionType.TCP)
            {
                return;
            }
            try
            {                
                TcpListener listener = (TcpListener)ar.AsyncState;
                Socket client = listener.EndAcceptSocket(ar);
                String sIP = ((IPEndPoint)client.RemoteEndPoint).Address.ToString();
                TcpListener.BeginAcceptSocket(Tcp_AcceptCallback, TcpListener);

                for (int i = TcpSocketList.Count - 1; i >= 0; i--)
                {
                    Socket temp = TcpSocketList[i];
                    if(temp == null)
                        continue;
                    string iplist = ((IPEndPoint)temp.RemoteEndPoint).Address.ToString();
                    if (string.Equals(iplist, sIP) == true)
                    {
                        TcpSocketList.RemoveAt(i);
                    }
                }
                TcpSocketList.Add(client);
                if (TcpSocketClient != null && string.Equals(((IPEndPoint)TcpSocketClient.RemoteEndPoint).Address.ToString(), sIP) == true)
                {
                    TcpSocketClient = client;
                    Tcp_ReceiveData(TcpSocketClient);
                }
                   
                if (null != my_Tcp_AcceptCallback)
                    my_Tcp_AcceptCallback(TcpSocketList, client);
            }
            catch (SocketException ex)
            {
                 ExceptionSolver(ex, "Tcp_AcceptCallback错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_AcceptCallback错误");
            }
        }

        private static void Tcp_ReceiveData(Socket client)
        {
            try
            {
                Array.Clear(TcpReceivebuffer, 0, TcpReceivebuffer.Length);
                SocketRecIAs = TcpSocketClient.BeginReceive(TcpReceivebuffer, 0, TcpBufferSize, 0, new AsyncCallback(Tcp_ReceiveCallback), null);
            }
            catch (SocketException ex)
            {
                ExceptionSolver(ex, "Tcp_ReceiveData错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_ReceiveData错误");
            }
        }
        public static bool Connection_Tcp_ReceiveData(int index)
        {
            try
            {
                TcpSocketClient = (Socket)TcpSocketList[index];
                Tcp_ReceiveData(TcpSocketClient);
                return true;
            }
            catch (SocketException ex)
            {
                ExceptionSolver(ex, "Tcp_ReceiveData错误");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_ReceiveData错误");
                return false;
            }

        }

        private static void Tcp_ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (ConnectionType == EnumConnectionType.TCP)
                {
                    int ReceLen = TcpSocketClient.EndReceive(ar);
                    if (ReceLen > 512)
                    {
                        Array.Clear(TcpReceivebuffer, 0, TcpReceivebuffer.Length);
                    }
                    if (ReceLen > 0)
                    {
                        byte[] buffer = new byte[ReceLen];
                        Array.Copy(TcpReceivebuffer, buffer, ReceLen);
                        Data_ReceiveCheck(buffer);
                        TcpSocketClient.BeginReceive(TcpReceivebuffer, 0, TcpBufferSize, 0, new AsyncCallback(Tcp_ReceiveCallback), null);
                    }
                }
            }
            catch (SocketException ex)
            {
                ExceptionSolver(ex, "Tcp_ReceiveCallback错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_ReceiveCallback错误");
            }
        }

        private static void Tcp_SendData(byte[] data)
        {
            try
            {
                TcpSocketClient.BeginSend(data, 0, data.Length, 0, new AsyncCallback(Tcp_SendCallBack), TcpSocketClient);
            }
            catch (SocketException ex)
            {
                ExceptionSolver(ex, "Tcp_SendData错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_SendData错误");
            }

        }

        private static void Tcp_SendCallBack(IAsyncResult ar)
        {
            TcpSocketClient.EndSend(ar);
        }
        private static void ExceptionSolver(SocketException sep, string title)
        {
            switch (sep.SocketErrorCode)
            {
                case SocketError.NotConnected:
                    //捕获ip地址输入错误的情况;
                    MessageBox.Show("不存在网络连接", title);
                    break;

                case SocketError.ConnectionAborted:
                    //在这里处理频繁出现的错误，
                    //比如IP不对，网线没插
                    MessageBox.Show("连接中止", title);
                    break;
                case SocketError.ConnectionRefused:
                    //远程主机正在主动拒绝连接;可能是连接的时候ip或port写错了;
                    MessageBox.Show("对方不接受连接,更可能是port的原因", title);
                    break;
                case SocketError.HostUnreachable:
                    MessageBox.Show("连接目标不可达", title);
                    break;
                case SocketError.TimedOut:
                    //尝试连接ip超时;
                    MessageBox.Show("尝试连接ip超时,更可能是ip的原因", title);
                    break;
                case SocketError.ConnectionReset:
                    MessageBox.Show("远程主机强迫关闭了一个现有连接", title);
                    break;
                default:
                    MessageBox.Show("捕获到" + sep.SocketErrorCode, title);
                    //这里直接报错，如果调试的时候出现这里的错误比较多，就移到上面解决，一般问题都是从来不出的
                    break;
            }
        }
        
        private static void Data_ReceiveCheck(byte[] buf)
        {

            serialReceiveBuffer.AddRange(buf);
            while (serialReceiveBuffer.Count >= 11)//至少要包含头（2字节）+长度（2字节）+校验（2字节）   
            {
                //2.1 查找数据头   
                if (serialReceiveBuffer[0] == 0x55 && serialReceiveBuffer[1] == 0xAA)
                {
                    //2.2 探测缓存数据是否有一条数据的字节，如果不够，就不用费劲的做其他验证了                    
                    ushort len = (ushort)(serialReceiveBuffer[4] + (serialReceiveBuffer[5] << 8));
                    if (serialReceiveBuffer.Count < len + 8) break;//数据不够的时候什么都不做   
                    //这里确保数据长度足够，数据头标志找到，我们开始计算校验   
                    //2.3 校验数据，确认数据正确 
                    byte[] CheckData = new byte[len];
                    serialReceiveBuffer.CopyTo(6, CheckData, 0, len);
                    ushort CheckCount = Serializer.Instance.GenerateCheckSum(CheckData);
                    ushort CheckCode = (ushort)(serialReceiveBuffer[len + 6] + (serialReceiveBuffer[len + 7] << 8));
                    if (CheckCode != CheckCount) //如果数据校验失败，丢弃这一包数据   
                    {
                        StringBuilder ShowErr = new StringBuilder();
                        for (int i = 0; i < (len + 8); i++)
                        {
                            string hex = Convert.ToString(serialReceiveBuffer[i], 16);
                            hex = " " + hex.PadLeft(2, '0'); // 用0占位补齐到2位，并在前面添加0x
                            ShowErr.Append(hex);
                        }
                        //MessageBox.Show("检验错误:" + ShowErr, "Data_ReceiveCheck");
                        serialReceiveBuffer.RemoveRange(0, len + 8);//从缓存中删除错误数据 
                        continue;//继续下一次循环   
                    }
                    //至此，已经被找到了一条完整数据。我们将数据直接分析，或是缓存起来一起分析 
                    byte[] RecData = new byte[len + 8];
                    serialReceiveBuffer.CopyTo(0, RecData, 0, len + 8);//复制一条完整数据到具体的数据缓存  
                    serialReceiveBuffer.RemoveRange(0, len + 8);//正确分析一条数据，从缓存中移除数据。  
                    if (null != my_Data_ReceiveCommand)
                        my_Data_ReceiveCommand(RecData);
                }
                else
                {
                    //这里是很重要的，如果数据开始不是头，则删除数据   
                    serialReceiveBuffer.RemoveAt(0);
                }
            }
        }


               
        #region Udp
        public static bool Udp_Open(string ipString, int port)
        {
            try
            {
                Udp_Close();
                udpClient = new UdpClient(new IPEndPoint(IPAddress.Parse(ipString), port));
                return true;
            }
            catch (SocketException ex)
            {
                ExceptionSolver(ex, "Udp_Open错误");
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Udp_Open失败");
                return false;
            }
        }
        public static bool Udp_Close()
        {
            if (udpClient != null)
            {
                udpClient.Close();
            }
            return true;
        }
        public static void Udp_Rec_Thread_Start()
        {
            try
            {
                if ((udpReceiveThread == null) || (udpReceiveThread.IsAlive == false))
                {
                    udpReceiveThread = new Thread(UDP_ReceiveData) { IsBackground = true };
                    udpReceiveThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Udp_Thread_Start失败");
            }
        }
        public static void Udp_Rec_Thread_Abort()
        {
            try
            {
                if (udpReceiveThread != null)
                {
                    udpReceiveThread.Abort();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Udp_Thread_Start失败");
            }
        }
        public static void Udp_SendData(string ipString, int port, byte[] sendBytes)
        {
            try
            {
                IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse(ipString), port);
                udpClient.Send(sendBytes, sendBytes.Length, remoteIpep);
            }
            catch (SocketException ex)
            {
                ExceptionSolver(ex, "Udp_SendData错误");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Udp_SendData失败");
            }
        }


        private static void UDP_ReceiveData(object obj)
        {

            IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                try
                {
                    //if (udpClient.Available <= 0) continue;
                    //if (udpClient.Client == null) continue;
                    byte[] bytRecv = udpClient.Receive(ref remoteIpep);
                    Udp_Data_ReceiveCheck(remoteIpep.Address.ToString(), remoteIpep.Port, bytRecv);
                    //string message = Encoding.Unicode.GetString(bytRecv, 0, bytRecv.Length);
                    //MessageBox.Show(string.Format("{0}[{1}]", remoteIpep, message));

                }
                catch (SocketException ex)
                {
                    ExceptionSolver(ex, "Udp_Receive错误");
                    break;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Udp_Receive失败");
                    break;
                }
            }
        }

        private static void Udp_Data_ReceiveCheck(string ipString, int port, byte[] buf)
        {
            List<byte> buf_temp = UdpReceivebuffer;
            buf_temp.AddRange(buf);
            while (buf_temp.Count >= 11)//至少要包含头（2字节）+长度（2字节）+校验（2字节）   
            {
                //2.1 查找数据头   
                if (buf_temp[0] == 0x55 && buf_temp[1] == 0xAA)
                {
                    //2.2 探测缓存数据是否有一条数据的字节，如果不够，就不用费劲的做其他验证了                    
                    ushort len = (ushort)(buf_temp[4] + (buf_temp[5] << 8));
                    if (buf_temp.Count < len + 8) break;//数据不够的时候什么都不做   
                    //这里确保数据长度足够，数据头标志找到，我们开始计算校验   
                    //2.3 校验数据，确认数据正确 
                    byte[] CheckData = new byte[len];
                    buf_temp.CopyTo(6, CheckData, 0, len);
                    ushort CheckCount = Serializer.Instance.GenerateCheckSum(CheckData);
                    ushort CheckCode = (ushort)(buf_temp[len + 6] + (buf_temp[len + 7] << 8));
                    if (CheckCode != CheckCount) //如果数据校验失败，丢弃这一包数据   
                    {
                        StringBuilder ShowErr = new StringBuilder();
                        for (int i = 0; i < (len + 8); i++)
                        {
                            string hex = Convert.ToString(buf_temp[i], 16);
                            hex = " " + hex.PadLeft(2, '0'); // 用0占位补齐到2位，并在前面添加0x
                            ShowErr.Append(hex);
                        }
                        //MessageBox.Show("检验错误:" + ShowErr, "Data_ReceiveCheck");
                        buf_temp.RemoveRange(0, len + 8);//从缓存中删除错误数据 
                        continue;//继续下一次循环   
                    }
                    //至此，已经被找到了一条完整数据。我们将数据直接分析，或是缓存起来一起分析 
                    byte[] RecData = new byte[len + 8];
                    buf_temp.CopyTo(0, RecData, 0, len + 8);//复制一条完整数据到具体的数据缓存  
                    buf_temp.RemoveRange(0, len + 8);//正确分析一条数据，从缓存中移除数据。  
                    if (null != my_Udp_Data_ReceiveCommand)
                        my_Udp_Data_ReceiveCommand(ipString, port, RecData);
                }
                else
                {
                    //这里是很重要的，如果数据开始不是头，则删除数据   
                    buf_temp.RemoveAt(0);
                }
            }
        }

        #endregion

    }
}
