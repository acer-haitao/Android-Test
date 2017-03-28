package activitytest.example.com.mylistview;

import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.Toast;

import java.util.ArrayList;
import java.util.List;

public class MainLayoutActivity extends AppCompatActivity {
    private List<Fruit> fruitList = new ArrayList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main_layout);
        initFruits();
        FruitAdapter adapter = new FruitAdapter(MainLayoutActivity.this, R.layout.fruit_layout, fruitList);
        ListView listView = (ListView) findViewById(R.id.list_view);
        listView.setAdapter(adapter);

        listView.setOnItemClickListener(new AdapterView.OnItemClickListener(){
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
                Fruit fruit = fruitList.get(position);
                Toast.makeText(MainLayoutActivity.this, fruit.getName(), Toast.LENGTH_LONG).show();
            }
        });

    }

    private  void  initFruits()
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

            Fruit apple7 = new Fruit("Apple7", R.drawable.a);
            fruitList.add(apple7);
        }
    }
}
