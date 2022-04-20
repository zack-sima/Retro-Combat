using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SettingsManager : MonoBehaviour{
    public bool isMobile;
    public GameObject[] hideOnMobile;
    public Toggle aimToggle, leanToggle, botToggle, rejoinToggle, graphicsToggle;
    public Slider sensitivitySlider;
    public void SetLanguage(string language) {
        MyPlayerPrefs.SetString("language", language);
    }
    public void ToScene(int index) {
        MyPlayerPrefs.SetInt("aiming", aimToggle.isOn ? 1 : 0);
        MyPlayerPrefs.SetInt("leaning", leanToggle.isOn ? 1 : 0);
        MyPlayerPrefs.SetFloat("sensitivity", sensitivitySlider.value);


        MyPlayerPrefs.SetInt("thirdPerson", botToggle.isOn ? 1 : 0);

        if (!isMobile) {
            //MyPlayerPrefs.SetInt("rejoin", rejoinToggle.isOn ? 1 : 0);
            MyPlayerPrefs.SetInt("hidecam", graphicsToggle.isOn ? 1 : 0);
        } else {
            //MyPlayerPrefs.SetInt("thirdPerson", 0);
            MyPlayerPrefs.SetInt("rejoin", 0);
            MyPlayerPrefs.SetInt("hidecam", 0);
        }


        SceneManager.LoadScene(index);
    }


    
    public void Start() {
        if (Application.platform == RuntimePlatform.WindowsPlayer) {
            MyPlayerPrefs.SetInt("bot", 1);
            MyPlayerPrefs.SetInt("rejoin", 1);
        }
#if !UNITY_EDITOR
        if (SystemInfo.deviceType == DeviceType.Handheld)
            isMobile = true;
        else
            isMobile = false;
#endif
        sensitivitySlider.value = MyPlayerPrefs.GetFloat("sensitivity");
        aimToggle.isOn = MyPlayerPrefs.GetInt("aiming") == 1;
        botToggle.isOn = MyPlayerPrefs.GetInt("thirdPerson") == 1;

        //rejoinToggle.isOn = MyPlayerPrefs.GetInt("rejoin") == 1;
        graphicsToggle.isOn = MyPlayerPrefs.GetInt("hidecam") == 1;



        leanToggle.isOn = MyPlayerPrefs.GetInt("leaning") == 1;

        if (isMobile) {
            foreach (GameObject i in hideOnMobile)
                Destroy(i);
        }
    }
    void Update()
    {
        
    }
}








