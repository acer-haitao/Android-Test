using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iValuableTestCommonCode;

namespace iValuableTestToolforFactory
{    
    public partial class Form1 : Form
    {
        private int order = 0;
        private bool response = false;
        private int response_count = 0;
        private int show_count = 0;
        private int color_count = 0;
        private Color[] show_color = new Color[] { Color.Red, Color.Green, Color.Blue,Color.White, Color.Black };
        private ushort DestinationAddress;
        private System.Timers.Timer TcpSendCMDTimer = new System.Timers.Timer(500);//实例化Timer类，设置间隔时间为10000毫秒；

       
        private WeighStableStruct WeighStableData = new WeighStableStruct();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {            
            this.Text +="  V1.1.3";

            textEdit1.Text = "8000";
            //设备
            comboBoxEdit3.Properties.Items.Add("1");
            comboBoxEdit3.Properties.Items.Add("2");
            comboBoxEdit3.Properties.Items.Add("3");
            comboBoxEdit3.Properties.Items.Add("4");
            comboBoxEdit3.Properties.Items.Add("5");
            comboBoxEdit3.SelectedIndex = 0;

            comboBoxEdit4.Properties.Items.Add("1000");
            comboBoxEdit4.Properties.Items.Add("2000");
            comboBoxEdit4.Properties.Items.Add("5000");
            comboBoxEdit4.Properties.Items.Add("10000");
            comboBoxEdit4.SelectedIndex = 1;

            comboBoxEdit5.Properties.Items.Add("500");
            comboBoxEdit5.Properties.Items.Add("1000");
            comboBoxEdit5.Properties.Items.Add("2000");
            comboBoxEdit5.Properties.Items.Add("5000");
            comboBoxEdit5.SelectedIndex = 0;

            ConnectionMode.my_Data_ReceiveCommand = Data_ReceiveCommand;
            ConnectionMode.my_Tcp_AcceptCallback = Data_Tcp_AcceptCallback;

            colorPickEdit1.Color = Color.FromArgb(0xFF, 0xFF, 0xFF);
            colorPickEdit2.Color = Color.FromArgb(0x00, 0xFF, 0x00);

            memoEdit2.Text = "1234567890\r\n一二三四五六七八九十\r\n蝶和 创新中心 ";

            TcpSendCMDTimer.Elapsed += TcpSendTimeoutEvent;//到达时间的时候执行事件；
            TcpSendCMDTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            SimpleButton but = new SimpleButton();
            but.Text = "刷新";
            simpleButton_form(but, null);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            TcpSendCMDTimer.Stop();
            ConnectionMode.Serial_Close();
        }
        private void ShowInfoBar(string message)
        {
            memoEdit1.MaskBox.AppendText(message + Environment.NewLine);
            //if (int.Parse(comboBoxEdit2.SelectedItem.ToString()) > 0)
            //{
            //    if (memoEdit1.Text.Length > int.Parse(comboBoxEdit2.SelectedItem.ToString()))
            //    {
            //        SimpleButton but = new SimpleButton();
            //        but.Text = "Clean";
            //        simpleButton_Click(but, null);
            //    }
            //}

        }
        private void simpleButton_form(object sender, EventArgs e)
        {
            SimpleButton button = sender as SimpleButton;
            switch (button.Text)
            {
                case "刷新":
                {
                    ConnectionMode.Serial_Scan();
                    ConnectionMode.LocalIP_Scan();

                    comboBoxEdit1.Properties.Items.Clear();
                    foreach (String client in ConnectionMode.Get_COMLocalIPList())
                    {
                        comboBoxEdit1.Properties.Items.Add(client);
                    }
                    comboBoxEdit1.Properties.Sorted = true;
                    if (comboBoxEdit1.SelectedIndex == -1)
                        comboBoxEdit1.SelectedIndex = 0;
                    break;
                }
                case "连接":
                {
                    simpleButton1.Text = "断开";
                    if (comboBoxEdit1.SelectedItem.ToString().Substring(0, 3) == "COM")
                    {
                        ConnectionMode.Serial_Open(comboBoxEdit1.SelectedItem.ToString());
                        ShowInfoBar(comboBoxEdit1.SelectedItem.ToString() + "打开成功");
                    }
                    else
                    {
                        ConnectionMode.Socket_Open(IPAddress.Parse(comboBoxEdit1.SelectedItem.ToString()), Convert.ToInt32(textEdit1.Text));
                        ShowInfoBar("Tcp打开成功");
                    }
                    break;
                }
                case "断开":
                {
                    simpleButton1.Text = "连接";
                    ConnectionMode.Serial_Close();
                    ConnectionMode.Socket_Close();
                    comboBoxEdit2.Properties.Items.Clear();
                    comboBoxEdit2.SelectedIndex = -1;
                    ShowInfoBar("Tcp关闭");                
                    break;
                }
                case "开始检验":
                {
                    order = 0;
                    show_count = 0;
                    color_count = 0;
                    pictureBox1.Image = null;
                    pictureBox2.Image = null;
                    pictureBox3.Image = null;
                    pictureBox4.Image = null;
                    pictureBox5.Image = null;
                    pictureBox6.Image = null;
                    pictureBox7.Image = null;
                    TcpSendCMDTimer.Start();
                    break;
                }
                case "开锁":
                {
                    Data_SendData(TransmitCmd.Instance.DllLockOpen(DestinationAddress)); 
                    break;
                }
                case "停止检验":
                {
                    TcpSendCMDTimer.Stop();
                    ShowInfoBar("检验已经停止");  
                    break;
                }
            }
        }
        public void TcpSendTimeoutEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            Invoke(new MethodInvoker(delegate
            {
                if (response == false)
                {
                    response_count++;
                    if (response_count >= 3)
                    {
                        response_count = 0;
                        TcpSendCMDTimer.Stop();
                        ShowInfoBar("检验已经停止");
                        return;
                    }
                }
                else
                {
                    response_count = 0;
                    response = false;
                }
                //
                if(show_count++ >= 4)
                {
                    show_count = 0;
                    Data_SendData(TransmitCmd.Instance.DisplayShowCleanAndChars(DestinationAddress, 0,0,400, 240,0, 0, 1.0f,
                        colorPickEdit1.Color, show_color[color_count++], System.Text.Encoding.Unicode.GetBytes(memoEdit2.Text)));
                    if (color_count >= 5)
                        color_count = 0;
                }
                else 
                {
                    if (order == 0)
                        Data_SendData(TransmitCmd.Instance.QuerySoftwareVersion(DestinationAddress));
                    else if (order == 1)
                    {
                        Data_SendData(TransmitCmd.Instance.QueryNvramState(DestinationAddress));
                        Data_SendData(TransmitCmd.Instance.RfidSetReadCardCount(DestinationAddress, 1));
                    }
                    else if (order == 2)
                    {
                        Data_SendData(TransmitCmd.Instance.DllLockOpenGet(DestinationAddress));
                    }
                    else if (order == 3)
                    {
                        Data_SendData(TransmitCmd.Instance.RfidQueryID(DestinationAddress));
                        Data_SendData(TransmitCmd.Instance.DllDoorOpenGet(DestinationAddress));
                    }
                    else if (order == 4)
                    {
                        Data_SendData(TransmitCmd.Instance.WeightSetEnable(DestinationAddress, 0x3f));
                        Data_SendData(TransmitCmd.Instance.WeightSetPanAndSensor(DestinationAddress, 1, new UInt16[1] { 0x3f }));
                    }
                    else
                    {
                        Data_SendData(TransmitCmd.Instance.QueryID(DestinationAddress));
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(DestinationAddress, 0xff));
                    }
                }
            }));
        }
        private void comboBoxEdit2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectionMode.Connection_Tcp_ReceiveData(comboBoxEdit2.SelectedIndex);
            ShowInfoBar("连接成功:" + IPAddress.Parse(comboBoxEdit2.SelectedItem.ToString()));
        }
        private void comboBoxEdit3_SelectedIndexChanged(object sender, EventArgs e)
        {
            DestinationAddress = ushort.Parse(comboBoxEdit3.SelectedItem.ToString());
        }
        private void Data_SendData(byte[] bytes)
        {
            if (ConnectionMode.Connection_DataSend(bytes) == true)
            {
                ShowInfoBar(string.Format("{0}{1}", "发送:",BitConverter.ToString(bytes).Replace('-', ' ')));
            }
            else
            {
                ShowInfoBar(String.Format("没有建立连接"));
            }

        }
        private void Data_Tcp_AcceptCallback(List<Socket> SocketList, Socket client_new)
        {
            Invoke(new MethodInvoker(delegate
            {
                comboBoxEdit2.Properties.Items.Clear();
                foreach (Socket client in SocketList)
                {
                    comboBoxEdit2.Properties.Items.Add((client.RemoteEndPoint as IPEndPoint).Address.ToString());
                }
                if (comboBoxEdit2.SelectedIndex == -1)
                    comboBoxEdit2.SelectedIndex = 0;
            }));
        }

        private void Data_ReceiveCommand(byte[] data)
        {
            Invoke(new MethodInvoker(delegate 
            {
                InstructionStructure instruction = Serializer.Instance.Deserialize(data);
                string UnitAddress =string.Format("{0}{1}{2}","地址：",instruction.DestinationAddress.ToString(),"  ") ;
                TransmitCmd.CommandTypes Cmd1 = (TransmitCmd.CommandTypes)(instruction.CommandType & 0xFF00);
            
                //debug
                ShowInfoBar(string.Format("{0}", BitConverter.ToString(data).Replace('-', ' ')));

                switch (Cmd1)
                {
                    case TransmitCmd.CommandTypes.System:
                        {
                            TransmitCmd.SystemCommand Cmd2 = (TransmitCmd.SystemCommand)(instruction.CommandType & 0x00FF);
                            switch (Cmd2)
                            {
                                case TransmitCmd.SystemCommand.GetError:
                                    {
                                        sbyte errorNumber;
                                        unchecked
                                        {
                                            errorNumber = (sbyte)instruction.CommandParameter[0];
                                        }
                                        if (errorNumber == -1)
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "校验错误"));
                                        }
                                        else if (errorNumber == -2)
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "下位机未定义该指令"));
                                            order++;
                                        }
                                        else if (errorNumber == -3)
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "指令参数错误"));
                                        }
                                        else if (errorNumber == -4)
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "长度超限"));
                                        }
                                        break;
                                    }
                                case TransmitCmd.SystemCommand.QuerySoftwareVersion:
                                    {
                                        string st = string.Format("版本：V{0}.{1}.{2}", instruction.CommandParameter[0], instruction.CommandParameter[1], instruction.CommandParameter[2]); ShowInfoBar(st);
                                        labelControl10.Text = st;
                                        pictureBox1.Image = Properties.Resources.ok;
                                        order++;
                                        break;
                                    }
                                case TransmitCmd.SystemCommand.QueryNvramState:
                                    {
                                        string st = string.Empty;
                                        byte type = instruction.CommandParameter[0];
                                        switch (type)
                                        {
                                            case 0: st = "铁电：读取失败 "; pictureBox2.Image = Properties.Resources.no; break;
                                            case 1: st = "铁电：FM25L04B "; pictureBox2.Image = Properties.Resources.ok; break;
                                            case 2: st = "铁电：FM25L16B "; pictureBox2.Image = Properties.Resources.ok; break;
                                        }
                                        labelControl12.Text = st;
                                        order++;
                                        break;
                                    }
                                case TransmitCmd.SystemCommand.QueryID:
                                case TransmitCmd.SystemCommand.ReportID:
                                    {
                                        //ShowInfoBar(string.Format("{0}{1}    {2}", UnitAddress, "拨码开关", BitConverter.ToUInt16(instruction.CommandParameter, 0)));
                                        UInt16 num = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                        labelControl9.Text = string.Format("拨码：{0}|{1:X}|{2}", num,num, Convert.ToString(num, 2));
                                        break;
                                    }
                            }
                            break;                    
                        }
                    case TransmitCmd.CommandTypes.RFID:
                        {
                            TransmitCmd.RfidCommand Cmd2 = (TransmitCmd.RfidCommand)(instruction.CommandType & 0x00FF);
                            switch (Cmd2)
                            {
                                case TransmitCmd.RfidCommand.RfidReportIn:
                                    {
                                        //ShowInfoBar(string.Format("{0}{1}{2}", UnitAddress, "上报卡移入", BitConverter.ToString(instruction.CommandParameter)));
                                        Data_SendData(TransmitCmd.Instance.RfidQueryID(DestinationAddress));
                                        break;
                                    }
                                case TransmitCmd.RfidCommand.RfidReportOut:
                                    {
                                        //ShowInfoBar(string.Format("{0}{1}{2}", UnitAddress, "上报卡移出", BitConverter.ToString(instruction.CommandParameter)));
                                        labelControl13.Text = "RFID：请刷卡";
                                        pictureBox3.Image = null;
                                        break;
                                    }
                                case TransmitCmd.RfidCommand.RfidQueryID:
                                    {
                                        UInt64 CardUid = BitConverter.ToUInt64(instruction.CommandParameter, 0);
                                        string hex = string.Empty;
                                        for (int i = 0; i < instruction.CommandParameter.Length; i++)
                                        {
                                            string tmp = Convert.ToString(instruction.CommandParameter[i], 16);
                                            hex += tmp.PadLeft(2, '0');
                                        }
                                        string st = string.Format("RFID：{0}",hex);
                                        ShowInfoBar(st);
                                        labelControl13.Text = st;
                                        pictureBox3.Image = Properties.Resources.ok;
                                        break;
                                    }
                            }
                            break;
                        }
                    case TransmitCmd.CommandTypes.Weigh:
                        {
                            TransmitCmd.WeightCommand Cmd2 = (TransmitCmd.WeightCommand)(instruction.CommandType & 0x00FF);
                            switch (Cmd2)
                            {

                                case TransmitCmd.WeightCommand.WeightSetEnable:
                                    {
                                        //ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置使能", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                        order++; 
                                        break;
                                    }
                                case TransmitCmd.WeightCommand.WeightQuerySensorADValue:
                                    {
                                        //ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询AD"));
                                        UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                        for (int i = 0, j = 0; i < 16; ++i)
                                        {
                                            if ((indexs & (1 << i)) != 0)
                                            {
                                                int ADi = BitConverter.ToInt32(instruction.CommandParameter, 2 + (j++) * sizeof(Int32));
                                                WeighStable(i, ADi);
                                                string st = string.Format("sensor{0}：{1}", i + 1, ADi);
                                                //ShowInfoBar(st);
                                                if (i == 0)
                                                    labelControl14.Text = st;
                                                else if (i == 1)
                                                    labelControl15.Text = st;
                                                else if (i == 2)
                                                    labelControl16.Text = st;
                                                else if (i == 3)
                                                    labelControl17.Text = st;
                                            }
                                        }
                                        break;
                                    }

                                default:
                                    {
                                        //ShowInfoBar(string.Format("{0}{1}", UnitAddress , "称重无该命令！"));
                                        break;
                                    }
                            }
                            break;
                        }
                    case TransmitCmd.CommandTypes.DoorKockLamp:
                        {
                            TransmitCmd.DoorKockLampCommand Cmd2 = (TransmitCmd.DoorKockLampCommand)(instruction.CommandType & 0x00FF);
                            switch (Cmd2)
                            {
                                case TransmitCmd.DoorKockLampCommand.LockOpenReport:
                                    {
                                        byte state = instruction.CommandParameter[0];
                                        //ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "锁状态上报", (state == 0) ? "锁开" : "锁闭"));
                                        string st = string.Format("锁状态：{0}", (state == 0) ? "锁开" : "锁闭");
                                        labelControl11.Text = st;
                                        break;
                                    }
                                case TransmitCmd.DoorKockLampCommand.DoorOpenReport:
                                    {
                                        byte state = instruction.CommandParameter[0];
                                        //ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "门状态上报", (state == 0) ? "门开" : "门闭"));
                                        string st = string.Format("门状态：{0}", (state == 0) ? "门开" : "门闭");
                                        labelControl18.Text = st;
                                        break;
                                    }
                                case TransmitCmd.DoorKockLampCommand.LockOpenGet:
                                    {
                                        string ret = string.Empty;
                                        byte state = instruction.CommandParameter[0];
                                        switch (state)
                                        {
                                            case 0: ret = "锁开"; break;
                                            case 1: ret = "锁闭"; break;
                                            case 2: ret = "未知"; break;
                                            default: ret = "未知错误"; break;
                                        }
                                        //ShowInfoBar(string.Format("{0}{1}       {2}", UnitAddress, "查询锁状态", ret));
                                        string st = string.Format("锁状态：{0}", ret);
                                        labelControl11.Text = st;
                                        order++; 
                                        break;
                                    }
                                case TransmitCmd.DoorKockLampCommand.DoorOpenGet:
                                    {
                                        string ret = string.Empty;
                                        byte state = instruction.CommandParameter[0];
                                        switch (state)
                                        {
                                            case 0: ret = "门开"; break;
                                            case 1: ret = "门闭"; break;
                                            case 2: ret = "未知"; break;
                                            default: ret = "未知错误"; break;
                                        }
                                        //ShowInfoBar(string.Format("{0}{1}       {2}", UnitAddress, "查询门状态", ret));
                                        string st = string.Format("门状态：{0}", ret);
                                        labelControl18.Text = st;
                                        order++; 
                                        break;
                                    }

                                default: break;
                            }
                            break;
                        }
                    case TransmitCmd.CommandTypes.Display:
                        {
                            TransmitCmd.DisplayCommand Cmd2 = (TransmitCmd.DisplayCommand)(instruction.CommandType & 0x00FF);
                            switch (Cmd2)
                            {

                                case TransmitCmd.DisplayCommand.DisplayShowCleanAndChars:
                                    {
                                        //byte state = instruction.CommandParameter[0];
                                        //ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "清屏显示字符", (state == 0) ? "成功" : "失败"));
                                        //order++;
                                        show_count++;
                                        break;
                                    }
                            }
                            break;
                        }

                    default:
                        {
                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "本测试工具未定义该主命令！"));
                            break;
                        }
                }
            }));
            //
            response = true;
        }

        void WeighStable(int sensor_index, Int32 weight)
        {	        
	        bool stable_temp = true;

	        //average
	        WeighStableData.weight_total[sensor_index] = 0;
            for (int i = 0; i < WeighStableStruct.WEIGHT_AVERAGE_COUNT; i++)
	        {
                if (i == WeighStableStruct.WEIGHT_AVERAGE_COUNT - 1)
			        WeighStableData.weight_data[sensor_index,i] = weight;
		        else
			        WeighStableData.weight_data[sensor_index,i] = WeighStableData.weight_data[sensor_index,i+1];
		        WeighStableData.weight_total[sensor_index] += WeighStableData.weight_data[sensor_index,i];	
	        }
            WeighStableData.weight_mean[sensor_index] = WeighStableData.weight_total[sensor_index] / WeighStableStruct.WEIGHT_AVERAGE_COUNT;

            //stable
            for (int i = 0; i < WeighStableStruct.WEIGHT_AVERAGE_COUNT; i++)
	        {
                if (Math.Abs(WeighStableData.weight_mean[sensor_index] - WeighStableData.weight_data[sensor_index, i]) > WeighStableStruct.WEIGHT_STABLE_AD)
		        {
			        stable_temp = false;
			        break;
		        }		
	        }	
	        if(stable_temp == true)
	        {		        
		        WeighStableData.weight_delta[sensor_index] = weight - WeighStableData.weight_last[sensor_index];

                float sensor_k = 16000.0f/float.Parse(comboBoxEdit4.SelectedItem.ToString());
                float weight_standard = int.Parse(comboBoxEdit5.SelectedItem.ToString());
                float sensor_w = Math.Abs(WeighStableData.weight_delta[sensor_index] / sensor_k);
                if (sensor_w > 10  && WeighStableData.weight_last[sensor_index] != 0)
                {
                    if( sensor_w > weight_standard*0.8  &&  sensor_w < weight_standard*1.2)
                    {
                        if (sensor_index == 0)
                            pictureBox4.Image = Properties.Resources.ok;
                        if (sensor_index == 1)
                            pictureBox5.Image = Properties.Resources.ok;
                        if (sensor_index == 2)
                            pictureBox6.Image = Properties.Resources.ok;
                        if (sensor_index == 3)
                            pictureBox7.Image = Properties.Resources.ok;
                    }
                }
                WeighStableData.weight_last[sensor_index] = weight;
	        }
        }

    }



    public class WeighStableStruct
    {
        public const int SENSOR_NUM = 6;
        public const int WEIGHT_AVERAGE_COUNT = 3;
        public const int WEIGHT_STABLE_AD = 10;

        public bool[] weight_tare = new bool[SENSOR_NUM];
        public bool[] weight_stable = new bool[SENSOR_NUM];
        public int[,] weight_data = new int[SENSOR_NUM, WEIGHT_AVERAGE_COUNT];
        public int[] weight_total = new int[SENSOR_NUM];
        public int[] weight_mean = new int[SENSOR_NUM];
        public int[] weight_delta = new int[SENSOR_NUM];
        public int[] weight_last = new int[SENSOR_NUM];
        public int[] weight_last_nv = new int[SENSOR_NUM];
        public int[] weight_drift = new int[SENSOR_NUM];
        public int weight_report_index;
        public int[] weight_report = new int[SENSOR_NUM];
    };

}
