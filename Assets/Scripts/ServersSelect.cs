using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ServersSelect : MonoBehaviour {
    public List<ServerBar> serverbars;
    readonly string[] mapNames = { "Shipment", "Warehouse", "Town", "Desert", "Village" };
    public ScrollRect scrollView;
    public GameObject serverBarPrefab;
    public Dropdown regionLockDropdown;


    public GameObject tutorialPrefab;
    void Start() {
        //serverbars[0].ipAddressField.text = MyPlayerPrefs.GetString("customip");
        //serverbars[0].nameInputfield.text = MyPlayerPrefs.GetString("customservername");
        StartCoroutine(LateStart());
    }
    public IEnumerator LateStart() {
        if (MyPlayerPrefs.GetInt("watchedTutorial") == 0) {
            MyPlayerPrefs.SetInt("watchedTutorial", 1);
            GameObject insItem = Instantiate(tutorialPrefab, GameObject.Find("Canvas").transform);
            insItem.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        }
        regionLockDropdown.value = MyPlayerPrefs.GetInt("region");
        yield return null;
        if (MyPlayerPrefs.GetString("savedServers") != "")
            UpdateServers(MyPlayerPrefs.GetString("savedServers"));
        RefreshServers();
    }
    public void ChangeScenes(int sceneId) {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneId);
    }
    public void RefreshServers() { //will update http status
        MyPlayerPrefs.SetInt("region", regionLockDropdown.value);
        StartCoroutine(GetServers());
    }
    IEnumerator GetServers() {
        UnityWebRequest r = null;
        switch (regionLockDropdown.value) {
        case 0:
            r = UnityWebRequest.Get("http://www.retrocombat.com:8001/servers");
            break;
        //case 1:
        //    r = UnityWebRequest.Get("http://cn.retrocombat.com:8001/servers");
        //    break;
        case 1:
            //TODO: TEMPORARY IP REDIRECT TO US SERVER
            r = UnityWebRequest.Get("http://47.88.27.128:8001/servers");
            break;
        }
        
        yield return r.SendWebRequest();
        if (r.downloadHandler.text != "") {
            scrollView.verticalScrollbar.value = 1;
            string message = r.downloadHandler.text.Substring(1, r.downloadHandler.text.Length - 2);
            print(message);
            if (message.Split('+').Length > 2)
                UpdateServers(message);
            StartCoroutine(GoThroughServers());
        }
        
    }
    void UpdateServers(string message) {
        MyPlayerPrefs.SetString("savedServers", message);
        List<string> ips = new List<string>(message.Split('+'));
        foreach (ServerBar s in serverbars) {
            Destroy(s.gameObject);
        }
        serverbars = new List<ServerBar>();
        
        int index = 0;
        foreach (string s in ips) {
            if (s.Split('|').Length == 2) {
                ServerBar insItem = Instantiate(serverBarPrefab, scrollView.content.transform).GetComponent<ServerBar>();
                insItem.transform.position = new Vector2(Screen.width / 2f - 16 * Screen.width / 1200f, Screen.height - 160 * Screen.width / 1200f - index * 100 * Screen.width / 1200f);
                insItem.canEdit = false;
                insItem.ipAddresses = s.Split('|')[0];

                insItem.nameInputfield.text = s.Split('|')[1];
                index++;





                serverbars.Add(insItem);

            }

        }
        scrollView.content.sizeDelta = new Vector2(scrollView.content.sizeDelta.x, index * 100f + 30f);
    }





    IEnumerator GoThroughServers() {
        foreach (ServerBar s in serverbars) {
            StartCoroutine(RequestRefresh(s));
            yield return null;
        }
    }
    IEnumerator RequestRefresh(ServerBar s) {
        float startTime = Time.timeSinceLevelLoad;
        bool error = false;
        UnityWebRequest r = null;
        try {
            s.mapNameDisplayer.text = "-";
            s.gameModeText.text = "-";
            s.gameMode = -1;
            s.pingTex.text = "-";
            s.joinTex.text = CustomFunctions.TranslateText("Join") + " (" + CustomFunctions.TranslateText("Unknown") + ")";
            r = UnityWebRequest.Get("http://" + s.ipAddresses + "/lobby");

        } catch {
            error = true;
        }
        if (!error) {
            yield return r.SendWebRequest();
            if (r.downloadHandler.text != "" && s != null) {
                string message = r.downloadHandler.text.Substring(1, r.downloadHandler.text.Length - 2);
//                print(message);
                if (message.Split('|').Length >= 2) {
                    try {
                        s.mapNameDisplayer.text = CustomFunctions.TranslateText(mapNames[int.Parse(message.Split('|')[0])]);
                        s.joinTex.text = CustomFunctions.TranslateText("Join") + " (" + message.Split('|')[1] + " " + CustomFunctions.TranslateText("Online") + ")";

                        int.TryParse(message.Split('|')[2], out s.gameMode);
                    } catch {
                    }

                    s.pingTex.text = (int)((Time.timeSinceLevelLoad - startTime) * 1000f) + "ms";
                }
            }
        }
    }

    // Update is called once per frame
    void Update() {
        //MyPlayerPrefs.SetString("customip", serverbars[0].ipAddressField.text);
        //MyPlayerPrefs.SetString("customservername", serverbars[0].nameInputfield .text);

    }
}
