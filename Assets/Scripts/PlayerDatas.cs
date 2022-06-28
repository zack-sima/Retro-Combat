using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Networking;
using UnityEngine.UI;
using System;

public enum Perks {None, FastReload, MoreHealthLessSpeed, LessRecoil, MoreSwift, MoreDamageLessHealth, MoreGrenadesLessAmmo, FasterShooting}
public class PlayerDatas : MonoBehaviour {
    public SaveData myData;
    public static PlayerDatas instance;

    public GameObject popupPrefab;
    public Text moneyText; 
    public Dictionary<int, int> perkUnlockLevels = new Dictionary<int, int>() {
        {0, 0}, //nothing, the default
        {1, 3}, //faster reloading/sleight of hand
        {2, 5}, //extra health/juggernaut
        {3, 7}, //less recoil/tight grip
        {4, -1}, //sprinter
        {5, 12}, //more damage/deep impact
        {6, 17}, //extra frag
        {7, -1}, //faster heal/respiration
        {8, 21}, //quicker shooting speed/double tap
    };
    public Dictionary<int, string> weaponNames = new Dictionary<int, string>() {
        { 0, "M4"}, //m4
        { 1, "M1911" }, //m1911
        { 3, "Akm" }, //akm
        { 4, "M9" }, //m9
        { 6,"SKS" }, //sks
        {7, "Uzi"}, //uzi
        {8, "Mp5"}, //mp5
        {9, "S686"}, //s686
        {10, "Deagle"}, //deagle
        {11, "VSS"}, //vss
        {12,"P17C" }, //p17c
        {13,"M107"}, //m107
        {14,"G36C" }, //g36c
        {15, "G3A4"}, //g3a4

    };
    public Dictionary<int, string> attachmentNames = new Dictionary<int, string>() {

        {1, "Red Dot"},
        {2, "ACOG"},
        {3, "Default Scope"},
        {4, "Silencer"},
        {5, "Extended Mag"}

    };
    public Dictionary<int, string> perkChineseDescriptions = new Dictionary<int, string>() {
        {0, "未选择技能"},
        {1, "+ 更换弹夹加快15%"},
        {2, "+ 生命值增加15%\n- 速度减慢10%"},
        {3, "+ 后坐力减少15%"},
        {4, "+ 速度增加15%"},
        {5, "+ 伤害增加12%\n- 生命值减少5%"},
        {6, "+ 多一个手榴弹\n- 弹药减少20%"},
        {7, "+ 恢复生命值加快25%"},
        {8, "+ 射击速度加快12%"}
    };
    public Dictionary<int, string> perkDescriptions = new Dictionary<int, string>() {
        {0, "No perk selected"},
        {1, "+ Reloading is 15% faster"},
        {2, "+ Player has 15% more health\n- Player is 10% slower"},
        {3, "+ Side and vertical recoil is reduced by 15%"},
        {4, "+ Player is 15% faster"},
        {5, "+ Player does 12% more damage\n- Player has 5% less health"},
        {6, "+ Player has an extra grenade\n- Player has 20% less ammunition"},
        {7, "+ Healing is 25% faster"},
        {8, "+ Shooting is 12% faster"}
    };
    public Dictionary<int, int> gunUnlockLevels = new Dictionary<int, int>() {
        {0, 1},
        {1, 5},
        {3, 2},
        {4, 1},
        {6, 4},
        {7, -1}, //-1 means unlocked in shop
        {8, 6},
        {9, 1},
        {10, 9},
        {11, -1},
        {12, 7},
        {13, 15},
        {14, 10},
        { 15, 12}
    };
    public Dictionary<int, string> perkNames = new Dictionary<int, string>() {
        {0, "No perk"},
        {1, "Sleight of hand"},
        {2, "Juggernaut"},
        {3, "Tighter grip"},
        {4, "Sprinter"},
        {5, "Deep impact"},
        {6, "Extra frag"},
        {7, "Respiration"},
        {8, "Double tap"}
    };
    public Dictionary<int, int[]> gunAttachmentsAvailable = new Dictionary<int, int[]> {
        {0, new int[] {1, 2, 4, 5} }, //m4: red dot, acog, silencer, extended mag
        {1, new int[] {5} }, //m1911
        {3, new int[] {1, 2, 4, 5} }, //akm: red dot, acog, silencer, extended mag
        {4, new int[] {4, 5} }, //m9: silencer, extended_mag
        {6, new int[] {1, 2, 4, 5} }, //sks
        {7, new int[] {1, 4, 5} }, //uzi
        {8, new int[] {1, 2, 4, 5} }, //mp5
        {9, new int[] {} }, //s686
        {10, new int[] {5} }, //deagle
        {11, new int[] {1, 2, 5} }, //vss
        {12, new int[] {4, 5} }, //glock
        {13, new int[] {1, 2, 4, 5} }, //m107
        {14, new int[] {1, 2, 4, 5} }, //g36c
        {15, new int[] {1, 2, 4, 5} }, //g3a4

    };
    public Dictionary<int, ShopWeapon> weapons = new Dictionary<int, ShopWeapon>() {
        { 0, new ShopWeapon(0) }, //m4
        { 1, new ShopWeapon(1) }, //m1911
        { 3, new ShopWeapon(3) }, //akm
        { 4, new ShopWeapon(4) }, //m9
        { 6, new ShopWeapon(6) }, //sks
        {7, new ShopWeapon(7) }, //uzi
        {8, new ShopWeapon(8) }, //mp5
        {9, new ShopWeapon(9) }, //s686
        {10, new ShopWeapon(10) }, //deagle
        {11, new ShopWeapon(11) }, //vss
        {12,new ShopWeapon(12) }, //p17c
        {13,new ShopWeapon(13) }, //m107
        {14,new ShopWeapon(14) }, //g36c
        {15, new ShopWeapon(15) }, //g3a4

    }; //all weapons, armory browsing for weapons should go through this
    public Dictionary<int, string> AttachmentNames { get => attachmentNames; set => attachmentNames = value; }
    IEnumerator CheckTime() {
        UnityWebRequest r = UnityWebRequest.Get("http://www.retrocombat.com:8001/server_time");
        yield return r.SendWebRequest();
        //returns web time
        //for rewards
        long outputTimeframe;
        if (long.TryParse(r.downloadHandler.text.Split('.')[0], out outputTimeframe)) {
            if (myData.lastDailySignin < outputTimeframe - 86500) {
                MyPlayerPrefs.SetInt("canRewardDaily", 1);
                if (myData.lastDailySignin < outputTimeframe - 170000) {
                    myData.consecutiveSigninDays = 0; //streak broken
                } else {
                    if (myData.consecutiveSigninDays < 10)
                    myData.consecutiveSigninDays++;
                }
                myData.lastDailySignin = outputTimeframe;
            }
            if (myData.lastVideoWatchTime < outputTimeframe - 7200) {
                myData.availableVideos = 3;
                myData.lastVideoWatchTime = outputTimeframe;
            }
        }
        SaveFile();
    }
    public void CreatePopup(GameObject prefab, string text, string text2="") {
        GameObject popupInsitem = Instantiate(prefab, GameObject.Find("Canvas").transform);
        popupInsitem.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 1f);
        try {
            if (text != "") {
                popupInsitem.GetComponent<Popup>().optionalText1.text = text;
            }
            if (text2 != "")
                popupInsitem.GetComponent<Popup>().optionalText2.text = text2;

            if (text.Contains("http")) {
                string linkStr = text.Substring(text.IndexOf("http"));
                linkStr = linkStr.Substring(0, linkStr.IndexOf("  "));
                popupInsitem.GetComponent<Popup>().embedLink = linkStr;
            } else {
                Destroy(popupInsitem.GetComponent<Popup>().primaryButton.gameObject);
            }
        } catch {
            print("tried to assign text but failed");
        }
    }
    void Awake() {
        instance = this;
        myData = BinaryPlayerSave.LoadData();
        StartCoroutine(CheckTime());
        try {
            //throw new Exception(); //force create a new data file
//patches old save files
        } catch {
            print("new data");
            myData = new SaveData();
            SaveFile();
        }
        myData.upgradeXp = (int)(Mathf.Pow(myData.xpLevel, 1.25f) * 25);



        foreach (KeyValuePair<int, int> i in gunUnlockLevels) {
            if (!myData.weaponsUnlocked.ContainsKey(i.Key) && i.Value <= myData.xpLevel && i.Value != -1) {
                myData.weaponsUnlocked.Add(i.Key, new ShopWeapon(i.Key));
            }
        }
        foreach (KeyValuePair<int, int> i in perkUnlockLevels) {
            if (!myData.perksUnlocked.Contains(i.Key) && i.Value <= myData.xpLevel && i.Value != -1) {
                myData.perksUnlocked.Add(i.Key);
            }
        }
        SaveFile();
    }
    void Start() {
        if (MyPlayerPrefs.GetInt("rewarded") == 1) {
            MyPlayerPrefs.SetInt("rewarded", 0);
            CreatePopup(popupPrefab, CustomFunctions.TranslateText("You received ") + MyPlayerPrefs.GetInt("rewardAmount") + CustomFunctions.TranslateText(" coins!"));
            print("yay");
        }
    }
    public void SaveFile() {
        BinaryPlayerSave.SaveData(myData);
    }
    void Update() {
        if (moneyText)
            moneyText.text = myData.money.ToString();
        if (myData.xp > myData.upgradeXp) {
            myData.xp -= myData.upgradeXp;
            myData.money += (myData.xpLevel + 9) * 15;
            myData.xpLevel++;
            myData.upgradeXp = (int)(Mathf.Pow(myData.xpLevel, 1.25f) * 25);
            SaveFile();
        }
    }
}
public static class BinaryPlayerSave {
    public static void SaveData(SaveData data) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/localdata.sav", FileMode.Create);

        bf.Serialize(stream, data);
        stream.Close();
    }
    //add integer parameter for different maps
    public static SaveData LoadData() {
        if (File.Exists(Application.persistentDataPath + "/localdata.sav")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(Application.persistentDataPath + "/localdata.sav", FileMode.Open);
            SaveData data;
            try {
                data = bf.Deserialize(stream) as SaveData;
                stream.Close();
            } catch {
                stream.Close();
                //error with existent data
                data = new SaveData();


            }
            return data;
            
        } else {
            //set initial playerprefs here
            //detect language
            if (Application.systemLanguage == SystemLanguage.ChineseSimplified || Application.systemLanguage == SystemLanguage.ChineseTraditional || Application.systemLanguage == SystemLanguage.ChineseSimplified) {
                MyPlayerPrefs.SetString("language", "Chinese");



                MyPlayerPrefs.SetInt("region", 1);

            }




            //detect legacy data from retro 2
            SaveData convertedOldData = LegacyBinarySave.MigrateToNewData();
            if (convertedOldData != null)
                return convertedOldData;
            else {
                return new SaveData();
            }
        }
    }
}
[System.Serializable]
public class ShopWeapon {
    public int gunId; //name is stored in gun script of corresponding id
    public List<int> attachments;
    public List<int> gunSkins; //might implement later
    public ShopWeapon(int gunId, List<int> attachments, List<int> gunSkins) { //copied
        this.gunId = gunId;
        this.attachments = attachments;
        this.gunSkins = gunSkins;
    }
    public ShopWeapon(int gunId) {
        attachments = new List<int>();
        this.gunSkins = new List<int>();
        this.gunId = gunId;
    }

}
[System.Serializable]
public class SaveData {
    public Dictionary<int, ShopWeapon> weaponsUnlocked;
    public List<int> perksUnlocked;

    public long lastVideoWatchTime; //available videos goes back to 3 when this is updated
    public long lastDailySignin, consecutiveSigninDays;


    public int availableVideos;
    public int money;
    public int xp;
    public int upgradeXp; //xp to upgrade to next level
    public int xpLevel;
    public SaveData() {
        money = 250;
        xp = 0;
        xpLevel = 1;
        availableVideos = 3;
        lastVideoWatchTime = 0;
        lastDailySignin = 0;

        weaponsUnlocked = new Dictionary<int, ShopWeapon>();
        perksUnlocked = new List<int>();
        
        //Uzi, m107, G36C cost money to buy
        //(level 30), (shop), etc for locked weapons/attachments
        
    }
}
