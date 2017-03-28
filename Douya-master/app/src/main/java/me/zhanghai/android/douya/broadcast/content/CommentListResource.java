/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.broadcast.content;

import com.android.volley.VolleyError;

import org.greenrobot.eventbus.Subscribe;
import org.greenrobot.eventbus.ThreadMode;

import java.util.Collections;
import java.util.List;

import me.zhanghai.android.douya.content.MoreRawListResourceFragment;
import me.zhanghai.android.douya.eventbus.CommentDeletedEvent;
import me.zhanghai.android.douya.network.Request;
import me.zhanghai.android.douya.network.api.ApiRequest;
import me.zhanghai.android.douya.network.api.info.apiv2.Comment;
import me.zhanghai.android.douya.network.api.info.apiv2.CommentList;

public abstract class CommentListResource
        extends MoreRawListResourceFragment<Comment, CommentList> {

    @Override
    protected Request<CommentList> onCreateRequest(boolean more, int count) {
        Integer start = more && has() ? get().size() : null;
        return onCreateRequest(start, count);
    }

    protected abstract ApiRequest<CommentList> onCreateRequest(Integer start, Integer count);

    @Override
    protected void onLoadStarted() {
        getListener().onLoadCommentListStarted(getRequestCode());
    }

    @Override
    protected void onLoadFinished(boolean more, int count, boolean successful, CommentList response,
                                  VolleyError error) {
        onLoadFinished(more, count, successful, response != null ? response.comments : null, error);
    }

    private void onLoadFinished(boolean more, int count, boolean successful, List<Comment> response,
                                VolleyError error) {
        getListener().onLoadCommentListFinished(getRequestCode());
        if (successful) {
            if (more) {
                append(response);
                getListener().onCommentListAppended(getRequestCode(),
                        Collections.unmodifiableList(response));
            } else {
                set(response);
                getListener().onCommentListChanged(getRequestCode(),
                        Collections.unmodifiableList(get()));
            }
        } else {
            getListener().onLoadCommentListError(getRequestCode(), error);
        }
    }

    protected void appendAndNotifyListener(List<Comment> commentList) {
        append(commentList);
        getListener().onCommentListAppended(getRequestCode(), commentList);
    }

    @Subscribe(threadMode = ThreadMode.MAIN)
    public void onCommentDeleted(CommentDeletedEvent event) {

        if (event.isFromMyself(this) || isEmpty()) {
            return;
        }

        List<Comment> commentList = get();
        for (int i = 0, size = commentList.size(); i < size; ) {
            Comment comment = commentList.get(i);
            if (comment.id == event.commentId) {
                commentList.remove(i);
                getListener().onCommentRemoved(getRequestCode(), i);
                --size;
            } else {
                ++i;
            }
        }
    }

    private Listener getListener() {
        return (Listener) getTarget();
    }

    public interface Listener {
        void onLoadCommentListStarted(int requestCode);
        void onLoadCommentListFinished(int requestCode);
        void onLoadCommentListError(int requestCode, VolleyError error);
        /**
         * @param newCommentList Unmodifiable.
         */
        void onCommentListChanged(int requestCode, List<Comment> newCommentList);
        /**
         * @param appendedCommentList Unmodifiable.
         */
        void onCommentListAppended(int requestCode, List<Comment> appendedCommentList);
        void onCommentRemoved(int requestCode, int position);
    }
}
