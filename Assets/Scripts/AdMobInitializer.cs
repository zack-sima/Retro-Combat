using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using TMPro;
using Balaso;

public class AdMobInitializer : MonoBehaviour {
    RewardedAdsButton controller = null;
    private static GameObject instance;
    public RewardedAd rewardedAd;
    public InterstitialAd interstitialAd;
    public bool initialized;
    public void AdmobStart() {
        initialized = true;
#if UNITY_ANDROID
        string rewardedUnitId = "ca-app-pub-9659065879138366/2861179478";
        string interstitialUnitId = "ca-app-pub-9659065879138366/8723525028";
#elif UNITY_IOS
        //string rewardedUnitId = "ca-app-pub-9659065879138366/6660823685"; /* "ca-app-pub-3940256099942544/1712485313";*/ //test placement right now
        string rewardedUnitId = "ca-app-pub-9659065879138366/6507891386"; //China iOS
        //string interstitialUnitId = "ca-app-pub-9659065879138366/4365832833";
        string interstitialUnitId = "ca-app-pub-9659065879138366/8052390423"; //China iOS
#else
        string rewardedUnitId = "unexpected_platform";
        string interstitialUnitId = "unexpected_platform";
#endif
        MobileAds.Initialize(initCompleteAction => { });

        // Initialize an InterstitialAd.
        this.interstitialAd = new InterstitialAd(interstitialUnitId);
        // Create an empty ad request.
        AdRequest requestInterstitial = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        this.interstitialAd.LoadAd(requestInterstitial);

        this.rewardedAd = new RewardedAd(rewardedUnitId);
        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }
    public void HandleRewardedAdLoaded(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardedAdLoaded event received");
    }

    public void HandleRewardedAdFailedToLoad(object sender, EventArgs args) {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToLoad event received");
    }

    public void HandleRewardedAdOpening(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardedAdOpening event received");
    }

    public void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs args) {
        MonoBehaviour.print(
            "HandleRewardedAdFailedToShow event received with message: "
                             + args.Message);
    }

    public void HandleRewardedAdClosed(object sender, EventArgs args) {
        MonoBehaviour.print("HandleRewardedAdClosed event received");
        controller.CancelledAd();
    }

    public void HandleUserEarnedReward(object sender, Reward args) {
        string type = args.Type;
        double amount = args.Amount;
        controller.RewardPlayer();
        MonoBehaviour.print(
            "HandleRewardedAdRewarded event received for "
                        + amount.ToString() + " " + type);
    }
    void Awake() {
#if UNITY_IOS
        AppTrackingTransparency.RegisterAppForAdNetworkAttribution();
        AppTrackingTransparency.UpdateConversionValue(3);
#endif
    }
    void Start() {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
            instance = gameObject;
        else {
            Destroy(gameObject);
        }
        controller = GameObject.Find("Freecoinsvid").GetComponent<RewardedAdsButton>();
#if UNITY_IOS
        AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;

        AppTrackingTransparency.AuthorizationStatus currentStatus = AppTrackingTransparency.TrackingAuthorizationStatus;
        Debug.Log(string.Format("Current authorization status: {0}", currentStatus.ToString()));
        if (currentStatus != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED) {
            Debug.Log("Requesting authorization...");
            AppTrackingTransparency.RequestTrackingAuthorization();
        } else {
            Debug.Log("Already authorized");
        }
#endif
    }

#if UNITY_IOS

    /// <summary>
    /// Callback invoked with the user's decision
    /// </summary>
    /// <param name="status"></param>
    private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus status) {
        switch (status) {
        case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
            Debug.Log("AuthorizationStatus: NOT_DETERMINED");
            break;
        case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
            Debug.Log("AuthorizationStatus: RESTRICTED");
            break;
        case AppTrackingTransparency.AuthorizationStatus.DENIED:
            Debug.Log("AuthorizationStatus: DENIED");
            break;
        case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
            Debug.Log("AuthorizationStatus: AUTHORIZED");
            break;
        }

        // Obtain IDFA
        Debug.Log($"IDFA: {AppTrackingTransparency.IdentifierForAdvertising()}");
    }
#endif
    // Update is called once per frame
    void Update() {

    }
}
