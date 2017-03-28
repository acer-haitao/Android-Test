package com.baidu;

import android.app.Activity;
import android.graphics.drawable.GradientDrawable.Orientation;
import android.hardware.Sensor;
import android.hardware.SensorEvent;
import android.hardware.SensorEventListener;
import android.hardware.SensorManager;
import android.os.Bundle;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.TextView;

public class Sensor_Info extends Activity implements SensorEventListener {
	private SensorManager sm;
	private Sensor Orientation;
	private Sensor light;
	private Sensor accelerometer;
	private TextView etori;
	private TextView etacc;
	private TextView etlight;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		// TODO Auto-generated method stub
		super.onCreate(savedInstanceState);
		setContentView(R.layout.sensorinfo);
		
		sm = (SensorManager) getSystemService(SENSOR_SERVICE);
		Orientation = sm.getDefaultSensor(Sensor.TYPE_ORIENTATION);
		light = sm.getDefaultSensor(Sensor.TYPE_LIGHT);
		accelerometer = sm.getDefaultSensor(Sensor.TYPE_ACCELEROMETER);

		sm.registerListener(this, Orientation, SensorManager.SENSOR_DELAY_GAME);
		sm.registerListener(this, light, SensorManager.SENSOR_DELAY_GAME);
		sm.registerListener(this, accelerometer,
				SensorManager.SENSOR_DELAY_GAME);
		findbutton();

	}

	public void findbutton() {

		etori = (TextView) findViewById(R.id.etori);
		etacc = (TextView) findViewById(R.id.etacc);
		etlight = (TextView) findViewById(R.id.etlight);

	}

	@Override
	public void onSensorChanged(SensorEvent event) {
		// TODO Auto-generated method stub
		float[] values = event.values;// (把监听到的值保存到values里面)

		switch (event.sensor.getType()) {// 判断传感器的类型把相应的数据显示到文本上去
		case Sensor.TYPE_ORIENTATION:
			etori.setText("绕Z轴转过的角度：" + values[0] + "\n绕X轴转过的角度：" + values[1]
					+ "\n绕Y轴转过的角度：" + values[2]);
			break;
		case Sensor.TYPE_ACCELEROMETER:
			etacc.setText("X轴方向上的加速度：" + values[0] + "\nY轴方向上的加速度：" + values[1]
					+ "\nZ轴方向上的加速度：" + values[2]);
			break;
		case Sensor.TYPE_LIGHT:
			etlight.setText("当前光照强度为：" + values[0]);
			break;
		default:
			break;
		}
}

	@Override
	public void onAccuracyChanged(Sensor sensor, int accuracy) {
		// TODO Auto-generated method stub

	}
}
