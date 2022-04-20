using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SPLobbyManager : MonoBehaviour{
    public InputField botInput;
    public Dropdown botDifficultDropdown, mapDropdown, gameModedropdown;
    public Sprite[] mapImages;

    public Image mapDisplay;
    public void ToScene(int index) {
        try {
            MyPlayerPrefs.SetInt("botCount", int.Parse(botInput.text));
            if (int.Parse(botInput.text) > 25) {
                MyPlayerPrefs.SetInt("botCount", 25);
            }
        } catch {
            MyPlayerPrefs.SetInt("botCount", 5);
        }
        MyPlayerPrefs.SetInt("difficulty", botDifficultDropdown.value);
        MyPlayerPrefs.SetInt("spMap", mapDropdown.value);
        MyPlayerPrefs.SetInt("spGameMode", gameModedropdown.value);
        MyPlayerPrefs.SetInt("singlePlayer", 1);
        SceneManager.LoadScene(index);
    }
    public void UpdatedPhoto() {
        if (mapImages.Length > 0)//lkm
        mapDisplay.sprite = mapImages[mapDropdown.value];
    }

    public void Start() {
        if (MyPlayerPrefs.GetInt("botCount") != 0)
            botInput.text = MyPlayerPrefs.GetInt("botCount").ToString();
        botDifficultDropdown.value = MyPlayerPrefs.GetInt("difficulty");
        mapDropdown.value = MyPlayerPrefs.GetInt("spMap");
        gameModedropdown.value = MyPlayerPrefs.GetInt("spGameMode");
        UpdatedPhoto();
    }
    void Update()
    {
        
    }
}








