package activitytest.example.com.uicustomviews;

import android.app.Activity;
import android.content.Context;
import android.support.v7.app.AppCompatActivity;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.Toast;


/**
 * Created by admin on 2017/2/22.
 */

public class TitleLayout extends LinearLayout{
    public TitleLayout(Context context, AttributeSet attrs)
    {
        super(context, attrs);
        LayoutInflater.from(context).inflate(R.layout.title,this);
        Button back = (Button) findViewById(R.id.back);
        Button edit = (Button) findViewById(R.id.edit);

        back.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                ((Activity)getContext()).finish();
            }
        });
        edit.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View v) {
                Toast.makeText(getContext(),"你点击了Edit按钮", Toast.LENGTH_LONG).show();
            }
        });
    }


}
