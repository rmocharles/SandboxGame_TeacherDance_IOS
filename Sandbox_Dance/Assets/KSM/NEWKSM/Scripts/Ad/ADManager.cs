using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;

public class ADManager : MonoBehaviour
{
    private static ADManager instance;

    private RewardedAd rewardedAd;
    private InterstitialAd interstitialAd;

    public static ADManager GetInstance()
    {
        if (instance == null)
            return null;
        return instance;
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (!instance) instance = this;
    }

    void Start()
    {
        MobileAds.SetiOSAppPauseOnBackground(true);

        List<String> deviceIds = new List<String>() { AdRequest.TestDeviceSimulator };

        // Add some test device IDs (replace with your own device IDs).
#if UNITY_IPHONE
        deviceIds.Add("96e23e80653bb28980d3f40beb58915c");
#elif UNITY_ANDROID
        deviceIds.Add("75EF8D155528C04DACBBA6F36F433035");
#endif

        // Configure TagForChildDirectedTreatment and test device IDs.
        RequestConfiguration requestConfiguration =
            new RequestConfiguration.Builder()
            .SetTagForChildDirectedTreatment(TagForChildDirectedTreatment.Unspecified)
            .SetTestDeviceIds(deviceIds).build();
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(HandleInitCompleteAction);



        RequestAndLoadRewardedAd();

        RequestAndLoadInterstitialAd();

    }

    private void HandleInitCompleteAction(InitializationStatus initstatus)
    {
        Debug.Log("Initialization complete.");

        // Callbacks from GoogleMobileAds are not guaranteed to be called on
        // the main thread.
        // In this example we use MobileAdsEventExecutor to schedule these calls on
        // the next Update() loop.
        MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            //statusText.text = "Initialization complete.";
            //RequestBannerAd();
        });
    }

    #region 보상형 광고

    public void RequestAndLoadRewardedAd()
    {
#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-5982380417386519/7125482217";
#else
        string adUnitId = "unexpected_platform";
#endif

        // create new rewarded ad instance
        rewardedAd = new RewardedAd(adUnitId);

        // Add Event Handlers
        rewardedAd.OnAdLoaded += (sender, args) =>
        {
        };
        rewardedAd.OnAdFailedToLoad += (sender, args) =>
        {
        };
        rewardedAd.OnAdOpening += (sender, args) =>
        {
        };
        rewardedAd.OnAdFailedToShow += (sender, args) =>
        {
        };
        rewardedAd.OnAdDidRecordImpression += (sender, args) =>
        {
        };
        rewardedAd.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Rewarded ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
        };

        // Create empty ad request
        rewardedAd.LoadAd(CreateAdRequest());
    }

    public void ShowRewardedAd(Action success, Action fail, Action loadFail)
    {
        rewardedAd.OnAdClosed += (sender, args) =>
        {
            Debug.LogError("Reward ad closed.");
            fail();
        };
        rewardedAd.OnUserEarnedReward += (sender, args) =>
        {
            Debug.LogError("User earned Reward ad reward: " + args.Amount);
            success();
        };

        if (rewardedAd != null)
        {
            rewardedAd.Show();
        }
        else
        {
            loadFail();
        }
        RequestAndLoadRewardedAd();
    }

    //string rewardID = "ca-app-pub-5982380417386519/7125482217";

    //public void LoadRewardAd()
    //{
    //    rewardedAd = new RewardedAd(rewardID);
    //    rewardedAd.LoadAd(CreateAdRequest());
    //    Debug.LogError(rewardedAd.IsLoaded());

    //}

    //public void RequestAndLoadRewardedAd(Action success, Action fail)
    //{


    //    // Add Event Handlers
    //    rewardedAd.OnAdLoaded += (sender, args) =>
    //    {
    //        Debug.LogError("Reward ad loaded.");
    //        rewardedAd.Show();
    //    };
    //    rewardedAd.OnAdFailedToLoad += (sender, args) =>
    //    {
    //        Debug.LogError("Reward ad failed to load.");
    //    };
    //    rewardedAd.OnAdOpening += (sender, args) =>
    //    {
    //        Debug.LogError("Reward ad opening.");
    //    };
    //    rewardedAd.OnAdFailedToShow += (sender, args) =>
    //    {
    //        Debug.LogError("Reward ad failed to show with error: " + args.AdError.GetMessage());
    //    };
    //    rewardedAd.OnAdClosed += (sender, args) =>
    //        {
    //            Debug.LogError("Reward ad closed.");
    //            fail();
    //};
    //rewardedAd.OnUserEarnedReward += (sender, args) =>
    //{
    //    Debug.LogError("User earned Reward ad reward: " + args.Amount);
    //    success();
    //};
    //    rewardedAd.OnAdDidRecordImpression += (sender, args) =>
    //    {
    //        Debug.LogError("Reward ad recorded an impression.");
    //    };
    //    rewardedAd.OnPaidEvent += (sender, args) =>
    //    {
    //        string msg = string.Format("{0} (currency: {1}, value: {2}",
    //                                    "Rewarded ad received a paid event.",
    //                                    args.AdValue.CurrencyCode,
    //                                    args.AdValue.Value);
    //        Debug.LogError(msg);
    //    };

    //    rewardedAd.Show();

    //}

    //public void ShowRewardedAd(Action success, Action fail, Action loadFail)
    //{
    //    if (rewardedAd.IsLoaded())
    //    {
    //        RequestAndLoadRewardedAd(() =>
    //        {
    //            success();
    //        },
    //    () =>
    //    {
    //        fail();
    //    });

    //        LoadRewardAd();
    //    }
    //    else
    //    {
    //        Debug.LogError("<color=red>" + rewardedAd.IsLoaded() + "</color>");
    //        loadFail();
    //    }

    //}

    #endregion

    #region 전면광고

    public void RequestAndLoadInterstitialAd()
    {

#if UNITY_EDITOR
        string adUnitId = "unused";
#elif UNITY_ANDROID
        string adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-5982380417386519/6195543926";
#else
        string adUnitId = "unexpected_platform";
#endif

        // Clean up interstitial before using it
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
        }

        interstitialAd = new InterstitialAd(adUnitId);

        // Add Event Handlers
        interstitialAd.OnAdLoaded += (sender, args) =>
        {
        };
        interstitialAd.OnAdFailedToLoad += (sender, args) =>
        {
        };
        interstitialAd.OnAdOpening += (sender, args) =>
        {
        };
        interstitialAd.OnAdClosed += (sender, args) =>
        {
        };
        interstitialAd.OnAdDidRecordImpression += (sender, args) =>
        {
        };
        interstitialAd.OnAdFailedToShow += (sender, args) =>
        {
        };
        interstitialAd.OnPaidEvent += (sender, args) =>
        {
            string msg = string.Format("{0} (currency: {1}, value: {2}",
                                        "Interstitial ad received a paid event.",
                                        args.AdValue.CurrencyCode,
                                        args.AdValue.Value);
        };

        // Load an interstitial ad
        interstitialAd.LoadAd(CreateAdRequest());
    }

    public void ShowAd(Action success)
    {
        interstitialAd.OnAdClosed += (sender, args) =>
        {
            success();
            RequestAndLoadInterstitialAd();
        };
        if (interstitialAd != null && interstitialAd.IsLoaded())
        {
            interstitialAd.Show();
        }
        else
        {
        }
    }

    //string frontID = "ca-app-pub-5982380417386519/6195543926";

    //public void LoadFrontAd()
    //{
    //    if (interstitialAd != null)
    //    {
    //        interstitialAd.Destroy();
    //    }
    //    interstitialAd = new InterstitialAd(frontID);
    //    interstitialAd.LoadAd(CreateAdRequest());
    //}

    //public void RequestAndLoadInterstitialAd(Action success = null)
    //{
    //    Debug.LogError("전면광고");

    //    //interstitialAd = new InterstitialAd(adUnitId);

    //    Debug.LogError("Requesting Interstitial ad.");
    //    // Add Event Handlers
    //    interstitialAd.OnAdLoaded += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad loaded.");
    //        interstitialAd.Show();
    //    };
    //    interstitialAd.OnAdFailedToLoad += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad failed to load with error: " + args.LoadAdError.GetMessage());
    //    };
    //    interstitialAd.OnAdOpening += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad opening.");
    //    };
    //    interstitialAd.OnAdClosed += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad closed.");
    //        if(success != null)
    //            success();
    //    };
    //    interstitialAd.OnAdDidRecordImpression += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad recorded an impression.");
    //    };
    //    interstitialAd.OnAdFailedToShow += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad failed to show.");
    //    };
    //    interstitialAd.OnPaidEvent += (sender, args) =>
    //    {
    //        string msg = string.Format("{0} (currency: {1}, value: {2}",
    //                                    "Interstitial ad received a paid event.",
    //                                    args.AdValue.CurrencyCode,
    //                                    args.AdValue.Value);
    //        Debug.LogError(msg);
    //    };

    //    // Load an interstitial ad
    //    interstitialAd.Show();
    //}

    //public void ShowAd(Action success)
    //{
    //    RequestAndLoadInterstitialAd(() =>
    //    {
    //        success();
    //    });
    //    LoadFrontAd();
    //}





    //string frontID = "ca-app-pub-5982380417386519/6195543926";

    //public void LoadFrontAd()
    //{
    //    interstitialAd = new InterstitialAd(frontID);
    //    interstitialAd.LoadAd(CreateAdRequest());
    //}

    //public void RequestAndLoadInterstitialAd(Action success)
    //{

    //    Debug.LogError("Requesting Interstitial ad.");
    //    // Add Event Handlers
    //    interstitialAd.OnAdLoaded += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad loaded.");
    //        interstitialAd.Show();
    //    };
    //    interstitialAd.OnAdFailedToLoad += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad failed to load with error: " + args.LoadAdError.GetMessage());
    //    };
    //    interstitialAd.OnAdOpening += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad opening.");
    //    };
    //    interstitialAd.OnAdClosed += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad closed.");
    //        success();
    //    };
    //    interstitialAd.OnAdDidRecordImpression += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad recorded an impression.");
    //    };
    //    interstitialAd.OnAdFailedToShow += (sender, args) =>
    //    {
    //        Debug.LogError("Interstitial ad failed to show.");
    //    };
    //    interstitialAd.OnPaidEvent += (sender, args) =>
    //    {
    //        string msg = string.Format("{0} (currency: {1}, value: {2}",
    //                                    "Interstitial ad received a paid event.",
    //                                    args.AdValue.CurrencyCode,
    //                                    args.AdValue.Value);
    //        Debug.LogError(msg);
    //    };

    //    interstitialAd.Show();
    //}

    //public void ShowAd(Action success)
    //{
    //    RequestAndLoadInterstitialAd(() =>
    //    {
    //        success();
    //    });
    //    LoadFrontAd();
    //}

    private AdRequest CreateAdRequest()
    {
        return new AdRequest.Builder().Build();
    }

    #endregion
}
