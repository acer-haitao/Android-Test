using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Configuration;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using iValuableTestCommonCode;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace iValuableTestTool
{
    public partial class Form2SetIP : Form
    {
        private const UInt16 UDP_REMOT_PORT = 18800;
        private const UInt16 UDP_LOCAL_PORT = 18801;
        private const string BCAST = "255.255.255.255";

        private const string MAC = "0.0.0.0.0.0";
        private const string IP = "192.168.8.101";
        private const string MASK = "255.255.255.0";
        private const string GATEWAY = "192.168.8.100";
        private const string SERVER_IP = "192.168.8.100";
        private const string SERVER_PORT = "8782";

        private string Mac;
        private string Ip ;
        private string Mask;
        private string Gateway;
        private string Server_Ip;
        private string Server_Port;

        
        public Form2SetIP()
        {
            InitializeComponent();
        }
        private void Form2SetIP_Load(object sender, EventArgs e)
        {
            SimpleButton but = new SimpleButton() { Text = "刷新" };
            Form2SetIP_simpleButton_Click(but, null);

            checkedListBoxControl1.Items.Add(BCAST);

            ConfigFileInitialize();
            ShowIPInfo();

            ConnectionMode.Udp_Rec_Thread_Start();
            ConnectionMode.my_Udp_Data_ReceiveCommand = Udp_Data_ReceiveCommand;
        }
        private void Form2SetIP_FormClosing(object sender, FormClosingEventArgs e)
        {
            //ConnectionMode.Udp_Rec_Thread_Abort();
        }

        private void ShowIPInfo()
        {
            //MAC
            var Macaddr = Mac.Split('.');
            textEdit1.Text = Macaddr[0];
            textEdit2.Text = Macaddr[1];
            textEdit3.Text = Macaddr[2];
            textEdit4.Text = Macaddr[3];
            textEdit5.Text = Macaddr[4];
            textEdit6.Text = Macaddr[5];
            //IP
            var Ipaddr = Ip.Split('.');
            textEdit7.Text = Ipaddr[0];
            textEdit8.Text = Ipaddr[1];
            textEdit9.Text = Ipaddr[2];
            textEdit10.Text = Ipaddr[3];
            //掩码
            var Maskaddr = Mask.Split('.');
            textEdit11.Text = Maskaddr[0];
            textEdit12.Text = Maskaddr[1];
            textEdit13.Text = Maskaddr[2];
            textEdit14.Text = Maskaddr[3];
            //网关
            var Gatewayaddr = Gateway.Split('.');
            textEdit15.Text = Gatewayaddr[0];
            textEdit16.Text = Gatewayaddr[1];
            textEdit17.Text = Gatewayaddr[2];
            textEdit18.Text = Gatewayaddr[3];
            //服务器IP
            var Server_Ipaddr = Server_Ip.Split('.');
            textEdit19.Text = Server_Ipaddr[0];
            textEdit20.Text = Server_Ipaddr[1];
            textEdit21.Text = Server_Ipaddr[2];
            textEdit22.Text = Server_Ipaddr[3];
            //服务器端口
            textEdit23.Text = Server_Port;
        }

        private void ConfigFileInitialize()
        {
            bool[] exist = new bool[6] { false, false, false, false, false, false };
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(file);
            
            foreach (string key in config.AppSettings.Settings.AllKeys)
            {
                if (key == "Mac") exist[0] = true;
                if (key == "Ip") exist[1] = true;
                if (key == "Mask") exist[2] = true;
                if (key == "Gateway") exist[3] = true;
                if (key == "Server_Ip") exist[4] = true;
                if (key == "Server_Port") exist[5] = true;
            }
            if (!exist[0]){ config.AppSettings.Settings.Add("Mac", MAC); config.Save(); ConfigurationManager.RefreshSection("appSettings"); }
            if (!exist[1]){ config.AppSettings.Settings.Add("Ip", IP); config.Save(); ConfigurationManager.RefreshSection("appSettings"); }
            if (!exist[2]) { config.AppSettings.Settings.Add("Mask", MASK); config.Save(); ConfigurationManager.RefreshSection("appSettings"); }
            if (!exist[3]) { config.AppSettings.Settings.Add("Gateway", GATEWAY); config.Save(); ConfigurationManager.RefreshSection("appSettings"); }
            if (!exist[4]) { config.AppSettings.Settings.Add("Server_Ip", SERVER_IP); config.Save(); ConfigurationManager.RefreshSection("appSettings"); }
            if (!exist[5]) { config.AppSettings.Settings.Add("Server_Port", SERVER_PORT); config.Save(); ConfigurationManager.RefreshSection("appSettings"); }

            Mac = config.AppSettings.Settings["Mac"].Value;
            Ip = config.AppSettings.Settings["Ip"].Value;
            Mask = config.AppSettings.Settings["Mask"].Value;
            Gateway = config.AppSettings.Settings["Gateway"].Value;
            Server_Ip = config.AppSettings.Settings["Server_Ip"].Value;
            Server_Port = config.AppSettings.Settings["Server_Port"].Value;
        }

        private void local_IP_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectionMode.Udp_Open(comboBoxEdit1.SelectedItem.ToString(), UDP_LOCAL_PORT);
        }
        private void Ip_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
        {
            if (checkedListBoxControl1.CheckedItems.Count > 0)
            {
                for (int i = 0; i < checkedListBoxControl1.Items.Count; i++)
                {
                    if (i != e.Index)
                    {
                        checkedListBoxControl1.SetItemCheckState(i, System.Windows.Forms.CheckState.Unchecked);
                    }
                }
            }
            else
            {
                checkedListBoxControl1.SetItemCheckState(e.Index, System.Windows.Forms.CheckState.Checked);
            }
        }
         private void Form2SetIP_simpleButton_Click(object sender, EventArgs e)
        {

            SimpleButton button = sender as SimpleButton;
            switch (button.Text)
            {

                case "刷新":
                    {
                        ConnectionMode.LocalIP_Scan();

                        comboBoxEdit1.Properties.Items.Clear();
                        foreach (String client in ConnectionMode.Get_LocalIPList())
                        {
                            comboBoxEdit1.Properties.Items.Add(client);
                        }
                        comboBoxEdit1.Properties.Sorted = true;
                        if (comboBoxEdit1.SelectedIndex == -1)
                            comboBoxEdit1.SelectedIndex = 0;
                        break;
                    }
                case "查找":
                    {                         
                        try
                        {                           
                            Byte[] sendBytes = TransmitCmd.Instance.LANGetDevices(0);
                            ConnectionMode.Udp_SendData(BCAST, UDP_REMOT_PORT, sendBytes);
                            ShowInfoBar(string.Format("{0}", BitConverter.ToString(sendBytes).Replace('-', ' ')));
                            ShowInfoBar(String.Format("发送{0}命令", button.Text));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_AcceptCallback错误");
                        }
                        break;
                    }
                case "读取":
                    {
                        try
                        {
                            if (checkedListBoxControl1.CheckedItems.Count > 0)
                            {
                                string Select_Ip = "";
                                for (int i = 0; i < checkedListBoxControl1.Items.Count; i++)
                                {
                                    if (checkedListBoxControl1.Items[i].CheckState == CheckState.Checked)
                                    {
                                       Select_Ip = checkedListBoxControl1.Items[i].ToString().Trim();
                                    }
                                }
                                Byte[] sendBytes = TransmitCmd.Instance.LANGetParameter(0);
                                ConnectionMode.Udp_SendData(Select_Ip, UDP_REMOT_PORT, sendBytes);
                                ShowInfoBar(string.Format("{0}", BitConverter.ToString(sendBytes).Replace('-', ' ')));
                                ShowInfoBar(String.Format("发送{0}命令", button.Text));
                            }
                            else
                            {
                                ShowInfoBar("没选择主控板");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_AcceptCallback错误");
                        }
                        break;
                    }
                case "默认":
                    {
                        ConfigFileInitialize();
                        ShowIPInfo();
                        break;
                    }
                case "设置":
                    {
                        try
                        {
                            if (checkedListBoxControl1.CheckedItems.Count > 0)
                            {
                                string Select_Ip = "";
                                for (int i = 0; i < checkedListBoxControl1.Items.Count; i++)
                                {
                                    if (checkedListBoxControl1.Items[i].CheckState == CheckState.Checked)
                                    {
                                        Select_Ip = checkedListBoxControl1.Items[i].ToString().Trim();
                                    }
                                }

                                byte[] s_mac = new byte[6]{Convert.ToByte(textEdit1.Text,16), Convert.ToByte(textEdit2.Text,16),Convert.ToByte(textEdit3.Text,16),
                                                            Convert.ToByte(textEdit4.Text,16), Convert.ToByte(textEdit5.Text,16),Convert.ToByte(textEdit6.Text,16)};
                                byte [] s_ip = new byte[4]{Convert.ToByte(textEdit7.Text), Convert.ToByte(textEdit8.Text),Convert.ToByte(textEdit9.Text),Convert.ToByte(textEdit10.Text)};                                
                                byte [] s_mask = new byte[4]{Convert.ToByte(textEdit11.Text), Convert.ToByte(textEdit12.Text),Convert.ToByte(textEdit13.Text),Convert.ToByte(textEdit14.Text)};                                
                                byte [] s_gatewau = new byte[4]{Convert.ToByte(textEdit15.Text), Convert.ToByte(textEdit16.Text),Convert.ToByte(textEdit17.Text),Convert.ToByte(textEdit18.Text)};
                                byte [] s_server_ip = new byte[4]{Convert.ToByte(textEdit19.Text), Convert.ToByte(textEdit20.Text),Convert.ToByte(textEdit21.Text),Convert.ToByte(textEdit22.Text)};
                                UInt16 s_server_port = UInt16.Parse(textEdit23.Text);

                                Byte[] sendBytes = TransmitCmd.Instance.LANSetParameter(0, s_mac, s_ip, s_mask, s_gatewau, s_server_ip, s_server_port);
                                ConnectionMode.Udp_SendData(Select_Ip, UDP_REMOT_PORT, sendBytes);
                                ShowInfoBar(string.Format("{0}", BitConverter.ToString(sendBytes).Replace('-', ' ')));
                                ShowInfoBar(String.Format("发送{0}命令", button.Text));
                            }
                            else
                            {
                                ShowInfoBar("没选择主控板");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "Tcp_AcceptCallback错误");
                        }
                        break;
                    }
            }
        }
        private void ShowInfoBar(string message)
        {
            //string timenew = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") +"  ";
            //memoEdit1.EditValue += timenew ;
            //memoEdit1.EditValue += message + Environment.NewLine;
            memoEdit1.MaskBox.AppendText(message + Environment.NewLine);
            if (memoEdit1.Text.Length > 10000)
            {
                memoEdit1.SelectedText = "";
            }
        }

        private void Udp_Data_ReceiveCommand(string ipString, int port, byte[] data)
        {
            try
            {
                Invoke(new MethodInvoker(delegate
                {
                    InstructionStructure instruction = Serializer.Instance.Deserialize(data);
                    string UnitAddress = string.Format("{0}{1}{2}", "地址：", instruction.DestinationAddress.ToString(), "  ");
                    TransmitCmd.CommandTypes Cmd1 = (TransmitCmd.CommandTypes)(instruction.CommandType & 0xFF00);

                    //debug
                    ShowInfoBar(string.Format("设备IP  {0}:{1}", ipString, port));
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
                                }
                                break;
                            }
                        case TransmitCmd.CommandTypes.LAN:
                            {
                                TransmitCmd.LANCommand Cmd2 = (TransmitCmd.LANCommand)(instruction.CommandType & 0x00FF);
                                switch (Cmd2)
                                {
                                    case TransmitCmd.LANCommand.LANGetDevices:
                                        {
                                            
                                            string string_temp = "";
                                            for(int i= 0; i<6;i++)
                                            {
                                                string_temp += string.Format("{0:X000}", instruction.CommandParameter[i]);
                                                if (i < 5) string_temp += ".";
                                            }
                                            ShowInfoBar(string.Format("MAC:{0}", string_temp));
                                            string_temp = "";
                                            for (int i = 6; i < 10; i++)
                                            {
                                                string_temp += instruction.CommandParameter[i].ToString();
                                                if (i < 9)  string_temp += ".";
                                            }
                                            ShowInfoBar(string.Format("IP:{0}", string_temp));

                                            bool Itis = false;
                                            CheckedListBoxItem anditem = new CheckedListBoxItem(string_temp, false);
                                            string y = anditem.ToString();
                                            for (int i = 0; i < checkedListBoxControl1.Items.Count; i++)
                                            {
                                                string x = checkedListBoxControl1.Items[i].Value.ToString();
                                                if (string.Compare(x, y) == 0)
                                                {
                                                    Itis = true;
                                                    break;
                                                }
                                            }
                                            if (Itis == false)
                                            {
                                                checkedListBoxControl1.Items.Add(anditem);
                                            }
                                            break;
                                        }
                                    case TransmitCmd.LANCommand.LANGetParameter:
                                        {
                                            string string_temp = "";
                                            for (int i = 0; i < 6; i++)
                                            {
                                                string_temp += string.Format("{0:X000}", instruction.CommandParameter[i]);
                                                if (i < 5) string_temp += ".";
                                            }
                                            Mac = string_temp;
                                            ShowInfoBar(string.Format("MAC:{0}", Mac));

                                            string_temp = "";
                                            for (int i = 6; i < 10; i++)
                                            {
                                                string_temp += instruction.CommandParameter[i].ToString();
                                                if (i < 9) string_temp += ".";
                                            }
                                            Ip = string_temp;
                                            ShowInfoBar(string.Format("IP:{0}", Ip));

                                            string_temp = "";
                                            for (int i = 10; i < 14; i++)
                                            {
                                                string_temp += instruction.CommandParameter[i].ToString();
                                                if (i < 13) string_temp += ".";
                                            }
                                            Mask = string_temp;
                                            ShowInfoBar(string.Format("Mask:{0}", Mask));

                                            string_temp = "";
                                            for (int i = 14; i < 18; i++)
                                            {
                                                string_temp += instruction.CommandParameter[i].ToString();
                                                if (i < 17) string_temp += ".";
                                            }
                                            Gateway = string_temp;
                                            ShowInfoBar(string.Format("Gateway:{0}", Gateway));

                                            string_temp = "";
                                            for (int i = 18; i < 22; i++)
                                            {
                                                string_temp += instruction.CommandParameter[i].ToString();
                                                if (i < 21) string_temp += ".";
                                            }
                                            Server_Ip = string_temp;
                                            ShowInfoBar(string.Format("Server_Ip:{0}", Server_Ip));

                                            string_temp = "";                                            
                                            string_temp = BitConverter.ToUInt16(instruction.CommandParameter, 22).ToString();                                            
                                            Server_Port = string_temp;
                                            ShowInfoBar(string.Format("Server_Port:{0}", Server_Port));

                                            ShowIPInfo();
                                            break;
                                        }
                                    case TransmitCmd.LANCommand.LANSetParameter:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}    {2}", UnitAddress, "设置以太网参数", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    default:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "本测试程序未定义该命令！"));
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "数据解析错误");
            }
        }
    }
}
