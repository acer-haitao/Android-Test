/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.user.content;

import com.android.volley.VolleyError;

import java.util.Collections;
import java.util.List;

import me.zhanghai.android.douya.content.MoreRawListResourceFragment;
import me.zhanghai.android.douya.network.Request;
import me.zhanghai.android.douya.network.api.ApiRequest;
import me.zhanghai.android.douya.network.api.info.apiv2.User;

public abstract class BaseUserListResource<ResponseType>
        extends MoreRawListResourceFragment<User, ResponseType> {

    @Override
    protected Request<ResponseType> onCreateRequest(boolean more, int count) {
        Integer start = more ? (has() ? get().size() : 0) : null;
        return onCreateRequest(start, count);
    }

    protected abstract ApiRequest<ResponseType> onCreateRequest(Integer start, Integer count);

    @Override
    protected void onLoadStarted() {
        getListener().onLoadUserListStarted(getRequestCode());
    }

    @Override
    protected final void onLoadFinished(boolean more, int count, boolean successful,
                                        ResponseType response, VolleyError error) {
        onCallRawLoadFinished(more, count, successful, response, error);
    }

    protected abstract void onCallRawLoadFinished(boolean more, int count, boolean successful,
                                                  ResponseType response, VolleyError error);

    protected void onRawLoadFinished(boolean more, int count, boolean successful,
                                     List<User> response, VolleyError error) {
        getListener().onLoadUserListFinished(getRequestCode());
        if (successful) {
            if (more) {
                append(response);
                getListener().onUserListAppended(getRequestCode(),
                        Collections.unmodifiableList(response));
            } else {
                set(response);
                getListener().onUserListChanged(getRequestCode(),
                        Collections.unmodifiableList(get()));
            }
        } else {
            getListener().onLoadUserListError(getRequestCode(), error);
        }
    }

    private Listener getListener() {
        return (Listener) getTarget();
    }

    public interface Listener {
        void onLoadUserListStarted(int requestCode);
        void onLoadUserListFinished(int requestCode);
        void onLoadUserListError(int requestCode, VolleyError error);
        /**
         * @param newUserList Unmodifiable.
         */
        void onUserListChanged(int requestCode, List<User> newUserList);
        /**
         * @param appendedUserList Unmodifiable.
         */
        void onUserListAppended(int requestCode, List<User> appendedUserList);
    }
}
