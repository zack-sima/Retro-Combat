using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WeaponsManager : MonoBehaviour {
    public SoldierWeaponsManager weaponsManager;
    public Dropdown primaryWeaponDropdown, secondaryWeaponsDropdown, attachmentDropdown;
    public Dropdown perksDropdown;
    public Image gunDisplayer1, gunDisplayer2, attachmentDisplay;
    public Image perkDisplayer;
    public Sprite[] primaryWeaponSprites, pistolSprites, attachmentSprites;
    public Sprite[] perkSprites;
    public List<int> primaryWeaponIds, secondaryWeaponIds, attachmentIds;
    PlayerDatas localData;
    public Text perkText;
    public GameObject statsParent;

    public RectTransform[] statistics;
    public void ToScene(int index) {
        //temporary_settings
        SceneManager.LoadScene(index);
    }
    public void SetGunPS(int weaponMode) {
        MyPlayerPrefs.SetInt("weaponMode", weaponMode);
        ToScene(3);
    }
    void StartFuncs() {
        if (MyPlayerPrefs.GetInt("weaponMode") == 0) {
            primaryWeaponDropdown.value = MyPlayerPrefs.GetInt("mg");
            Destroy(secondaryWeaponsDropdown.gameObject);
            Destroy(perksDropdown.gameObject);
            Destroy(perkText.gameObject);
            for (int i = 0; i < primaryWeaponIds.Count; i++) {
                if (!localData.myData.weaponsUnlocked.ContainsKey(primaryWeaponIds[i])) {
                    if (localData.gunUnlockLevels[primaryWeaponIds[i]] == -1)
                        primaryWeaponDropdown.options[i].text += " (" + CustomFunctions.TranslateText("Shop") + ")";
                    else {
                        primaryWeaponDropdown.options[i].text += " (" + CustomFunctions.TranslateText("Level") + " " + localData.gunUnlockLevels[primaryWeaponIds[i]] + ")";

                    }

                }
            }
        } else if (MyPlayerPrefs.GetInt("weaponMode") == 1) {
            secondaryWeaponsDropdown.value = MyPlayerPrefs.GetInt("ps");
            Destroy(primaryWeaponDropdown.gameObject);
            Destroy(perksDropdown.gameObject);
            Destroy(perkText.gameObject);
            for (int i = 0; i < secondaryWeaponIds.Count; i++) {
                if (!localData.myData.weaponsUnlocked.ContainsKey(secondaryWeaponIds[i])) {
                    if (localData.gunUnlockLevels[secondaryWeaponIds[i]] == -1)
                        secondaryWeaponsDropdown.options[i].text += " (" + CustomFunctions.TranslateText("Shop") + ")";
                    else {
                        secondaryWeaponsDropdown.options[i].text += " (" + CustomFunctions.TranslateText("Level") + " " + localData.gunUnlockLevels[secondaryWeaponIds[i]] + ")";

                    }

                }
            }
        } else {
            Destroy(statsParent);
            perksDropdown.value = MyPlayerPrefs.GetInt("pk");
            Destroy(primaryWeaponDropdown.gameObject);
            Destroy(secondaryWeaponsDropdown.gameObject);
            Destroy(attachmentDropdown.gameObject);
            for (int i = 0; i < perksDropdown.options.Count; i++) {
                if (!localData.myData.perksUnlocked.Contains(i)) {
                    if (localData.perkUnlockLevels[i] == -1)
                        perksDropdown.options[i].text += " (" + CustomFunctions.TranslateText("Shop") + ")";
                    else {
                        perksDropdown.options[i].text += " (" + CustomFunctions.TranslateText("Level") + " " + localData.perkUnlockLevels[i] + ")";

                    }

                }
            }
        }
    }









    int localSelectedGun = 0; //use this to set myplayerpref so locked weapons can't be selected
    public void Start() {
        localData = GetComponent<PlayerDatas>();
        StartFuncs();

        if (MyPlayerPrefs.GetInt("weaponMode") == 2)
            OnPerkChanged();
        else {
            OnWeaponChanged();

        }



    }
    public void OnWeaponChanged() {
        if (MyPlayerPrefs.GetInt("weaponMode") == 0) {
            gunDisplayer1.sprite = primaryWeaponSprites[primaryWeaponDropdown.value];
        } else
            gunDisplayer2.sprite = pistolSprites[secondaryWeaponsDropdown.value];
        if (MyPlayerPrefs.GetInt("weaponMode") == 0) {
            localSelectedGun = primaryWeaponIds[primaryWeaponDropdown.value];
            if (localData.myData.weaponsUnlocked.ContainsKey(localSelectedGun)) {
                MyPlayerPrefs.SetInt("mainGun", primaryWeaponIds[primaryWeaponDropdown.value]);
                MyPlayerPrefs.SetInt("mg", primaryWeaponDropdown.value); //for local saving
                primaryWeaponDropdown.transform.GetChild(0).GetComponent<Image>().enabled = false;
            } else
                primaryWeaponDropdown.transform.GetChild(0).GetComponent<Image>().enabled = true;



        } else {
            localSelectedGun = secondaryWeaponIds[secondaryWeaponsDropdown.value];
            if (localData.myData.weaponsUnlocked.ContainsKey(localSelectedGun)) {
                MyPlayerPrefs.SetInt("pistol", secondaryWeaponIds[secondaryWeaponsDropdown.value]);
                MyPlayerPrefs.SetInt("ps", secondaryWeaponsDropdown.value);
                secondaryWeaponsDropdown.transform.GetChild(0).GetComponent<Image>().enabled = false;
            } else {
                secondaryWeaponsDropdown.transform.GetChild(0).GetComponent<Image>().enabled = true;
            }
        }
        attachmentIds = new List<int>();
        attachmentDropdown.options = new List<Dropdown.OptionData>();
        

        if (weaponsManager.weapons[localSelectedGun].GetComponent<Gun>().scoped) {
            attachmentDropdown.options.Add(new Dropdown.OptionData(CustomFunctions.TranslateText("Default Scope")));
            attachmentIds.Add(3); //3 for default scopes
        } else {
            attachmentDropdown.options.Add(new Dropdown.OptionData(CustomFunctions.TranslateText("No Attachment")));
            attachmentIds.Add(0);
        }
        if (weaponsManager.weapons[localSelectedGun].GetComponent<Gun>().redDotAimAnchor != null) {
            attachmentIds.Add(1);
            attachmentDropdown.options.Add(new Dropdown.OptionData(CustomFunctions.TranslateText("Red Dot") + " " + (CheckWeaponAndAttachmentUnlocked(1) ? "" : "(" + CustomFunctions.TranslateText("Shop") + ")")));
        }
        if (weaponsManager.weapons[localSelectedGun].GetComponent<Gun>().acogAimAnchor != null) {
            attachmentIds.Add(2);
            attachmentDropdown.options.Add(new Dropdown.OptionData(CustomFunctions.TranslateText("ACOG") + " " + (CheckWeaponAndAttachmentUnlocked(2) ? "" : "(" + CustomFunctions.TranslateText("Shop") + ")")));
        }
        if (weaponsManager.weapons[localSelectedGun].GetComponent<Gun>().silencer != null) {
            attachmentIds.Add(4);
            attachmentDropdown.options.Add(new Dropdown.OptionData(CustomFunctions.TranslateText("Silencer") + " " + (CheckWeaponAndAttachmentUnlocked(4) ? "" : "(" + CustomFunctions.TranslateText("Shop") + ")")));
        }
        if (weaponsManager.weapons[localSelectedGun].GetComponent<Gun>().gunId != 9) { //not shotgun
            attachmentIds.Add(5);
            attachmentDropdown.options.Add(new Dropdown.OptionData(CustomFunctions.TranslateText("Extended Mag") + " " + (CheckWeaponAndAttachmentUnlocked(5) ? "" : "(" + CustomFunctions.TranslateText("Shop") + ")")));
        }
        attachmentDropdown.value = MyPlayerPrefs.GetInt("gun" + localSelectedGun + "AttachmentRaw");

        attachmentDropdown.captionText.text = attachmentDropdown.options[attachmentDropdown.value].text;
        OnAttachmentChanged();
    }
    bool CheckWeaponAndAttachmentUnlocked(int attachmentId) {
        return localData.myData.weaponsUnlocked.ContainsKey(localSelectedGun) && localData.myData.weaponsUnlocked[localSelectedGun].attachments.Contains(attachmentId);
    }
    public void OnPerkChanged() {
        perkDisplayer.sprite = perkSprites[perksDropdown.value];

        if (perksDropdown.value == 0|| localData.myData.perksUnlocked.Contains(perksDropdown.value)) {

            MyPlayerPrefs.SetInt("chosenPerk", perksDropdown.value);
                MyPlayerPrefs.SetInt("pk", perksDropdown.value); //for local saving
            perksDropdown.transform.GetChild(0).GetComponent<Image>().enabled = false;
        } else
            perksDropdown.transform.GetChild(0).GetComponent<Image>().enabled = true;
        if (MyPlayerPrefs.GetString("language") == "Chinese") {
            perkText.text = localData.perkChineseDescriptions[perksDropdown.value];
        } else {
            perkText.text = localData.perkDescriptions[perksDropdown.value];
        }


      }
    public void OnAttachmentChanged() {
        attachmentDisplay.sprite = attachmentSprites[attachmentIds[attachmentDropdown.value]];
        Gun g = weaponsManager.weapons[localSelectedGun].GetComponent<Gun>();
        statistics[0].localScale = new Vector2(g.damage / 80f, 1f);
        statistics[1].localScale = new Vector2(0.06f / g.shootCooldown, 1f);
        statistics[2].localScale = new Vector2(g.magSize / 50f, 1f);
        statistics[3].localScale = new Vector2(0.9f / g.recoil, 1f);
        statistics[4].localScale = new Vector2(0.9f / g.reloadTime, 1f);
        if (attachmentDropdown.value == 0 || CheckWeaponAndAttachmentUnlocked(attachmentIds[attachmentDropdown.value])) {
            attachmentDropdown.transform.GetChild(0).GetComponent<Image>().enabled = false;
            MyPlayerPrefs.SetInt("gun" + localSelectedGun + "Attachment", attachmentIds[attachmentDropdown.value]);
            MyPlayerPrefs.SetInt("gun" + localSelectedGun + "AttachmentRaw", attachmentDropdown.value);
        } else {
            //
            attachmentDropdown.transform.GetChild(0).GetComponent<Image>().enabled = true;
        }
    }
    // Update is called once per frame
    void Update() {

    }
}

