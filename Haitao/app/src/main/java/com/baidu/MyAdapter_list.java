package com.baidu;

import java.util.ArrayList;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.TextView;

public class MyAdapter_list extends BaseAdapter {

	private int resource;
	private ArrayList<Info_list> list;
	private Context context;

	public MyAdapter_list(int resource, ArrayList<Info_list> list,
			Context context) {
		this.resource = resource;
		this.list = list;
		this.context = context;

	}

	@Override
	public int getCount() {
		// TODO Auto-generated method stub
		return list.size();
	}

	@Override
	public Object getItem(int position) {
		// TODO Auto-generated method stub
		return position;
	}

	@Override
	public long getItemId(int position) {
		// TODO Auto-generated method stub
		return position;
	}

	@Override
	public View getView(int position, View convertView, ViewGroup parent) {
		// TODO Auto-generated method stub
		View inflater = LayoutInflater.from(context).inflate(resource, null);
		Info_list info = list.get(position);

		int image = info.getImage();
		String name = info.getName();
		String phonenum = info.getPhonenum();
		String adress = info.getAdress();
		
		ImageView iv =(ImageView) inflater.findViewById(R.id.image);
		TextView tv = (TextView) inflater.findViewById(R.id.name);
		TextView pv = (TextView) inflater.findViewById(R.id.phonenum);
		TextView av = (TextView) inflater.findViewById(R.id.adress);
		
		iv.setImageResource(image);
		tv.setText("姓名：\n"+name);
		pv.setText("联系电话：\n" + phonenum);
		av.setText("家庭住址："+adress);
		
		
		return inflater;
	}

}
