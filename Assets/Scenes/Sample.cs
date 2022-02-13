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

using UnityEngine;
using StartApp;
using System;

public class Sample : MonoBehaviour {
    private InterstitialAd ad;

    void Start() {
        AdSdk.Instance.DisableReturnAds();
        AdSdk.Instance.SetTestAdsEnabled(true);
        var config = new SplashConfig {
            TemplateTheme = SplashConfig.Theme.Blaze
        };
        AdSdk.Instance.ShowSplash(config);
        AdSdk.Instance.SetUserConsent(
            "pas",
            true,
            (long)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds);

        // AdSdk.Instance.ShowDefaultAd();

        ad = AdSdk.Instance.CreateInterstitial("myTagForFullscreen");
        ad.RaiseAdLoaded += (sender, e) => {
            Debug.Log("Unity::RaiseAdLoaded");
            if (ad.IsReady()) {
                ad.ShowAd();
            }
        };

        ad.RaiseAdShown += (sender, e) => Debug.Log("Unity::RaiseAdShown");
        ad.RaiseAdLoadingFailed += (sender, e) => Debug.Log(string.Format("Unity::RaiseAdLoadingFailed {0}", e.Message));
        ad.RaiseAdClosed += (sender, e) => Debug.Log("Unity::RaiseAdClosed");
        ad.RaiseAdClicked += (sender, e) => Debug.Log("Unity::RaiseAdClicked");
        ad.RaiseAdVideoCompleted += (sender, e) => Debug.Log("Unity::RaiseAdVideoCompleted");
        InvokeRepeating("LoadInterstitial", 10.0f, 0.0f);

        var banner = AdSdk.Instance.CreateBanner("myTagForBanner");
        banner.ShowInPosition(BannerAd.BannerPosition.Top, BannerAd.BannerType.Mrec);

        banner.RaiseBannerShown += (sender, e) => Debug.Log("Unity::RaiseBannerShown");
        banner.RaiseBannerLoadingFailed += (sender, e) => Debug.Log(string.Format("Unity::RaiseBannerLoadingFailed {0}", e.Message));
        banner.RaiseBannerClicked += (sender, e) => Debug.Log("Unity::RaiseBannerClicked");
        banner.Hide();

        if (!banner.IsShownInPosition(BannerAd.BannerPosition.Top)) {
            // AdSdk.Instance.ShowDefaultBanner(BannerAd.BannerPosition.Bottom, "myBottomBanner");
            AdSdk.Instance.ShowDefaultBanner();
            banner.ShowInPosition(BannerAd.BannerPosition.Top);
        }
    }

    void Update() {
        if (Input.GetKeyUp(KeyCode.Escape)) {
            AdSdk.Instance.OnBackPressed();
        }
    }

    void LoadInterstitial() {
        ad.LoadAd(InterstitialAd.AdType.Video);
    }
}
