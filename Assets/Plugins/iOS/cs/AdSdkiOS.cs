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

using System.Runtime.InteropServices;
using UnityEngine;
using System;

namespace StartApp
{
    public class AdSdkiOS : AdSdk
    {
        public enum UnityOrientation
        {
            Portrait,
            Landscape,
            AutoRotation
        };

        public static AdSdkiOS ImplInstance
        {
            get
            {
                return (AdSdkiOS)Instance;
            }
        }

        string mDeveloperId;
        string mApplicatonId;
        bool mSplashWasShown;
        UnityOrientation Orientation { get; set; }
        bool mPaused;
        readonly GameObject mGameObject = new GameObject();

        public AdSdkiOS()
        {
            #if !UNITY_EDITOR
                Setup();

                mGameObject.name = mGameObject.GetInstanceID().ToString();
                mGameObject.AddComponent<PauseListenerComponent>().Parent = this;
            #endif
        }

        public void Setup()
        {
            #if !UNITY_EDITOR
                Orientation = UnityOrientation.AutoRotation;
                UpdateOrientation();

                if (!string.IsNullOrEmpty(mApplicatonId))
                {
                    return;
                }

                sta_setUnityVersion(WrapperVersion);

                if (!ReadDataFromTextFile())
                {
                    throw new ArgumentException("Error in initializing Application ID or Developer ID, Verify you initialized them in StartAppDataiOS in resources");
                }

                if (string.IsNullOrEmpty(mDeveloperId))
                {
                    sta_initilize(mApplicatonId, "");
                }
                else
                {
                    sta_initilize(mApplicatonId, mDeveloperId);
                }
            #endif
        }

        public override InterstitialAd CreateInterstitial(string tag = null)
        {
            return new InterstitialAdiOS(tag);
        }

        public override BannerAd CreateBanner(string tag = null)
        {
            return new BannerAdiOS(tag);
        }

        public override void SetUserConsent(string consentType, bool enabled, long timestamp)
        {
            #if !UNITY_EDITOR
                Setup();
                sta_setUserConsent(enabled, consentType, timestamp);
            #endif
        }

        public override void ShowSplash(SplashConfig config = null)
        {
            #if !UNITY_EDITOR
                if (mSplashWasShown)
                {
                    return;
                }

                Setup();

                if (config == null)
                {
                    sta_showDefaultSplashAd();
                }
                else
                {
                    sta_showSplashAd(config.AppName, config.LogoFileName, (int)config.TemplateTheme, (int)config.ScreenOrientation);
                }

                mSplashWasShown = true;
            #endif
        }

        public override void DisableReturnAds()
        {
            #if !UNITY_EDITOR
                sta_disableReturnAd();
            #endif
        }

        public override bool OnBackPressed()
        {
            return false;
        }
		
        public override void SetTestAdsEnabled(bool enabled)
        {
            #if !UNITY_EDITOR
                sta_setTestAdsEnabled(enabled);
            #endif
        }

        public void UpdateOrientation()
        {
            #if !UNITY_EDITOR
                sta_setUnityAutoRotation((int)Orientation);
                sta_setUnitySupportedOrientations(SupportedOrientations());
            #endif
        }

        int SupportedOrientations()
        {
            var orientation = UnityOrientation.Portrait;
            bool isPort = false;
            bool isLand = false;

            if (Orientation == UnityOrientation.AutoRotation)
            {
                isPort |= Screen.autorotateToPortrait == true || Screen.autorotateToPortraitUpsideDown == true;
                isLand |= Screen.autorotateToLandscapeRight == true || Screen.autorotateToLandscapeLeft == true;

                if (isPort && !isLand)
                {
                    orientation = UnityOrientation.Portrait;
                }
                if (!isPort && isLand)
                {
                    orientation = UnityOrientation.Landscape;
                }

                if (isPort && isLand)
                {
                    orientation = UnityOrientation.AutoRotation;
                }
                return (int)orientation;
            }

            return (int)Orientation;
        }

        bool ReadDataFromTextFile()
        {
            bool result = false;
            int assigned = 0;

            var data = Resources.Load("StartAppDataiOS") as TextAsset;
            var userData = data.ToString();

            string[] splitData = userData.Split('\n');
            string[] singleData;

            for (int i = 0; i < splitData.Length; i++)
            {
                singleData = splitData[i].Split('=');
                if (singleData[0].ToLower().CompareTo("applicationid") == 0)
                {
                    assigned++;
                    mApplicatonId = singleData[1].Trim();
                }

                if (singleData[0].ToLower().CompareTo("developerid") == 0)
                {
                    assigned++;
                    mDeveloperId = singleData[1].Trim();
                }
                else if (singleData[0].ToLower().CompareTo("accountid") == 0)
                {
                    assigned++;
                    mDeveloperId = singleData[1].Trim();
                }

                if (singleData[0].ToLower().CompareTo("returnads") == 0)
                {
                    if (singleData[1].ToLower().Equals("false"))
                    {
                        assigned++;
                        DisableReturnAds();
                    }
                }
            }

            result |= assigned >= 1;
            return result;
        }

        class PauseListenerComponent : MonoBehaviour
        {
            public AdSdkiOS Parent { get; set; }

            void OnApplicationPause(bool pauseStatus)
            {
                #if !UNITY_EDITOR
                    if (Parent.mPaused != pauseStatus)
                    {
                        if (pauseStatus)
                        {
                            Debug.Log("Unity::EnterBackground");
                            sta_enterBackground();
                        }
                        else
                        {
                            Debug.Log("Unity::EnterForeground");
                            sta_enterForeground();
                        }
                    }
                    Parent.mPaused = pauseStatus;
                #endif
            }
        }

        #if !UNITY_EDITOR
            [DllImport("__Internal")]
            static extern void sta_initilize(string appId, string devId);

            [DllImport("__Internal")]
            static extern void sta_setUserConsent(bool consent, string consentType, long timestamp);
		
            [DllImport("__Internal")]
            static extern void sta_setTestAdsEnabled(bool enabled);

            [DllImport("__Internal")]
            static extern void sta_disableReturnAd();

            [DllImport("__Internal")]
            static extern void sta_enterBackground();

            [DllImport("__Internal")]
            static extern void sta_enterForeground();

            [DllImport("__Internal")]
            static extern void sta_setUnityVersion(string unityVersion);

            [DllImport("__Internal")]
            static extern void sta_setUnitySupportedOrientations(int supportedOrientations);

            [DllImport("__Internal")]
            static extern void sta_setUnityAutoRotation(int autoRotation);

            [DllImport("__Internal")]
            static extern void sta_showDefaultSplashAd();

            [DllImport("__Internal")]
            static extern void sta_showSplashAd(string appName, string logoImageName, int theme, int orientation);
        #endif
    }
}

#endif