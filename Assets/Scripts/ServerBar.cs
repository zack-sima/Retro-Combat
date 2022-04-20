using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ServerBar : MonoBehaviour {
    public readonly string[] gameModes = {"TDM", "TDM (Hard)"};
    public bool canEdit;
    public int gameMode;
    public string ipAddresses;
    public InputField nameInputfield, ipAddressField;
    public Text mapNameDisplayer, pingTex, joinTex;
    public Text gameModeText;
    public ServersSelect masterController;
    void Start() {
        gameMode = -1;
        if (!canEdit) { //official servers
            nameInputfield.readOnly = true;
            ipAddressField.interactable = false;
        }
    }
    public void JoinGame() {
        MyPlayerPrefs.SetInt("singlePlayer", 0);
        MyPlayerPrefs.SetString("ip", ipAddresses);
        SceneManager.LoadScene(1);
    }
    void Update()
    {
        if (gameMode != -1) {
            gameModeText.text = CustomFunctions.TranslateText(gameModes[gameMode]);
        }
    }
}