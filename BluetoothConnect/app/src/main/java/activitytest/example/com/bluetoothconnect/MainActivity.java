package activitytest.example.com.bluetoothconnect;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.os.AsyncTask;
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

import java.io.InputStream;
import java.io.OutputStream;
import java.lang.reflect.Method;
import java.util.UUID;

public class MainActivity extends AppCompatActivity  implements View.OnClickListener {

    private Button btn21,btn22,btn23;
    private TextView txt21,txt22;
    private EditText editText21;

    //device var
    private BluetoothAdapter mBluetoothAdapter = null;
    private BluetoothSocket btSocket = null;
    private OutputStream outputStream = null;
    private InputStream inputStream = null;

    private static final UUID MY_UUID = UUID.fromString("00001101-0000-1000-8000-00805F9B34FB");
    private static  String address = "50:A7:2B:F4:4B:DA";//要连接蓝牙的MAC地址

    private ReceiveThread rThread = null;
    private String ReceivData = "";

    private MyHandler handler;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        initFindView();
        initBluetooth();
        handler = new MyHandler();


    }
    private void initFindView()
    {
        btn21 = (Button)findViewById(R.id.button21);//断断
        btn22 = (Button)findViewById(R.id.button22);//连接
        btn23 = (Button)findViewById(R.id.button23);//发送

        txt21 = (TextView)findViewById(R.id.textView21);//当前没有连接任何设备
        txt22 = (TextView)findViewById(R.id.textView22);//接收数据

        editText21 = (EditText)findViewById(R.id.editText21);//输入


        btn21.setOnClickListener(this);
        btn22.setOnClickListener(this);
        btn23.setOnClickListener(this);
    }

    @Override
    public void onClick(View v) {
        switch (v.getId())
        {
            case R.id.button21://断开
                if (btSocket != null)
                {
                    try {
                        btSocket.close();
                        btSocket = null;
                        if(rThread == null)
                        {
                            rThread.join();
                        }
                        txt21.setText("当前已断连接");
                    } catch (InterruptedException e)
                    {
                        e.printStackTrace();
                    }
                    catch (Exception e) {
                        e.printStackTrace();
                    }
                }
                break;
            case R.id.button22://连接
                if(!mBluetoothAdapter.isEnabled())//判断蓝牙是否打开
                {
                    mBluetoothAdapter.enable();
                }
                mBluetoothAdapter.startDiscovery();

                //创建连接
                new ConnectTask().execute(address);
                break;
            case R.id.button23://发送
                new SendTask().execute(editText21.getText().toString());
                break;
        }
    }

    public void initBluetooth()
    {
        mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();
        if(mBluetoothAdapter == null)
        {
            Toast.makeText(this, "您的手机不支持蓝牙!", Toast.LENGTH_LONG).show();
            finish();
            return;
        }
    }

    //发送消息线程
    class SendTask extends AsyncTask<String, String, String>
    {
        @Override
        protected void onPostExecute(String s) {
            super.onPostExecute(s);
            txt21.setText(s);
            editText21.setText("");
        }

        @Override
        protected String doInBackground(String... params) {
            if (btSocket == null)
            {
                return "还没创建连接";
            }
            if(params[0].length() > 0)//不是空白字符
            {
                byte[] msgbuff = params[0].getBytes();
                try {
                    outputStream.write(msgbuff);
                } catch (Exception e) {
                    Log.e("error", "ON RESUME: Exception during write.", e);
                    return "发送失败";
                }
            }
            return "发送成功";
        }
    }

    //接收消息线程
    class ReceiveThread extends Thread{

        String buffer = "";

        @Override
        public void run() {
            while(btSocket != null)
            {
                byte[] buff = new byte[1024];
                try {
                    inputStream = btSocket.getInputStream();
                    inputStream.read(buff);//读取数据存储在buff中
                    processBuff(buff,1024);
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        }

        private void processBuff(byte[] buff, int size)
        {
            int length = 0;
            for (int i = 0; i < size; i++) {
                if (buff[i] > '\0') {
                    length++;
                } else {
                    break;
                }
            }
            byte[] newbuff = new byte[length];//用于存储真正的数据
            for (int j = 0; j < length; j++) {
                newbuff[j] = buff[j];
            }
            ReceivData = ReceivData + new String(newbuff);
            Log.e("data",ReceivData);
            Message msg = Message.obtain();
            msg.what = 1;
            handler.sendMessage(msg);
        }
    }

    class MyHandler extends Handler {
        @Override
        public void handleMessage(Message msg) {
            switch (msg.what)
            {
                case 1:
                    txt22.setText(ReceivData);
                    break;
            }

        }
    }
    //连接蓝牙设备的异步操作
    class ConnectTask extends AsyncTask<String,String,String>
    {
        @Override
        protected String doInBackground(String... params) {

            /*连接失败*/
            /*
            Log.e("test", "Connect");
            BluetoothDevice device = mBluetoothAdapter.getRemoteDevice(params[0]);
            Log.e("error", "ON RESUME: BT connection established, data transfer link open.");
            try{

                btSocket  =(BluetoothSocket) device.getClass().getMethod("createRfcommSocket", new Class[] {int.class}).invoke(device,1);
                btSocket.connect();

            } catch (Exception e) {
                try {
                    btSocket.close();
                    return "Socket创建失败";
                } catch (Exception e2) {
                    Log .e("error","ON RESUME: Unable to close socket during connection failure", e2);
                    return "Socket 关闭失败";
                }
            }
            */
            BluetoothDevice device = mBluetoothAdapter.getRemoteDevice(params[0]);
            Method m ;
            try {
                m = device.getClass().getMethod("createRfcommSocket", new Class[] {int.class});

                btSocket = (BluetoothSocket) m.invoke(device, Integer.valueOf(1));
            } catch (Exception e) {
                e.printStackTrace();
            }

            //取消搜索
            mBluetoothAdapter.cancelDiscovery();
            try {
                outputStream = btSocket.getOutputStream();
            } catch (Exception e) {
                Log.e("error", "ON RESUME: Output stream creation failed.", e);
                return "Socket流创建失败";
            }
            return "蓝牙连接正常,Socket 创建成功";
        }

        @Override //主线程中运行可以更新界面
        protected void onPostExecute(String s) {
            rThread = new ReceiveThread();//连接成功可以监听
            rThread.start();
            txt21.setText(s);
            super.onPostExecute(s);
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        try {
            if(rThread == null)
            {
                btSocket.close();
                btSocket = null;
                rThread.join();
            }
            this.finish();
        } catch (InterruptedException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
