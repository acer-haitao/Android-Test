package activitytest.example.com.activitytest;

import android.content.Intent;

import android.os.Bundle;

import android.support.v7.app.AppCompatActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;

public class SecondActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.second_layout);

        //接收来自FirstActivity传送过来的数据
        //Intent intent = getIntent();
        //String data = intent.getStringExtra("extra_data");
        //Log.d("SecondActivity", data);

        //再点击事件中返回数据逻辑
        Button button2 = (Button) findViewById(R.id.button_2);
        button2.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent();
                intent.putExtra("hello", "Hello FirstActivity");
                Log.i("Test","setOnClickListener");
                setResult(RESULT_OK, intent);
                finish();
            }
        });
    }
}
