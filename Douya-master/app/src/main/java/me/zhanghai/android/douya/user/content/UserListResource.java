/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.user.content;

import com.android.volley.VolleyError;

import me.zhanghai.android.douya.network.api.ApiRequest;
import me.zhanghai.android.douya.network.api.info.apiv2.UserList;

public abstract class UserListResource extends BaseUserListResource<UserList> {

    protected abstract ApiRequest<UserList> onCreateRequest(Integer start, Integer count);

    @Override
    protected void onCallRawLoadFinished(boolean more, int count, boolean successful, UserList response, VolleyError error) {
        onRawLoadFinished(more, count, successful, response != null ? response.users : null, error);
    }
}
