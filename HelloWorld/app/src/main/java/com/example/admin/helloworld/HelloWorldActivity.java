package com.example.admin.helloworld;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;

public class HelloWorldActivity extends AppCompatActivity {

    private static final String TAG = "HelloWorldActivity";
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.hello_world_layout);
        Log.v("HelloworldActivity","日志 Log.v");
        Log.d("HelloworldActivity","调试 Log.d");
        Log.i("HelloworldActivity","重要 Log.i");
        Log.w("HelloworldActivity","警告 Log.w");
        Log.e("HelloworldActivity","错误 Log.e");
        Log.d(TAG, "onCreate: ");
        Log.i(TAG, "onCreate: ");
    }


}
