/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.content;

public abstract class ListResourceFragment<ResourceListType, ResponseType>
        extends ResourceFragment<ResourceListType, ResponseType> {

    protected abstract int getSize(ResourceListType resource);

    public boolean isEmpty() {
        return !has() || getSize(get()) == 0;
    }
}
