package com.dih.udplinechart;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.SocketException;
import java.util.ArrayList;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;

import lecho.lib.hellocharts.gesture.ContainerScrollType;
import lecho.lib.hellocharts.model.Axis;
import lecho.lib.hellocharts.model.Line;
import lecho.lib.hellocharts.model.LineChartData;
import lecho.lib.hellocharts.model.PointValue;
import lecho.lib.hellocharts.model.ValueShape;
import lecho.lib.hellocharts.model.Viewport;
import lecho.lib.hellocharts.view.LineChartView;

import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.app.Activity;
import android.graphics.Color;
import android.util.Log;
import android.widget.Toast;

/**
 * 实现udp通信，并且将获取到的数据绘制成实时显示的折线图
 * */

public class MainActivity extends Activity {
	
	private static final int udpPort = 9993;//监听的端口号
	private RecvThread recvThread;
	private DatagramSocket socket;//udp通信的socket
	private ArrayList<String> timeList = new ArrayList<String>();//存放时间的集合
	private ArrayList<String> dataList = new ArrayList<String>();//存放数据的集合
	
	private LineChartView lineChartView;//折线图的视图view
	private LineChartData lineChartData;//折线图的数据
	private List<Line> lineList;//线的集合
	private List<PointValue> pointValueList;//实时添加新的点的集合
	private List<PointValue> points;//折线图中点的集合
	private int position = 0;//
	private Axis axisY ;//y坐标轴
	private boolean isFinish = false;
	private Timer timer;
	

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		recvThread = new RecvThread();
		recvThread.start();
		initView();
		showLineChart();
	}
	
	//定义一个定时器，用来实时获取数据，并且绘制折线图
	private void showLineChart(){
		timer = new Timer();
		timer.schedule(new TimerTask() {
			
			@Override
			public void run() {
				// TODO Auto-generated method stub
				if(!isFinish){
					if(position <points.size()-1){
					//实时添加新的点
					pointValueList.add(points.get(position));
					float x = points.get(position).getX();//获取添加点的x坐标
					//根据新的点画出新的线
					Line line = new Line(pointValueList);
					line.setColor(Color.DKGRAY);//给线设置颜色
					line.setShape(ValueShape.CIRCLE);//设置节点的图形样式，DIAMOND菱形、SQUARE方形、CIRCLE圆形
					line.setCubic(false);//曲线是否平滑，true表示为曲线，false表示为折线
					lineList.add(line);
					lineChartData = initDatas(lineList);
					lineChartView.setLineChartData(lineChartData);//为图表设置数据，类型为lineChartData
					//根据节点的横坐标实时的变换坐标的视图范围
					Viewport port;
					if(x>50){
						port = initViewport(x-50, x);
					}else{
						port = initViewport(0, 50);
					}
					lineChartView.setMaximumViewport(port);
					lineChartView.setCurrentViewport(port);
					position++;
				}
					if(position >points.size()-1){
						isFinish = true;
					}
				}
			}
		}, 1000, 5000);
	}
	
	
	//初始化控件
	private void initView(){
		lineChartView = (LineChartView) findViewById(R.id.lineChart);
		lineList = new ArrayList<Line>();
		pointValueList = new ArrayList<PointValue>();
		//初始化坐标
		axisY = new Axis();
		axisY.setName("强度");//设置y轴的名称
		axisY.setTextColor(Color.parseColor("#FFFFFF"));//设置y轴的字体颜色
		
		lineChartData = initDatas(null);
		lineChartView.setLineChartData(lineChartData);
		
		Viewport port = initViewport(0, 50);
		lineChartView.setCurrentViewportWithAnimation(port);
		lineChartView.setInteractive(false);//设置图标是是否可以和用户互动，true为可以false为不可以
		lineChartView.setScrollEnabled(false);
		lineChartView.setValueTouchEnabled(false);
		lineChartView.setFocusableInTouchMode(false);
		lineChartView.setViewportCalculationEnabled(false);
		lineChartView.setContainerScrollEnabled(true, ContainerScrollType.HORIZONTAL);
		lineChartView.startDataAnimation();
		initData();
	}

	private LineChartData initDatas(List<Line> lines){
		LineChartData lineChartdata = new LineChartData(lines);
		lineChartdata.setAxisYLeft(axisY);
		return lineChartdata;
	}
	private Viewport initViewport(float left,float right){
		Viewport port = new Viewport();
		port.top = 20;
		port.bottom = 0;
		port.left = left;
		port.right = right;
		return port;
	}
	//初始化数据
	private void initData(){
		points = new ArrayList<PointValue>();
		points.addAll(getFirstChart());
	}
	//获取绘制折线图中添加点的集合
	private List<PointValue> getFirstChart(){
		List<PointValue> getPoints = new ArrayList<PointValue>();
		if(timeList.size()>0){
			for(int i=0;i<timeList.size();i++){
				float x = Float.parseFloat(timeList.get(i));
				float y = Float.parseFloat(dataList.get(i));
				PointValue value = new PointValue(x, y);
				getPoints.add(value);
			}
		}else{
			Toast.makeText(this, "折线图的点不能为空", Toast.LENGTH_LONG).show();
		}
		
		return getPoints;
	}
	//handler处理消息
	 Handler handler = new Handler(){
		public void handleMessage(Message msg) {
			switch (msg.what) {
			case 1:
				timeList.add((String) msg.obj);
				Log.e("timeList>>>>", timeList+"");
				break;

			case 2:
				dataList.add((String) msg.obj);
				break;
			}
		}
	};
	
	//开启一个子线程用来实现UDP通信
	private class RecvThread extends Thread{
		private int t = 0;
		@Override
		public void run() {
			// TODO Auto-generated method stub
			super.run();
			try {
				socket = new DatagramSocket(udpPort);
				byte data[] = new byte[1024];
				DatagramPacket packet = new DatagramPacket(data, data.length);
				while(true){
						socket.receive(packet);//接收数据
						
//						String result = new String(packet.getData(), packet.getOffset(), packet.getLength());
//						Log.e("result>>>>>", result);
						
						//获取时间，此处为自定义的一个时间，因为五秒接收一次数据所以如下
						Message message1 = new Message();
						message1.what = 1;
						message1.obj = t+"";
						Log.e("t>>>>", t+"");
						handler.sendMessage(message1);
						t += 5;//时间间隔为五秒，
						
						//获取强度，由于接受到的数据为ASCII码表中的十进制所以需要先转换成十六进制
						//创建一个StringBuffer对象，用于拼串
						StringBuffer sb = new StringBuffer();
						//根据指令格式可知data[20]到data[23]位表示强度的指令此处截取出来
						for(int i=20;i<24;i++){
							int a = data[i];
							//如果a大于零小于16则需要在转换成十六进制的后的数据前加一个0；
							if(a>0&& a<16){
								String hex = Integer.toHexString(a);
								sb.append("0"+hex);
							}else if(a>= 16){
								//如果a大于等于16直接转换成十六进制
								String hex = Integer.toHexString(a);
								sb.append(hex);
							}else{
								//如果a为负数则需要在转换成十六进制后截取后两位
								String hex = Integer.toHexString((a & 0x000000FF) | 0xFFFFFF00).substring(6);
								sb.append(hex);
							}
						}
						//将转换后的十六进制拼成一个字符串
						String strHex = sb.toString();
						Log.e("strHex>>>>>>", strHex);
						//将strSB字符串两两一组并颠倒过来
						String strNu = "";
						String[] strData= new String[strHex.length()/2]; 
						for(int i=0;i<strHex.length();i++){
							strNu += strHex.charAt(i);
							if((i+1)%2 == 0){
								strData[i/2] = strNu;
								strNu = "";
							}
						}
						//将颠倒后的数据拼成一个字符串
						String powerHex = strData[3]+strData[2]+strData[1]+strData[0];
						Log.e("powerHex >>>", powerHex);
						//将32位的十六进制浮点数转换成十进制小数
						float dec = Float.intBitsToFloat(Integer.parseInt(powerHex, 16)); 
						Log.e("dec>>>", dec+"");
						Message message2 = new Message();
						message2.what = 2;
						message2.obj = dec+"";
						handler.sendMessage(message2);
				}
			} catch (SocketException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}

	//停止接收数据
	private void shutDownRecv(){
		new Thread(){
			public void run() {
				if(recvThread != null){
					recvThread.interrupt();
					socket.close();
					recvThread = null;
				}
			}
		}.start();
	}

	@Override
	protected void onDestroy() {
		// TODO Auto-generated method stub
		super.onDestroy();
		shutDownRecv();
		timer.cancel();
	}
}
