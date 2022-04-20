using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Translator : MonoBehaviour {
    public string chineseTranslation;
    string englishTranslation;
    public bool constantUpdate, thickChinese;
    public Font chineseFontDat;
    private Font englishFontDat;

    void Start() {
        englishFontDat = GetComponent<Text>().font;
        englishTranslation = GetComponent<Text>().text;
        if (!constantUpdate) {
            if (MyPlayerPrefs.GetString("language") == "Chinese") {
                GetComponent<Text>().font = chineseFontDat;
                if (chineseTranslation != "")
                   GetComponent<Text>().text = chineseTranslation;
                if (thickChinese)
                    GetComponent<Text>().fontStyle = FontStyle.Bold;
            } else {
                GetComponent<Text>().text = englishTranslation;
                GetComponent<Text>().font = englishFontDat;
            }
        }
    }
    void Update() {
        if (constantUpdate) {
            if (MyPlayerPrefs.GetString("language") == "Chinese") {
                GetComponent<Text>().font = chineseFontDat;

                if (thickChinese)
                    GetComponent<Text>().fontStyle = FontStyle.Bold;
                GetComponent<Text>().text = chineseTranslation;
            } else {
                GetComponent<Text>().text = englishTranslation;
                GetComponent<Text>().font = englishFontDat;

            }
        }
    }
}