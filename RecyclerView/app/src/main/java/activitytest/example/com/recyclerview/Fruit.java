package activitytest.example.com.recyclerview;

/**
 * Created by admin on 2017/2/23.
 */

public class Fruit {
    //变量
    private String name;
    private  int imageId;

    //构造方法
    public Fruit(String name, int imageId)
    {
        this.name = name;
        this.imageId = imageId;
    }

    //封装
    public int getImageId() {
        return imageId;
    }

    public String getName() {
        return name;
    }
}
