package activitytest.example.com.activitytest;

import android.content.Intent;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;

import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

public class FirstActivity extends AppCompatActivity {

    @Override //创建菜单
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.main, menu);//给当前活动创建菜单
        return true;//false 创建菜单无法显示
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId())
        {
            case R.id.add_item :
                Toast.makeText(this, "你点击了增加按钮", Toast.LENGTH_LONG).show();
                break;
            case R.id.remove_item :
                Toast.makeText(this, "你点击了删除按钮", Toast.LENGTH_LONG).show();
                break;
            default:
        }
        return true;
    }


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //加载布局
        setContentView(R.layout.first_layout);
        //使用Toast提醒
        Button button1 = (Button) findViewById(R.id.button_1);
        button1.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Log.i("data","Test-button1");
                //Toast.makeText(FirstActivity.this, "你点击了按钮！",Toast.LENGTH_SHORT).show();
                //参数1：上下文 参数2 消息内容 参数3 显示时长
                //销毁一个活动
                // finish();

                //显式Intent启动第二个活动
               //Intent intent = new Intent(FirstActivity.this, SecondActivity.class);
                //startActivity(intent);

                //隐式
                //Intent intent = new Intent("activity.example.com.activitytest.ACTION_START");
                //startActivity(intent);

                //更多隐式Intent用法
               // Intent intent = new Intent(Intent.ACTION_VIEW);
                //intent.setData(Uri.parse("http://www.baidu.com")); //打开浏览器
               // startActivity(intent);

                //拨打电话 指定其他协议
                //Intent intent = new Intent(Intent.ACTION_DIAL);
                //intent.setData(Uri.parse("tel:10086"));
                //startActivity(intent);

                //向下一个活动传送数据
                //String data = "Hello SecondActivity";
                //Intent intent = new Intent(FirstActivity.this, SecondActivity.class);
                //intent.putExtra("extra_data", data);
                //startActivity(intent);

                //返回数据给上一个活动
                Intent intent = new Intent(FirstActivity.this, SecondActivity.class);
                startActivityForResult(intent, 1);
            }
        });

    }
    //SecondActivity销毁之后会回上一个活动的OnActivityResult
    //重写此方法来得到返回数据
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {

        switch (requestCode)
        {
            case 1:
                if(requestCode == RESULT_OK)
                {
                    String returnData = data.getStringExtra("hello");
                    Log.d("FirstActivity", returnData);
                }
                break;
            default:
                Log.i("Test","onActivityResult");
        }
    }

}
