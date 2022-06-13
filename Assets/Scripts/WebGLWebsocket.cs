
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class WebGLWebsocket : MonoBehaviour {
    public NetworkManager networkMaster;
#if !UNITY_EDITOR && UNITY_WEBGL
    [DllImport("__Internal")]
    private static extern void WebSocketInit(string uri);

    [DllImport("__Internal")]
    private static extern void WebSocketSend(string message);

    [DllImport("__Internal")]
    private static extern void WebSocketClose();

    public void BeginWebsocket(string uri) {
        WebSocketInit(uri);
    }
    public void CloseWebsocket() {
        WebSocketClose();
    }
    string lastString = "";
    public void ReceivedWebsocket(string message) {
        //code is here
        bool isJson = true;
        message = lastString + message;
        try {
            NetworkManager.Room roomTest = (NetworkManager.Room)MyJsonUtility.FromJson(typeof(NetworkManager.Room), message);
        } catch {
            isJson = false;
        }
        if (!isJson && message.Split('|').Length != 3) {
            //cannot be parsed so is stored
            lastString += message;
            print("waiting");
            return;
        }
        
        if (!isJson && message.Split('|').Length == 3) {
            networkMaster.ReceivedId(int.Parse(message.Split('|')[0]), int.Parse(message.Split('|')[1]), int.Parse(message.Split('|')[2]));
        } else if (isJson) {
            networkMaster.UpdatePlayers(message);
        }
        lastString = "";
    }
    public void OnConnected() {
        StartCoroutine(TimedRetriver());
    }
    IEnumerator TimedRetriver() {
        while (true) {
            for (float i = 0f; i < networkMaster.callTime; i += Time.deltaTime)
                yield return null;
            try {
                if (networkMaster.playerId == -1) {
                    WebSocketSend("requestId");
                } else {
                    WebSocketSend(networkMaster.GetPlayerData());
                }
                networkMaster.ResetTriggers();

            } catch {
                Debug.Log("error_2");
            }
        }
    }
#endif
}

