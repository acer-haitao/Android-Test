package activitytest.example.com.mylistview;

import android.content.Context;

import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import java.util.List;

/**
 * Created by admin on 2017/2/24.
 */

public class FruitAdapter extends ArrayAdapter<Fruit> {


    private int resourceId;

    /**
     * 构造函数用于将上下文和 子项布局id 数据传递进来
     * @param context
     * @param textViewResourceId
     * @param objects
     */
    public  FruitAdapter(Context context, int textViewResourceId, List<Fruit> objects)
    {
        super(context, textViewResourceId, objects);
        resourceId = textViewResourceId;
        Log.i("Hello","构造方法");
    }


    /**
     * 此方法用于在每个子项被滚动到屏幕时候会被调用
     * @param position
     * @param convertView
     * @param parent
     * @return
     */
    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        Fruit fruit = getItem(position);//获取当前的Fruit实例
        View view = LayoutInflater.from(getContext()).inflate(resourceId, parent, false);//加载传入的布局
        ImageView fruitImage = (ImageView) view.findViewById(R.id.img);
        TextView fruitName = (TextView) view.findViewById(R.id.name);

        //设置图片和文字
        fruitImage.setImageResource(fruit.getImageId());
        fruitName.setText(fruit.getName());
        Log.i("Hello","GetView");

        return view;//将布局返回去
    }

}
