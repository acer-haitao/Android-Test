/*
 * Copyright (c) 2016 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.util;

public class MoreTextUtils {

    private MoreTextUtils() {}

    public static boolean equalsAny(CharSequence text, CharSequence... array) {
        for (CharSequence element : array) {
            if (android.text.TextUtils.equals(text, element)) {
                return true;
            }
        }
        return false;
    }
}
