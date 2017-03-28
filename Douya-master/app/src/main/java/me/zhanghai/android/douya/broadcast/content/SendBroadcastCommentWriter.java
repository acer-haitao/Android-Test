/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.broadcast.content;

import android.content.Context;

import com.android.volley.VolleyError;

import me.zhanghai.android.douya.R;
import me.zhanghai.android.douya.content.ResourceWriter;
import me.zhanghai.android.douya.eventbus.BroadcastCommentSendErrorEvent;
import me.zhanghai.android.douya.eventbus.BroadcastCommentSentEvent;
import me.zhanghai.android.douya.eventbus.EventBusUtils;
import me.zhanghai.android.douya.network.Request;
import me.zhanghai.android.douya.network.api.ApiError;
import me.zhanghai.android.douya.network.api.ApiRequests;
import me.zhanghai.android.douya.network.api.info.apiv2.Comment;
import me.zhanghai.android.douya.util.LogUtils;
import me.zhanghai.android.douya.util.ToastUtils;

class SendBroadcastCommentWriter extends ResourceWriter<SendBroadcastCommentWriter, Comment> {

    private long mBroadcastId;
    private String  mComment;

    SendBroadcastCommentWriter(long broadcastId, String comment,
                               SendBroadcastCommentManager manager) {
        super(manager);

        mBroadcastId = broadcastId;
        mComment = comment;
    }

    public long getBroadcastId() {
        return mBroadcastId;
    }

    public String getComment() {
        return mComment;
    }

    @Override
    protected Request<Comment> onCreateRequest() {
        return ApiRequests.newSendBroadcastCommentRequest(mBroadcastId, mComment);
    }

    @Override
    public void onResponse(Comment response) {

        ToastUtils.show(R.string.broadcast_send_comment_successful, getContext());

        EventBusUtils.postAsync(new BroadcastCommentSentEvent(mBroadcastId, response, this));

        stopSelf();
    }

    @Override
    public void onErrorResponse(VolleyError error) {

        LogUtils.e(error.toString());
        Context context = getContext();
        ToastUtils.show(context.getString(R.string.broadcast_send_comment_failed_format,
                ApiError.getErrorString(error, context)), context);

        EventBusUtils.postAsync(new BroadcastCommentSendErrorEvent(mBroadcastId, this));

        stopSelf();
    }
}
