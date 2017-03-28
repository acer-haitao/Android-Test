package activitytest.example.com.recyclerview;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.support.v7.widget.LinearLayoutManager;
import android.support.v7.widget.RecyclerView;

import java.util.ArrayList;
import java.util.List;

public class MainActivity extends AppCompatActivity {
    private List<Fruit> fruitList = new ArrayList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        initFruit();
        RecyclerView recyclerView = (RecyclerView) findViewById(R.id.recycler_view);
        LinearLayoutManager layoutManager = new LinearLayoutManager(this);
        recyclerView.setLayoutManager(layoutManager);
        FruitAdapter adapter = new FruitAdapter(fruitList);
        recyclerView.setAdapter(adapter);
    }

    private void  initFruit()
    {
        for (int i = 0; i < 2; i++)
        {
            Fruit apple1 = new Fruit("Apple1", R.drawable.a);
            fruitList.add(apple1);
            Fruit apple2 = new Fruit("Apple2", R.drawable.b);
            fruitList.add(apple2);
            Fruit apple3 = new Fruit("Apple3", R.drawable.c);
            fruitList.add(apple3);
            Fruit apple4 = new Fruit("Apple4", R.drawable.d);
            fruitList.add(apple4);
            Fruit apple5 = new Fruit("Apple5", R.drawable.e);
            fruitList.add(apple5);
            Fruit apple6 = new Fruit("Apple6", R.drawable.f);
            fruitList.add(apple6);

        }
    }
}
