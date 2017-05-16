using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using iValuableTestCommonCode;



namespace iValuableTestTool
{
    public partial class Form1 : Form
    {        
        //port
        private const string PORT_NUMBER = "8782";

        //scan unit
        private bool ScanState = false;
        private int ScanCycle = 100;
        private bool ScanRecOk = false;
        private int ScanRetry = 0;
        private UInt16 ScanNumber = 1;
        //calibration
        private bool CaliState = false;
        private int CaliCycle = 1000;
        private int CaliCycleCount = 0;
        //unit
        private bool UnitState = false;
        private int UnitCycle = 100;
        private SimpleButton UnitSendButton = null;
        private byte[] UnitBytes = null;
        private UInt16 UnitListAddr = 1;
        private UInt16 UnitListIndex = 0;
        private static List<UInt16> UnitList = new List<UInt16>();
        System.Timers.Timer UnitSendTimer;
        private int TimerCycle = 100;
        //boot
        private FileStream Boot_fs;
        string Boot_Firmwarefile;
        private ushort Boot_PageNumber = 0;
        //Card
        private UInt64 CardUid = 0;
        private bool CardWriteCharge = false;
        //pan calibration max 6 sensor
        private int PanCalibration = -1;
        private double[,] PanAd = new double[7,7];
        private float[] PanK = new float[6];
        //Test 
        private bool TestWeight = false;
        private double TestWeightCount = 0;
        //Calibration


        public Form1()
        {
            InitializeComponent();
        }

        #region Form_load
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text += "  V1.3.8";
            LogHelper.WriteLog(string.Format("当前的时间{0}", DateTime.Now.ToString()));
            //
            SimpleButton but = new SimpleButton();
            but.Text = "刷新";
            Form_simpleButton_Click(but, null);
            //simpleButton_getip_Click(sender,e);
            simpleButton1.Text = "开始";
            textEdit1.Text = ConfigPortNumberRead();
            ShowStatusBar("Tcp关闭");
            //
            ConnectionMode.my_Data_ReceiveCommand = Tcp_Data_ReceiveCommand;
            ConnectionMode.my_Tcp_AcceptCallback = Data_Tcp_AcceptCallback;
            //timer
            UnitSendTimer = new System.Timers.Timer(100);
            UnitSendTimer.Elapsed += UnitSendTimeroutEvent;
            UnitSendTimer.AutoReset = true;
            UnitSendTimer.Interval = TimerCycle;
            UnitSendTimer.Start();
            //系统
            comboBoxEdit63.Properties.Items.Add("正常");
            comboBoxEdit63.Properties.Items.Add("调试1");
            comboBoxEdit63.Properties.Items.Add("调试2");
            comboBoxEdit63.Properties.Items.Add("调试3");
            comboBoxEdit63.Properties.Items.Add("调试4");
            comboBoxEdit63.SelectedIndex = 0;
            //RFID
            textEdit3.Text = "0";
            textEdit4.Text = "4";
            //中药房卡
            textEdit5.Text = "1";
            textEdit7.Text = "1234567890123456";
            memoEdit3.Text = "A1234567\r\n张某某\r\nABCDEFG";
            //充电柜卡
            textEdit9.Text = "郑大医院";
            //乐高柜卡
            textEdit10.Text = "0";
            //毒麻柜
            comboBoxEdit35.Properties.Items.Add("取出");
            comboBoxEdit35.Properties.Items.Add("待取");
            comboBoxEdit35.Properties.Items.Add("取用");
            comboBoxEdit35.Properties.Items.Add("退还");
            comboBoxEdit35.SelectedIndex = 1;
            textEdit11.Text = "16";   //手术室名称
            textEdit52.Text = "100";   //盒重
            textEdit12.Text = "张某某";     //配药人（最大4个汉字）
            textEdit13.Text = "李某某";     //领用人
            textEdit14.Text = "王某某";     //退还人
            //时间
            foreach (Control control in xtraTabPage11.Controls)
            {
                if (control is DateEdit)
                {
                    ((DateEdit)control).Properties.DisplayFormat.FormatString = "yyyy/MM/dd HH:mm:ss";
                    ((DateEdit)control).Properties.DisplayFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    ((DateEdit)control).Properties.EditFormat.FormatString = "yyyy/MM/dd HH:mm:ss";
                    ((DateEdit)control).Properties.EditFormat.FormatType = DevExpress.Utils.FormatType.DateTime;
                    ((DateEdit)control).Properties.Mask.EditMask = "yyyy/MM/dd HH:mm:ss";
                    ((DateEdit)control).Properties.VistaDisplayMode = DevExpress.Utils.DefaultBoolean.True;
                    ((DateEdit)control).Properties.VistaEditTime = DevExpress.Utils.DefaultBoolean.True;
                }
            }

            DateTime DateTime_1 = DateTime.Now;
            //DateTime_1 += TimeZoneInfo.Local.BaseUtcOffset;
            dateEdit1.EditValue = DateTime_1.AddHours(-5);    //放入柜子时间
            dateEdit2.EditValue = DateTime_1.AddHours(-3);     //领用时间
            dateEdit3.EditValue = DateTime_1;                    //退还时间

            textEdit15.Text = "123456";       //清单号
            comboBoxEdit36.Properties.Items.Add("否");  //是否是合并清单（1：是）
            comboBoxEdit36.Properties.Items.Add("是");
            comboBoxEdit36.SelectedIndex = 0;
            textEdit16.Text = "3";           //药品数量

            //药品1
            textEdit27.Text = "1234501";      //药品ID
            comboBoxEdit37.Properties.Items.Add("冰箱");  //1：柜子0：冰箱
            comboBoxEdit37.Properties.Items.Add("柜子");
            comboBoxEdit37.SelectedIndex = 1;
            textEdit23.Text = "10";      //放入数量
            textEdit24.Text = "1";       //领用数量
            textEdit25.Text = "9";       //退还数量
            //药品2
            textEdit33.Text = "1234502";      //药品ID
            comboBoxEdit38.Properties.Items.Add("冰箱");  //1：柜子0：冰箱
            comboBoxEdit38.Properties.Items.Add("柜子");
            comboBoxEdit38.SelectedIndex = 1;
            textEdit34.Text = "10";      //放入数量
            textEdit36.Text = "2";       //领用数量
            textEdit35.Text = "8";       //退还数量
            //药品3
            textEdit38.Text = "1234503";      //药品ID
            comboBoxEdit39.Properties.Items.Add("冰箱");  //1：柜子0：冰箱
            comboBoxEdit39.Properties.Items.Add("柜子");
            comboBoxEdit39.SelectedIndex = 1;
            textEdit39.Text = "10";      //放入数量
            textEdit41.Text = "3";       //领用数量
            textEdit40.Text = "7";       //退还数量
            //药品4
            textEdit43.Text = "1234504";      //药品ID
            comboBoxEdit40.Properties.Items.Add("冰箱");  //1：柜子0：冰箱
            comboBoxEdit40.Properties.Items.Add("柜子");
            comboBoxEdit40.SelectedIndex = 1;
            textEdit44.Text = "10";      //放入数量
            textEdit46.Text = "4";       //领用数量
            textEdit45.Text = "6";       //退还数量
            //药品5
            textEdit48.Text = "1234505";      //药品ID
            comboBoxEdit41.Properties.Items.Add("冰箱");  //1：柜子0：冰箱
            comboBoxEdit41.Properties.Items.Add("柜子");
            comboBoxEdit41.SelectedIndex = 1;
            textEdit49.Text = "10";      //放入数量
            textEdit51.Text = "5";       //领用数量
            textEdit50.Text = "5";       //退还数量
            //测试
            for (int i = 1; i < 11; i++)
            {
                comboBoxEdit43.Properties.Items.Add(i.ToString());
            }
            comboBoxEdit43.SelectedIndex = 4;

            //K值
            foreach (Control control in xtraTabPage15.Controls)
            {
                if (control is TextEdit)
                {
                    ((TextEdit)control).Text = "0";
                }
            }
            //量程
            foreach (Control control in xtraTabPage12.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("1000");
                    ((ComboBoxEdit)control).Properties.Items.Add("2000");
                    ((ComboBoxEdit)control).Properties.Items.Add("5000");
                    ((ComboBoxEdit)control).Properties.Items.Add("10000");
                    ((ComboBoxEdit)control).SelectedIndex = 1;
                }
            }
            //精度
            foreach (Control control in xtraTabPage16.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("0.005");
                    ((ComboBoxEdit)control).SelectedIndex = 0;
                }
            }
            //校准重量
            foreach (Control control in xtraTabPage17.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("500");
                    ((ComboBoxEdit)control).Properties.Items.Add("1000");
                    ((ComboBoxEdit)control).Properties.Items.Add("2000");
                    ((ComboBoxEdit)control).Properties.Items.Add("5000");
                    ((ComboBoxEdit)control).SelectedIndex = 1;
                }
            }
            //安全过载
            foreach (Control control in xtraTabPage18.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("1.2");
                    ((ComboBoxEdit)control).SelectedIndex = 0;
                }
            }
            //最大过载
            foreach (Control control in xtraTabPage19.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("1.5");
                    ((ComboBoxEdit)control).SelectedIndex = 0;
                }
            }


            //设置秤盘对应传感器
            checkedListBoxControl3.Items[0].CheckState = CheckState.Checked;
            checkedListBoxControl3.Items[1].CheckState = CheckState.Checked;
            checkedListBoxControl3.Items[2].CheckState = CheckState.Checked;
            checkedListBoxControl3.Items[3].CheckState = CheckState.Checked;
            checkedListBoxControl4.Visible = false;
            checkedListBoxControl5.Visible = false;
            checkedListBoxControl6.Visible = false;
            checkedListBoxControl7.Visible = false;
            checkedListBoxControl8.Visible = false;
            labelControl34.Visible = false;
            labelControl35.Visible = false;
            labelControl36.Visible = false;
            labelControl37.Visible = false;
            labelControl38.Visible = false;
            //整秤盘校准重量
            foreach (Control control in xtraTabPage23.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("1000");
                    ((ComboBoxEdit)control).Properties.Items.Add("2000");
                    ((ComboBoxEdit)control).Properties.Items.Add("5000");
                    ((ComboBoxEdit)control).Properties.Items.Add("10000");
                    ((ComboBoxEdit)control).SelectedIndex = 2;
                }
            }
            //设置纠偏门限
            foreach (Control control in xtraTabPage24.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("1");
                    ((ComboBoxEdit)control).Properties.Items.Add("2");
                    ((ComboBoxEdit)control).Properties.Items.Add("5");
                    ((ComboBoxEdit)control).Properties.Items.Add("10");
                    ((ComboBoxEdit)control).SelectedIndex = 2;
                }
            }


            //原始上报重量
            comboBoxEdit67.Properties.Items.Add("0");
            comboBoxEdit67.Properties.Items.Add("1");
            comboBoxEdit67.Properties.Items.Add("2");
            comboBoxEdit67.Properties.Items.Add("3");
            comboBoxEdit67.SelectedIndex = 0;

            comboBoxEdit76.Properties.Items.Add("5");
            comboBoxEdit76.Properties.Items.Add("10");
            comboBoxEdit76.Properties.Items.Add("20");
            comboBoxEdit76.Properties.Items.Add("30");
            comboBoxEdit76.SelectedIndex = 0;



            //稳态上报门限
            foreach (Control control in xtraTabPage28.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("1");
                    ((ComboBoxEdit)control).Properties.Items.Add("2");
                    ((ComboBoxEdit)control).Properties.Items.Add("5");
                    ((ComboBoxEdit)control).Properties.Items.Add("10");
                    ((ComboBoxEdit)control).SelectedIndex = 2;
                }
            }
            //稳态参数
            textEdit71.Text = "6";
            textEdit72.Text = "0.05";
            textEdit73.Text = "1";

            //校准传感器
            foreach (Control control in xtraTabPage36.Controls)
            {
                if (control is CheckEdit)
                {
                    ((CheckEdit)control).Checked = true;
                }
            }
            checkEdit1.Checked = false;
            foreach (Control control in xtraTabPage36.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("0");
                    ((ComboBoxEdit)control).Properties.Items.Add("1");
                    ((ComboBoxEdit)control).Properties.Items.Add("2");
                    ((ComboBoxEdit)control).Properties.Items.Add("3");
                    ((ComboBoxEdit)control).Properties.Items.Add("4");
                    ((ComboBoxEdit)control).Properties.Items.Add("5");
                    ((ComboBoxEdit)control).Properties.Items.Add("6");
                    ((ComboBoxEdit)control).SelectedIndex = 0;
                }
            }
            comboBoxEdit66.Properties.Items.Clear();
            comboBoxEdit66.Properties.Items.Add("500");
            comboBoxEdit66.Properties.Items.Add("1000");
            comboBoxEdit66.Properties.Items.Add("2000");
            comboBoxEdit66.Properties.Items.Add("5000");
            comboBoxEdit66.SelectedIndex = 1;


            //货物
            comboBoxEdit42.Properties.Items.Add("取出");
            comboBoxEdit42.Properties.Items.Add("待取");
            comboBoxEdit42.Properties.Items.Add("取用");
            comboBoxEdit42.Properties.Items.Add("退还");
            comboBoxEdit42.SelectedIndex = 2;
            //单品重量
            textEdit2.Text = "100.0";
            textEdit17.Text = "100.0";
            textEdit55.Text = "100.0";
            textEdit57.Text = "100.0";
            textEdit59.Text = "100.0";
            textEdit61.Text = "100.0";


            //引导
            colorPickEdit1.Color = Color.FromArgb(0x00, 0xFF, 0x00);
            textEdit6.Text = "5";
            textEdit8.Text = "5";
            //清屏
            colorPickEdit2.Color = Color.FromArgb(0x00, 0x00, 0x00);
            textEdit18.Text = "0";
            textEdit19.Text = "0";
            textEdit20.Text = "400";
            textEdit21.Text = "240";
            //显示字符
            colorPickEdit3.Color = Color.FromArgb(0xff, 0xff, 0xff);
            colorPickEdit4.Color = Color.FromArgb(0x00, 0x00, 0x00);
            textEdit42.Text = "5";
            textEdit47.Text = "5";
            textEdit37.Text = "1.0";
            memoEdit6.Text = "123\r\n一二三";
            //清屏显示字符

            textEdit69.Text = "310";
            textEdit70.Text = "0";
            textEdit68.Text = "70";
            textEdit67.Text = "140";
            colorPickEdit13.Color = Color.FromArgb(0xff, 0xff, 0xff);
            colorPickEdit12.Color = Color.FromArgb(0x00, 0xff, 0x00);
            textEdit65.Text = "330";
            textEdit66.Text = "3";
            textEdit63.Text = "1.3";
            memoEdit8.Text = "易\r\n潮\r\n解";
            //显示字符每行独立设置
            colorPickEdit5.Color = Color.FromArgb(0x00, 0x00, 0x00);
            colorPickEdit6.Color = Color.FromArgb(0xff, 0xff, 0xff);
            textEdit26.Text = "1.0";
            comboBoxEdit55.Properties.Items.Add("左");
            comboBoxEdit55.Properties.Items.Add("中");
            comboBoxEdit55.Properties.Items.Add("右");
            comboBoxEdit55.SelectedIndex = 0;
            colorPickEdit7.Color = Color.FromArgb(0xff, 0xff, 0xff);
            textEdit56.Text = "2.5";
            comboBoxEdit56.Properties.Items.Add("左");
            comboBoxEdit56.Properties.Items.Add("中");
            comboBoxEdit56.Properties.Items.Add("右");
            comboBoxEdit56.SelectedIndex = 0;
            colorPickEdit8.Color = Color.FromArgb(0xff, 0xff, 0xff);
            textEdit58.Text = "1.5";
            comboBoxEdit57.Properties.Items.Add("左");
            comboBoxEdit57.Properties.Items.Add("中");
            comboBoxEdit57.Properties.Items.Add("右");
            comboBoxEdit57.SelectedIndex = 0;
            colorPickEdit9.Color = Color.FromArgb(0xff, 0xff, 0xff);
            textEdit60.Text = "1.0";
            comboBoxEdit58.Properties.Items.Add("左");
            comboBoxEdit58.Properties.Items.Add("中");
            comboBoxEdit58.Properties.Items.Add("右");
            comboBoxEdit58.SelectedIndex = 2;
            colorPickEdit10.Color = Color.FromArgb(0xff, 0xff, 0xff);
            textEdit62.Text = "1.0";
            comboBoxEdit59.Properties.Items.Add("左");
            comboBoxEdit59.Properties.Items.Add("中");
            comboBoxEdit59.Properties.Items.Add("右");
            comboBoxEdit59.SelectedIndex = 0;
            colorPickEdit11.Color = Color.FromArgb(0xff, 0xff, 0xff);
            textEdit64.Text = "1.0";
            comboBoxEdit60.Properties.Items.Add("左");
            comboBoxEdit60.Properties.Items.Add("中");
            comboBoxEdit60.Properties.Items.Add("右");
            comboBoxEdit60.SelectedIndex = 0;
            memoEdit7.Text = "123456789\r\n百花蛇舌草\r\n100g\r\n\f987654321\r\n藿香正气水\r\n35g";
            comboBoxEdit61.Properties.Items.Add("不存");
            comboBoxEdit61.Properties.Items.Add("保存");
            comboBoxEdit61.SelectedIndex = 0;
            comboBoxEdit68.Properties.Items.Add("新建");
            comboBoxEdit68.Properties.Items.Add("追加");
            comboBoxEdit68.SelectedIndex = 0;

            //数量显示样式
            comboBoxEdit62.Properties.Items.Add("一种货物");
            comboBoxEdit62.Properties.Items.Add("两种货物每种三行");
            comboBoxEdit62.Properties.Items.Add("三种货物每种两行");
            comboBoxEdit62.Properties.Items.Add("六种货物每种一行");
            comboBoxEdit62.SelectedIndex = 0;
            comboBoxEdit64.Properties.Items.Add("否");
            comboBoxEdit64.Properties.Items.Add("是");
            comboBoxEdit64.SelectedIndex = 0;
            comboBoxEdit65.Properties.Items.Add("否");
            comboBoxEdit65.Properties.Items.Add("是");
            comboBoxEdit65.SelectedIndex = 0;

            //LED
            foreach (Control control in xtraTabPage39.Controls)
            {
                if (control is ComboBoxEdit)
                {
                    ((ComboBoxEdit)control).Properties.Items.Add("黑");
                    ((ComboBoxEdit)control).Properties.Items.Add("蓝");
                    ((ComboBoxEdit)control).Properties.Items.Add("红");
                    ((ComboBoxEdit)control).Properties.Items.Add("紫");
                    ((ComboBoxEdit)control).Properties.Items.Add("绿");
                    ((ComboBoxEdit)control).Properties.Items.Add("青");
                    ((ComboBoxEdit)control).Properties.Items.Add("黄");
                    ((ComboBoxEdit)control).Properties.Items.Add("白");
                    ((ComboBoxEdit)control).SelectedIndex = 0;
                }
            }
            comboBoxEdit97.Properties.Items.Add("0");
            comboBoxEdit97.Properties.Items.Add("1");
            comboBoxEdit97.SelectedIndex = 0;

            comboBoxEdit98.Properties.Items.Add("0");
            comboBoxEdit98.Properties.Items.Add("10");
            comboBoxEdit98.Properties.Items.Add("50");
            comboBoxEdit98.Properties.Items.Add("100");
            comboBoxEdit98.SelectedIndex = 1;


            //单元列表
            textEdit53.Text = "0";
            textEdit54.Text = "10";
            for (int i = 0; i < 5; i++)
            {
                checkedListBoxControl10.Items.Add(new CheckedListBoxItem(String.Format("单元{0}", i), false));
            }
            textEdit74.Text = "11";

        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnitSendTimer.Stop();
            UnitSendTimer.Close();  
            ConnectionMode.Serial_Close();
            ConnectionMode.Tcp_Close();
            System.Environment.Exit(System.Environment.ExitCode);
        }
        #endregion

        #region Form_button
        private void Form_simpleButton_Click(object sender, EventArgs e)
        {
            SimpleButton button = sender as SimpleButton;
            switch (button.Text)
            {
                case "查找设置主板":
                    {
                        Form2SetIP form2 = new Form2SetIP();
                        form2.Show(this);
                        break;
                    }
                case "刷新":
                    {
                        ConnectionMode.Serial_Scan();
                        ConnectionMode.LocalIP_Scan();

                        comboBoxEdit1.Properties.Items.Clear();
                        foreach (String client in ConnectionMode.Get_COMList())
                        {
                            comboBoxEdit1.Properties.Items.Add(client);
                        } 
                        foreach (String client in ConnectionMode.Get_LocalIPList())
                        {
                            comboBoxEdit1.Properties.Items.Add(client);
                        }
                        comboBoxEdit1.Properties.Sorted = true;
                        if (comboBoxEdit1.SelectedIndex == -1)
                            comboBoxEdit1.SelectedIndex = 0;
                        break;
                    }
                case "开始":
                    {

                        if (comboBoxEdit1.SelectedItem.ToString().Substring(0, 3) == "COM")
                        {
                            if (ConnectionMode.Serial_Open(comboBoxEdit1.SelectedItem.ToString()) == true)
                            {
                                ShowStatusBar(comboBoxEdit1.SelectedItem.ToString() + "打开成功");
                                simpleButton1.Text = "停止";
                            }
                        }
                        else
                        {
                            ConfigPortNumberSet(textEdit1.Text);
                            if (ConnectionMode.Tcp_Open(IPAddress.Parse(comboBoxEdit1.SelectedItem.ToString()), Convert.ToInt32(textEdit1.Text)) == true)
                            {
                                ShowStatusBar("Tcp打开成功");
                                ShowInfoBar("Tcp打开");
                                simpleButton1.Text = "停止";
                            }
                        }
                        break;
                    }
                case "停止":
                    {
                        simpleButton1.Text = "开始";
                        ConnectionMode.Serial_Close();
                        ConnectionMode.Tcp_Close();
                        comboBoxEdit2.Properties.Items.Clear();
                        comboBoxEdit2.SelectedIndex = -1;
                        ShowStatusBar("Tcp关闭");
                        ShowInfoBar("Tcp关闭");
                        break;
                    }
                case "清空":
                    {
                        memoEdit1.EditValue = string.Empty;
                        break;
                    }
                case "全选":
                    {
                        button.Text = "全不选";
                        for (int i = 0; i < checkedListBoxControl10.Items.Count; i++)
                        {
                            checkedListBoxControl10.SetItemChecked(i, true);
                        }
                        break;
                    }
                case "全不选":
                    {
                        button.Text = "全选";
                        for (int i = 0; i < checkedListBoxControl10.Items.Count; i++)
                        {
                            checkedListBoxControl10.SetItemChecked(i, false);
                        }
                        break;
                    }
                case "查找":
                    {
                        if (UnitState == true)
                        {
                            ShowInfoBar(String.Format("请等待...", button.Text));
                        }
                        else
                        {
                            simpleButton75.Text = "全选";
                            checkedListBoxControl10.Items.Clear();
                            ScanRetry = 0;
                            ScanNumber = UInt16.Parse(textEdit53.Text);
                            UnitList.Clear();
                            ScanState = true;
                        }
                        break;
                    }

                case "添加":
                    {
                        bool Itis = false;
                        CheckedListBoxItem anditem = new CheckedListBoxItem(String.Format("单元{0}", UInt16.Parse(textEdit74.Text)), false);
                        string y = anditem.ToString();

                        
                        for (int i = 0; i < checkedListBoxControl10.Items.Count; i++)
                        {
                            string x = checkedListBoxControl10.Items[i].Value.ToString();
                            if (string.Compare(x,y ) == 0)
                            {
                                Itis = true;
                                break;
                            }
                        }
                        if(Itis == false)
                        { 
                            checkedListBoxControl10.Items.Add(anditem);
                            labelControl166.Text = "合计：" + checkedListBoxControl10.Items.Count.ToString();
                        }
                        
                        break;
                    }
            }

        }

        private void Unit_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
        {
            UnitList.Clear();
            for (int i = 0; i < checkedListBoxControl10.Items.Count; i++)
            {
                if (checkedListBoxControl10.GetItemChecked(i))
                {
                    string unitindex = checkedListBoxControl10.Items[i].Value.ToString().Substring(2);
                    UnitList.Add(UInt16.Parse(unitindex));
                }
            }
        }

        public void UnitSendTimeroutEvent(object source, System.Timers.ElapsedEventArgs e)
        {           
            try
            {
                Invoke(new MethodInvoker(delegate
                {
                    if (ScanState == true)
                    {
                        if (ScanRecOk == true)
                        {
                            checkedListBoxControl10.Items.Add(new CheckedListBoxItem(String.Format("单元{0}", ScanNumber), false));
                            ScanRecOk = false;
                            ScanRetry = 1;
                            ScanNumber++;
                        }
                        else
                        {
                            if (ScanRetry++ > 1)
                            {
                                ScanRetry = 1;
                                ScanNumber++;
                            }
                        }
                        if (ScanNumber > UInt16.Parse(textEdit54.Text))
                        {
                            ScanState = false;
                            labelControl166.Text = "合计：" + checkedListBoxControl10.Items.Count.ToString();
                        }
                        else
                        {
                            byte[] ByteTemp = TransmitCmd.Instance.QueryHeartbeat(ScanNumber);
                            if (ConnectionMode.Connection_DataSend(ByteTemp) == true)
                            {
                                ShowInfoBar(string.Format("{0}", BitConverter.ToString(ByteTemp).Replace('-', ' ')));
                                ShowInfoBar(String.Format("发送至 {0} 心跳命令", ScanNumber));
                            }
                            else
                            {
                                ShowInfoBar(String.Format("没有建立连接"));
                                ScanState = false;
                                labelControl166.Text = "合计：";
                            }
                        }
                    }
                    if(UnitState == true)
                    {
                        if (UnitListIndex >= UnitList.Count)
                        {
                            UnitState = false;
                        }
                        else
                        {
                            UnitBytes[2] = (byte)(UnitList[UnitListIndex]);
                            UnitBytes[3] = (byte)(UnitList[UnitListIndex] >> 8);
                            UnitListIndex++;
                            if (ConnectionMode.Connection_DataSend(UnitBytes) == true)
                            {
                                if (UnitSendButton != null)
                                {
                                    ShowInfoBar(string.Format("{0}", BitConverter.ToString(UnitBytes).Replace('-', ' ')));
                                    ShowInfoBar(String.Format("发送{0}命令", UnitSendButton.Text));
                                }
                            }
                            else
                            {
                                ShowInfoBar(String.Format("没有建立连接"));
                                UnitState = false;
                            }
                        }
                    }
                    if (CaliState == true && (CaliCycleCount++) > (CaliCycle / TimerCycle)) 
                    {
                        CaliCycleCount = 0;
                        if (UnitList.Count == 0)
                        {
                            ShowInfoBar(String.Format("没选择单元"));
                        }
                        else
                        {
                            byte[] data1 = TransmitCmd.Instance.WeightQuerySensorADValue(UnitList[0], 0xffff);
                            byte[] data2 = TransmitCmd.Instance.WeightQuerySensorWeight(UnitList[0], 0xffff);
                            byte[] data0 = data1.Concat(data2).ToArray();

                            if (ConnectionMode.Connection_DataSend(data0) == true)
                            {
                                ShowInfoBar(string.Format("{0}", BitConverter.ToString(data0).Replace('-', ' ')));
                                ShowInfoBar(String.Format("发送读AD、读重量命令"));
                            }
                            else
                            {
                                ShowInfoBar(String.Format("没有建立连接"));
                                CaliState = false;
                            }
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + ex.InnerException + ex.StackTrace, "UnitSendTimeroutEvent");
            }
        }
        private void ShowStatusBar(string text)
        {
            labelControl4.Text = text;
        }
        private void ShowInfoBar(string message)
        {
            //string timenew = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff") +"  ";
            //memoEdit1.EditValue += timenew ;
            //memoEdit1.EditValue += message + Environment.NewLine;
            memoEdit1.MaskBox.AppendText(message + Environment.NewLine); 
            if (memoEdit1.Text.Length > 10000)
            {
                memoEdit1.Select(0, 2000);
                string path = System.Windows.Forms.Application.StartupPath + "\\iValuableTestToolLog.txt";
                if (!File.Exists(path))
                {
                    FileInfo myfile = new FileInfo(path);
                    FileStream fs = myfile.Create();
                    fs.Close();
                }
                StreamWriter sw = File.AppendText(path);
                sw.Write(memoEdit1.SelectedText);
                sw.Flush();
                sw.Close();
                memoEdit1.SelectedText = "";
            }
        }
        #endregion
        #region Config_file

        private string ConfigPortNumberRead()
        {
            string PortNumber = "";
            //获取Configuration对象
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(file);

            bool exist = false; 
            foreach (string key in config.AppSettings.Settings.AllKeys) 
            {
                if (key == "PortNumber")
                { 
                    exist = true; 
                } 
            } 
            if (exist) 
            { 
                //根据Key读取<add>元素的Value
                PortNumber = config.AppSettings.Settings["PortNumber"].Value;
            }
            else
            {
                PortNumber = PORT_NUMBER;
                //增加<add>元素
                config.AppSettings.Settings.Add("PortNumber", PortNumber);
                //一定要记得保存，写不带参数的config.Save()也可以
                config.Save();
                //刷新，否则程序读取的还是之前的值（可能已装入内存）
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
            return PortNumber;
            
        }
        private void ConfigPortNumberSet(string  portnum)
        {
            //获取Configuration对象
            string file = System.Windows.Forms.Application.ExecutablePath;
            Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(file);
            //根据Key读取<add>元素的Value
            string port = config.AppSettings.Settings["PortNumber"].Value;

            if(string.Equals(port,portnum) == false)
            {
                //写入<add>元素的Value
                config.AppSettings.Settings["PortNumber"].Value = portnum;
                //一定要记得保存，写不带参数的config.Save()也可以
                config.Save(ConfigurationSaveMode.Modified);
                //刷新，否则程序读取的还是之前的值（可能已装入内存）
                System.Configuration.ConfigurationManager.RefreshSection("appSettings");
            }
        }

        #endregion
        #region Tcp

        private void comboBoxEdit2_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConnectionMode.Connection_Tcp_ReceiveData(comboBoxEdit2.SelectedIndex);
            ShowStatusBar("连接成功:" + IPAddress.Parse(comboBoxEdit2.SelectedItem.ToString()));
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
                ShowInfoBar("连接成功:" + ((IPEndPoint)client_new.RemoteEndPoint).Address.ToString());

            }));
        }   
    #endregion


    #region DataProcessing

        private void Data_SendData(byte[] bytes, SimpleButton button)
        {
            if (ScanState == false)
            {
                if (UnitList.Count == 0)
                {
                    ShowInfoBar(String.Format("没选择单元", button.Text));
                }
                else if (UnitState == false)
                {
                    UnitBytes = bytes;
                    UnitListIndex = 0;
                    UnitSendButton = button;
                    UnitState = true;
                }
            }
            else
            {
                ShowInfoBar(String.Format("查询单元没完成，请等待...", button.Text));
            }
        }
        private void Tcp_Data_ReceiveCommand(byte[] data)
        {
            try
            {
                Invoke(new MethodInvoker(delegate
                {
                    InstructionStructure instruction = Serializer.Instance.Deserialize(data);
                    string UnitAddress = string.Format("{0}{1}{2}", "地址：", instruction.DestinationAddress.ToString(), "  ");
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
                                    case TransmitCmd.SystemCommand.QueryDeviceType:
                                        {
                                            string ret = string.Empty;
                                            byte type = instruction.CommandParameter[0];
                                            switch (type)
                                            {
                                                case 1: ret = "通用主控板"; break;
                                                case 2: ret = "货架单元板"; break;
                                                case 3: ret = "乐高柜抽屉单元板"; break;
                                                case 4: ret = "乐高柜秤盘单元板"; break;

                                                case 5: ret = "充电柜单元板"; break;
                                                case 6: ret = "调剂车单元板"; break;
                                                case 7: ret = "存取柜单元板"; break;
                                                case 8: ret = "预配发架转运车单元板"; break;

                                                case 10: ret = "绑定台单元板"; break;
                                                case 11: ret = "饮片柜单元板"; break;
                                                case 12: ret = "贵重饮片柜抽屉单元板"; break;
                                                case 13: ret = "贵重饮片柜秤盘单元板"; break;

                                                case 20: ret = "踏力板单元板"; break;

                                                case 30: ret = "单串口显示板"; break;
                                                case 31: ret = "双串口显示板"; break;
                                                case 32: ret = "CAN口串口显示板"; break;

                                                case 40: ret = "毒麻柜单元板"; break;
                                                case 50: ret = "MOTEK称重连续输出单元板"; break;

                                                case 60: ret = "主控板407"; break;
                                                case 61: ret = "主控板427"; break;

                                                default: ret = "本测试程序未定义"; break;
                                            }
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询设备类型"));
                                            ShowInfoBar(string.Format("{0}", ret));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.QueryMaterialNumber:
                                        {
                                            System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询电路板物料号"));
                                            ShowInfoBar(string.Format("{0}", System.Text.Encoding.ASCII.GetString(instruction.CommandParameter).TrimEnd('\0')));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.QueryCircuitBoardVersion:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询电路板版本号"));
                                            ShowInfoBar(string.Format("V{0}.{1}.{2}", instruction.CommandParameter[0], instruction.CommandParameter[1], instruction.CommandParameter[2]));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.QuerySoftwareVersion:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询软件版本号"));
                                            ShowInfoBar(string.Format("V{0}.{1}.{2}", instruction.CommandParameter[0], instruction.CommandParameter[1], instruction.CommandParameter[2]));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.QueryProtocolVersion:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询通讯协议版本号"));
                                            ShowInfoBar(string.Format("V{0}.{1}.{2}", instruction.CommandParameter[0], instruction.CommandParameter[1], instruction.CommandParameter[2]));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.QuerySNNumber:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询电路板SN号"));
                                            ShowInfoBar(string.Format("{0}", BitConverter.ToUInt32(instruction.CommandParameter, 0)));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.QueryHeartbeat:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询设备状态（心跳）"));
                                            if (ScanState == true && instruction.DestinationAddress != 0)
                                            {
                                                ScanRecOk = true;
                                            }
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.SetEraseSetting:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}    {2}", UnitAddress, "擦除系统设置", (instruction.CommandParameter[0] == 0) ? "成功" : "卡移出"));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.SetModeSwitch:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "开启调试模式"));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.QueryNvramState:
                                        {
                                            string ret = string.Empty;
                                            byte type = instruction.CommandParameter[0];
                                            switch (type)
                                            {
                                                case 0: ret = "读取失败 "; break;
                                                case 1: ret = "FM25L04B "; break;
                                                case 2: ret = "FM25L16B "; break;
                                            }
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询铁电芯片状态："));
                                            ShowInfoBar(string.Format("{0}", ret));
                                            break;
                                        }
                                        
                                    case TransmitCmd.SystemCommand.QuerySettingVersion:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询系统设置版本号"));
                                            //ShowInfoBar(string.Format("{0:x}{1:x}{2:x}{3:x}", BitConverter.ToChar(instruction.CommandParameter, 0), 
                                            //    BitConverter.ToChar(instruction.CommandParameter, 0), BitConverter.ToChar(instruction.CommandParameter, 0),
                                            //    BitConverter.ToChar(instruction.CommandParameter, 0)));

                                            ShowInfoBar(string.Format("{0}", BitConverter.ToString(instruction.CommandParameter)));
                                            break;
                                        }
                                    case TransmitCmd.SystemCommand.QueryID:
                                    case TransmitCmd.SystemCommand.ReportID:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}    {2}", UnitAddress, "拨码开关（十进制）   ", BitConverter.ToUInt16(instruction.CommandParameter, 0)));
                                            ShowInfoBar(string.Format("{0}{1}    {2:X}", UnitAddress, "拨码开关（十六进制）", BitConverter.ToUInt16(instruction.CommandParameter, 0)));
                                            ShowInfoBar(string.Format("{0}{1}    {2}", UnitAddress, "拨码开关（二进制）   ", Convert.ToString(BitConverter.ToUInt16(instruction.CommandParameter, 0), 2)));
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
                        case TransmitCmd.CommandTypes.Upgrade:
                            {
                                TransmitCmd.UpgradeCommand Cmd2 = (TransmitCmd.UpgradeCommand)(instruction.CommandType & 0x00FF);
                                switch (Cmd2)
                                {
                                    case TransmitCmd.UpgradeCommand.UpgradeToBoot:
                                        {

                                            break;
                                        }
                                    case TransmitCmd.UpgradeCommand.UpgradeToNormal:
                                        {

                                            break;
                                        }
                                    case TransmitCmd.UpgradeCommand.UpgradeReadID:
                                        {

                                            break;
                                        }
                                    case TransmitCmd.UpgradeCommand.UpgradeCheckSector:
                                        {

                                            break;
                                        }
                                    case TransmitCmd.UpgradeCommand.UpgradeEraseSector:
                                        {

                                            break;
                                        }
                                    case TransmitCmd.UpgradeCommand.UpgradeErase:
                                        {

                                            break;
                                        }
                                    case TransmitCmd.UpgradeCommand.UpgradeWritePage:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}    {2}", UnitAddress, "设置每页内容", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));

                                            SimpleButton but = new SimpleButton();
                                            but.Text = "固件升级";
                                            Data_SendUpgradeCommand(but, null);
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
                        case TransmitCmd.CommandTypes.RFID:
                            {
                                TransmitCmd.RfidCommand Cmd2 = (TransmitCmd.RfidCommand)(instruction.CommandType & 0x00FF);
                                switch (Cmd2)
                                {
                                    case TransmitCmd.RfidCommand.RfidQueryBand:
                                        {
                                            string ret = string.Empty;
                                            byte par = instruction.CommandParameter[0];
                                            switch (par)
                                            {
                                                case 1: ret = "125kHz"; break;
                                                case 2: ret = "134.2KHz"; break;
                                                case 3: ret = "13.56MHz"; break;
                                                case 4: ret = "27.12MHz"; break;
                                                case 5: ret = "433MHz"; break;
                                                case 6: ret = "840~845MH"; break;
                                                case 7: ret = "920~925MHz"; break;
                                                case 8: ret = "2.45GHz"; break;
                                                case 9: ret = "5.8GHz"; break;
                                                default: break;
                                            }
                                            ShowInfoBar(string.Format("{0}{1}{2}", UnitAddress, "查询频段:", ret));
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidQueryProtocol:
                                        {
                                            string ret = string.Empty;
                                            byte par = instruction.CommandParameter[0];
                                            switch (par)
                                            {
                                                case 1: ret = "ISO15693"; break;
                                                case 2: ret = "ISO18000-3"; break;
                                                case 3: ret = "ISO14443A/B"; break;
                                                case 4: ret = "NFC"; break;
                                                default: break;
                                            }
                                            ShowInfoBar(string.Format("{0}{1}{2}", UnitAddress, "查询协议:", ret));
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidQueryID:
                                        {
                                            CardUid = BitConverter.ToUInt64(instruction.CommandParameter, 0);
                                            string hex = string.Empty;
                                            for (int i = 0; i < instruction.CommandParameter.Length; i++)
                                            {
                                                string tmp = Convert.ToString(instruction.CommandParameter[i], 16);
                                                hex += " " + tmp.PadLeft(2, '0');
                                            }
                                            ShowInfoBar(string.Format("{0}{1}   {2}", UnitAddress, "查询卡号:", hex));
                                            if (CardWriteCharge == true)
                                            {
                                                Data_SendRfidCommand(simpleButton18, null);
                                                CardWriteCharge = false;
                                            }
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidReportIn:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}{2}", UnitAddress, "上报卡移入", BitConverter.ToString(instruction.CommandParameter)));
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidReportOut:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}{2}", UnitAddress, "上报卡移出", BitConverter.ToString(instruction.CommandParameter)));
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidQueryCapacity:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}是    {2}字节", UnitAddress, "查询RFID容量", BitConverter.ToUInt16(instruction.CommandParameter, 0)));
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidSetErase:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}    {2}", UnitAddress, "设置RFID擦除", (instruction.CommandParameter[0] == 0) ? "成功" : "卡移出"));
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidQueryData:
                                        {
                                            string ret = string.Empty;
                                            sbyte par = (sbyte)instruction.CommandParameter[2];
                                            switch (par)
                                            {
                                                case 0: ret = "成功"; break;
                                                case -1: ret = "卡移出"; break;
                                                case -2: ret = "长度超限"; break;
                                                case -3: ret = "读失败"; break;
                                                case -4: ret = "卡内长度为零"; break;
                                                case -5: ret = "起始地址大于卡内长度"; break;
                                                default: ret = "未知错误"; break;
                                            }
                                            ShowInfoBar(string.Format("{0}{1}       {2}{3}{4}{5}", UnitAddress, "查询内容", ret, Environment.NewLine, "起始地址:", instruction.CommandParameter[0].ToString()));
                                            if (par == 0)
                                            {
                                                string hex = string.Empty;
                                                for (int i = 3; i < instruction.CommandParameter.Length; i++)
                                                {
                                                    string tmp = Convert.ToString(instruction.CommandParameter[i], 16);
                                                    hex += " " + tmp.PadLeft(2, '0');
                                                }
                                                ShowInfoBar(string.Format("内容:{0}", hex));
                                            }
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidSetdata:
                                        {
                                            string ret = string.Empty;
                                            sbyte par = (sbyte)instruction.CommandParameter[2];
                                            switch (par)
                                            {
                                                case 0: ret = "成功"; break;
                                                case -1: ret = "卡移出"; break;
                                                case -2: ret = "长度超限"; break;
                                                case -3: ret = "写失败"; break;
                                                case -4: ret = "卡内长度为零"; break;
                                                case -5: ret = "起始地址大于卡内长度"; break;
                                                default: ret = "未知错误"; break;
                                            }
                                            ShowInfoBar(string.Format("{0}{1}       {2}", UnitAddress, "设置内容", ret));
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidSetReadCardCount:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}    {2}", UnitAddress, "设置读卡次数", (instruction.CommandParameter[0] == 0) ? "成功" : "卡移出"));
                                            break;
                                        }
                                    case TransmitCmd.RfidCommand.RfidGetReadCardCount:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}    {2}次，时长{3}毫秒", UnitAddress, "查询读卡次数", instruction.CommandParameter[0], instruction.CommandParameter[0] * 200));
                                            break;
                                        }
                                    default:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "RFID无该命令！"));
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
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置使能", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryEnable:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询使能"));

                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 传感器J{0}启用！", i + 1));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetRange:
                                        {

                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置量程", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryRange:
                                        {

                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询量程"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 传感器J{0}的量程值 = {1}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetSafeOverload:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置安全过载", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQuerySafeOverload:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询安全过载"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 传感器J{0}的安全过载值 = {1}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetMaxOverload:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置最大过载", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryMaxOverload:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询最大过载"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 传感器J{0}的最大过载值 = {1}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetPrecision:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置精度", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryPrecision:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询精度"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 传感器J{0}的精度值 = {1}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetSensorCalibrationWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置传感器校准重量", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQuerySensorCalibrationWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询传感器校准重量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 传感器J{0}的校准重量值 = {1}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSensorCalibration:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "校准传感器", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetSensorKValue:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置传感器K值", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQuerySensorKValue:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询K值"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    float k = BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float));
                                                    ShowInfoBar(String.Format(" 传感器J{0}的K值 = {1}", i + 1, k));

                                                    if (i == 0)
                                                        labelControl177.Text = k.ToString("F2");
                                                    else if (i == 1)
                                                        labelControl184.Text = k.ToString("F2");
                                                    else if (i == 2)
                                                        labelControl188.Text = k.ToString("F2");
                                                    else if (i == 3)
                                                        labelControl196.Text = k.ToString("F2");
                                                    else if (i == 4)
                                                        labelControl200.Text = k.ToString("F2");
                                                    else if (i == 5)
                                                        labelControl204.Text = k.ToString("F2");
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQuerySensorADValue:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询AD"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    int ADi = BitConverter.ToInt32(instruction.CommandParameter, 2 + (j++) * sizeof(Int32));
                                                    if (PanCalibration >= 0)
                                                    {
                                                        PanAd[PanCalibration, j - 1] = Convert.ToDouble(ADi);
                                                    }
                                                    else
                                                    {
                                                        ShowInfoBar(String.Format(" 传感器J{0}的AD = {1}", i + 1, ADi));
                                                    }
                                                    if (i == 0)
                                                        labelControl179.Text = ADi.ToString();
                                                    else if (i == 1)
                                                        labelControl182.Text = ADi.ToString();
                                                    else if (i == 2)
                                                        labelControl186.Text = ADi.ToString();
                                                    else if (i == 3)
                                                        labelControl194.Text = ADi.ToString();
                                                    else if (i == 4)
                                                        labelControl198.Text = ADi.ToString();
                                                    else if (i == 5)
                                                        labelControl202.Text = ADi.ToString();
                                                }
                                            }

                                            int SensorCount = checkedListBoxControl1.CheckedItemsCount;
                                            if (PanCalibration > 0)
                                                PanAd[PanCalibration, SensorCount] = float.Parse(comboBoxEdit34.SelectedItem.ToString());

                                            if (PanCalibration >= 0)
                                            {
                                                for (int i = 0; i < SensorCount + 1; i++)
                                                {
                                                    string tt = string.Format("第{0}次：", i);
                                                    for (int j = 0; j < SensorCount + 1; j++)
                                                    {
                                                        tt += string.Format("{0,-10:F2}  ", PanAd[i, j]);
                                                    }
                                                    ShowInfoBar(tt);
                                                }
                                            }
                                            if (TestWeight)
                                            {

                                                TestWeightCount += 0.1;
                                                Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, 0x3f), new SimpleButton());
                                                //
                                                labelControl107.Text = Math.Round(TestWeightCount * 10).ToString();
                                            }


                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSensorCleared:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "清零", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryZeroValue:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询零值"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    Int32 zero = BitConverter.ToInt32(instruction.CommandParameter, 2 + (j++) * sizeof(Int32));
                                                    ShowInfoBar(String.Format(" 传感器J{0}的零值 = {1}", i + 1, zero));

                                                    if (i == 0)
                                                        labelControl178.Text = zero.ToString();
                                                    else if (i == 1)
                                                        labelControl183.Text = zero.ToString();
                                                    else if (i == 2)
                                                        labelControl187.Text = zero.ToString();
                                                    else if (i == 3)
                                                        labelControl195.Text = zero.ToString();
                                                    else if (i == 4)
                                                        labelControl199.Text = zero.ToString();
                                                    else if (i == 5)
                                                        labelControl203.Text = zero.ToString();
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightPeeling:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "去皮", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryPeeling:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询皮值"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 传感器J{0}的皮值 = {1}", i + 1, BitConverter.ToInt32(instruction.CommandParameter, 2 + (j++) * sizeof(Int32))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQuerySensorWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询传感器重量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            float totle = 0;
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    float weight = BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float));
                                                    ShowInfoBar(String.Format(" 传感器J{0}的重量值 = {1:N2}", i + 1, weight));

                                                    if (i == 0)
                                                        labelControl180.Text = weight.ToString("F1");
                                                    else if (i == 1)
                                                        labelControl181.Text = weight.ToString("F1");
                                                    else if (i == 2)
                                                        labelControl185.Text = weight.ToString("F1");
                                                    else if (i == 3)
                                                        labelControl193.Text = weight.ToString("F1");
                                                    else if (i == 4)
                                                        labelControl197.Text = weight.ToString("F1");
                                                    else if (i == 5)
                                                        labelControl201.Text = weight.ToString("F1");
                                                    totle += weight;
                                                }
                                            }
                                            labelControl205.Text = "总重量："+totle.ToString("F1");
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryPanWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询秤盘重量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的重量值 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            if (TestWeight)
                                            {
                                                float[] PanSensor = new float[6] { 0, 0, 0, 0, 0, 0 };
                                                TestWeightCount += 0.1;
                                                //PanSensor[0] = 111.11f + (float)TestWeightCount;
                                                //PanSensor[1] = 222.22f + (float)TestWeightCount;
                                                //PanSensor[2] = 333.33f + (float)TestWeightCount;
                                                //PanSensor[3] = 444.44f + (float)TestWeightCount;
                                                //PanSensor[4] = 555.55f + (float)TestWeightCount;
                                                //PanSensor[5] = 666.66f + (float)TestWeightCount;
                                                Data_SendData(TransmitCmd.Instance.WeightQueryPanWeight(UnitListAddr, 0x3f), new SimpleButton());
                                                //
                                                labelControl107.Text = Math.Round(TestWeightCount * 10).ToString();
                                            }
                                            break;
                                        }

                                    case TransmitCmd.WeightCommand.WeightSetDriftCritical:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置纠偏门限", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryDriftCritical:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询纠偏门限"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的纠偏门限 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryDriftWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询纠偏后重量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的纠偏后重量 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetReportCritical:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置上报门限", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryReportCritical:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询上报门限"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的上报门限 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightReportDiffWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "上报重量差"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的重量差 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetSteatyPara:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置秤盘稳态参数", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightGetSteatyPara:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询秤盘稳态参数"));

                                            ShowInfoBar(String.Format(" 求稳数据量 = {0}", BitConverter.ToUInt16(instruction.CommandParameter, 0)));
                                            ShowInfoBar(String.Format(" 单品重量系数 = {0}", BitConverter.ToSingle(instruction.CommandParameter, 2)));
                                            ShowInfoBar(String.Format(" 求稳最大极差 = {0}", BitConverter.ToSingle(instruction.CommandParameter, 6)));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetPanAndSensor:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置秤盘与传感器对应关系", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryPanAndSensor:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询秤盘与传感器对应关系"));
                                            UInt16 Pindexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((Pindexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}包含：", i + 1));
                                                    UInt16 Sindexs = BitConverter.ToUInt16(instruction.CommandParameter, 2 + (j++) * sizeof(UInt16));
                                                    for (int k = 0; k < 16; ++k)
                                                    {
                                                        if ((Sindexs & (1 << k)) != 0)
                                                        {
                                                            ShowInfoBar(String.Format(" 传感器J{0}", k + 1));
                                                        }
                                                    }

                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetSensorAndPan:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置传感器与秤盘对应关系", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQuerySensorAndPan:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询传感器与秤盘对应关系"));
                                            UInt16 Sindexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((Sindexs & (1 << i)) != 0)
                                                {
                                                    byte Pnum = instruction.CommandParameter[2 + (j++)];
                                                    ShowInfoBar(String.Format(" 传感器{0}包含于秤盘{1}", i + 1, Pnum));
                                                    if (i == 0)
                                                        comboBoxEdit69.SelectedIndex = Pnum;
                                                    else if (i == 1)
                                                        comboBoxEdit70.SelectedIndex = Pnum;
                                                    else if (i == 2)
                                                        comboBoxEdit71.SelectedIndex = Pnum;
                                                    else if (i == 3)
                                                        comboBoxEdit73.SelectedIndex = Pnum;
                                                    else if (i == 4)
                                                        comboBoxEdit74.SelectedIndex = Pnum;
                                                    else if (i == 5)
                                                        comboBoxEdit75.SelectedIndex = Pnum;
                                                }
                                                else
                                                {
                                                    if (i == 0)
                                                        comboBoxEdit69.SelectedIndex = 0;
                                                    else if (i == 1)
                                                        comboBoxEdit70.SelectedIndex = 0;
                                                    else if (i == 2)
                                                        comboBoxEdit71.SelectedIndex = 0;
                                                    else if (i == 3)
                                                        comboBoxEdit73.SelectedIndex = 0;
                                                    else if (i == 4)
                                                        comboBoxEdit74.SelectedIndex = 0;
                                                    else if (i == 5)
                                                        comboBoxEdit75.SelectedIndex = 0;
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightQueryRawWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询原始重量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的原始重量 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightReportRawWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "上报原始重量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的原始重量 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightSetRawReportCycle:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置原始周期", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightGetRawReportCycle:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询原始周期"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的原始周期 = {1:N2}", i + 1, BitConverter.ToUInt16(instruction.CommandParameter, 2 + (j++) * sizeof(UInt16))));
                                                }
                                            }
                                            break;
                                        }

                                    case TransmitCmd.WeightCommand.WeightSetRawReportLevel:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置原始门限", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.WeightCommand.WeightGetRawReportLevel:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询原始门限"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的原始门限 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    default:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "称重无该命令！"));
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
                                            ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "锁状态上报", (state == 0) ? "锁开" : "锁闭"));
                                            break;
                                        }
                                    case TransmitCmd.DoorKockLampCommand.DoorOpenReport:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "门状态上报", (state == 0) ? "门开" : "门闭"));
                                            break;
                                        }
                                    case TransmitCmd.DoorKockLampCommand.LockOpen:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "开锁", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
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
                                            ShowInfoBar(string.Format("{0}{1}       {2}", UnitAddress, "查询锁状态", ret));
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
                                            ShowInfoBar(string.Format("{0}{1}       {2}", UnitAddress, "查询门状态", ret));
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
                                    case TransmitCmd.DisplayCommand.DisplayClean:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "清屏", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowChars:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "显示字符", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowCleanChars:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "清屏显字", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowCleanAndChars:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "清屏显示字符", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowCleanCharsStamp:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "清屏特殊标记", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowCleanCharsLineForm:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "设置格式", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowCleanCharsLineText:
                                        {
                                            Int16 Capacity = BitConverter.ToInt16(instruction.CommandParameter, 0);
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "设置内容", (Capacity == 0) ? "成功" : Capacity.ToString()));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowCleanCharsLineTextPage:
                                        {
                                            Int16 Capacity = BitConverter.ToInt16(instruction.CommandParameter, 0);
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "设置翻屏内容", (Capacity == 0) ? "成功" : Capacity.ToString()));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayGuideOpen:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "开引导", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayGuideClose:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}:  {2}", UnitAddress, "关引导", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowCleanCharsLineCapacity:
                                        {
                                            UInt32 Capacity = BitConverter.ToUInt32(instruction.CommandParameter, 0);
                                            ShowInfoBar(string.Format("{0}{1}： {2} 字节", UnitAddress, "字符存储容量", Capacity));
                                            break;
                                        }
                                    case TransmitCmd.DisplayCommand.DisplayShowCleanCharsLineCapacityUsed:
                                        {
                                            UInt32 Capacity = BitConverter.ToUInt32(instruction.CommandParameter, 0);
                                            ShowInfoBar(string.Format("{0}{1}： {2} 字节", UnitAddress, "字符占用容量", Capacity));
                                            break;
                                        }
                                    default: break;
                                }
                                break;
                            }
                        case TransmitCmd.CommandTypes.Goods:
                            {
                                TransmitCmd.GoodsCommand Cmd2 = (TransmitCmd.GoodsCommand)(instruction.CommandType & 0x00FF);
                                switch (Cmd2)
                                {
                                    case TransmitCmd.GoodsCommand.GoodsSetWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置单品重量", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.GoodsCommand.GoodsGetWeight:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询单品重量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}单品重量 = {1:N2}", i + 1, BitConverter.ToSingle(instruction.CommandParameter, 2 + (j++) * sizeof(float))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.GoodsCommand.GoodsGetSteadyQuantity:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询稳态数量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}稳态数量 = {1}", i + 1, BitConverter.ToInt16(instruction.CommandParameter, 2 + (j++) * sizeof(Int16))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.GoodsCommand.GoodsSetReportEnable:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}  {2}", UnitAddress, "设置上报稳态使能", (instruction.CommandParameter[0] == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.GoodsCommand.GoodsGetReportEnable:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询上报稳态使能"));

                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}上报稳态数量启用！", i + 1));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.GoodsCommand.GoodsReportQuantity:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "上报稳态数量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}的稳态数量 = {1}", i + 1, BitConverter.ToInt16(instruction.CommandParameter, 2 + (j++) * sizeof(Int16))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.GoodsCommand.GoodsGetDriftQuantity:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询纠偏数量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}纠偏数量 = {1}", i + 1, BitConverter.ToInt16(instruction.CommandParameter, 2 + (j++) * sizeof(Int16))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.GoodsCommand.GoodsGetRawQuantity:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询原始数量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}原始数量 = {1}", i + 1, BitConverter.ToInt16(instruction.CommandParameter, 2 + (j++) * sizeof(Int16))));
                                                }
                                            }
                                            break;
                                        }
                                    case TransmitCmd.GoodsCommand.GoodsGetOpenDoorQuantity:
                                        {
                                            ShowInfoBar(string.Format("{0}{1}", UnitAddress, "查询开门计重数量"));
                                            UInt16 indexs = BitConverter.ToUInt16(instruction.CommandParameter, 0);
                                            for (int i = 0, j = 0; i < 16; ++i)
                                            {
                                                if ((indexs & (1 << i)) != 0)
                                                {
                                                    ShowInfoBar(String.Format(" 秤盘{0}开门计重数量 = {1}", i + 1, BitConverter.ToInt16(instruction.CommandParameter, 2 + (j++) * sizeof(Int16))));
                                                }
                                            }
                                            break;
                                        }
                                    default: break;
                                }
                                break;
                            }
                        case TransmitCmd.CommandTypes.LED:
                            {
                                TransmitCmd.LEDCommand Cmd2 = (TransmitCmd.LEDCommand)(instruction.CommandType & 0x00FF);
                                switch (Cmd2)
                                {
                                    case TransmitCmd.LEDCommand.LEDGetStyle:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "查询LED样式", state));
                                            break;
                                        }
                                    case TransmitCmd.LEDCommand.LEDSetStyle:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "设置LED样式", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.LEDCommand.LEDSetColour:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "设置LED颜色", (state == 0) ? "成功" : "失败"));
                                            break;
                                        }
                                    case TransmitCmd.LEDCommand.LEDGetBright:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "查询LED亮度", state));
                                            break;
                                        }
                                    case TransmitCmd.LEDCommand.LEDSetBright:
                                        {
                                            byte state = instruction.CommandParameter[0];
                                            ShowInfoBar(string.Format("{0}{1}      {2}", UnitAddress, "设置LED亮度", (state == 0) ? "成功" : "失败"));
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
        private void Data_SendSystemCommand(object sender, EventArgs e)
        {
            SimpleButton button = sender as SimpleButton;
            switch (button.Text)
            {
                case "查询设备类型": { Data_SendData(TransmitCmd.Instance.QueryDeviceType(UnitListAddr), sender as SimpleButton); break; }
                case "查询电路板物料号": { Data_SendData(TransmitCmd.Instance.QueryMaterialNumber(UnitListAddr), sender as SimpleButton); break; }
                case "查询电路板版本号": { Data_SendData(TransmitCmd.Instance.QueryCircuitBoardVersion(UnitListAddr), sender as SimpleButton); break; }
                case "查询软件版本号": { Data_SendData(TransmitCmd.Instance.QuerySoftwareVersion(UnitListAddr), sender as SimpleButton); break; }
                case "查询通讯协议版本号": { Data_SendData(TransmitCmd.Instance.QueryProtocolVersion(UnitListAddr), sender as SimpleButton); break; }
                case "查询电路板序列号": { Data_SendData(TransmitCmd.Instance.QuerySNNumber(UnitListAddr), sender as SimpleButton); break; }
                case "查询设备状态（心跳）": { Data_SendData(TransmitCmd.Instance.QueryHeartbeat(UnitListAddr), sender as SimpleButton); break; }
                case "开启调试模式": { Data_SendData(TransmitCmd.Instance.SetModeSwitch(UnitListAddr, byte.Parse(comboBoxEdit63.SelectedIndex.ToString())), sender as SimpleButton); break; }
                case "擦除系统设置": 
                    {
                        var result = MessageBox.Show("单元的系统设置将全部擦除！" + Boot_Firmwarefile, "警告！", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.Yes)
                        {
                            Data_SendData(TransmitCmd.Instance.SetEraseSetting(UnitListAddr), sender as SimpleButton);
                        }                        
                        break;
                    }
                case "查询铁电芯片状态": { Data_SendData(TransmitCmd.Instance.QueryNvramState(UnitListAddr), sender as SimpleButton); break; }
                case "查询系统设置版本号": { Data_SendData(TransmitCmd.Instance.QuerySettingVersion(UnitListAddr), sender as SimpleButton); break; }
                case "查询拨码": { Data_SendData(TransmitCmd.Instance.QueryID(UnitListAddr), sender as SimpleButton); break; }
                default: break;
            }
        }
        private void Data_SendUpgradeCommand(object sender, EventArgs e)
        {
            SimpleButton button = sender as SimpleButton;
            switch (button.Text)
            {
                case "选择固件": 
                    {
                        OpenFileDialog fileDialog = new OpenFileDialog();
                        fileDialog.Multiselect = true;
                        fileDialog.Title = "请选择文件";
                        fileDialog.Filter = "二进制文件 | *.bin|  所有文件 | *.*";
                        if (fileDialog.ShowDialog() == DialogResult.OK)
                        {
                            Boot_Firmwarefile = fileDialog.FileName;
                            Boot_fs = new FileStream(Boot_Firmwarefile, FileMode.Open);
                            Boot_fs.Seek(0, SeekOrigin.Begin);
                            //MessageBox.Show("已选择文件:" + Boot_Firmwarefile, "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        labelControl129.Text = fileDialog.SafeFileName;
                        break; 
                    }
                case "进入升级模式": { Data_SendData(TransmitCmd.Instance.UpgradeToBoot(UnitListAddr), sender as SimpleButton); break; }
                case "进入正常模式": { Data_SendData(TransmitCmd.Instance.UpgradeToNormal(UnitListAddr), sender as SimpleButton); break; }
                case "读唯一标识": { Data_SendData(TransmitCmd.Instance.UpgradeReadID(UnitListAddr), sender as SimpleButton); break; }
                case "擦除全部区域": { Data_SendData(TransmitCmd.Instance.UpgradeErase(UnitListAddr), sender as SimpleButton); break; }
                case "固件升级": 
                    {
                        byte[] byData = new byte[0x80];
                        if(Boot_fs == null)
                        {
                            MessageBox.Show("未选择固件文件", "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                        int ret = Boot_fs.Read(byData, 0, 0x80);
                        if(ret > 0)
                        {
                            Data_SendData(TransmitCmd.Instance.UpgradeWritePage(UnitListAddr, Boot_PageNumber, byData), sender as SimpleButton); 
                            Boot_PageNumber++;
                        }
                        else
                        {
                            Boot_PageNumber = 0;
                            Boot_fs.Seek(0,SeekOrigin.Begin);
                            //Boot_fs.Close();
                            MessageBox.Show("固件升级完成", "选择文件提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }                        
                        break; 
                    }         
                default: break;
            }
        }
        private void Data_SendRfidCommand(object sender, EventArgs e)
        {
            SimpleButton button = sender as SimpleButton;          
            switch (button.Text)
            {
                case "查询频段": { Data_SendData(TransmitCmd.Instance.RfidQueryBand(UnitListAddr), sender as SimpleButton); break; }
                case "查询协议": { Data_SendData(TransmitCmd.Instance.RfidQueryProtocol(UnitListAddr), sender as SimpleButton); break; }
                case "查询卡号": { Data_SendData(TransmitCmd.Instance.RfidQueryID(UnitListAddr), sender as SimpleButton); break; }
                case "查询卡容量": { Data_SendData(TransmitCmd.Instance.RfidQueryCapacity(UnitListAddr), sender as SimpleButton); break; }
                case "擦除整卡": { Data_SendData(TransmitCmd.Instance.RfidSetErase(UnitListAddr), sender as SimpleButton); break; }
                case "查询内容": { Data_SendData(TransmitCmd.Instance.RfidQueryData(UnitListAddr,Convert.ToUInt16(textEdit3.Text),Convert.ToUInt16(textEdit4.Text)), sender as SimpleButton); break; }
                case "设置内容": { Data_SendData(TransmitCmd.Instance.RfidSetdata(UnitListAddr, Convert.ToUInt16(textEdit3.Text), HexStringToByteArray(memoEdit2.Text), HexStringToByteArray(memoEdit2.Text).Length), sender as SimpleButton); break; }
                case "查询读卡次数": { Data_SendData(TransmitCmd.Instance.RfidGetReadCardCount(UnitListAddr), sender as SimpleButton); break; }
                case "设置读卡次数": { Data_SendData(TransmitCmd.Instance.RfidSetReadCardCount(UnitListAddr, byte.Parse(comboBoxEdit43.SelectedItem.ToString())), sender as SimpleButton); break; }
                
                case "写中药房卡": 
                    {
                        TransmitCmd.ZYFStruct ZyfInfo = new TransmitCmd.ZYFStruct()
                        {
                            CardHead = System.Text.Encoding.Default.GetBytes("DIHM"),
                            Length = 12,
                            Checkout = 0,
                            Version = new byte[3] { 2, 2, 0 },
                        };
                        ZyfInfo.Type = System.Byte.Parse(textEdit5.Text);

                        ZyfInfo.ID = System.BitConverter.GetBytes(UInt64.Parse(textEdit7.Text));

                        const int ContentsLength = 92;
                        byte[] Contents = new byte[ContentsLength];
                        byte[] ContentsS = System.Text.Encoding.Unicode.GetBytes(memoEdit3.Text);
                        int len = ContentsS.Length;
                        if (len > ContentsLength)
                            len = ContentsLength;
                        Array.Copy(ContentsS, Contents, len);
                        ZyfInfo.Contents = Contents;

                        ZyfInfo.Length += (UInt16)ContentsS.Length;
                        byte[] ZyfData = StructToBytes(ZyfInfo);
                        Data_SendData(TransmitCmd.Instance.RfidSetdata(UnitListAddr, 0, ZyfData, ZyfInfo.Length + 8), sender as SimpleButton);
                        break;
                    }
                case "写充电柜卡":
                    {
                        if (CardUid == 0)
                        { 
                            Data_SendData(TransmitCmd.Instance.RfidQueryID(UnitListAddr), sender as SimpleButton);
                            CardWriteCharge = true;
                        }
                        else
                        {
                            TransmitCmd.ChargeStruct ChargeInfo = new TransmitCmd.ChargeStruct()
                            {
                                CardHead = System.Text.Encoding.Default.GetBytes("DIHC"),
                                Length = 12,
                                Checkout = 0,
                                Version = new byte[3] { 1, 1, 0 },
                                Reserve = 0
                            };
                            const int PWDLength = 8;
                            byte[] PasswordD = new byte[PWDLength];
                            byte[] PasswordS = System.Text.Encoding.Unicode.GetBytes(textEdit9.Text);
                            int len = PasswordS.Length;
                            if (len > PWDLength)
                                len = PWDLength;
                            Array.Copy(PasswordS, PasswordD, len);
                            UInt64 PWD = BitConverter.ToUInt64(PasswordD, 0);

                            byte rol = (byte)(CardUid & 0x0f);
                            UInt64 pin = CardUid ^ PWD;
                            ChargeInfo.Password = crol(pin, rol);
                            byte[] ChargeData = StructToBytes(ChargeInfo);
                            Data_SendData(TransmitCmd.Instance.RfidSetdata(UnitListAddr, 0, ChargeData, ChargeInfo.Length + 8), sender as SimpleButton);
                        }                        
                        break;
                    }
                case "写乐高柜卡":
                    {
                        TransmitCmd.LegoStruct LegoInfo = new TransmitCmd.LegoStruct()
                        {
                            CardHead = System.Text.Encoding.Default.GetBytes("DIHE"),
                            Weight = float.Parse(textEdit10.Text)
                        };
                        byte[] LegoData = StructToBytes(LegoInfo);
                        Data_SendData(TransmitCmd.Instance.RfidSetdata(UnitListAddr, 0, LegoData,8), sender as SimpleButton);
                        break;
                    }
                case "写毒麻柜卡":
                    {
                        TransmitCmd.NarcoticStruct NarcoticInfo = new TransmitCmd.NarcoticStruct()
                        {
                            CardHead = System.Text.Encoding.Default.GetBytes("DIHA"),
                            Length = 100,
                            Checkout = 0,
                            Version = new byte[3] { 1, 1, 0 },
                            Material = new TransmitCmd.NarcoticMaterialStruct[5]
                        };
                        NarcoticInfo.State = (byte)(comboBoxEdit35.SelectedIndex);
            
                        int Length = 8;
                        byte[] TargS = new byte[Length];
                        byte[] SourS = System.Text.Encoding.Unicode.GetBytes(textEdit11.Text);
                        int len = SourS.Length;
                        if (len > Length)
                            len = Length;
                        Array.Copy(SourS, TargS, len);
                        NarcoticInfo.RoomNub = TargS;

                        NarcoticInfo.BoxWeight = float.Parse(textEdit52.Text);

                        Length = 8;
                        TargS = new byte[Length];
                        SourS = System.Text.Encoding.Unicode.GetBytes(textEdit12.Text);
                        len = SourS.Length;
                        if (len > Length)
                            len = Length;
                        Array.Copy(SourS, TargS, len);
                        NarcoticInfo.InName = TargS;

                        Length = 8;
                        TargS = new byte[Length];
                        SourS = System.Text.Encoding.Unicode.GetBytes(textEdit13.Text);
                        len = SourS.Length;
                        if (len > Length)
                            len = Length;
                        Array.Copy(SourS, TargS, len);
                        NarcoticInfo.OutName = TargS;

                        Length = 8;
                        TargS = new byte[Length];
                        SourS = System.Text.Encoding.Unicode.GetBytes(textEdit14.Text);
                        len = SourS.Length;
                        if (len > Length)
                            len = Length;
                        Array.Copy(SourS, TargS, len);
                        NarcoticInfo.ReturnName = TargS;

                        NarcoticInfo.InTime = (uint)(dateEdit1.DateTime.ToUniversalTime() - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
                        NarcoticInfo.OutTime = (uint)(dateEdit2.DateTime.ToUniversalTime() - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
                        NarcoticInfo.ReturnTime = (uint)(dateEdit3.DateTime.ToUniversalTime() - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;

                        NarcoticInfo.DetailedNum = System.UInt32.Parse(textEdit15.Text);
                        NarcoticInfo.DetailedMerge = (byte)comboBoxEdit36.SelectedIndex;
                        NarcoticInfo.MaterialQty = System.UInt32.Parse(textEdit16.Text);

                        NarcoticInfo.Material[0].MaterialID = System.UInt32.Parse(textEdit27.Text);
                        NarcoticInfo.Material[0].Location = (byte)comboBoxEdit37.SelectedIndex;
                        NarcoticInfo.Material[0].InNumber = (byte)System.UInt32.Parse(textEdit23.Text);
                        NarcoticInfo.Material[0].OutNumber = (byte)System.UInt32.Parse(textEdit24.Text);
                        NarcoticInfo.Material[0].ReturnNumber = (byte)System.UInt32.Parse(textEdit25.Text);

                        NarcoticInfo.Material[1].MaterialID = System.UInt32.Parse(textEdit33.Text);
                        NarcoticInfo.Material[1].Location = (byte)comboBoxEdit38.SelectedIndex;
                        NarcoticInfo.Material[1].InNumber = (byte)System.UInt32.Parse(textEdit34.Text);
                        NarcoticInfo.Material[1].OutNumber = (byte)System.UInt32.Parse(textEdit35.Text);
                        NarcoticInfo.Material[1].ReturnNumber = (byte)System.UInt32.Parse(textEdit36.Text);

                        NarcoticInfo.Material[2].MaterialID = System.UInt32.Parse(textEdit27.Text);
                        NarcoticInfo.Material[2].Location = (byte)comboBoxEdit39.SelectedIndex;
                        NarcoticInfo.Material[2].InNumber = (byte)System.UInt32.Parse(textEdit23.Text);
                        NarcoticInfo.Material[2].OutNumber = (byte)System.UInt32.Parse(textEdit24.Text);
                        NarcoticInfo.Material[2].ReturnNumber = (byte)System.UInt32.Parse(textEdit25.Text);

                        NarcoticInfo.Material[3].MaterialID = System.UInt32.Parse(textEdit27.Text);
                        NarcoticInfo.Material[3].Location = (byte)comboBoxEdit40.SelectedIndex;
                        NarcoticInfo.Material[3].InNumber = (byte)System.UInt32.Parse(textEdit23.Text);
                        NarcoticInfo.Material[3].OutNumber = (byte)System.UInt32.Parse(textEdit24.Text);
                        NarcoticInfo.Material[3].ReturnNumber = (byte)System.UInt32.Parse(textEdit25.Text);

                        NarcoticInfo.Material[4].MaterialID = System.UInt32.Parse(textEdit27.Text);
                        NarcoticInfo.Material[4].Location = (byte)comboBoxEdit41.SelectedIndex;
                        NarcoticInfo.Material[4].InNumber = (byte)System.UInt32.Parse(textEdit23.Text);
                        NarcoticInfo.Material[4].OutNumber = (byte)System.UInt32.Parse(textEdit24.Text);
                        NarcoticInfo.Material[4].ReturnNumber = (byte)System.UInt32.Parse(textEdit25.Text);

                        byte[] NarcoticData = StructToBytes(NarcoticInfo);
                        Data_SendData(TransmitCmd.Instance.RfidSetdata(UnitListAddr, 0, NarcoticData, NarcoticInfo.Length + 8), sender as SimpleButton);
                        break;
                    }
                default: break;
            }

        }
        private void Data_SendWeightCommand(object sender, EventArgs e)
        {
            UInt16 SensorIndex = 0,PanIndex = 0; 
            SimpleButton button = sender as SimpleButton;
            foreach (CheckedListBoxItem item in checkedListBoxControl1.CheckedItems)
            {
                SensorIndex += UInt16.Parse(item.Value.ToString());
            }
            foreach (CheckedListBoxItem item in checkedListBoxControl2.CheckedItems)
            {
                PanIndex += UInt16.Parse(item.Value.ToString());
            }
            switch (button.Text)
            {
                case "查询使能": { Data_SendData(TransmitCmd.Instance.WeightQueryEnable(UnitListAddr), sender as SimpleButton); break; }
                case "设置使能": { Data_SendData(TransmitCmd.Instance.WeightSetEnable(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询皮值": { Data_SendData(TransmitCmd.Instance.WeightQueryPeeling(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "清零": { Data_SendData(TransmitCmd.Instance.WeightSensorCleared(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询零值": { Data_SendData(TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询校准重量": { Data_SendData(TransmitCmd.Instance.WeightQuerySensorCalibrationWeight(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询量程": { Data_SendData(TransmitCmd.Instance.WeightQueryRange(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询安全过载": { Data_SendData(TransmitCmd.Instance.WeightQuerySafeOverload(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询最大过载": { Data_SendData(TransmitCmd.Instance.WeightQueryMaxOverload(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "校准传感器": { Data_SendData(TransmitCmd.Instance.WeightSensorCalibration(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询精度": { Data_SendData(TransmitCmd.Instance.WeightQueryPrecision(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询K值": { Data_SendData(TransmitCmd.Instance.WeightQuerySensorKValue(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询AD值": { PanCalibration = -1; TestWeight = false; Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "查询传感器重量": { Data_SendData(TransmitCmd.Instance.WeightQuerySensorWeight(UnitListAddr, SensorIndex), sender as SimpleButton); break; }
                case "设置K值": 
                    { 
                        float[] k = new float[6] {float.Parse(textEdit22.Text),float.Parse(textEdit28.Text),float.Parse(textEdit29.Text),
                                                  float.Parse(textEdit30.Text),float.Parse(textEdit31.Text),float.Parse(textEdit32.Text),};
                        Data_SendData(TransmitCmd.Instance.WeightSetSensorKValue(UnitListAddr, SensorIndex,k), sender as SimpleButton); 
                        break;
                    }
                case "设置量程":
                    {
                        float[] range = new float[6] {float.Parse(comboBoxEdit32.SelectedItem.ToString()),float.Parse(comboBoxEdit31.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit30.SelectedItem.ToString()),float.Parse(comboBoxEdit29.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit28.SelectedItem.ToString()),float.Parse(comboBoxEdit27.SelectedItem.ToString()),};
                        Data_SendData(TransmitCmd.Instance.WeightSetRange(UnitListAddr, SensorIndex, range), sender as SimpleButton);
                        break;
                    }
                case "设置精度":
                    {
                        float[] precision = new float[6] {float.Parse(comboBoxEdit26.SelectedItem.ToString()),float.Parse(comboBoxEdit25.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit24.SelectedItem.ToString()),float.Parse(comboBoxEdit23.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit22.SelectedItem.ToString()),float.Parse(comboBoxEdit21.SelectedItem.ToString()),};
                        Data_SendData(TransmitCmd.Instance.WeightSetPrecision(UnitListAddr, SensorIndex, precision), sender as SimpleButton);
                        break;
                    }
                case "设置校准重量":
                    {
                        float[] CalibrationWeight = new float[6] {float.Parse(comboBoxEdit3.SelectedItem.ToString()),float.Parse(comboBoxEdit4.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit5.SelectedItem.ToString()),float.Parse(comboBoxEdit6.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit7.SelectedItem.ToString()),float.Parse(comboBoxEdit8.SelectedItem.ToString()),};
                        Data_SendData(TransmitCmd.Instance.WeightSetSensorCalibrationWeight(UnitListAddr, SensorIndex, CalibrationWeight), sender as SimpleButton);
                        break;
                    }
                case "设置安全过载":
                    {
                        float[] SafeOverload = new float[6] {float.Parse(comboBoxEdit14.SelectedItem.ToString()),float.Parse(comboBoxEdit13.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit12.SelectedItem.ToString()),float.Parse(comboBoxEdit11.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit10.SelectedItem.ToString()),float.Parse(comboBoxEdit9.SelectedItem.ToString()),};
                        Data_SendData(TransmitCmd.Instance.WeightSetSafeOverload(UnitListAddr, SensorIndex, SafeOverload), sender as SimpleButton);
                        break;
                    }
                case "设置最大过载":
                    {
                        float[] MaxOverload = new float[6] {float.Parse(comboBoxEdit20.SelectedItem.ToString()),float.Parse(comboBoxEdit19.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit18.SelectedItem.ToString()),float.Parse(comboBoxEdit17.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit16.SelectedItem.ToString()),float.Parse(comboBoxEdit15.SelectedItem.ToString()),};
                        Data_SendData(TransmitCmd.Instance.WeightSetMaxOverload(UnitListAddr, SensorIndex, MaxOverload), sender as SimpleButton);
                        break;
                    }


                case "去皮": { Data_SendData(TransmitCmd.Instance.WeightPeeling(UnitListAddr, PanIndex), sender as SimpleButton); break; }
                case "查询对应传感器": { Data_SendData(TransmitCmd.Instance.WeightQueryPanAndSensor(UnitListAddr, PanIndex), sender as SimpleButton); break; }
                case "查询稳态重量": 
                    {
                        TestWeight = false;
                        Data_SendData(TransmitCmd.Instance.WeightQueryPanWeight(UnitListAddr, PanIndex), sender as SimpleButton); 
                        break;
                    }
                case "查询原始重量":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQueryRawWeight(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }
                case "查询开门计重":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQueryOpenDoorWeight(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }
                case "增加秤盘":
                    {
                        if (labelControl34.Visible == false)
                        {
                            labelControl34.Visible = true;
                            checkedListBoxControl4.Visible = true;
                        }
                        else if (labelControl35.Visible == false)
                        {
                            labelControl35.Visible = true;
                            checkedListBoxControl5.Visible = true;
                        }
                        else if (labelControl36.Visible == false)
                        {
                            labelControl36.Visible = true;
                            checkedListBoxControl6.Visible = true;
                        }
                        else if (labelControl37.Visible == false)
                        {
                            labelControl37.Visible = true;
                            checkedListBoxControl7.Visible = true;
                        }
                        else if (labelControl38.Visible == false)
                        {
                            labelControl38.Visible = true;
                            checkedListBoxControl8.Visible = true;
                        }
                        break;
                    }
                case "减少秤盘":
                    {
                        if (labelControl38.Visible == true)
                        {
                            labelControl38.Visible = false;
                            checkedListBoxControl8.Visible = false;
                            foreach (CheckedListBoxItem item in checkedListBoxControl8.Items)
                            {
                                item.CheckState = CheckState.Unchecked;
                            }
                        }
                        else if (labelControl37.Visible == true)
                        {
                            labelControl37.Visible = false;
                            checkedListBoxControl7.Visible = false;
                            foreach (CheckedListBoxItem item in checkedListBoxControl7.Items)
                            {
                                item.CheckState = CheckState.Unchecked;
                            }
                        }
                        else if (labelControl36.Visible == true)
                        {
                            labelControl36.Visible = false;
                            checkedListBoxControl6.Visible = false;
                            foreach (CheckedListBoxItem item in checkedListBoxControl6.Items)
                            {
                                item.CheckState = CheckState.Unchecked;
                            }
                        }
                        else if (labelControl35.Visible == true)
                        {
                            labelControl35.Visible = false;
                            checkedListBoxControl5.Visible = false;
                            foreach (CheckedListBoxItem item in checkedListBoxControl5.Items)
                            {
                                item.CheckState = CheckState.Unchecked;
                            }
                        }
                        else if (labelControl34.Visible == true)
                        {
                            labelControl34.Visible = false;
                            checkedListBoxControl4.Visible = false;
                            foreach (CheckedListBoxItem item in checkedListBoxControl4.Items)
                            {
                                item.CheckState = CheckState.Unchecked;
                            }
                        }
                        break;
                    }
                case "设置对应传感器": 
                    {
                        UInt16 pIndex = 0, pIndexTemp = 0;
                        UInt16[] PanSensor = new UInt16[6] { 0, 0, 0, 0, 0, 0 };

                        foreach (Control control in xtraTabPage22.Controls)
                        {
                            if ((control is CheckedListBoxControl) && (((CheckedListBoxControl)control).Visible == true))
                            {
                                pIndex += (UInt16)(1 << pIndexTemp);
                                pIndexTemp++;
                            }
                        }
                        //秤盘1
                        foreach (CheckedListBoxItem item in checkedListBoxControl3.CheckedItems)
                        {
                            PanSensor[0] += UInt16.Parse(item.Value.ToString());
                        }
                        //秤盘2
                        foreach (CheckedListBoxItem item in checkedListBoxControl4.CheckedItems)
                        {
                            PanSensor[1] += UInt16.Parse(item.Value.ToString());
                        }
                        //秤盘3
                        foreach (CheckedListBoxItem item in checkedListBoxControl5.CheckedItems)
                        {
                            PanSensor[2] += UInt16.Parse(item.Value.ToString());
                        }
                        //秤盘4
                        foreach (CheckedListBoxItem item in checkedListBoxControl6.CheckedItems)
                        {
                            PanSensor[3] += UInt16.Parse(item.Value.ToString());
                        }
                        //秤盘5
                        foreach (CheckedListBoxItem item in checkedListBoxControl7.CheckedItems)
                        {
                            PanSensor[4] += UInt16.Parse(item.Value.ToString());
                        }
                        //秤盘6
                        foreach (CheckedListBoxItem item in checkedListBoxControl8.CheckedItems)
                        {
                            PanSensor[5] += UInt16.Parse(item.Value.ToString());
                        }

                        Data_SendData(TransmitCmd.Instance.WeightSetPanAndSensor(UnitListAddr, pIndex, PanSensor), sender as SimpleButton); 
                        break; 
                    }

                case "设置纠偏门限":
                    {
                        float[] DriftCritical = new float[6] {float.Parse(comboBoxEdit48.SelectedItem.ToString()),float.Parse(comboBoxEdit47.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit46.SelectedItem.ToString()),float.Parse(comboBoxEdit45.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit44.SelectedItem.ToString()),float.Parse(comboBoxEdit33.SelectedItem.ToString()),};
                        Data_SendData(TransmitCmd.Instance.WeightSetDriftCritical(UnitListAddr, PanIndex, DriftCritical), sender as SimpleButton);
                        break;
                    }
                case "查询纠偏门限":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQueryDriftCritical(UnitListAddr, PanIndex), sender as SimpleButton); 
                        break;
                    }
                case "查询纠偏重量":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQueryDriftWeight(UnitListAddr, PanIndex), sender as SimpleButton); 
                        break;
                    }
                case "设置原始周期":
                    {
                        UInt16 cyc = UInt16.Parse(comboBoxEdit67.SelectedItem.ToString());
                        UInt16 [] RawCyc = new UInt16 [6] { cyc, cyc, cyc, cyc, cyc, cyc };
                        Data_SendData(TransmitCmd.Instance.WeightSetRawReportCycle(UnitListAddr, PanIndex, RawCyc), sender as SimpleButton);
                        break;
                    }
                case "查询原始周期":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightGetRawReportCycle(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }
                case "设置原始门限":
                    {
                        float level = float.Parse(comboBoxEdit76.SelectedItem.ToString());
                        float[] Rawlevel = new float[6] { level, level, level, level, level, level };
                        Data_SendData(TransmitCmd.Instance.WeightSetRawReportLevel(UnitListAddr, PanIndex, Rawlevel), sender as SimpleButton);
                        break;
                    }
                case "查询原始门限":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightGetRawReportLevel(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }


                case "设置稳态门限":
                    {
                        float[] ReportCritical = new float[6] {float.Parse(comboBoxEdit54.SelectedItem.ToString()),float.Parse(comboBoxEdit53.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit52.SelectedItem.ToString()),float.Parse(comboBoxEdit51.SelectedItem.ToString()),
                                                float.Parse(comboBoxEdit50.SelectedItem.ToString()),float.Parse(comboBoxEdit49.SelectedItem.ToString()),};
                        Data_SendData(TransmitCmd.Instance.WeightSetReportCritical(UnitListAddr, PanIndex, ReportCritical), sender as SimpleButton);
                        break;
                    }
                case "查询稳态门限":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQueryReportCritical(UnitListAddr, PanIndex), sender as SimpleButton); 
                        break;
                    }
                case "设置稳态参数":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightSetSteatyPara(UnitListAddr, UInt16.Parse(textEdit71.Text),float.Parse(textEdit72.Text),float.Parse(textEdit73.Text)), sender as SimpleButton);
                        break;
                    }
                case "查询稳态参数":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightGetSteatyPara(UnitListAddr), sender as SimpleButton);
                        break;
                    }

                case "清零数据": 
                    {
                        TestWeight = false;
                        PanCalibration = 0;
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, SensorIndex), sender as SimpleButton); 
                        break; 
                    }
                case "第一次数据": 
                    { 
                        PanCalibration = 1;
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, SensorIndex), sender as SimpleButton); 
                        break; 
                    }
                case "第二次数据": 
                    { 
                        PanCalibration = 2;
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, SensorIndex), sender as SimpleButton); 
                        break; 
                    }
                case "第三次数据": 
                    { 
                        PanCalibration = 3;
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, SensorIndex), sender as SimpleButton); 
                        break; 
                    }
                case "第四次数据": 
                    {
                        PanCalibration = 4;
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, SensorIndex), sender as SimpleButton); 
                        break; 
                    }
                case "计算K值": 
                    { 
                        PanCalibration = 5;
                        int SensorCount = checkedListBoxControl1.CheckedItemsCount;

                        double[,] SensorAd = new double[SensorCount, SensorCount + 1];                        
                        for (int i = 0; i < SensorCount;i++ )
                        {
                            for (int j = 0; j < SensorCount + 1; j++)
                            {
                                SensorAd[i, j] = PanAd[i + 1, j] - PanAd[0, j];
                            }
                        }

                        double[] res = LieZhuXiaoYuan(SensorAd);
                        for (int i = 0; i < SensorCount; i++)
                        {
                            PanK[i] = 1/(float)res[i];
                            ShowInfoBar(string.Format(" 传感器J{0}的K值 = {1}", i, PanK[i]));
                        }
                        break;
                    }
                case "导入K值":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightSetSensorKValue(UnitListAddr, SensorIndex, PanK), sender as SimpleButton); 
                        break;
                    }
                //校准传感器------------------------------------------------------------------
               
                case "读秤盘":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorAndPan(UnitListAddr, 0xffff), sender as SimpleButton); 
                        break;
                    }
                case "设秤盘":
                    {
                        byte[] Paramter = new byte[] {  (byte)comboBoxEdit69.SelectedIndex,
                                                        (byte)comboBoxEdit70.SelectedIndex,
                                                        (byte)comboBoxEdit71.SelectedIndex,
                                                        (byte)comboBoxEdit73.SelectedIndex,
                                                        (byte)comboBoxEdit74.SelectedIndex,
                                                        (byte)comboBoxEdit75.SelectedIndex};
                        Data_SendData(TransmitCmd.Instance.WeightSetSensorAndPan(UnitListAddr, 0x3f,Paramter), sender as SimpleButton);
                        break;
                    }
                case "读K值":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorKValue(UnitListAddr, 0xffff), sender as SimpleButton);
                        break;
                    }
                case "读零值":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, 0xffff), sender as SimpleButton);
                        break;
                    }
                case "读AD":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, 0xffff), sender as SimpleButton);
                        break;
                    }
                case "读重量":
                    {
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorWeight(UnitListAddr, 0xffff), sender as SimpleButton);
                        break;
                    }
                case "全部清零":
                    {
                        byte[] data1 = TransmitCmd.Instance.WeightSensorCleared(UnitListAddr, 0xffff);
                        byte[] data2 = TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, 0xffff);
                        Data_SendData(data1.Concat(data2).ToArray(), sender as SimpleButton);
                        break;
                    }

                case "":
                    {
                        
                         switch (button.Tag.ToString())
                         {
                             case "c1": 
                                 {
                                     UInt16 index = 1 << 0;
                                     byte[] data1 = TransmitCmd.Instance.WeightSensorCleared(UnitListAddr, index);
                                     byte[] data2 = TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).ToArray(), sender as SimpleButton);
                                     break; 
                                 }
                             case "c2":
                                 {
                                     UInt16 index = 1 << 1;
                                     byte[] data1 = TransmitCmd.Instance.WeightSensorCleared(UnitListAddr, index);
                                     byte[] data2 = TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).ToArray(), sender as SimpleButton);
                                     break; 
                                 }
                             case "c3":
                                 {
                                     UInt16 index = 1 << 2;
                                     byte[] data1 = TransmitCmd.Instance.WeightSensorCleared(UnitListAddr, index);
                                     byte[] data2 = TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).ToArray(), sender as SimpleButton);
                                     break; 
                                 }
                             case "c4":
                                 {
                                     UInt16 index = 1 << 3;
                                     byte[] data1 = TransmitCmd.Instance.WeightSensorCleared(UnitListAddr, index);
                                     byte[] data2 = TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).ToArray(), sender as SimpleButton);
                                     break; 
                                 }
                             case "c5":
                                 {
                                     UInt16 index = 1 << 4;
                                     byte[] data1 = TransmitCmd.Instance.WeightSensorCleared(UnitListAddr, index);
                                     byte[] data2 = TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).ToArray(), sender as SimpleButton);
                                     break; 
                                 }
                             case "c6":
                                 {
                                     UInt16 index = 1 << 5;
                                     byte[] data1 = TransmitCmd.Instance.WeightSensorCleared(UnitListAddr, index);
                                     byte[] data2 = TransmitCmd.Instance.WeightQueryZeroValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).ToArray(), sender as SimpleButton);
                                     break; 
                                 }

                             case "j1": 
                                 { 
                                     UInt16 index = 1 << 0;
                                     byte[] data1 = TransmitCmd.Instance.WeightSetSensorCalibrationWeight(UnitListAddr, index, new float[1] { float.Parse(comboBoxEdit66.SelectedItem.ToString())});
                                     byte[] data2 = TransmitCmd.Instance.WeightSensorCalibration(UnitListAddr, index);
                                     byte[] data3 = TransmitCmd.Instance.WeightQuerySensorKValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).Concat(data3).ToArray(), sender as SimpleButton);
                                     break; 
                                 }
                             case "j2":
                                 {
                                     UInt16 index = 1 << 1;
                                     byte[] data1 = TransmitCmd.Instance.WeightSetSensorCalibrationWeight(UnitListAddr, index, new float[1] { float.Parse(comboBoxEdit66.SelectedItem.ToString()) });
                                     byte[] data2 = TransmitCmd.Instance.WeightSensorCalibration(UnitListAddr, index);
                                     byte[] data3 = TransmitCmd.Instance.WeightQuerySensorKValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).Concat(data3).ToArray(), sender as SimpleButton);
                                     break;
                                 }
                             case "j3":
                                 {
                                     UInt16 index = 1 << 2;
                                     byte[] data1 = TransmitCmd.Instance.WeightSetSensorCalibrationWeight(UnitListAddr, index, new float[1] { float.Parse(comboBoxEdit66.SelectedItem.ToString()) });
                                     byte[] data2 = TransmitCmd.Instance.WeightSensorCalibration(UnitListAddr, index);
                                     byte[] data3 = TransmitCmd.Instance.WeightQuerySensorKValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).Concat(data3).ToArray(), sender as SimpleButton);
                                     break;
                                 }
                             case "j4":
                                 {
                                     UInt16 index = 1 << 3;
                                     byte[] data1 = TransmitCmd.Instance.WeightSetSensorCalibrationWeight(UnitListAddr, index, new float[1] { float.Parse(comboBoxEdit66.SelectedItem.ToString()) });
                                     byte[] data2 = TransmitCmd.Instance.WeightSensorCalibration(UnitListAddr, index);
                                     byte[] data3 = TransmitCmd.Instance.WeightQuerySensorKValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).Concat(data3).ToArray(), sender as SimpleButton);
                                     break;
                                 }
                             case "j5":
                                 {
                                     UInt16 index = 1 << 4;
                                     byte[] data1 = TransmitCmd.Instance.WeightSetSensorCalibrationWeight(UnitListAddr, index, new float[1] { float.Parse(comboBoxEdit66.SelectedItem.ToString()) });
                                     byte[] data2 = TransmitCmd.Instance.WeightSensorCalibration(UnitListAddr, index);
                                     byte[] data3 = TransmitCmd.Instance.WeightQuerySensorKValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).Concat(data3).ToArray(), sender as SimpleButton);
                                     break;
                                 }
                             case "j6":
                                 {
                                     UInt16 index = 1 << 5;
                                     byte[] data1 = TransmitCmd.Instance.WeightSetSensorCalibrationWeight(UnitListAddr, index, new float[1] { float.Parse(comboBoxEdit66.SelectedItem.ToString()) });
                                     byte[] data2 = TransmitCmd.Instance.WeightSensorCalibration(UnitListAddr, index);
                                     byte[] data3 = TransmitCmd.Instance.WeightQuerySensorKValue(UnitListAddr, index);
                                     Data_SendData(data1.Concat(data2).Concat(data3).ToArray(), sender as SimpleButton);
                                     break;
                                 }
                         }
                        break;
                    }

                //校准传感器------------------------------------------------------------------
                case "查AD":
                    {
                        TestWeight = true;
                        TestWeightCount = 0;
                        Data_SendData(TransmitCmd.Instance.WeightQuerySensorADValue(UnitListAddr, 0x3f), sender as SimpleButton);
                        break;
                    }
                case "查重量":
                    {
                        TestWeight = true;
                        TestWeightCount = 0;
                        float[] PanSensor = new float[6];
                        Data_SendData(TransmitCmd.Instance.WeightQueryPanWeight(UnitListAddr, 0x3f), sender as SimpleButton);
                        break;
                    }
                case "停止":
                    {
                        TestWeight = false;
                        break;
                    }
                default:
                    {
                        MessageBox.Show(button.Text);
                        break;
                    }
            }
        }

        private void Data_SendDoorLockLedCommand(object sender, EventArgs e)
        {
            
            SimpleButton button = sender as SimpleButton;            
            switch (button.Text)
            {
                case "开锁":
                    {
                        Data_SendData(TransmitCmd.Instance.DllLockOpen(UnitListAddr), sender as SimpleButton);
                        Data_SendData(TransmitCmd.Instance.DisplayGuideOpen(UnitListAddr, colorPickEdit1.Color, Convert.ToUInt16(textEdit6.Text), Convert.ToByte(textEdit8.Text)), sender as SimpleButton);
                        break;
                    }
                case "查询锁状态":
                    {
                        Data_SendData(TransmitCmd.Instance.DllLockOpenGet(UnitListAddr), sender as SimpleButton);
                        break;
                    }
                case "查询门状态":
                    {
                        Data_SendData(TransmitCmd.Instance.DllDoorOpenGet(UnitListAddr), sender as SimpleButton);
                        break;
                    }
                default: break;

            }
        }
        private void Data_AutoRead_AD_CheckedChanged(object sender, EventArgs e)
        {
            if (checkEdit1.Checked == true)
            {
                CaliState = true;
            }
            else
            {
                CaliState = false;
            }
        }

        private void Data_SendGoodsCommand(object sender, EventArgs e)
        {
            SimpleButton button = sender as SimpleButton;

            UInt16 PanIndex = 0;
            foreach (CheckedListBoxItem item in checkedListBoxControl9.CheckedItems)
            {
                PanIndex += UInt16.Parse(item.Value.ToString());
            }
            switch (button.Text)
            {
               case "设置状态":
                    {
                        Data_SendData(TransmitCmd.Instance.GoodsSetState(UnitListAddr, Convert.ToByte(comboBoxEdit42.SelectedIndex)), sender as SimpleButton);
                        break;
                    }
               case "设置单品重量":
                    {
                        float[] weight = new float[6] {float.Parse(textEdit2.Text),float.Parse(textEdit17.Text),float.Parse(textEdit55.Text),
                                                  float.Parse(textEdit57.Text),float.Parse(textEdit59.Text),float.Parse(textEdit61.Text),};
                        Data_SendData(TransmitCmd.Instance.GoodsSetWeight(UnitListAddr, 0x3F, weight), sender as SimpleButton);
                        break;
                    }
               case "查询单品重量":
                    {
                        Data_SendData(TransmitCmd.Instance.GoodsGetWeight(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }
               case "设置稳态上报使能":
                    {
                        Data_SendData(TransmitCmd.Instance.GoodsSetReportEnable(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }
               case "查询稳态上报使能":
                    {
                        Data_SendData(TransmitCmd.Instance.GoodsGetReportEnable(UnitListAddr), sender as SimpleButton);
                        break;
                    }
               case "查询稳态数量":
                    {
                        Data_SendData(TransmitCmd.Instance.GoodsGetSteadyQuantity(UnitListAddr, PanIndex), sender as SimpleButton); 
                        break;
                    }
               case "查询纠偏数量":
                    {
                        Data_SendData(TransmitCmd.Instance.GoodsGetDriftQuantity(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }
               case "查询原始数量":
                    {
                        Data_SendData(TransmitCmd.Instance.GoodsGetRawQuantity(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }
               case "查询开门数量":
                    {
                        Data_SendData(TransmitCmd.Instance.GoodsGetOpenDoorQuantity(UnitListAddr, PanIndex), sender as SimpleButton);
                        break;
                    }
                default: break;

            }
        }
        private void Data_SendDisplayCommand(object sender, EventArgs e)
        {

            SimpleButton button = sender as SimpleButton;
            switch (button.Text)
            {
                case "打开屏幕":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayOpen(UnitListAddr, 1), sender as SimpleButton);
                        break;
                    }
                case "关闭屏幕":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayOpen(UnitListAddr, 0), sender as SimpleButton);
                        break;
                    }
                case "清屏":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayClean(UnitListAddr, Convert.ToUInt16(textEdit18.Text), Convert.ToUInt16(textEdit19.Text), Convert.ToUInt16(textEdit20.Text), Convert.ToUInt16(textEdit21.Text), colorPickEdit2.Color), sender as SimpleButton);
                        break;
                    }
                case "显示字符":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowChars(UnitListAddr, Convert.ToUInt16(textEdit42.Text), Convert.ToUInt16(textEdit47.Text), (float)Convert.ToDouble(textEdit37.Text), colorPickEdit3.Color, colorPickEdit4.Color, System.Text.Encoding.Unicode.GetBytes(memoEdit6.Text)), sender as SimpleButton);
                        break;
                    }
                case "清屏显字":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowCleanChars(UnitListAddr, Convert.ToUInt16(textEdit42.Text), Convert.ToUInt16(textEdit47.Text), (float)Convert.ToDouble(textEdit37.Text), colorPickEdit3.Color, colorPickEdit4.Color, System.Text.Encoding.Unicode.GetBytes(memoEdit6.Text)), sender as SimpleButton);
                        break;
                    }
                case "特殊标记存":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowStamp(UnitListAddr, Convert.ToUInt16(textEdit42.Text), Convert.ToUInt16(textEdit47.Text), (float)Convert.ToDouble(textEdit37.Text), colorPickEdit3.Color, colorPickEdit4.Color, System.Text.Encoding.Unicode.GetBytes(memoEdit6.Text)), sender as SimpleButton);
                        break;
                    }
                case "清屏显示字符":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowCleanAndChars(UnitListAddr, Convert.ToUInt16(textEdit69.Text), 
                            Convert.ToUInt16(textEdit70.Text), Convert.ToUInt16(textEdit68.Text), Convert.ToUInt16(textEdit67.Text),
                            Convert.ToUInt16(textEdit65.Text), Convert.ToUInt16(textEdit66.Text), (float)Convert.ToDouble(textEdit63.Text), 
                            colorPickEdit13.Color, colorPickEdit12.Color, System.Text.Encoding.Unicode.GetBytes(memoEdit8.Text)), sender as SimpleButton);
                        break;
                    }
                case "清屏特殊标记":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowCleanCharsStamp(UnitListAddr, Convert.ToUInt16(textEdit69.Text),
                            Convert.ToUInt16(textEdit70.Text), Convert.ToUInt16(textEdit68.Text), Convert.ToUInt16(textEdit67.Text),
                            Convert.ToUInt16(textEdit65.Text), Convert.ToUInt16(textEdit66.Text), (float)Convert.ToDouble(textEdit63.Text),
                            colorPickEdit13.Color, colorPickEdit12.Color, System.Text.Encoding.Unicode.GetBytes(memoEdit8.Text)), sender as SimpleButton);
                        break;
                    }
                case "开引导":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayGuideOpen(UnitListAddr, colorPickEdit1.Color, Convert.ToUInt16(textEdit6.Text), Convert.ToByte(textEdit8.Text)), sender as SimpleButton);
                        break;
                    }
                case "关引导":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayGuideClose(UnitListAddr), sender as SimpleButton);
                        break;
                    }
                case "查询存储容量":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowCleanCharsLineCapacity(UnitListAddr), sender as SimpleButton);
                        break;
                    }
                case "查询占用容量":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowCleanCharsLineCapacityUsed(UnitListAddr), sender as SimpleButton);
                        break;
                    }
                case "设置格式":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowCleanCharsLineForm(UnitListAddr, colorPickEdit5.Color,
                            colorPickEdit6.Color, float.Parse(textEdit26.Text), Convert.ToByte(comboBoxEdit55.SelectedIndex),
                            colorPickEdit7.Color, float.Parse(textEdit56.Text), Convert.ToByte(comboBoxEdit56.SelectedIndex),
                            colorPickEdit8.Color, float.Parse(textEdit58.Text), Convert.ToByte(comboBoxEdit57.SelectedIndex),
                            colorPickEdit9.Color, float.Parse(textEdit60.Text), Convert.ToByte(comboBoxEdit58.SelectedIndex),
                            colorPickEdit10.Color, float.Parse(textEdit62.Text), Convert.ToByte(comboBoxEdit59.SelectedIndex),
                            colorPickEdit11.Color, float.Parse(textEdit64.Text), Convert.ToByte(comboBoxEdit60.SelectedIndex)), sender as SimpleButton);
                        break;
                    }
                case "插入翻屏符":
                    {
                        memoEdit7.Text += "\f";
                        break;
                    }
                case "设置内容":
                    {
                        ShowInfoBar(string.Format("设置内容的字节数目：{0}字节", memoEdit7.Text.Count()*2));
                        if (comboBoxEdit68.SelectedIndex == 0)
                            Data_SendData(TransmitCmd.Instance.DisplayShowCleanCharsLineText(UnitListAddr, Convert.ToByte(comboBoxEdit61.SelectedIndex), System.Text.Encoding.Unicode.GetBytes(memoEdit7.Text)),sender as SimpleButton);
                        else
                            Data_SendData(TransmitCmd.Instance.DisplayShowCleanCharsLineTextPage(UnitListAddr, Convert.ToByte(comboBoxEdit61.SelectedIndex),Convert.ToByte(comboBoxEdit68.SelectedIndex), System.Text.Encoding.Unicode.GetBytes(memoEdit7.Text)), sender as SimpleButton);
                        break;
                    }
                case "自动关屏":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowSetAutoOffScreen(UnitListAddr, 1), sender as SimpleButton);
                        break;
                    }
                case "不自动关":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowSetAutoOffScreen(UnitListAddr, 0), sender as SimpleButton);
                        break;
                    }
                case "查自动关屏":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowGetAutoOffScreen(UnitListAddr, 0), sender as SimpleButton);
                        break;
                    }
                case "设置样式":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowSetWQStyle(UnitListAddr, Convert.ToByte(comboBoxEdit64.SelectedIndex),
                                      Convert.ToByte(comboBoxEdit65.SelectedIndex),Convert.ToByte(comboBoxEdit62.SelectedIndex)), sender as SimpleButton);
                        break;
                    }
                case "查询样式":
                    {
                        Data_SendData(TransmitCmd.Instance.DisplayShowGetWQStyle(UnitListAddr, 0), sender as SimpleButton);
                        break;
                    }
                default: break;

            }
        }
        private void Data_SendLedCommand(object sender, EventArgs e)
        {

            SimpleButton button = sender as SimpleButton;
            switch (button.Text)
            {
                case "查询LED样式":
                    {
                        Data_SendData(TransmitCmd.Instance.LEDGetStyle(UnitListAddr), sender as SimpleButton);
                        break;
                    }
                case "设置LED样式":
                    {
                        Data_SendData(TransmitCmd.Instance.LEDSetStyle(UnitListAddr, (byte)comboBoxEdit97.SelectedIndex), sender as SimpleButton);
                        break;
                    }
                case "点亮LED":
                    {
                        byte[] led = new byte[20] { (byte)comboBoxEdit92.SelectedIndex, 
                                                    (byte)comboBoxEdit93.SelectedIndex, 
                                                    (byte)comboBoxEdit94.SelectedIndex, 
                                                    (byte)comboBoxEdit95.SelectedIndex, 
                                                    (byte)comboBoxEdit96.SelectedIndex, 

                                                    (byte)comboBoxEdit82.SelectedIndex, 
                                                    (byte)comboBoxEdit83.SelectedIndex, 
                                                    (byte)comboBoxEdit84.SelectedIndex, 
                                                    (byte)comboBoxEdit85.SelectedIndex, 
                                                    (byte)comboBoxEdit86.SelectedIndex, 

                                                    (byte)comboBoxEdit91.SelectedIndex, 
                                                    (byte)comboBoxEdit90.SelectedIndex, 
                                                    (byte)comboBoxEdit89.SelectedIndex, 
                                                    (byte)comboBoxEdit88.SelectedIndex, 
                                                    (byte)comboBoxEdit87.SelectedIndex, 

                                                    (byte)comboBoxEdit77.SelectedIndex, 
                                                    (byte)comboBoxEdit78.SelectedIndex, 
                                                    (byte)comboBoxEdit79.SelectedIndex, 
                                                    (byte)comboBoxEdit80.SelectedIndex, 
                                                    (byte)comboBoxEdit81.SelectedIndex };
                        Data_SendData(TransmitCmd.Instance.LEDSetColour(UnitListAddr, 0xFFFFF, led), sender as SimpleButton);
                        break;
                    }

                case "查询LED亮度":
                    {
                        Data_SendData(TransmitCmd.Instance.LEDGetBright(UnitListAddr), sender as SimpleButton);
                        break;
                    }
                case "设置LED亮度":
                    {
                        Data_SendData(TransmitCmd.Instance.LEDSetBright(UnitListAddr, byte.Parse(comboBoxEdit98.SelectedItem.ToString())), sender as SimpleButton);
                        break;
                    }
            }
        }


    #endregion

    #region  form_pan

        private void panSensor_ItemCheck(object sender, DevExpress.XtraEditors.Controls.ItemCheckEventArgs e)
        {

            UInt16 SensorIndex = 0;
            UInt16[] SIndex = new UInt16[6] { 0, 0, 0, 0, 0, 0 };
            CheckedListBoxControl clbc = sender as CheckedListBoxControl;
            //
            foreach (CheckedListBoxItem item in checkedListBoxControl1.CheckedItems)
            {
                SensorIndex += UInt16.Parse(item.Value.ToString());
            }

            //秤盘1
            foreach (CheckedListBoxItem item in checkedListBoxControl3.CheckedItems)
            {
                SIndex[0] += UInt16.Parse(item.Value.ToString());
            }
            //秤盘2
            foreach (CheckedListBoxItem item in checkedListBoxControl4.CheckedItems)
            {
                SIndex[1] += UInt16.Parse(item.Value.ToString());
            }
            //秤盘3
            foreach (CheckedListBoxItem item in checkedListBoxControl5.CheckedItems)
            {
                SIndex[2] += UInt16.Parse(item.Value.ToString());
            }
            //秤盘4
            foreach (CheckedListBoxItem item in checkedListBoxControl6.CheckedItems)
            {
                SIndex[3] += UInt16.Parse(item.Value.ToString());
            }
            //秤盘5
            foreach (CheckedListBoxItem item in checkedListBoxControl7.CheckedItems)
            {
                SIndex[4] += UInt16.Parse(item.Value.ToString());
            }
            //秤盘6
            foreach (CheckedListBoxItem item in checkedListBoxControl8.CheckedItems)
            {
                SIndex[5] += UInt16.Parse(item.Value.ToString());
            }
            //

            foreach (Control control in xtraTabPage22.Controls)
            {
                if ((control is CheckedListBoxControl) && (control != clbc))
                {
                    for (int i = 0; i < 6; ++i)
                    {
                        if (((SIndex[0] & (1 << i)) == 0) &&
                            ((SIndex[1] & (1 << i)) == 0) &&
                            ((SIndex[2] & (1 << i)) == 0) &&
                            ((SIndex[3] & (1 << i)) == 0) &&
                            ((SIndex[4] & (1 << i)) == 0) &&
                            ((SIndex[5] & (1 << i)) == 0))
                        {
                            ((CheckedListBoxControl)control).Items[i].Enabled = true;
                        }
                        else
                        {
                            if (((CheckedListBoxControl)control).Items[i].CheckState == CheckState.Unchecked)
                            {
                                ((CheckedListBoxControl)control).Items[i].Enabled = false;
                            }  
                        }
                        control.Invalidate();
                    }
                }
            }

        }

    #endregion

    #region  public
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "");
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
            {
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            }
            return buffer;
        }
        public static byte[] StructToBytes(object structObj)
        {
            int size = Marshal.SizeOf(structObj);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structObj, buffer, false);
                byte[] bytes = new byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        public static object ByteToStruct(byte[] bytes, Type strcutType)
        {
            int size = Marshal.SizeOf(strcutType);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return Marshal.PtrToStructure(buffer, strcutType);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
        UInt64 crol(UInt64 d, int l)
        {
            UInt64 left = d << l;
            UInt64 right = d >> (64 - l);
            UInt64 temp = left | right;
            return temp;
        }
        UInt64 cror(UInt64 d, int l)
        {
            UInt64 right = d >> l;
            UInt64 left = d << (64 - l);
            UInt64 temp = left | right;
            return temp;
        }
         
        public double[] LieZhuXiaoYuan(double[,] a)
        {
            const double e = 0.00001F;
            int _rows = a.GetLength(0);
            int _cols = a.GetLength(1);
            double[] x = new double[_rows];


            for (int k = 0; k < _rows - 1; k++)
            {
                //选主元[这一列的绝对值最大值]  
                double ab_max = -1;
                int max_ik = 0;
                for (int i = k; i < _cols - 1; i++)
                {
                    if (Math.Abs(a[i, k]) > ab_max)
                    {
                        ab_max = Math.Abs(a[i, k]);
                        max_ik = i;
                    }
                }
                //交换行处理[先判断是否为0矩阵]  
                if (ab_max < e)
                {//0矩阵情况  
                    MessageBox.Show("0矩阵情况");
                    break;
                }
                else if (max_ik != k)
                {//是否是当前行，不是交换  
                    double temp;
                    for (int j = 0; j < _cols; j++)
                    {
                        temp = a[max_ik,j];
                        a[max_ik,j] = a[k,j];
                        a[k,j] = temp;
                    }
                }
                //消元计算  
                for (int i = k + 1; i < _rows; i++)
                {
                    double kk = a[i, k] / a[k, k];
                    for (int j = k; j < _cols; j++)
                    {
                        a[i,j] -= kk * a[k,j];
                    }
                }
                //输出中间计算过程 
                ShowInfoBar(string.Format("第{0}次消元结果如下：", k + 1));
                for (int i = 0; i < a.GetLength(0); i++)
                {
                    string tt = "";
                    for (int j = 0; j < a.GetLength(1); j++)
                    {
                        tt += string.Format("{0,-10:F2}  ", a[i, j]);
                    }
                    ShowInfoBar(tt);
                }
                if (k < _rows - 2)
                    continue;
                else
                {
                    if (Math.Abs(a[_rows - 1, _rows - 1]) < e)
                    {
                        MessageBox.Show("0矩阵情况");
                        break;
                    }
                    else
                    {//回代求解 
                        for (int i = _rows - 1; i >= 0; i--)
                        {
                            x[i] = a[i, _cols - 1];
                            for (int j = i + 1; j < _cols -1; j++)
                                x[i] -= a[i,j] * x[j];
                            x[i] /= a[i,i];
                        }
                    }
                }
            }
            //输出结果  
            ShowInfoBar("结果为：");
            for (int i = 0; i < _rows; i++)
            {
                ShowInfoBar(string.Format("x{0}=  {1}", i, x[i]));
            }
            return x;
        }

    #endregion

        

     

    }
}
