/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.link;

import android.content.Context;

import me.zhanghai.android.douya.R;
import me.zhanghai.android.douya.settings.info.Settings;
import me.zhanghai.android.douya.util.ToastUtils;

public class NotImplementedManager {

    private NotImplementedManager() {}

    public static void openDoumail(Context context) {
        UrlHandler.open("https://www.douban.com/doumail/", context);
    }

    public static void openSearch(Context context) {
        if (Settings.PROGRESSIVE_THIRD_PARTY_APP.getValue()
                && FrodoBridge.search(null, null, null, context)) {
            return;
        }
        UrlHandler.open("https://www.douban.com/search", context);
    }

    public static void sendBroadcast(String topic, Context context) {
        if (FrodoBridge.sendBroadcast(topic, context)) {
            return;
        }
        UrlHandler.open("https://www.douban.com/#isay-cont", context);
    }

    public static void showNotYetImplementedToast(Context context) {
        ToastUtils.show(R.string.not_yet_implemented, context);
    }

    public static void signUp(Context context) {
        UrlHandler.open("https://www.douban.com/accounts/register", context);
    }
}
