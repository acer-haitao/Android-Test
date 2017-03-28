/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.content;

import com.android.volley.VolleyError;

import me.zhanghai.android.douya.network.Request;

public abstract class MoreListResourceFragment<ResourceListType, ResponseType>
        extends ListResourceFragment<ResourceListType, ResponseType> {

    private static final int DEFAULT_LOAD_COUNT = 20;

    private boolean mLoadingMore;
    private boolean mCanLoadMore = true;

    private int mLoadCount;

    protected abstract ResourceListType addAll(ResourceListType resource, ResourceListType more);

    @Override
    protected void set(ResourceListType resource) {
        super.set(resource);
        mCanLoadMore = getSize(resource) == mLoadCount;
    }

    protected void append(ResourceListType more) {
        super.set(addAll(get(), more));
        mCanLoadMore = getSize(more) == mLoadCount;
    }

    public boolean isLoadingMore() {
        return mLoadingMore;
    }

    @Override
    protected void onLoadOnStart() {
        load(false);
    }

    @Override
    public final void load() {
        throw new UnsupportedOperationException("Use load(boolean, int) instead");
    }

    public void load(boolean more, int count) {

        if (more && !mCanLoadMore) {
            return;
        }

        if (!isLoading()) {
            mLoadingMore = more;
            mLoadCount = count;
        }

        super.load();
    }

    public void load(boolean loadMore) {
        load(loadMore, getDefaultLoadCount());
    }

    protected int getDefaultLoadCount() {
        return DEFAULT_LOAD_COUNT;
    }

    @Override
    protected final Request<ResponseType> onCreateRequest() {
        return onCreateRequest(mLoadingMore, mLoadCount);
    }

    protected abstract Request<ResponseType> onCreateRequest(boolean more, int count);

    @Override
    protected final void onLoadFinished(boolean successful, ResponseType response,
                                        VolleyError error) {
        onLoadFinished(mLoadingMore, mLoadCount, successful, response, error);
        mLoadingMore = false;
    }

    protected abstract void onLoadFinished(boolean more, int count, boolean successful,
                                           ResponseType response, VolleyError error);
}
