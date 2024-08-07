﻿/*
Copyright 2019 StartApp Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

#if UNITY_ANDROID

using UnityEngine;
using System;

namespace StartApp
{
    public class BannerAdAndroid : BannerAd, IDisposable
    {
        static readonly int ALIGN_PARENT_BOTTOM;
        static readonly int ALIGN_PARENT_TOP;
        
    #if !UNITY_EDITOR
        static readonly int ID_LAYOUT = 11;
        static readonly int ID_UNITY = 12;
        static readonly int MATCH_PARENT;
        static readonly int WRAP_CONTENT;
        static readonly int CENTER_HORIZONTAL;
        static readonly int VISIBLE;

        AndroidJavaObject mBanner;
    #endif

        BannerPosition mCurrentPosition;
        string mAdTag;
        bool mDisposed;

        static BannerAdAndroid()
        {
            #if !UNITY_EDITOR
                AdSdkAndroid.ImplInstance.Setup();

                var viewParams = new AndroidJavaClass("android.view.ViewGroup$LayoutParams");
                MATCH_PARENT = viewParams.GetStatic<int>("MATCH_PARENT");
                WRAP_CONTENT = viewParams.GetStatic<int>("WRAP_CONTENT");

                var relativeLayout = new AndroidJavaClass("android.widget.RelativeLayout");
                CENTER_HORIZONTAL = relativeLayout.GetStatic<int>("CENTER_HORIZONTAL");
                ALIGN_PARENT_BOTTOM = relativeLayout.GetStatic<int>("ALIGN_PARENT_BOTTOM");
                ALIGN_PARENT_TOP = relativeLayout.GetStatic<int>("ALIGN_PARENT_TOP");

                var viewClass = new AndroidJavaClass("android.view.View");
                VISIBLE = viewClass.GetStatic<int>("VISIBLE");
            #endif
        }

        public BannerAdAndroid(string tag = null)
        {
            mAdTag = tag;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (mDisposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
            }

        #if !UNITY_EDITOR
            AdSdkAndroid.ImplInstance.Activity.Call("runOnUiThread", new AndroidJavaRunnable(RemoveBannerImplementation));
        #endif

            mDisposed = true;
        }

        ~BannerAdAndroid()
        {
            Dispose(false);
        }

        public override void ShowInPosition(BannerPosition position, BannerType type)
        {
            #if !UNITY_EDITOR
                var jContent = new AndroidJavaObject("java.lang.String", "content");
                var jId = new AndroidJavaObject("java.lang.String", "id");
                var jPackage = new AndroidJavaObject("java.lang.String", "android");

                AdSdkAndroid.ImplInstance.Activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    var layout = AdSdkAndroid.ImplInstance.Activity.Call<AndroidJavaObject>("findViewById", ID_LAYOUT);

                    if (layout == null)
                    {
                        var res = AdSdkAndroid.ImplInstance.Activity.Call<AndroidJavaObject>("getResources");
                        var id = res.Call<int>("getIdentifier", jContent, jId, jPackage);
                        var contentParent = AdSdkAndroid.ImplInstance.Activity.Call<AndroidJavaObject>("findViewById", id);
                        var unityView = contentParent.Call<AndroidJavaObject>("getChildAt", 0);
                        unityView.Call("setId", ID_UNITY);
                        contentParent.Call("removeView", unityView);

                        layout = new AndroidJavaObject("android.widget.RelativeLayout", AdSdkAndroid.ImplInstance.Activity);
                        layout.Call("setId", ID_LAYOUT);
                        contentParent.Call("addView", layout);

                        var layoutParams = new AndroidJavaObject("android.widget.RelativeLayout$LayoutParams", MATCH_PARENT, MATCH_PARENT);
                        layout.Call("addView", unityView, layoutParams);
                    }

                    var bannerParams = new AndroidJavaObject("android.widget.RelativeLayout$LayoutParams", WRAP_CONTENT, WRAP_CONTENT);
                    bannerParams.Call("addRule", CENTER_HORIZONTAL);
                    bannerParams.Call("addRule", GetRuleFromBannerPosition(position));

                    if (mBanner == null)
                    {
                        string jClass = type == BannerAd.BannerType.Mrec
                            ? "com.startapp.sdk.ads.banner.Mrec"
                            : type == BannerAd.BannerType.Cover
                                ? "com.startapp.sdk.ads.banner.Cover"
                                : "com.startapp.sdk.ads.banner.bannerstandard.BannerStandard";

                        var adprefs = new AndroidJavaObject("com.startapp.sdk.adsbase.model.AdPreferences");
                        if (mAdTag != null)
                        {
                            adprefs.Call<AndroidJavaObject>("setAdTag", mAdTag);
                        }

                        mBanner = new AndroidJavaObject(jClass, AdSdkAndroid.ImplInstance.Activity, adprefs);
                        mBanner.Call("setBannerListener", new ImplementationBannerListener(this));

                        layout.Call("addView", mBanner, bannerParams);
                    }
                    else
                    {
                        mBanner.Call("setLayoutParams", bannerParams);
                        mBanner.Call("showBanner");
                    }

                    mCurrentPosition = position;
                }));
            #endif
        }

        public override void Hide()
        {
            #if !UNITY_EDITOR
                AdSdkAndroid.ImplInstance.Activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    mBanner.Call("hideBanner");
                }));
            #endif
        }

        public override bool IsShownInPosition(BannerPosition position)
        {
            return IsVisible() && mCurrentPosition == position;
        }

        bool IsVisible()
        {
            #if !UNITY_EDITOR
                return mBanner != null && mBanner.Call<int>("getVisibility") == VISIBLE;
            #else
                return false;
            #endif
        }

        public void RemoveBannerImplementation()
        {
            #if !UNITY_EDITOR
                if (mBanner == null)
                {
                    return;
                }

                var layout = AdSdkAndroid.ImplInstance.Activity.Call<AndroidJavaObject>("findViewById", ID_LAYOUT);

                if (layout != null)
                {
                    layout.Call("removeView", mBanner);
                    mBanner = null;
                }
            #endif
        }

        static int GetRuleFromBannerPosition(BannerPosition position)
        {
            return position == BannerPosition.Top ? ALIGN_PARENT_TOP : ALIGN_PARENT_BOTTOM;
        }

    #if !UNITY_EDITOR
        class ImplementationBannerListener : AndroidJavaProxy
        {
            readonly BannerAdAndroid mParent;

            public ImplementationBannerListener(BannerAdAndroid parent) : base("com.startapp.sdk.ads.banner.BannerListener")
            {
                mParent = parent;
            }

            void onReceiveAd(AndroidJavaObject banner)
            {
                mParent.OnRaiseBannerShown();
            }

            void onFailedToReceiveAd(AndroidJavaObject banner)
            {
                var errorMessage = banner.Call<AndroidJavaObject>("getErrorMessage");
                mParent.OnRaiseBannerLoadingFailed(errorMessage.Call<string>("toString"));
            }

            void onClick(AndroidJavaObject banner)
            {
                mParent.OnRaiseBannerClicked();
            }

            void onImpression(AndroidJavaObject banner)
            {
            }
        }
    #endif
    }
}

#endif