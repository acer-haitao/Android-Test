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
 * ʵ��udpͨ�ţ����ҽ���ȡ�������ݻ��Ƴ�ʵʱ��ʾ������ͼ
 * */

public class MainActivity extends Activity {
	
	private static final int udpPort = 9993;//�����Ķ˿ں�
	private RecvThread recvThread;
	private DatagramSocket socket;//udpͨ�ŵ�socket
	private ArrayList<String> timeList = new ArrayList<String>();//���ʱ��ļ���
	private ArrayList<String> dataList = new ArrayList<String>();//������ݵļ���
	
	private LineChartView lineChartView;//����ͼ����ͼview
	private LineChartData lineChartData;//����ͼ������
	private List<Line> lineList;//�ߵļ���
	private List<PointValue> pointValueList;//ʵʱ����µĵ�ļ���
	private List<PointValue> points;//����ͼ�е�ļ���
	private int position = 0;//
	private Axis axisY ;//y������
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
	
	//����һ����ʱ��������ʵʱ��ȡ���ݣ����һ�������ͼ
	private void showLineChart(){
		timer = new Timer();
		timer.schedule(new TimerTask() {
			
			@Override
			public void run() {
				// TODO Auto-generated method stub
				if(!isFinish){
					if(position <points.size()-1){
					//ʵʱ����µĵ�
					pointValueList.add(points.get(position));
					float x = points.get(position).getX();//��ȡ��ӵ��x����
					//�����µĵ㻭���µ���
					Line line = new Line(pointValueList);
					line.setColor(Color.DKGRAY);//����������ɫ
					line.setShape(ValueShape.CIRCLE);//���ýڵ��ͼ����ʽ��DIAMOND���Ρ�SQUARE���Ρ�CIRCLEԲ��
					line.setCubic(false);//�����Ƿ�ƽ����true��ʾΪ���ߣ�false��ʾΪ����
					lineList.add(line);
					lineChartData = initDatas(lineList);
					lineChartView.setLineChartData(lineChartData);//Ϊͼ���������ݣ�����ΪlineChartData
					//���ݽڵ�ĺ�����ʵʱ�ı任�������ͼ��Χ
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
	
	
	//��ʼ���ؼ�
	private void initView(){
		lineChartView = (LineChartView) findViewById(R.id.lineChart);
		lineList = new ArrayList<Line>();
		pointValueList = new ArrayList<PointValue>();
		//��ʼ������
		axisY = new Axis();
		axisY.setName("ǿ��");//����y�������
		axisY.setTextColor(Color.parseColor("#FFFFFF"));//����y���������ɫ
		
		lineChartData = initDatas(null);
		lineChartView.setLineChartData(lineChartData);
		
		Viewport port = initViewport(0, 50);
		lineChartView.setCurrentViewportWithAnimation(port);
		lineChartView.setInteractive(false);//����ͼ�����Ƿ���Ժ��û�������trueΪ����falseΪ������
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
	//��ʼ������
	private void initData(){
		points = new ArrayList<PointValue>();
		points.addAll(getFirstChart());
	}
	//��ȡ��������ͼ����ӵ�ļ���
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
			Toast.makeText(this, "����ͼ�ĵ㲻��Ϊ��", Toast.LENGTH_LONG).show();
		}
		
		return getPoints;
	}
	//handler������Ϣ
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
	
	//����һ�����߳�����ʵ��UDPͨ��
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
						socket.receive(packet);//��������
						
//						String result = new String(packet.getData(), packet.getOffset(), packet.getLength());
//						Log.e("result>>>>>", result);
						
						//��ȡʱ�䣬�˴�Ϊ�Զ����һ��ʱ�䣬��Ϊ�������һ��������������
						Message message1 = new Message();
						message1.what = 1;
						message1.obj = t+"";
						Log.e("t>>>>", t+"");
						handler.sendMessage(message1);
						t += 5;//ʱ����Ϊ���룬
						
						//��ȡǿ�ȣ����ڽ��ܵ�������ΪASCII����е�ʮ����������Ҫ��ת����ʮ������
						//����һ��StringBuffer��������ƴ��
						StringBuffer sb = new StringBuffer();
						//����ָ���ʽ��֪data[20]��data[23]λ��ʾǿ�ȵ�ָ��˴���ȡ����
						for(int i=20;i<24;i++){
							int a = data[i];
							//���a������С��16����Ҫ��ת����ʮ�����Ƶĺ������ǰ��һ��0��
							if(a>0&& a<16){
								String hex = Integer.toHexString(a);
								sb.append("0"+hex);
							}else if(a>= 16){
								//���a���ڵ���16ֱ��ת����ʮ������
								String hex = Integer.toHexString(a);
								sb.append(hex);
							}else{
								//���aΪ��������Ҫ��ת����ʮ�����ƺ��ȡ����λ
								String hex = Integer.toHexString((a & 0x000000FF) | 0xFFFFFF00).substring(6);
								sb.append(hex);
							}
						}
						//��ת�����ʮ������ƴ��һ���ַ���
						String strHex = sb.toString();
						Log.e("strHex>>>>>>", strHex);
						//��strSB�ַ�������һ�鲢�ߵ�����
						String strNu = "";
						String[] strData= new String[strHex.length()/2]; 
						for(int i=0;i<strHex.length();i++){
							strNu += strHex.charAt(i);
							if((i+1)%2 == 0){
								strData[i/2] = strNu;
								strNu = "";
							}
						}
						//���ߵ��������ƴ��һ���ַ���
						String powerHex = strData[3]+strData[2]+strData[1]+strData[0];
						Log.e("powerHex >>>", powerHex);
						//��32λ��ʮ�����Ƹ�����ת����ʮ����С��
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

	//ֹͣ��������
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
