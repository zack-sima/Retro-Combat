using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using Unity.Collections.LowLevel.Unsafe;
//this is the standalone implementation of networking (disabled on Html5 builds)
public class NativeWebsocket : MonoBehaviour {
    public NetworkManager networkMaster;
    Uri u;
    ClientWebSocket cws = null;
    ArraySegment<byte> buf = new ArraySegment<byte>(new byte[1024]);
    public void EstablishWebsocket(Uri uri) {
        u = uri;
        Connect();
    }

    async void Connect() {
        cws = new ClientWebSocket();
        try {
            await cws.ConnectAsync(u, CancellationToken.None);
            if (cws.State == WebSocketState.Open) Debug.Log("connected");
            StartCoroutine(TimedRetriver());
        } catch (Exception e) { Debug.Log("woe " + e.Message); }
    }
    public void CloseWebSocket() {
        try {
            cws.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        } catch {
            //cannot close
        }
    }
    bool sendThreadWaiting = false;
    async void SaySomething(string message) {
        sendThreadWaiting = true;
        networkMaster.ResetTriggers();
        ArraySegment<byte> b = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
        await cws.SendAsync(b, WebSocketMessageType.Text, true, CancellationToken.None);
        sendThreadWaiting = false;
        GetStuff();
    }
    IEnumerator TimedRetriver() {
        while (true && networkMaster.callTime != 0f) {
            for (float i = 0f; i < networkMaster.callTime; i += Time.deltaTime)
                yield return null;
            if (!threadWaiting && !sendThreadWaiting) {
                if (networkMaster.playerId == -1)
                    SaySomething("requestId");
                else {
                    SaySomething(networkMaster.GetPlayerData());
                }
            } else {
                print("thread not finished");
            }
        }
    }
    bool threadWaiting = false;

    string lastWebsocket = "";
    async void GetStuff() {
        threadWaiting = true;
        //NOTE: needs to be received completely; check with WebGL websocket
        WebSocketReceiveResult r = await cws.ReceiveAsync(buf, CancellationToken.None);
        threadWaiting = false;

        if (!r.EndOfMessage) {
            lastWebsocket += Encoding.UTF8.GetString(buf.Array, 0, r.Count);
            return;
        }
        
        string message = lastWebsocket + Encoding.UTF8.GetString(buf.Array, 0, r.Count);
        if (message.Split('|').Length == 3) {
            networkMaster.ReceivedId(int.Parse(message.Split('|')[0]), int.Parse(message.Split('|')[1]), int.Parse(message.Split('|')[2]));
        } else if (message != "invalid format") {
            networkMaster.UpdatePlayers(message);
        }
        //empty out
        lastWebsocket = "";
    }
}



