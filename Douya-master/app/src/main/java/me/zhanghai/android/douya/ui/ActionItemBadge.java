/*
 * Copyright (c) 2015 Zhang Hai <Dreaming.in.Code.ZH@Gmail.com>
 * All Rights Reserved.
 */

package me.zhanghai.android.douya.ui;

import android.app.Activity;
import android.content.Context;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.GradientDrawable;
import android.support.v4.content.ContextCompat;
import android.support.v4.view.ViewCompat;
import android.text.TextUtils;
import android.view.MenuItem;
import android.view.View;
import android.view.Window;
import android.widget.ImageView;
import android.widget.TextView;

import butterknife.ButterKnife;
import me.zhanghai.android.douya.R;
import me.zhanghai.android.douya.util.CheatSheetUtils;
import me.zhanghai.android.douya.util.ViewUtils;

public class ActionItemBadge {

    public static void setup(final MenuItem menuItem, Drawable icon, int count,
                             final Activity activity) {

        View actionView = menuItem.getActionView();
        actionView.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                activity.onMenuItemSelected(Window.FEATURE_OPTIONS_PANEL, menuItem);
            }
        });
        CharSequence title = menuItem.getTitle();
        if (!TextUtils.isEmpty(title)) {
            CheatSheetUtils.setup(actionView, title);
        }

        ImageView iconImage = ButterKnife.findById(actionView, R.id.icon);
        iconImage.setImageDrawable(icon);

        TextView badgeText = ButterKnife.findById(actionView, R.id.badge);
        Context themedContext = badgeText.getContext();
        ViewCompat.setBackground(badgeText, new BadgeDrawable(themedContext));
        badgeText.setTextColor(ViewUtils.getColorFromAttrRes(R.attr.colorPrimary, 0,
                themedContext));

        update(badgeText, count);
    }

    public static void setup(MenuItem menuItem, int iconResId, int count, Activity activity) {
        setup(menuItem, ContextCompat.getDrawable(activity, iconResId), count, activity);
    }

    private static void update(TextView badgeText, int count) {
        badgeText.setText(String.valueOf(count));
        ViewUtils.setVisibleOrGone(badgeText, count != 0);
    }

    public static void update(MenuItem menuItem, int count) {
        update(ButterKnife.<TextView>findById(menuItem.getActionView(), R.id.badge), count);
    }

    private static class BadgeDrawable extends GradientDrawable {

        public BadgeDrawable(Context context) {
            setColor(ViewUtils.getColorFromAttrRes(R.attr.colorControlNormal, 0, context));
        }

        @Override
        public void setBounds(int left, int top, int right, int bottom) {
            super.setBounds(left, top, right, bottom);

            setCornerRadius(Math.min(right - left, bottom - top));
        }
    }
}
