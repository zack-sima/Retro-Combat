using System;
using System.Collections;
using System.Collections.Generic;
//using NotificationSamples; //deprecated
#if UNITY_ANDROID
using Google.Play.Review;
#endif
using TMPro;
using UnityEngine;
#if UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MenuManager : MonoBehaviour {
	public Sprite[] displayImages;
	public SpriteRenderer displayerTitle;
	public RewardedAdsButton rewardedAdsMaster;
	PlayerDatas localStorage;
	public Dropdown allianceDropdown;
	public ModelSoldier soldierModel;
	public InputField playerNameInput;
	public bool isMobile;
	public GameObject[] hideOnMobile;
	public RectTransform levelBarr;
	public Text levelText;
	public GameObject dailyLoginPrefab;
	public void ToggleFullscreen() {
		Screen.fullScreen = !Screen.fullScreen;
		if (Screen.fullScreen) {
			Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
		} else {
			Screen.fullScreenMode = FullScreenMode.Windowed;
		}

	}
	public void SetLanguage(string language) {
		MyPlayerPrefs.SetString("language", language);
	}
	public void ToScene(int index) {
		//temporary_settings
		MyPlayerPrefs.SetInt("alliance", allianceDropdown.value);

		if (playerNameInput.text == "") {
			playerNameInput.text = "Player";
		}

		localStorage.SaveFile();
		MyPlayerPrefs.SetString("playerName", playerNameInput.text);
		SceneManager.LoadScene(index);
	}

	Boolean spawned = false;

	//	public GameNotificationsManager manager;
	public void ShowNotificationAfterDelay(int sec) {
		string[] notificationsTitlesEnglish = new string[] {
			"Try some new modes!",
			"Try some multiplayer!",
			"Come back and hone your skills",
			"Drive a plane!",
			"Drive a tank!",
		};
		string[] notificationsBodiesEnglish = new string[] {
			"Third person is available in settings.",
			"Tanks and planes work in multiplayer as well. Play with a friend?",
			"Try using a silencer in combat!",
			"Vehicles are fun to use!",
			"Try fighting in tank combat!"

		};
		string[] notificationsTitlesChinese = new string[] {
			"尝试一些新玩法吧！",
			"多人模式玩玩？",
			"联系一下你的技巧？",
			"尝试开一下坦克？",
			"尝试开飞机？"
		};
		string[] notificationsBodiesChinese = new string[] {
			"第三人称视角练习吃鸡。",
			"多人可以开坦克飞机了！！！",
			"试试各种配件吧？",
			"试一试坦克大战吧！",
			"也可以用枪打飞机的（多人模式）"
		};
		int chineseRandomFactor = UnityEngine.Random.Range(0, notificationsBodiesChinese.Length);
		int englishRandomFactor = UnityEngine.Random.Range(0, notificationsBodiesEnglish.Length);

		//ShowNotificationAfterDelay(MyPlayerPrefs.GetString("language") == "Chinese" ? notificationsTitlesChinese[chineseRandomFactor] : notificationsTitlesEnglish[englishRandomFactor], PlayerPrefs.GetString("language") == "Chinese" ? notificationsBodiesChinese[chineseRandomFactor] : notificationsBodiesEnglish[englishRandomFactor], DateTime.Now.AddSeconds(sec));
	}
	//public void ShowNotificationAfterDelay(string title, string body, DateTime time) {
	//	IGameNotification createNotification = manager.CreateNotification();
	//	if (createNotification != null) {
	//		createNotification.Title = title;
	//		createNotification.Body = body;
	//		createNotification.DeliveryTime = time;

	//		var notificationToDisplay = manager.ScheduleNotification(createNotification);
	//		//notificationToDisplay.Reschedule = true; //to schedule here
	//	}
	//}
	//public void ScheduleNotification() {
	//	var channel = new GameNotificationChannel("channel_1", "Default Game Channel", "Generic notifications");
	//	if (!manager.Initialized)
	//		manager.Initialize(channel);

	//	//resets the existing notifications
	//	manager.CancelAllNotifications();
	//	manager.DismissAllNotifications();
	//	MyPlayerPrefs.SetInt("notifications_scheduled", MyPlayerPrefs.GetInt("notifications_scheduled") + 1);
	//	if (MyPlayerPrefs.GetInt("notifications_scheduled") > 1) {
	//		ShowNotificationAfterDelay(UnityEngine.Random.Range(2000000, 3500000));
	//	} else {
	//		ShowNotificationAfterDelay(UnityEngine.Random.Range(200000, 350000));
	//	}
	//}
	public void Start() {
		localStorage = GetComponent<PlayerDatas>();
		displayerTitle.sprite = displayImages[UnityEngine.Random.Range(0, displayImages.Length)];
		if (MyPlayerPrefs.GetFloat("sensitivity") == 0f)
			MyPlayerPrefs.SetFloat("sensitivity", 0.35f);
		if (MyPlayerPrefs.GetInt("pistol") == 0 * (1 - 2))
			MyPlayerPrefs.SetInt("pistol", 4);

#if !UNITY_EDITOR

        if (SystemInfo.deviceType == DeviceType.Handheld) {
            ScheduleNotification();
            isMobile = true;
        } else
            isMobile = false;
#endif
		if (isMobile) {
			foreach (GameObject i in hideOnMobile)
				Destroy(i);
		}
		MyPlayerPrefs.SetInt("Visits", MyPlayerPrefs.GetInt("Visits") + 1);

		print(MyPlayerPrefs.GetInt("Visits") + "_visits");
		if (MyPlayerPrefs.GetInt("Visits") >= 15) {
			MyPlayerPrefs.SetInt("Visits", -100);
			print("asked for review");
#if UNITY_ANDROID
            var reviewManager = new ReviewManager();

            // start preloading the review prompt in the background
            var playReviewInfoAsyncOperation = reviewManager.RequestReviewFlow();

            // define a callback after the preloading is done
            playReviewInfoAsyncOperation.Completed += playReviewInfoAsync => {
                if (playReviewInfoAsync.Error == ReviewErrorCode.NoError) {
                    // display the review prompt
                    var playReviewInfo = playReviewInfoAsync.GetResult();
                    reviewManager.LaunchReviewFlow(playReviewInfo);
                } else {
                    // handle error when loading review prompt
                }
            };
#elif UNITY_IOS
            Device.RequestStoreReview();
#endif
		}
		if (MyPlayerPrefs.GetInt("alliance") == -1) {
			MyPlayerPrefs.SetInt("alliance", 0);
		}
		allianceDropdown.value = MyPlayerPrefs.GetInt("alliance");
		playerNameInput.text = MyPlayerPrefs.GetString("playerName");



		SkinsUpdate();

	}
	public void SkinsUpdate() {
		soldierModel.soldierCountry = allianceDropdown.value <= 0 ? SoldierCountry.Russian : (SoldierCountry)(allianceDropdown.value - 1);
		soldierModel.weapon = MyPlayerPrefs.GetInt("mainGun");//primaryWeaponIds[primaryWeaponDropdown.value];
		soldierModel.UpdateSkinAndWeapon();
	}
	void Update() {

		if (!spawned &&
			(MyPlayerPrefs.GetInt("canRewardDaily") == 1 && SystemInfo.deviceType == DeviceType.Handheld) &&
			rewardedAdsMaster.admobControl != null && rewardedAdsMaster.admobControl.rewardedAd.IsLoaded()) {
			SaveData myData = BinaryPlayerSave.LoadData();
			int reward = 75 + (int)myData.consecutiveSigninDays * 25;
			localStorage.CreatePopup(dailyLoginPrefab, CustomFunctions.TranslateText("Streak: ") + localStorage.myData.consecutiveSigninDays + CustomFunctions.TranslateText(" day(s)"), reward.ToString());

			spawned = true;
		}

		playerNameInput.text = playerNameInput.text.Replace("|", "");
		playerNameInput.text = playerNameInput.text.Replace("=", "");
		playerNameInput.text = playerNameInput.text.Replace(":", "");
		playerNameInput.text = playerNameInput.text.Replace("+", "");
		playerNameInput.text = playerNameInput.text.Replace("&", "");
		levelBarr.localScale = new Vector2((float)localStorage.myData.xp / (float)localStorage.myData.upgradeXp, 1f);
		levelText.text = CustomFunctions.TranslateText("Level") + " " + localStorage.myData.xpLevel + " (" + localStorage.myData.xp + "/" + localStorage.myData.upgradeXp + ")";
	}
}