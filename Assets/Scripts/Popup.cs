using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Popup : MonoBehaviour {
    public Text optionalText1; //can be assigned if required
    public Text optionalText2;
    public bool isDisconnect; //reconnect for disconnected bot
    public string embedLink;
    public Button primaryButton;

    public void ReloadCurrentScene() {
        NetworkManager net = GameObject.Find("NetworkMaster").GetComponent<NetworkManager>();
        net.CloseWebsockets();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void GoToLink() {
        Application.OpenURL(embedLink);
    }
    public void ChangeScene(int index) {
        NetworkManager net = GameObject.Find("NetworkMaster").GetComponent<NetworkManager>();
        if (!net.isSinglePlayer)
            net.CloseWebsockets();
        MyPlayerPrefs.SetInt("playAdsTimer", MyPlayerPrefs.GetInt("playAdsTimer") + 1);
        SceneManager.LoadScene(index);
    }
    public void ChangeScene() {
        ChangeScene(0);
    }
    public void DismissPopup() {
        Destroy(gameObject);
    }
    public void ResumeGame() {
        //unpause the game
        Cursor.lockState = CursorLockMode.Locked;
        NetworkManager net = GameObject.Find("NetworkMaster").GetComponent<NetworkManager>();
        net.paused = false;
        net.chatInput.DeactivateInputField();
        DismissPopup();
    }
    void Start() {
        Cursor.lockState = CursorLockMode.None;
        if (isDisconnect && GameObject.Find("NetworkMaster").GetComponent<NetworkManager>().player.useBotControls && MyPlayerPrefs.GetInt("rejoin") == 1) {
            //auto reconnect to game
            ChangeScene(1);
        }
    }
}