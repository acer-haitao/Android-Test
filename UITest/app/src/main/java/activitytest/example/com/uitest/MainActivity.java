package activitytest.example.com.uitest;


import android.app.ProgressDialog;
import android.content.DialogInterface;
import android.support.v7.app.AlertDialog;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.Toast;

public class MainActivity extends AppCompatActivity implements View.OnClickListener{

    private EditText editText;//点击按钮 接收输入的文字并且显示
    private ImageView imageView;
    private static int flag = 0;
    private ProgressBar progressBar;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        Button button = (Button) findViewById(R.id.button1);
        Button button1 = (Button) findViewById(R.id.button2);
        Button button2 = (Button) findViewById(R.id.button3);
        Button button3 = (Button) findViewById(R.id.button4);
        Button button4 = (Button) findViewById(R.id.button5);

        editText = (EditText) findViewById(R.id.EditText);

        imageView = (ImageView) findViewById(R.id.img1);

        progressBar = (ProgressBar) findViewById(R.id.progressBar);

        button.setOnClickListener(this);
        button1.setOnClickListener(this);
        button2.setOnClickListener(this);
        button3.setOnClickListener(this);
        button4.setOnClickListener(this);

    }

    @Override
    public void onClick(View v) {
        switch (v.getId())
        {
            case R.id.button1:
                //点击按钮获取输入内容并显示
                Log.d("Hello","Test");
                String inputText = editText.getText().toString();
                Toast.makeText(MainActivity.this, inputText, Toast.LENGTH_LONG).show();
                break;
            case R.id.button2:
                Log.d("Hello", "点击按钮切换图片");
                if(flag == 0)
                {
                    imageView.setImageResource(R.drawable.img2);
                    flag = 1;
                }
                else if(flag == 1)
                {
                    imageView.setImageResource(R.drawable.img1);
                    flag = 0;
                }
                break;
            case R.id.button3:
                if(progressBar.getVisibility() == View.GONE)
                {
                    progressBar.setVisibility(View.VISIBLE);
                    int progress = progressBar.getProgress();
                    progress += 10;
                    progressBar.setProgress(progress);
                }
                else
                {
                   progressBar.setVisibility(View.GONE);
                }
                break;
            case R.id.button4:
                AlertDialog.Builder dialog = new AlertDialog.Builder(MainActivity.this);
                dialog.setTitle("温馨提示：");
                dialog.setMessage("确认充值10000元");
                dialog.setCancelable(false);

                dialog.setPositiveButton("确认", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        Toast.makeText(MainActivity.this, "充值成功",Toast.LENGTH_LONG).show();
                    }
                });
                dialog.setNegativeButton("取消", new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        Toast.makeText(MainActivity.this, "取消成功", Toast.LENGTH_LONG).show();
                    }
                });
                dialog.show();
                break;
            case R.id.button5:
                ProgressDialog progressDialog = new ProgressDialog(MainActivity.this);
                progressDialog.setTitle("正在加载......");
                progressDialog.setMessage("Loading......");
                progressDialog.setCancelable(true);
                progressDialog.show();
                break;
            default:
                break;
        }
    }
}
