using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RecentMessages : MonoBehaviour {
    string language = "English";
    bool messageReady = false;
    public Image RedDotImage;
    void Start(){
        language = MyPlayerPrefs.GetString("language");
        StartCoroutine(GetMessage());
    }
    public PlayerDatas localData;
    public GameObject popupMsgPrefab;
    string displayedMessage;
    IEnumerator GetMessage() {

        UnityWebRequest r = UnityWebRequest.Get("http://www.retrocombat.com:8001/" + (language == "Chinese" ? "server_msg_chinese" : "server_msg_english"));
        yield return r.SendWebRequest();
        displayedMessage = r.downloadHandler.text.Substring(1, r.downloadHandler.text.Length - 2);
        //returns web time
        //for rewards
        if (language == "Chinese") {
            if (MyPlayerPrefs.GetString("messages_chinese") != displayedMessage)
                RedDotImage.enabled = true;
        } else {
            if (MyPlayerPrefs.GetString("messages_english") != displayedMessage)
                RedDotImage.enabled = true;
        }
        messageReady = true;
    }
    //called to show message
    public void ShowMessage() {
        if (messageReady) {
            if (language == "Chinese") {
                MyPlayerPrefs.SetString("messages_chinese", displayedMessage);
                RedDotImage.enabled = false;
            } else {
                MyPlayerPrefs.SetString("messages_english", displayedMessage);
                RedDotImage.enabled = false;
            }
            localData.CreatePopup(popupMsgPrefab, displayedMessage);
        }
    }
    
    void Update()
    {
        
    }
}
