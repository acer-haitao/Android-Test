package activitytest.example.com.sockettest;


import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import java.io.BufferedWriter;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStreamWriter;
import java.io.PrintWriter;
import java.net.InetAddress;
import java.net.Socket;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.Scanner;


public class MainActivity extends AppCompatActivity implements View.OnClickListener, Runnable {


    private EditText edt_ip, edt_port, edt_getMsg;//IP port 发送输入
    private Button bt_connect, bt_close, bt_send;//连接、断开、发送按钮
    private TextView text_show;//显示TextView

    private int port, connect_flag = 0;//连接成功标志位
    private String ip = "";//存储获取输入的IP

    private String time = "";// 获取系统当前时间
    private String localip;//获取本地IP

    private Socket socket = null;

    private PrintWriter pw;//send
    private String sendMsg;//获取发送的信息
    //private DataOutputStream write;

    private Scanner in;
    private String content = "";//用于接收数据
    private StringBuffer sb = null;//缓存接收的数据

    /**
     * 处理UI界面更新
     */
    public Handler handler = new Handler() {

        @Override
        public void handleMessage(Message msg) {
            if (msg.what == 0x123) {
                sb.append(content);
                text_show.setText(sb.toString());
            }
        }
    };

    /*onCreate一般用来加载布局用的*/
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        sb = new StringBuffer();
        init_findView();  //findviewByID
        text_show.setText("");
    }

    //处理点击事件
    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.connet:
                if (connect_flag == 0) {
                    new Thread() {
                        @Override
                        public void run() {
                            try {
                                get_ip_port(); // 处理输入ip和端口号
                                connectServer(ip, port); //开启线程开始连接服务器
                            } catch (IOException e) {
                                e.printStackTrace();
                            }
                        }
                    }.start();

                } else {
                    //bug
                    Toast.makeText(this, "TCP已连接成功", Toast.LENGTH_LONG).show();
                }
                break;
            case R.id.colse:
                //线程怎么关闭
                try {
                    close_socket();
                } catch (Exception e) {
			e.printStackTrace();
                }
                break;
            case R.id.sendButton:
                sendMsg();
                break;
            default:
                break;
        }
    }

    /**
     * 用来处理获取输入框里的内容
     */
    private void get_ip_port() {
        //获取端口号
        String str = edt_port.getText().toString();
        try {
            port = Integer.parseInt(str);
        } catch (Exception e) {
            e.printStackTrace();
        }
        //IP
        ip = edt_ip.getText().toString();
        Log.i("IP", ip);
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

        bt_connect.setOnClickListener(this);
        bt_close.setOnClickListener(this);
        bt_send.setOnClickListener(this);
    }

    /**
     * 获取系统时间
     */
    private void getTime() {
        SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd-HH:mm:ss");
        time = format.format(new Date());
        Log.e("msg", time);
    }

    /**
     * 获取本地IP地址
     */
    private void getIP() throws IOException {
        InetAddress address = InetAddress.getLocalHost();
        localip = address.getHostAddress();
    }

    /**
     * send发送消息
     */
    private void sendMsg() {
        sendMsg = edt_getMsg.getText().toString();
        try {
            pw.write("From:" + sendMsg);
            pw.flush();
            //write.writeUTF(sendMsg);
            //write.flush();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    /**
     * recv接收服务器消息
     */

    @Override
    public void run() {
        Log.i("Test","run");
        while (true) {
            if (socket != null && socket.isConnected()) {
                if (!socket.isInputShutdown()) {
                    try {
                        Log.i("Test","isInputShutDown");
                       if ((content = in.nextLine()) != null) {
                            Log.i("Test",content);
                            content += "\n";
                          // text_show.setText(content); //线程里不能更新UI
                            handler.sendEmptyMessage(0x123);
                        }
                        /*
                        if ((content = in.readLine()) != null) {
                            Log.i("Test",content);
                            content += "\n";
                          // text_show.setText(content);
                            handler.sendEmptyMessage(0x123);

                        }
                        */
                    } catch (Exception e) {
                        e.printStackTrace();
                    }
                }
            }
        }
    }

    /**
     * 关闭socket
     */
    private void close_socket() throws IOException {
        socket.close();
        connect_flag = 0;
        getTime();
        Toast.makeText(this, time + " TCP正在断开连接", Toast.LENGTH_SHORT).show();
        Log.d("connect", "TCP断开连接");
    }

    /**
     * 连接服务器
     *
     * @param ip
     * @param port
     * @throws IOException
     */
    private void connectServer(String ip, int port) throws IOException {
        //1 创建socket对象， 指定服务器IP +　Port
        try {
            socket = new Socket("192.168.4.234", 6800);
        } catch (Exception e) {
            Log.d("connect", time + "TCP连接失败");
            e.printStackTrace();
        }
        if (socket.isConnected()) {
            getTime();
            connect_flag = 1;
           // text_show.setText(time + "TCP连接成功");//此处更显界面socke异常停止

            /*
            Looper.prepare();
            Toast.makeText(this, time + "TCP连接成功", Toast.LENGTH_LONG).show();//线程里开对话框显示有问题
            Looper.loop();
            */
            Log.d("connect", time + "连接成功");

            pw = new PrintWriter(new BufferedWriter(new OutputStreamWriter(socket.getOutputStream())), true);
           // write = new DataOutputStream(socket.getOutputStream());

           // in = new BufferedReader(new InputStreamReader(socket.getInputStream(), "UTF-8"));
            InputStream instream = socket.getInputStream();
            in = new Scanner(instream);

            //4 获取服务器响应信息
            new Thread(MainActivity.this).start();//启动接收服务器消息的线程
        } else {
            text_show.setText(time + "TCP连接失败");
            Log.d("connect", time + "TCP连接失败");
        }
        //3 获取客户端IP地址
        getTime();
        getIP();
        //2 获取输出流，向服务器发送信息
        pw.write("From:" + time + " " + localip + " :TCP is Connected!");
        pw.flush();
       // write.writeUTF("From:" + time + " " + localip + " :TCP is Connected!");
      //  write.flush();
        //5 关闭输出流
        //socket.shutdownOutput();
        //socket.close();
    }
}

