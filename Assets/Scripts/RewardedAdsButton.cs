using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Button))]
public class RewardedAdsButton : MonoBehaviour {
    
    public PlayerDatas localData;
    public bool isDailyReward;
    Button myButton;

    [HideInInspector]
    public AdMobInitializer admobControl = null;

    //if this exceeds 10 then stop trying to call
    int calledThreshold = 0;

    void Start() {
        admobControl = GameObject.Find("AdMobManager").GetComponent<AdMobInitializer>();
        if (!admobControl.initialized)
            admobControl.AdmobStart();
        if (isDailyReward)
            MyPlayerPrefs.SetInt("daily", 1);
        else {
            MyPlayerPrefs.SetInt("daily", 0);
        }

        myButton = GetComponent<Button>();
        // Set interactivity to be dependent on the Placement’s status:
        //myButton.interactable = Advertisement.IsReady(myPlacementId);

        // Map the ShowRewardedVideo function to the button’s click listener:
        if (myButton) myButton.onClick.AddListener(ShowRewardedVideo);

    }
    void Update() {
        if (MyPlayerPrefs.GetInt("blockedAds") == 0 && admobControl.interstitialAd.IsLoaded() && MyPlayerPrefs.GetInt("playAdsTimer") >= 2) {
            ShowInterstitial();
        }
        if (admobControl.rewardedAd.IsLoaded()) {

            if (isDailyReward) {
                CheckAdsCount();
            } else {
                if (myButton.interactable == false && localData.myData.availableVideos > 0) {
                    CheckAdsCount();
                }
                if (myButton.interactable && localData.myData.availableVideos <= 0) {
                    CheckAdsCount();
                }
            }
        }
    }
    void ShowRewardedVideo() {
        if (admobControl.rewardedAd.IsLoaded()) {
            calledThreshold++;
            if (calledThreshold > 10) {
                print("over calling");
                return;
            }
            admobControl.initialized = false;
            admobControl.rewardedAd.Show();
        }
    }
    void ShowInterstitial() {
        if (admobControl.interstitialAd.IsLoaded()) {
            calledThreshold++;
            if (calledThreshold > 10) {
                print("over calling");
                return;
            }
            admobControl.initialized = false;
            admobControl.interstitialAd.Show();

            MyPlayerPrefs.SetInt("playAdsTimer", -1);
            print("interstitial ad shown");
        }
    }
    // Implement IUnityAdsListener interface methods:
    public void OnUnityAdsReady(string placementId) {
        // If the ready Placement is rewarded, activate the button:
        if (placementId == "rewardedVideo" && myButton != null) {
            CheckAdsCount();
        }
    }
    public void CreateDailyReward(bool watchedVideo) {
        MyPlayerPrefs.SetInt("canRewardDaily", 0);
        SaveData myData = BinaryPlayerSave.LoadData();
        print("finished and rewarded!");
        int reward = 75 + (int)myData.consecutiveSigninDays * 25;
        if (watchedVideo)
            reward *= 2;
        myData.money += reward;
        //display popup for reward (set persistent prefs, checked at player data)
        MyPlayerPrefs.SetInt("rewardAmount", reward);
        MyPlayerPrefs.SetInt("rewarded", 1);

        BinaryPlayerSave.SaveData(myData);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void CheckAdsCount() {
        if (isDailyReward) {
            myButton.interactable = true; //watches ad once
        } else {
            if (localData.myData.availableVideos <= 0) {
                myButton.transform.GetChild(0).GetComponent<Text>().text = CustomFunctions.TranslateText("CHECK LATER");
                myButton.interactable = false;
            } else if (localData.myData.availableVideos > 0) {
                myButton.transform.GetChild(0).GetComponent<Text>().text = CustomFunctions.TranslateText("FREE COINS!");
                myButton.interactable = true;
            }
        }
    }
    public void CancelledAd() {
        //reload the page to make sure another ad is loaded
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void RewardPlayer() {
        SaveData myData = BinaryPlayerSave.LoadData();
        print("finished and rewarded!");
        myData.availableVideos--;
        int reward = 150;//Random.Range(125, 190);
        myData.money += reward;
        //display popup for reward (set persistent prefs, checked at player data)
        MyPlayerPrefs.SetInt("rewardAmount", reward);
        MyPlayerPrefs.SetInt("rewarded", 1);

        BinaryPlayerSave.SaveData(myData);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void FinishedRewarded() {
        if (MyPlayerPrefs.GetInt("daily") == 1) {
            CreateDailyReward(true);
        } else {
            RewardPlayer();
        }
    }
    

}