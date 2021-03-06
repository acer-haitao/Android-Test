﻿using DevExpress.XtraEditors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using iValuableTestCommonCode;
using System.Windows.Forms.DataVisualization.Charting;

namespace iValuableTestToolforMotek
{
    public partial class Form1 : Form
    {
        //serial
        bool SerialConnected = false;
        bool PauseFlag = false;
        System.Timers.Timer SerialConnectedTimer = new System.Timers.Timer(500);//实例化Timer类，设置间隔时间为10000毫秒；
        LabelControl[] LabelWeight = new LabelControl[5];

        //ball//line
        Graphics gra = null;
        int range;
        const int PIC_WEIGHT = 350*3/2;
        const int PIC_HEIGHT = 230*3/2;
        int[] lineX = new int[3];
        int[] lineY = new int[3];
        int[,] lineXY = new int[5, 2];

        //log
        string LogPath = "";

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += "  V1.2.1";
            labelControl2.Text = "No device connection";
            labelControl3.Text = "0";
            labelControl4.Text = "0";
            labelControl5.Text = "0";
            labelControl6.Text = "0";
            labelControl7.Text = "0";
            //自动清除行数
            comboBoxEdit2.Properties.Items.Add("0");
            comboBoxEdit2.Properties.Items.Add("20000");
            comboBoxEdit2.Properties.Items.Add("50000");
            comboBoxEdit2.Properties.Items.Add("100000");
            comboBoxEdit2.SelectedIndex = 1;
            //量程
            comboBoxEdit3.Properties.Items.Add("1000");
            comboBoxEdit3.Properties.Items.Add("2000");
            comboBoxEdit3.Properties.Items.Add("5000");
            comboBoxEdit3.Properties.Items.Add("10000");
            comboBoxEdit3.SelectedIndex = 1;

            LabelWeight[0] = labelControl3;
            LabelWeight[1] = labelControl4;
            LabelWeight[2] = labelControl5;
            LabelWeight[3] = labelControl6;
            LabelWeight[4] = labelControl7;

            ConnectionMode.my_Data_ReceiveCommand = Data_ReceiveCommand;

            //System.Timers.Timer t = new System.Timers.Timer(1000);//实例化Timer类，设置间隔时间为10000毫秒；
            SerialConnectedTimer.Elapsed += SerialConnectedTimeoutEvent;//到达时间的时候执行事件；
            SerialConnectedTimer.AutoReset = true;//设置是执行一次（false）还是一直执行(true)；

            chart1.Series[0].Color = Color.Red;
            chart1.Series[1].Color = Color.Purple;
            chart1.Series[2].Color = Color.Yellow;
            chart1.Series[3].Color = Color.Green;
            chart1.Series[4].Color = Color.Blue;


            Bitmap bm = new Bitmap(PIC_WEIGHT, PIC_HEIGHT);
            pictureBox1.Size = bm.Size;
            gra = pictureBox1.CreateGraphics();
            Bitmap bm1 = new Bitmap(PIC_WEIGHT*112/100, PIC_HEIGHT*112/100);
            pictureBox2.Size = bm1.Size;
            button1.Location = new Point(pictureBox1.Location.X - button4.Width / 2, pictureBox1.Location.Y - button4.Height / 2);
            button2.Location = new Point(pictureBox1.Location.X + PIC_WEIGHT - button4.Width / 2, pictureBox1.Location.Y - button4.Height / 2);
            button3.Location = new Point(pictureBox1.Location.X - button4.Width / 2, pictureBox1.Location.Y + PIC_HEIGHT - button4.Height / 2);
            button4.Location = new Point(pictureBox1.Location.X + PIC_WEIGHT - button4.Width / 2, pictureBox1.Location.Y + PIC_HEIGHT - button4.Height / 2);

            labelControl9.Location = new Point(labelControl8.Location.X, labelControl8.Location.Y + pictureBox2.Size.Height + 25);
            labelControl10.Location = new Point(labelControl8.Location.X + pictureBox1.Size.Width, labelControl8.Location.Y + pictureBox2.Size.Height + 25);
            labelControl11.Location = new Point(labelControl8.Location.X + pictureBox1.Size.Width, labelControl8.Location.Y);

            string timenew = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            LogPath = System.Windows.Forms.Application.StartupPath + "\\iValuableTestforMotek" + timenew + ".log";
            if (File.Exists(LogPath))
            {
                LogPath = LogPath.Insert(LogPath.LastIndexOf('.'), "(1)");
            }
            FileInfo myfile = new FileInfo(LogPath);
            FileStream fs = myfile.Create();
            fs.Close();

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ConnectionMode.Serial_Close();
            SimpleButton but = new SimpleButton();
            but.Text = "Clean";
            simpleButton_Click(but, null);
        }
        private void comboBoxEdit3_SelectedIndexChanged(object sender, EventArgs e)
        {
            range = int.Parse(comboBoxEdit3.SelectedItem.ToString());
        }
        private void ShowInfoBar(string message)
        {
            string timenew = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "  ";
            memoEdit1.MaskBox.AppendText(timenew + message + Environment.NewLine);
            if(int.Parse(comboBoxEdit2.SelectedItem.ToString()) > 0)
            {
                if (memoEdit1.Text.Length > int.Parse(comboBoxEdit2.SelectedItem.ToString()))
                {
                    SimpleButton but = new SimpleButton();
                    but.Text = "Clean";
                    simpleButton_Click(but, null);
                }
            }

        }
        private void simpleButton_Click(object sender, EventArgs e)
        {
            SimpleButton button = sender as SimpleButton;
            switch (button.Text)
            {
                case "Scan":
                    {
                        SerialConnected = false;
                        labelControl2.Text = "No device connection";
                        memoEdit1.Text = "";
                        foreach (var series in chart1.Series)
                        {
                            series.Points.Clear();
                        }

                        ConnectionMode.Serial_Scan();
                        comboBoxEdit1.Properties.Items.Clear();
                        foreach (String client in ConnectionMode.Get_COMLocalIPList())
                        {
                            comboBoxEdit1.Properties.Items.Add(client);
                        }                        
                        comboBoxEdit1.SelectedIndex = 0;

                        ConnectionMode.Serial_Open(comboBoxEdit1.SelectedItem.ToString());
                        ConnectionMode.Connection_DataSend(TransmitCmd.Instance.QueryHeartbeat(0));
                        SerialConnectedTimer.Start();
                        break;
                    }
                case "Pause":
                    {
                        PauseFlag = !PauseFlag;
                        break;
                    }
                case "Clean":
                    {
                        StreamWriter sw = File.AppendText(LogPath);
                        sw.Write(memoEdit1.Text);
                        sw.Flush();
                        sw.Close();
                        memoEdit1.Text = "";
                        foreach (var series in chart1.Series)
                        {
                            series.Points.Clear();
                        }
                        break;
                    }
                case "Peel":
                    {
                        ConnectionMode.Connection_DataSend(TransmitCmd.Instance.WeightPeeling(0, 0x0f));
                        break;
                    }
                default:
                    {
                        MessageBox.Show(button.Text);
                        break;
                    }
            }
        }
        public void SerialConnectedTimeoutEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            if (SerialConnected)
            {
               SerialConnectedTimer.Stop();
            }
            else
            { 
                if (comboBoxEdit1.SelectedIndex+1 < comboBoxEdit1.Properties.Items.Count)
                {
                    Invoke(new MethodInvoker(delegate
                    {
                        comboBoxEdit1.SelectedIndex++;
                        ConnectionMode.Serial_Open(comboBoxEdit1.SelectedItem.ToString());
                        ConnectionMode.Connection_DataSend(TransmitCmd.Instance.QueryHeartbeat(0));
                    }));
                }
                else
                {
                    SerialConnectedTimer.Stop();
                    MessageBox.Show("Unable to connect to device, Please check the serial port.");
                }
            } 
        }
         private void Data_ReceiveCommand(byte[] data)
        {
            if (PauseFlag)
                 return ;
            Invoke(new MethodInvoker(delegate 
            {
                InstructionStructure instruction = Serializer.Instance.Deserialize(data);
                string UnitAddress =string.Format("{0}{1}{2}","地址：",instruction.DestinationAddress.ToString(),"  ") ;
                TransmitCmd.CommandTypes Cmd1 = (TransmitCmd.CommandTypes)(instruction.CommandType & 0xFF00);
            
                //debug
                //ShowInfoBar(string.Format("{0}", BitConverter.ToString(data).Replace('-', ' ')));

                switch (Cmd1)
                {
                    case TransmitCmd.CommandTypes.System:
                        {
                            TransmitCmd.SystemCommand Cmd2 = (TransmitCmd.SystemCommand)(instruction.CommandType & 0x00FF);
                            switch (Cmd2)
                            {                               
                                case TransmitCmd.SystemCommand.QueryHeartbeat:
                                    {
                                        //ShowInfoBar(string.Format("{0}{1}", UnitAddress , "查询设备状态（心跳）"));
                                        if (SerialConnected == false)
                                        {
                                            SerialConnected = true;
                                            labelControl2.Text = "device connected";
                                        }
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
                                case TransmitCmd.WeightCommand.WeightReportDiffWeight:
                                    {
                                        //ShowInfoBar(string.Format("{0}{1}", UnitAddress, "上报重量差"));
                                        UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                        float[] TempWeight = new float[4];
                                        float TempWeightAll = 0;
                                        for (int i = 0, j = 0; i < 16; ++i)
                                        {
                                            if ((indexs & (1 << i)) != 0)
                                            {
                                                TempWeight[i] = BitConverter.ToSingle(instruction.CommandParameter, 2 + j * sizeof(float));
                                                TempWeightAll += TempWeight[i];
                                                LabelWeight[i].Text = TempWeight[i].ToString();
                                                //ShowInfoBar(String.Format(" 秤盘{0}的重量差 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + j * sizeof(float))));
                                                j++;
                                            }
                                        }
                                        //LabelWeight[4].Text = TempWeightAll.ToString("#0.00");
                                        LabelWeight[4].Text = TempWeightAll.ToString();
                                        chart1.Series[0].Points.Add(TempWeight[0]);
                                        chart1.Series[1].Points.Add(TempWeight[1]);
                                        chart1.Series[2].Points.Add(TempWeight[2]);
                                        chart1.Series[3].Points.Add(TempWeight[3]);
                                        chart1.Series[4].Points.Add(TempWeightAll);

                                        ShowInfoBar(string.Format("  {0:N2}  {1:N2}  {2:N2}  {3:N2}  {4:N2}", 
                                                                TempWeight[0], TempWeight[1], TempWeight[2], TempWeight[3], TempWeightAll));

                                        //清屏
                                        gra.Clear(pictureBox1.BackColor);
                                        //坐标、外框、位置格
                                        int partnum = 8;
                                        for (int i = 0; i <= partnum; i++)
                                        {
                                            gra.DrawLine(new Pen(Color.Gainsboro, 1.0f), new Point(PIC_WEIGHT * i / partnum, 0), new Point(PIC_WEIGHT * i / partnum, PIC_HEIGHT));
                                            gra.DrawLine(new Pen(Color.Gainsboro, 1.0f), new Point(0, PIC_HEIGHT * i / partnum), new Point(PIC_WEIGHT, PIC_HEIGHT * i / partnum));
                                        }
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(PIC_WEIGHT / 2, 0), new Point(PIC_WEIGHT / 2, PIC_HEIGHT));
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(0, PIC_HEIGHT / 2), new Point(PIC_WEIGHT, PIC_HEIGHT / 2));
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(0, 0), new Point(0, PIC_HEIGHT));
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(0, PIC_HEIGHT-1), new Point(PIC_WEIGHT, PIC_HEIGHT-1));
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(PIC_WEIGHT-1, PIC_HEIGHT), new Point(PIC_WEIGHT-1, 0));
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(PIC_WEIGHT, 0), new Point(0, 0));
                                        //画圆
                                        double ball_x_temp = (TempWeight[1]+ TempWeight[2]) / (TempWeightAll + 0.1);
                                        double ball_y_temp = (TempWeight[0]+ TempWeight[1]) / (TempWeightAll + 0.1);
                                        int ball_x = Convert.ToInt32(pictureBox1.Size.Width * ball_x_temp);
                                        int ball_y = Convert.ToInt32(pictureBox1.Size.Height * ball_y_temp);
                                        gra.FillEllipse(new SolidBrush(Color.Blue), ball_x - 10, ball_y - 10, 20, 20);
                                        //垂直画线                                        
                                        lineX[0] = (int)((1 - (TempWeight[0] + TempWeight[3]) / (range * 2)) * PIC_WEIGHT / 2);
                                        lineX[1] = PIC_WEIGHT / 2 + (int)(((TempWeight[1] + TempWeight[2]) / (range * 2)) * PIC_WEIGHT / 2);
                                        lineX[2] = lineX[0] + lineX[1] - PIC_WEIGHT / 2;
                                        gra.DrawLine(new Pen(Color.Black, 10.0f), new Point(lineX[0], 0), new Point(lineX[1], 0));
                                        gra.DrawLine(new Pen(Color.Black, 10.0f), new Point(PIC_WEIGHT / 2, PIC_HEIGHT), new Point(lineX[2], PIC_HEIGHT));
                                        lineY[0] = (int)((1 - (TempWeight[2] + TempWeight[3]) / (range * 2)) * PIC_HEIGHT / 2);
                                        lineY[1] = PIC_HEIGHT / 2 + (int)(((TempWeight[1] + TempWeight[0]) / (range * 2)) * PIC_HEIGHT / 2);
                                        lineY[2] = lineY[0] + lineY[1] - PIC_HEIGHT / 2;
                                        gra.DrawLine(new Pen(Color.Black, 10.0f), new Point(0, lineY[0]), new Point(0, lineY[1]));
                                        gra.DrawLine(new Pen(Color.Black, 10.0f), new Point(PIC_WEIGHT, PIC_HEIGHT / 2), new Point(PIC_WEIGHT, lineY[2]));
                                        //每个传感器画线
                                        lineXY[0, 0] = (int)((1 - (TempWeight[0] / (range + 0.1))) * PIC_WEIGHT / 2);
                                        lineXY[0, 1] = PIC_HEIGHT / 2 + (int)(((TempWeight[0] / (range + 0.1))) * PIC_HEIGHT / 2);
                                        gra.DrawLine(new Pen(Color.Red, 5.0f), new Point(PIC_WEIGHT / 2, PIC_HEIGHT / 2), new Point(lineXY[0, 0], lineXY[0, 1]));
                                        lineXY[1, 0] = PIC_WEIGHT / 2 + (int)(((TempWeight[1] / (range + 0.1))) * PIC_WEIGHT / 2);
                                        lineXY[1, 1] = PIC_HEIGHT / 2 + (int)(((TempWeight[1] / (range + 0.1))) * PIC_HEIGHT / 2);
                                        gra.DrawLine(new Pen(Color.Purple, 5.0f), new Point(PIC_WEIGHT / 2, PIC_HEIGHT / 2), new Point(lineXY[1, 0], lineXY[1, 1]));
                                        lineXY[2, 0] = PIC_WEIGHT / 2 + (int)(((TempWeight[2] / (range + 0.1))) * PIC_WEIGHT / 2);
                                        lineXY[2, 1] = (int)((1 - (TempWeight[2] / (range + 0.1))) * PIC_HEIGHT / 2);
                                        gra.DrawLine(new Pen(Color.Yellow, 5.0f), new Point(PIC_WEIGHT / 2, PIC_HEIGHT / 2), new Point(lineXY[2, 0], lineXY[2, 1]));
                                        lineXY[3, 0] = (int)((1 - (TempWeight[3] / (range + 0.1))) * PIC_WEIGHT / 2);
                                        lineXY[3, 1] = (int)((1 - (TempWeight[3] / (range + 0.1))) * PIC_HEIGHT / 2);
                                        gra.DrawLine(new Pen(Color.Green, 5.0f), new Point(PIC_WEIGHT / 2, PIC_HEIGHT / 2), new Point(lineXY[3, 0], lineXY[3, 1]));
                                        //包络线
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(lineXY[0, 0], lineXY[0, 1]), new Point(lineXY[1, 0], lineXY[1, 1]));
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(lineXY[1, 0], lineXY[1, 1]), new Point(lineXY[2, 0], lineXY[2, 1]));
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(lineXY[2, 0], lineXY[2, 1]), new Point(lineXY[3, 0], lineXY[3, 1]));
                                        gra.DrawLine(new Pen(Color.Black, 2.0f), new Point(lineXY[3, 0], lineXY[3, 1]), new Point(lineXY[0, 0], lineXY[0, 1]));
                                        //合力矢量
                                        lineXY[4, 0] = lineXY[0, 0] + lineXY[3, 0] + lineXY[1, 0] + lineXY[2, 0] - (PIC_WEIGHT/2*3);
                                        lineXY[4, 1] = lineXY[0, 1] + lineXY[3, 1] + lineXY[1, 1] + lineXY[2, 1] - (PIC_HEIGHT/2*3);
                                        gra.DrawLine(new Pen(Color.Blue, 5.0f), new Point(PIC_WEIGHT / 2, PIC_HEIGHT / 2), new Point(lineXY[4, 0], lineXY[4, 1]));
                                        gra.DrawEllipse(new Pen(Color.Blue, 2.0f), lineXY[4, 0] - 8, lineXY[4, 1] - 8, 16, 16);
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

    }
}
