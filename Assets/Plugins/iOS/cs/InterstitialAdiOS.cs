/*
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

#if UNITY_IOS

using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace StartApp
{
    public class InterstitialAdiOS : InterstitialAd, IDisposable
    {
        bool mDisposed;
        readonly GameObject mGameObject = new GameObject();
        readonly string mAdTag;

        static InterstitialAdiOS()
        {
            AdSdkiOS.ImplInstance.Setup();
        }

        public InterstitialAdiOS(string tag = null)
        {
            mAdTag = tag;
            mGameObject.name = mGameObject.GetInstanceID().ToString();
            mGameObject.AddComponent<ListenerComponent>().Parent = this;
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
            sta_removeAdObject(mGameObject.name);
            mDisposed = true;
        }

        ~InterstitialAdiOS()
        {
            Dispose(false);
        }

        public override void LoadAd(AdType mode)
        {
            if (mode == AdType.Rewarded)
            {
                sta_loadRewardedVideoAd(mGameObject.name, mAdTag);
            }
            else if (mode == AdType.Video)
            {
                sta_loadVideoAd(mGameObject.name, mAdTag);
            }
            else
            {
                sta_loadAd(mGameObject.name, mAdTag);
            }
        }

        public override bool ShowAd()
        {
            sta_showAd(mGameObject.name);
            return true;
        }

        public override bool IsReady()
        {
            return sta_isReady(mGameObject.name);
        }

        class ListenerComponent : MonoBehaviour
        {
            public InterstitialAdiOS Parent { get; set; }

            void OnDidLoadAd()
            {
                Parent.OnRaiseAdLoaded();
            }

            void OnFailedLoadAd(string error)
            {
                Parent.OnRaiseAdLoadingFailed(error);
            }

            void OnDidShowAd()
            {
                Parent.OnRaiseAdShown();
            }

            void OnDidSendImpression()
            {
                Parent.OnRaiseAdImpressionSent();
            }

            void OnDidCloseAd()
            {
                Parent.OnRaiseAdClosed();
            }

            void OnDidClickAd()
            {
                Parent.OnRaiseAdClicked();
            }

            void OnDidCompleteVideo()
            {
                Parent.OnRaiseAdVideoCompleted();
            }
        }

        [DllImport("__Internal")]
        static extern void sta_loadAd(string gameObjectName, string tag);

        [DllImport("__Internal")]
        static extern void sta_showAd(string gameObjectName);

        [DllImport("__Internal")]
        static extern void sta_loadRewardedVideoAd(string gameObjectName, string tag);

        [DllImport("__Internal")]
        static extern void sta_loadVideoAd(string gameObjectName, string tag);

        [DllImport("__Internal")]
        static extern bool sta_isReady(string gameObjectName);

        [DllImport("__Internal")]
        static extern void sta_removeAdObject(string gameObjectName);
    }
}

#endif