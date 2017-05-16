package activitytest.example.com.sockettest;


import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.Socket;
import java.net.SocketException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Scanner;

import static activitytest.example.com.sockettest.R.id.paswd;


public class MainActivity extends AppCompatActivity implements View.OnClickListener {


    private EditText edt_ip, edt_port, edt_getMsg;//IP port 发送输入控件
    private Button bt_connect, bt_close, bt_send, bt_show, showdata, ctlbtn,udpserv;//连接、断开、发送按钮、UDP
    private TextView text_show;//显示TextView
    private CheckBox paswd_check;



    private int port, connect_flag = 0;//连接成功标志位
    private String ip = "";//存储获取输入的IP

    private String time = "";// 获取系统当前时间
    private String localip;//获取本地IP

    private Socket socket = null;
    public DatagramSocket udpsocket = null;

    private PrintWriter pw;//send
    private String sendMsg, recvMsg;//获取发送的信息
    //private DataOutputStream write;

    private Scanner in;
    private StringBuffer sb = null;//缓存接收的数据

    private boolean running;//循环接收标志位
    private Handler myHandler;//刷新UI线程

    private static final String formatUTF = "GBK";//编码格式

    private SharedPreferences save_ip_port;//保存IP
    private String save_ip;
    private String save_port;
    private boolean save_flag;
    private SharedPreferences.Editor editor;



    /*onCreate一般用来加载布局用的*/
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        init_findView();  //findviewByID
        myHandler = new MyHandler();
        save_ip_port();


    }

    //处理点击事件
    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.connet:
                new StartThread().start();
                break;
            case R.id.colse:
                //线程怎么关闭
                running = false;
                try {
                    socket.close();
                    udpsocket.close();
                }
                catch (Exception e) {
                    e.printStackTrace();
                }
                break;
            case R.id.sendButton:
                sendMsg();
                break;
            case R.id.conctrl:
                Intent i = new Intent(MainActivity.this, ContrlActivity.class);
                startActivity(i);
                break;
            case R.id.showdata:
                Intent test = new Intent(MainActivity.this, ShowDataActivity.class);
                startActivity(test);
                break;
            case R.id.udpserv:
                new UdpServConnect().start();
                break;
            default:
                break;
        }
    }

    /**
     * 用来处理获取输入框里的内容
     */
    public void get_ip_port() {
        //获取端口号
        String str = edt_port.getText().toString();
        try {
            port = Integer.parseInt(str);
        } catch (Exception e) {
            e.printStackTrace();
        }
        //IP
        ip = edt_ip.getText().toString();
        Log.d("IP", ip);
    }

    /**
     * 初始化布局寻找id
     */
    private void init_findView() {
        edt_ip = (EditText) findViewById(R.id.ip);//获取IP输入框内容
        edt_port = (EditText) findViewById(R.id.port);//获取端口号
        edt_getMsg = (EditText) findViewById(R.id.inputMsg);//获取输入消息内容
        text_show = (TextView) findViewById(R.id.text_rev_show);//显示接收的信息

        bt_connect = (Button) findViewById(R.id.connet);//连接
        bt_close = (Button) findViewById(R.id.colse);//断开
        bt_send = (Button) findViewById(R.id.sendButton);//获取发送
        showdata = (Button)findViewById(R.id.showdata);//显示网页温度
        ctlbtn = (Button)findViewById(R.id.conctrl);//显示网页温度
        paswd_check = (CheckBox)findViewById(paswd);//保存密码
        udpserv = (Button)findViewById(R.id.udpserv);//udpserv

        bt_connect.setOnClickListener(this);
        bt_close.setOnClickListener(this);
        bt_send.setOnClickListener(this);
        showdata.setOnClickListener(this);
        ctlbtn.setOnClickListener(this);
        udpserv.setOnClickListener(this);
    }

    /**
     * 保存IP地址和端口号
     */
    private void save_ip_port()
    {
        save_ip_port = getSharedPreferences("init",MODE_PRIVATE);
        editor = save_ip_port.edit();
        save_ip = save_ip_port.getString("save_ip",null);
        save_port = save_ip_port.getString("save_port",null);
        save_flag = save_ip_port.getBoolean("save_flag",false);

        if (save_flag) {
            edt_ip.setText(save_ip);
            edt_port.setText(save_port);
            paswd_check.setChecked(true);
        }
    }

    /**
     * 获取系统时间
     */
    private void getTime() {
        SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        time = format.format(new Date());
        Log.d("msg", time);
    }

    /**
     * 获取本地IP地址
     */
    private void getIP() throws IOException {
        InetAddress address = InetAddress.getLocalHost();
        localip = address.getHostAddress();
    }

    /**
     * 发送信息
     */
    private void sendMsg()
    {
        sendMsg = edt_getMsg.getText().toString();//从输入框获取消息内容
        try {
            pw.write(sendMsg);
            pw.flush();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * 向服务器发送连接信息
     */
    private void CntToServer()
    {
        try {
            getIP();
            getTime();
            pw.write(time + "--From--" + localip + "-->TCP is connected!");
            pw.flush();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * UDPSERV服务端
     */
    public class UdpServConnect extends Thread
    {
        @Override
        public void run() {
            get_ip_port();//获取输入的IP、端口号
            try {
                //1创建服务器端
                udpsocket = new DatagramSocket(port);
                //2创建数据报 用于接收客户端数据
                byte data[] = new byte[1024];
                DatagramPacket packet = new DatagramPacket(data, data.length);
                //3接收客户端发送的数据
                while(true)
                {
                    udpsocket.receive(packet);
                    String udprevcMsg = new String(data, 0, packet.getLength());
                    //4更新到界面显示
                    Message udprecv = myHandler.obtainMessage();
                    udprecv.what = 3;
                    udprecv.obj = udprevcMsg;
                    myHandler.sendMessage(udprecv);
                    Log.d("udp","123455");
                }
            } catch (SocketException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }
    /**
     * 连接服务器
     * @throws IOException
     */
    private class StartThread extends Thread {

        @Override
        public void run() {
            try {
                get_ip_port();//获取输入的IP、端口号
                socket = new Socket(ip, port);//连接服务器
                if (socket.isConnected()) {
                    //打开输入流
                   // InputStream instream = socket.getInputStream();
                    in = new Scanner(socket.getInputStream(),formatUTF);

                    //打开输出流
                    pw = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream(),formatUTF)), true);

                    //开启接收线程
                    running = true;
                    new RecvThread(socket).start();

                    //发送连接成功的状态设置
                    Message msg0 = myHandler.obtainMessage();//实例化对象
                    msg0.what=0;
                    myHandler.sendMessage(msg0);

                    //向服务器发送连接信息
                    CntToServer();
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

    /**
     * 接收数据线程
     */
    private class RecvThread extends Thread {
        private InputStream inputStream;

        public RecvThread(Socket socket) throws IOException {
            inputStream = socket.getInputStream();
        }

        @Override
        public void run() {
            while (running) {
                try {
                    if ((recvMsg = in.nextLine()) != null) {
                        recvMsg += "\n";
                    }
                } catch (Exception e) {
                    running = false;//防止服务器断开连接导致程序异常

                    Message recvNullMsg = myHandler.obtainMessage();
                    recvNullMsg.what = 2;
                    myHandler.sendMessage(recvNullMsg);//发送信息通知客户端已关闭
                    e.printStackTrace();
                    //设置按钮状态
                    // setButtonFlag(true,bt_connect);//非UI线程不能操作UI会报错
                    break;
                }

                //把消息发送到主线程
                Message sendToMain = myHandler.obtainMessage();
                sendToMain.what = 1;
                sendToMain.obj = recvMsg;//将接收到的信息传递给sendMain
                myHandler.sendMessage(sendToMain);//发送给Handler更新

                try {
                    sleep(400);
                }catch (InterruptedException e)
                {
                    e.printStackTrace();
                }

            }

            //如果没有进入while循环
            Message recvNullMsg = myHandler.obtainMessage();
            recvNullMsg.what = 2;
            myHandler.sendMessage(recvNullMsg);//发送信息通知客户端已关闭
        }
    }

    class MyHandler extends Handler {
        @Override
        public void handleMessage(Message msg) {
           switch (msg.what)
           {
               case 0:
                   getTime();
                   toastShow(time + ":TCP连接成功");
                   text_show.setText(time +"-->TCP连接成功");
                   //设置按钮状态为不能点击
                   setButtonFlag(true,bt_connect);
                   break;
               case 1:
                   String str = (String) msg.obj;
                   text_show.setText(str);
                   break;
               case 2:
                   getTime();
                   toastShow(time+":已断开连接");
                   text_show.setText(time+":服务器已断开连接");
                   setButtonFlag(false,bt_connect);
                   break;
               case 3://udp
                   getTime();
                   String strudp = (String) msg.obj;
                   toastShow(time+":UDP服务器开启");
                   text_show.setText(time + ":" + strudp);
                   break;
               default:
                   break;
           }
        }
    }

    /**
     * 设置按钮的状态
     */
    private void setButtonFlag(boolean flag, Button bt_show) {
        bt_show.setEnabled(!flag);
    }

    /**
     * 提示连接状态
     * @param s
     */
    private void toastShow(String s) {
        Toast.makeText(this, s, Toast.LENGTH_SHORT).show();
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        //记住密码
        if (paswd_check.isChecked()) {
            editor.putString("save_ip",edt_ip.getText().toString());
            editor.putString("save_port",edt_port.getText().toString());
            editor.putBoolean("save_flag",true);
            editor.commit();
        }
        else
        {
            editor.clear();
            editor.commit();
        }

    }
}

