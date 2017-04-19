package activitytest.example.com.sockettest;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.webkit.WebView;
import android.webkit.WebViewClient;

public class ContrlActivity extends AppCompatActivity {

    private WebView webView;//显示网页
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_contrl);
        //显示网页
        webView = (WebView) findViewById(R.id.webconctrl);
        webView.getSettings().setJavaScriptEnabled(true);
        webView.setWebViewClient(new WebViewClient());
        webView.loadUrl("http://yuhaitao.iok.la/HT-Test.html");
    }
}
