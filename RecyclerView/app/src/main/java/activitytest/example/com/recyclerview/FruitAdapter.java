package activitytest.example.com.recyclerview;

import android.content.Context;

import android.support.v7.widget.RecyclerView;
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

public class FruitAdapter extends RecyclerView.Adapter<FruitAdapter.ViewHolder> {


    private int resourceId;
    private List<Fruit> mFruitList;

    static class  ViewHolder extends RecyclerView.ViewHolder {
        ImageView fruitImage;
        TextView fruitName;


        public ViewHolder(View itemView) {
            super(itemView);
            fruitImage = (ImageView)itemView.findViewById(R.id.img);
            fruitName = (TextView)itemView.findViewById(R.id.name);
        }
    }

    /**
     * 构造函数用于将上下文和 子项布局id 数据传递进来
     * @param context
     * @param textViewResourceId
     * @param objects
     */
    public FruitAdapter(List<Fruit> fruitList)
    {
        mFruitList = fruitList;
    }

    @Override
    public FruitAdapter.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType) {
       View view = LayoutInflater.from(parent.getContext()).inflate(R.layout.fruit_layout, parent, false);
        ViewHolder holder = new ViewHolder(view);
        return holder;
    }

    @Override
    public void onBindViewHolder(FruitAdapter.ViewHolder holder, int position) {
        Fruit fruit = mFruitList.get(position);
        holder.fruitImage.setImageResource(fruit.getImageId());
        holder.fruitName.setText(fruit.getName());
    }

    @Override
    public int getItemCount() {
        return mFruitList.size();
    }
}
