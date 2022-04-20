using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.UI;

public class DropdownTranslator : MonoBehaviour {
    public string[] chineseTranslation;
    public string[] englishTranslation;
    public Font chineseFontDat;
    private Font englishFontDat;
    public bool constantUpdate;
    void Awake() {
        int originalDropdownValue = GetComponent<Dropdown>().value;
        if (englishFontDat == null)
            englishFontDat = GetComponent<Dropdown>().captionText.font;
        deltaLanguage = MyPlayerPrefs.GetString("language");
        if (MyPlayerPrefs.GetString("language") == "Chinese") {
            foreach (Text i in GetComponentsInChildren<Text>()) {
                i.font = chineseFontDat;
            }
            foreach (Text i in GetComponent<Dropdown>().template.GetComponentsInChildren<Text>()) {
                i.font = chineseFontDat;
            }
            
            int index = 0;
            bool newEnglish = false;
            if (englishTranslation.Length == 0) {
                englishTranslation = new string[GetComponent<Dropdown>().options.Count];
                newEnglish = true;
            }
            foreach (Dropdown.OptionData i in GetComponent<Dropdown>().options) {
                if (newEnglish)
                    englishTranslation[index] = i.text;
                if (chineseTranslation[0] != "")
                    i.text = chineseTranslation[index];
                index++;
            }
            if (chineseTranslation[0] != "")
                GetComponent<Dropdown>().captionText.text = chineseTranslation[originalDropdownValue];
        } else if (englishTranslation != null && englishTranslation.Length != 0 && MyPlayerPrefs.GetString("language") != "") {
                foreach (Text i in GetComponentsInChildren<Text>()) {
                    i.font = englishFontDat;
                }
                int index = 0;
                foreach (Dropdown.OptionData i in GetComponent<Dropdown>().options) {
                    i.text = englishTranslation[index];
                    index++;
                }
                GetComponent<Dropdown>().captionText.text = englishTranslation[originalDropdownValue];
         }
        GetComponent<Dropdown>().value = originalDropdownValue;
    }
    IEnumerator DelayedRepeatCall() {
        yield return null;
        Awake();
    }
    string deltaLanguage = "";
    void Update() {




        if (constantUpdate)
            StartCoroutine(DelayedRepeatCall());

        if (deltaLanguage != MyPlayerPrefs.GetString("language")) {
            Awake();
        }
    }
}




















