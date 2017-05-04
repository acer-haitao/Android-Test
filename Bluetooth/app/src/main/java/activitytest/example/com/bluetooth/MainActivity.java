package activitytest.example.com.bluetooth;

import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

public class MainActivity extends AppCompatActivity implements View.OnClickListener{
    private BluetoothAdapter mBluetoothAdapter;
    private TextView textview1, textview2, textview3;
    private Button btn, btnauto;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        initFindView();
        mBluetoothAdapter = BluetoothAdapter.getDefaultAdapter();//得到蓝牙适配器

        /**
         * 动态注册蓝牙搜索广播接收者
         */
        IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
        registerReceiver(mReceiver,filter);
        IntentFilter filter1 = new IntentFilter(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);
        registerReceiver(mReceiver,filter1);
    }

    private void initFindView()
    {
        textview1 = (TextView)findViewById(R.id.text1);
        textview2 = (TextView)findViewById(R.id.text2);
        textview3 = (TextView)findViewById(R.id.text3);

        btn = (Button)findViewById(R.id.btn);
        btnauto = (Button)findViewById(R.id.btnauto);

        btn.setOnClickListener(this);
        btnauto.setOnClickListener(this);
    }


    @Override
    public void onClick(View v) {
        switch (v.getId())
        {
            case R.id.btn:
                if(!mBluetoothAdapter.isEnabled())//判断蓝牙是否打开
                {
                    mBluetoothAdapter.isEnabled();
                }
                mBluetoothAdapter.startDiscovery();//开始广播
                textview1.setText("正在搜索......");
                break;
            case R.id.btnauto:
                Intent intent = new Intent(MainActivity.this, MainAutoActivity.class);
                startActivity(intent);
                break;
        }
    }

    /**
     * 自定义的广播接收器对象必须要继承BroadcastReceiver,然后重写onReceive方法，
     * 处理接收的数据的代码就写在这个方法里面。
     */
    public BroadcastReceiver mReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();
            Log.e("Test",action);

            if (action.equals(BluetoothDevice.ACTION_FOUND)) {
                BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
                if (device.getBondState()== BluetoothDevice.BOND_BONDED)
                {
                    textview2.append("\n" + device.getName() + "==>" + device.getAddress() + "\n");

                }
                else if(device.getBondState() != BluetoothDevice.BOND_BONDED)
                {
                    textview3.append("\n" + device.getName() + "==>" + device.getAddress() + "\n");
                }
            } else if(action.equals(BluetoothAdapter.ACTION_DISCOVERY_FINISHED)){

                    textview1.append("搜索完成");
            }

        }
    };

    @Override
    protected void onDestroy() {
        super.onDestroy();
        unregisterReceiver(mReceiver);
        Log.e("Test","解除注册");
    }
}
