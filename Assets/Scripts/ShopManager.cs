using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour{
    // Start is called before the first frame update
    public PlayerDatas localData;
    public GameObject popupPrefab;
    public Button noAdsButton;
    
    void Start(){
        if (MyPlayerPrefs.GetInt("blockedAds") == 1) {
            noAdsButton.interactable = false;
        }
    }
    public void ToScene(int index) {
        SceneManager.LoadScene(index);
    }
    // Update is called once per frame
    void Update(){
        
    }
    public void RemoveAds() {
        if (localData.myData.money >= 550 && MyPlayerPrefs.GetInt("blockedAds") == 0) {
            MyPlayerPrefs.SetInt("blockedAds", 1);
            localData.myData.money -= 550;
            localData.SaveFile();
            noAdsButton.interactable = false;
        }
    }
    void GenerateReward() {
        float random = Random.Range(0f, 1f);
        if (random < 0.62f) {
            //attachment
            Dictionary<int, int> availableSelectedChoices = new Dictionary<int, int>();
            foreach (KeyValuePair<int, int[]> i in localData.gunAttachmentsAvailable) {
                if (localData.myData.weaponsUnlocked.ContainsKey(i.Key)) {
                    foreach (int j in i.Value) {
                        if (!localData.myData.weaponsUnlocked[i.Key].attachments.Contains(j)) {
                            //adds the first attachment not unlocked to the availableselectedchoices list
                            availableSelectedChoices.Add(i.Key, j);
                            break;
                        }
                    }
                }
            }
            if (availableSelectedChoices.Count == 0) {
                //no choices available
                GenerateReward();
                return;
            } else {
                int randomAdd = Random.Range(0, availableSelectedChoices.Count);
                localData.myData.weaponsUnlocked[new List<int>(availableSelectedChoices.Keys)[randomAdd]].attachments.Add(new List<int>(availableSelectedChoices.Values)[randomAdd]);
                localData.CreatePopup(popupPrefab, CustomFunctions.TranslateText(localData.attachmentNames[new List<int>(availableSelectedChoices.Values)[randomAdd]]) + " " + CustomFunctions.TranslateText("for") + " " + CustomFunctions.TranslateText(localData.weaponNames[new List<int>(availableSelectedChoices.Keys)[randomAdd]]));
            }
        } else if (random < 0.7f) {
            //weapon
            List<int> availableSelectedChoices = new List<int>();
            foreach (int i in localData.weapons.Keys) {
                if (!localData.myData.weaponsUnlocked.ContainsKey(i) && localData.gunUnlockLevels[i] < 0) {
                    //adds the first attachment not unlocked to the availableselectedchoices list
                    availableSelectedChoices.Add(i);
                }
            }
            if (availableSelectedChoices.Count == 0) {
                //no choices available
                GenerateReward();
                return;
            } else {
                int randomAdd = Random.Range(0, availableSelectedChoices.Count);
                localData.myData.weaponsUnlocked.Add(availableSelectedChoices[randomAdd], new ShopWeapon(availableSelectedChoices[randomAdd]));
                localData.CreatePopup(popupPrefab, CustomFunctions.TranslateText(localData.weaponNames[availableSelectedChoices[randomAdd]]));
            }
        } else if (random < 0.78f) {
            //xp
            int xpRand = (int)(Random.Range(35, 76) * Mathf.Sqrt(localData.myData.xpLevel + 7));
            localData.myData.xp += xpRand;
            localData.CreatePopup(popupPrefab, xpRand + CustomFunctions.TranslateText(" ")+ CustomFunctions.TranslateText("xp"));
        } else {
            //perk
            List<int> availableSelectedChoices = new List<int>();
            foreach (int i in localData.perkUnlockLevels.Keys) {
                if (!localData.myData.perksUnlocked.Contains(i) && localData.perkUnlockLevels[i] < 0) {
                    //adds the first attachment not unlocked to the availableselectedchoices list
                    availableSelectedChoices.Add(i);
                }
            }
            if (availableSelectedChoices.Count == 0) {
                //no choices available
                GenerateReward();
                return;
            } else {
                int randomAdd = Random.Range(0, availableSelectedChoices.Count);
                localData.myData.perksUnlocked.Add(availableSelectedChoices[randomAdd]);
                localData.CreatePopup(popupPrefab, CustomFunctions.TranslateText(localData.perkNames[availableSelectedChoices[randomAdd]]) + " (" + CustomFunctions.TranslateText("perk") + ")");
            }
        }
        



    }
    public void SpinReward() {
        if (localData.myData.money >= 350) {
            //proceed to spin
            localData.myData.money -= 350;

            //loot table:
            //35% for attachment (check if unlocked gun has missing attachment)
            //10% new weapon (if any not unlocked yet)
            //30% for xp
            //25% for perk
            GenerateReward();


            //if drawn loot is unavailable, a redraw will be done
            
            localData.SaveFile();
        }
    }
}
