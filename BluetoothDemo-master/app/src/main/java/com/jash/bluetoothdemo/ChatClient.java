package com.jash.bluetoothdemo;

import android.bluetooth.BluetoothDevice;
import android.bluetooth.BluetoothSocket;
import android.os.Handler;
import android.os.Message;

import java.io.DataInputStream;
import java.io.DataOutputStream;
import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

/**
 * Created by jash
 * Date: 15-9-23
 * Time: 下午2:55
 */
public class ChatClient implements Runnable{
    private final BluetoothDevice device;
    private BluetoothSocket socket;
    private List<Handler> list = new ArrayList<>();

    public ChatClient(BluetoothSocket socket) {
        this.socket = socket;
        device = socket.getRemoteDevice();
        new Thread(this).start();
    }
    public void send(final String msg){
        new Thread(){
            @Override
            public void run() {
                try {
                    DataOutputStream os = new DataOutputStream(socket.getOutputStream());
                    os.writeUTF(msg);
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
        }.start();
    }
    public void register(Handler handler){
        list.add(handler);
    }
    public void unregister(Handler handler){
        list.remove(handler);
    }
    @Override
    public void run() {
        try {
            DataInputStream is = new DataInputStream(socket.getInputStream());
            String msg;
            while ((msg = is.readUTF()) != null) {
                Message message = Message.obtain();
                message.what = 0;
                message.obj = device.getName() + ":" + msg;
                for (Handler h : list) {
                    h.sendMessage(message);
                }
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}
