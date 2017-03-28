/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.broadcast.content;

import android.content.Context;

import com.android.volley.VolleyError;

import me.zhanghai.android.douya.R;
import me.zhanghai.android.douya.content.ResourceWriter;
import me.zhanghai.android.douya.eventbus.BroadcastDeletedEvent;
import me.zhanghai.android.douya.eventbus.EventBusUtils;
import me.zhanghai.android.douya.network.Request;
import me.zhanghai.android.douya.network.api.ApiError;
import me.zhanghai.android.douya.network.api.ApiRequests;
import me.zhanghai.android.douya.network.api.info.apiv2.Broadcast;
import me.zhanghai.android.douya.util.LogUtils;
import me.zhanghai.android.douya.util.ToastUtils;

class DeleteBroadcastWriter extends ResourceWriter<DeleteBroadcastWriter, Broadcast> {

    private long mBroadcastId;

    DeleteBroadcastWriter(long broadcastId, DeleteBroadcastManager manager) {
        super(manager);

        mBroadcastId = broadcastId;
    }

    public long getBroadcastId() {
        return mBroadcastId;
    }

    @Override
    protected Request<Broadcast> onCreateRequest() {
        return ApiRequests.newDeleteBroadcastRequest(mBroadcastId);
    }

    @Override
    public void onResponse(Broadcast response) {

        ToastUtils.show(R.string.broadcast_delete_successful, getContext());

        EventBusUtils.postAsync(new BroadcastDeletedEvent(response.id, this));

        stopSelf();
    }

    @Override
    public void onErrorResponse(VolleyError error) {

        LogUtils.e(error.toString());
        Context context = getContext();
        ToastUtils.show(context.getString(R.string.broadcast_delete_failed_format,
                ApiError.getErrorString(error, context)), context);

        stopSelf();
    }
}
