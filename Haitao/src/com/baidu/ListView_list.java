package com.baidu;

import java.util.ArrayList;

import android.app.Activity;
import android.os.Bundle;
import android.widget.Adapter;
import android.widget.ListView;

public class ListView_list extends Activity {
	private int image[] = { R.drawable.ic_launcher, R.drawable.ic_launcher,R.drawable.ic_launcher,R.drawable.ic_launcher, R.drawable.ic_launcher,
			R.drawable.ic_launcher };
	private String name[] = { "习近平", "李克强", "温家宝" ,"马云","奥巴马","普京"};
	private String adress[] = {"北京海淀区西三旗北京明园大学", "北京海淀区西三旗北京明园大学", "北京海淀区西三旗北京明园大学", "北京海淀区西三旗北京明园大学","北京海淀区西三旗北京明园大学" ,"北京海淀区西三旗北京明园大学"};
	private String phonenum[] = { "123456789","123456789", "666666","123456789","8888888", "9999999" };
	private ArrayList<Info_list> list;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		// TODO Auto-generated method stub
		super.onCreate(savedInstanceState);
		setContentView(R.layout.myview_list);
		ListView lv = (ListView) findViewById(R.id.lv);// this

		list = new ArrayList<Info_list>();
		for (int i = 0; i < 6; i++) {
			list.add(new Info_list(image[i], name[i], adress[i], phonenum[i]));
		}
		MyAdapter_list adapter = new MyAdapter_list(
				R.layout.listview_item_list, list, ListView_list.this);
		lv.setAdapter(adapter);
	}
}
