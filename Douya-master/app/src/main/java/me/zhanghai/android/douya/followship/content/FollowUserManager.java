/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.followship.content;

import android.content.Context;

import me.zhanghai.android.douya.R;
import me.zhanghai.android.douya.content.ResourceWriterManager;
import me.zhanghai.android.douya.network.api.info.apiv2.UserInfo;
import me.zhanghai.android.douya.util.ToastUtils;

public class FollowUserManager extends ResourceWriterManager<FollowUserWriter> {

    private static class InstanceHolder {
        public static final FollowUserManager VALUE = new FollowUserManager();
    }

    public static FollowUserManager getInstance() {
        return InstanceHolder.VALUE;
    }

    /**
     * @deprecated Use {@link #write(UserInfo, boolean, Context)} instead.
     */
    public void write(String userIdOrUid, boolean like, Context context) {
        add(new FollowUserWriter(userIdOrUid, like, this), context);
    }

    public boolean write(UserInfo userInfo, boolean like, Context context) {
        if (shouldWrite(userInfo, context)) {
            add(new FollowUserWriter(userInfo, like, this), context);
            return true;
        } else {
            return false;
        }
    }

    private boolean shouldWrite(UserInfo userInfo, Context context) {
        if (userInfo.isOneself()) {
            ToastUtils.show(R.string.user_follow_error_cannot_follow_oneself, context);
            return false;
        } else {
            return true;
        }
    }

    public boolean isWriting(String userIdOrUid) {
        return findWriter(userIdOrUid) != null;
    }

    public boolean isWritingFollow(String userIdOrUid) {
        FollowUserWriter writer = findWriter(userIdOrUid);
        return writer != null && writer.isFollow();
    }

    private FollowUserWriter findWriter(String userIdOrUid) {
        for (FollowUserWriter writer : getWriters()) {
            if (writer.hasUserIdOrUid(userIdOrUid)) {
                return writer;
            }
        }
        return null;
    }
}
