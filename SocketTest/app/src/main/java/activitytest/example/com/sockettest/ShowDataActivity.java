package activitytest.example.com.sockettest;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.webkit.WebView;
import android.webkit.WebViewClient;

public class ShowDataActivity extends AppCompatActivity {

    private WebView showdatawebView;//显示网页
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_show_data);

        //显示网页
        showdatawebView = (WebView) findViewById(R.id.webTest);
        showdatawebView.getSettings().setJavaScriptEnabled(true);
        showdatawebView.setWebViewClient(new WebViewClient());
        showdatawebView.loadUrl("http://yuhaitao.iok.la/env1.html");
    }
}
