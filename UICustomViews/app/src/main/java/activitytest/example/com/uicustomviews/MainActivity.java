package activitytest.example.com.uicustomviews;

import android.support.v7.app.ActionBar;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.widget.ArrayAdapter;
import android.widget.ListView;

public class MainActivity extends AppCompatActivity {

    //先准备数据 此数据可以从网上下载 或者从数据库提取
    private  String[] data = {"Apple","banna","afaf","fafas","abcdef","11111","2222","33333","44444", "5555", "66666", "7777", "88888"};

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        ActionBar actionBar = getSupportActionBar();
        if (actionBar != null)
        {
            actionBar.hide();
        }
        //数组中的数据无法直接传递给ListView 需要借助适配器完成
        //ArrayAdapter通过泛型来指定需要的数据，然后在构造函数中把要适配的数据传入
        //参数1;传入当前上下文
        // 参数2: ListView子项布局id
        // 参数3： 需要适配的数据
        ArrayAdapter<String> adapter = new ArrayAdapter<String>(MainActivity.this, android.R.layout.simple_list_item_1, data);
        ListView listView = (ListView) findViewById(R.id.listView);

        //该方法将构建好的适配器对象传递进去 这样ListView和数据之间建立关联
        listView.setAdapter(adapter);
    }
}
