using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class SoldierData {
    public SoldierAnimator player;
    public Vector3 position;
    Vector3 deltaPosition;
    public Vector3 rotation;
    public string playerName;
    public int score; //local only
    public bool dying, deltaDying;
    public bool moving, running;
    public float pickupGunRotation;
    public bool online; //this is counted every update network
    public int weapon;
    public float spineRotation, crouchRotation;
    public int shootingCount, shootingCode, deltaShootingCode; //if the codes don't match shooting is updated
    public void UpdatePlayer(float pollTime, bool firstTime) { //will not activate shooting if firsttime
        player.dying = dying;

        if (deltaDying && !dying) { //teleport if player is respawning
            player.dyingAnimating = false;
            player.InterpolatePlayer(position, rotation, spineRotation, crouchRotation, pickupGunRotation, 0f);
        } else {
            player.InterpolatePlayer(position, rotation, spineRotation, crouchRotation, pickupGunRotation, pollTime);
        }
        player.movingForward = moving;
        player.running = running;
        if (player.dying) {
            player.movingForward = false;
            player.running = false;
        }

        if ((player.instantiatedGun == null || player.instantiatedGun.gunId != weapon) && !player.isVehicle) {
            player.shootingCumulatedStorage = 0;
            player.SetGun(player.weapons[weapon]);
        }
        if (deltaShootingCode != shootingCode) {
            deltaShootingCode = shootingCode;
            if (!firstTime)
                player.shootingCumulatedStorage += shootingCount;
        }
        deltaDying = dying;
        deltaPosition = position;
    }
}
//this script organizes the networking for both native and html5 sockets
public class NetworkManager : MonoBehaviour
{
    Uri u;
    public bool localNetworking;
    public bool isMobile = false;



    public Image loadingScreen;
    public float callTime;

    [HideInInspector]
    public int playerId;
    public int mapId;
    public SoldierAnimator player;
    public PlayerDatas localData;
    public NativeWebsocket nativeWeb;
    public WebGLWebsocket webGLWebsocket;
    public Dictionary<int, SoldierData> playerInformation;
    public GameObject playerPrefab, disconnectPrefab, pausePrefab;
    public GameObject maxoutPrefab, mapicon, oldVersionPrefab;
    public Camera pregameCamera; //disabled when player successfully joins
    public Map map;
    public Map[] maps;
    public InputField chatInput;
    public Image hitMarkerDisplay, scopeDisplay;
    public RectTransform crossHair;
    public Text ammoTex;
    public Transform chatParent;
    public Text messageBox;
    public Image gameFinishedImage;
    public Text score1;
    public Text score2;
    public Text timerText;
    public Image healthDisplay;
    public Button vehicleRidingButton;
    public RawImage bigMap, smallMap;
    bool firstTime = true;
    
    
    //mobile ui elelments
    public GameObject ballRoller;
    public RectTransform fireLeft, fireRight;

    [HideInInspector]
    public Vector2 fireLeftPoint, fireRightPoint;
    public GameObject[] mobileUI;
    [HideInInspector]
    public int killerId = -1, gameMode = -1;
    public AudioSource hitMarker;




    public void RideVehicle() {
        player.RideVehicle();
    }
    public void TurnOnMarker() {
        StartCoroutine(CrossMarkerOn());
    }
    IEnumerator CrossMarkerOn() {
        hitMarkerDisplay.enabled = true;
        for (float i = 0f; i < 0.07f; i += Time.deltaTime) { yield return null; }
        hitMarkerDisplay.enabled = false;


        hitMarker.Play();
    }
    public bool cannotChangeLatest; //system messages that cannot be overridden
    public string latestMessage; //this message is sent to server
    public void ChatAMessage(string message) {
        if (!cannotChangeLatest) {
            latestMessage = message;
            chatInput.text = "";
        }
    }
    public void PlayerChatMessages() {
        if (chatInput.text.Replace(" ", "") != "")
            ChatAMessage(MyPlayerPrefs.GetString("playerName") + ": " + chatInput.text);
    }
    bool chatOn = true;
    //mobile feature.
    public void ToggleChat() {
        chatOn = !chatOn;
        if (!chatOn) {
            chatParent.Translate(Vector3.up * 300f * Screen.width / 1200f);
        } else {
            chatParent.Translate(-Vector3.up * 300f * Screen.width / 1200f);
        }
    }
    public bool isSinglePlayer;
    void Start() {

        UnityEngine.Random.InitState(DateTime.Now.Second);
        isSinglePlayer = MyPlayerPrefs.GetInt("singlePlayer") == 1;

        player.useBotControls = MyPlayerPrefs.GetInt("bot") == 1;
        print(MyPlayerPrefs.GetString("ip"));
    #if !UNITY_EDITOR
        if (SystemInfo.deviceType == DeviceType.Handheld)
            isMobile = true;
        else
            isMobile = false;
#endif
        if (!isMobile) {
            foreach (GameObject i in mobileUI)
                Destroy(i.gameObject);
        }
        
        playerInformation = new Dictionary<int, SoldierData>();
        playerId = -1; //game will only start when playerId becomes a position integer
        player.playerId = -1;
        player.sensitivity *= 0.2f + MyPlayerPrefs.GetFloat("sensitivity") * 2.1f;
        if (isSinglePlayer) {
            StartLocalGame();
        } else {
            loadingScreen.transform.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (localNetworking) {
                u = new Uri("ws://localhost:8000/0/server/" + MyPlayerPrefs.GetString("playerName") + "/" + (MyPlayerPrefs.GetInt("alliance") - 1).ToString() + "/" + CustomFunctions.GetServerVersion());
            } else {
                u = new Uri("ws://" + MyPlayerPrefs.GetString("ip") + "/server/" + MyPlayerPrefs.GetString("playerName") + "/" + (MyPlayerPrefs.GetInt("alliance") - 1).ToString() + "/" + CustomFunctions.GetServerVersion());
            }


#if UNITY_WEBGL && !UNITY_EDITOR
        player.sensitivity *= 0.16f;
        webGLWebsocket.BeginWebsocket(u.ToString());
#else
            nativeWeb.EstablishWebsocket(u);
#endif
        }
    }
    //will not do any networking
    public void StartLocalGame() {
        player.mainCam.GetComponent<Camera>().enabled = true;
        Destroy(timerText.gameObject);
        Destroy(gameFinishedImage.gameObject);
        Destroy(chatInput.transform.parent.gameObject);
        
        player.playerId = 0;
        if (MyPlayerPrefs.GetInt("alliance") == 2) {
            player.playerId = 1;
        }
        ReceivedId(player.playerId, MyPlayerPrefs.GetInt("spMap"), MyPlayerPrefs.GetInt("spGameMode"));
        playerInformation.Add(player.playerId, new SoldierData());
        playerInformation[player.playerId].player = player;
        player.RespawnPlayer();
        for (int j = 1; j < MyPlayerPrefs.GetInt("botCount") + 1; j++) {

            int i = j;
            if (player.playerId == 1)
                i += 1;
            playerInformation.Add(i, new SoldierData());
            playerInformation[i].player = Instantiate(playerPrefab).GetComponent<SoldierAnimator>();

            playerInformation[i].player.soldierCountry = (SoldierCountry)(i % 2);
            playerInformation[i].player.playerId = i;
            playerInformation[i].player.masterController = this;
            playerInformation[i].player.playerTeam = i % 2;
            playerInformation[i].player.isLocalBot = true;
            playerInformation[i].playerName = CustomFunctions.TranslateText("Bot") + "_" + j;

            playerInformation[i].player.useBotControls = true;
            playerInformation[i].player.RespawnPlayer();
            playerInformation[i].player.AssignSkin();

            playerInformation[i].player.PickRandomWeapon();
            playerInformation[i].player.randomBotRotation = 1;


        }
        player.StartGame();
    }
    public void CloseWebsockets() {
#if UNITY_WEBGL && !UNITY_EDITOR
        webGLWebsocket.CloseWebsocket();
#else
        nativeWeb.CloseWebSocket();
#endif
    }
    public void SwapScoreEnabled() {
        score1.enabled = !score1.enabled;
        score2.enabled = !score2.enabled;
    }

    public void SpawnPopup(GameObject popup) {
        GameObject insItem = Instantiate(popup, GameObject.Find("Canvas").transform);
        insItem.transform.localPosition = Vector3.zero;
    }
    public bool gameStopped = false, paused = false;
    public void TogglePause() {
        if (!paused) {
            SpawnPopup(pausePrefab);
            paused = true;
        }
    }
    
    void Update() {
        if (Input.GetKey(KeyCode.H) && Input.GetKey(KeyCode.I) && Input.GetKey(KeyCode.D)) { //hide ui for trailer
            smallMap.gameObject.SetActive(false);
            bigMap.gameObject.SetActive(false);
            ammoTex.transform.Translate(Vector3.up * 10000f);
            if (chatParent != null) {
                chatParent.gameObject.SetActive(false);
                timerText.gameObject.SetActive(false);
            }
            crossHair.position = Vector3.up * 1023100;
        }
        if (isSinglePlayer) {
            score1.text = "";
            score2.text = "";
            Dictionary<int, int> team1Scores = new Dictionary<int, int>();
            Dictionary<int, int> team2Scores = new Dictionary<int, int>();
            int totalScore1 = 0, totalScore2 = 0;
            foreach (SoldierData d in playerInformation.Values) {
                if (d.player.playerId % 2 == 0) {
                    totalScore1 += d.score;
                } else {
                    totalScore2 += d.score;
                }

            }

            team1Scores.Add(-1, totalScore1);
            team2Scores.Add(-2, totalScore2);
            foreach (SoldierData d in playerInformation.Values) {
                if (d.player.playerId % 2 == 0) {
                    team1Scores.Add(d.player.playerId, d.score);
                } else {
                    team2Scores.Add(d.player.playerId, d.score);
                }

            }
            IterateKeyValuePair(team1Scores);
            IterateKeyValuePair(team2Scores);
        } else if (gameTimer < 0f) {
            gameFinishedImage.transform.position = new Vector2(Screen.width / 2f, Screen.height / 2f);
            if (totalScore2 > totalScore1 && playerId % 2 == 1 || totalScore1 > totalScore2 && playerId % 2 == 0) {
                gameFinishedImage.color = new Color(0f, 1f, 0f, 0.7f);
                gameFinishedImage.transform.GetChild(0).GetComponent<Text>().text = CustomFunctions.TranslateText("Victory!");
            } else if (totalScore2 > totalScore1 && playerId % 2 == 0 || totalScore1 > totalScore2 && playerId % 2 == 1) {
                gameFinishedImage.color = new Color(1f, 0f, 0f, 0.7f);
                gameFinishedImage.transform.GetChild(0).GetComponent<Text>().text = CustomFunctions.TranslateText("Defeat");

            } else {
                gameFinishedImage.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);
                gameFinishedImage.transform.GetChild(0).GetComponent<Text>().text = CustomFunctions.TranslateText("Draw match");

            }
        } else {
            gameFinishedImage.transform.position = new Vector2(9000f, Screen.height / 2f);
        }




        if (isMobile) {
            fireLeftPoint = new Vector2(fireLeft.position.x, fireLeft.position.y);
            fireRightPoint = new Vector2(fireRight.position.x, fireRight.position.y);
        }
        if (!isSinglePlayer) {
            deltaLatency += Time.deltaTime;

            if (deltaLatency > 7f && ((deltaLatency > 10f && (isMobile || MyPlayerPrefs.GetInt("hidcam") == 1)) || !firstTime)) {
                if (!gameStopped) {
                    SpawnPopup(disconnectPrefab);
                }
                gameStopped = true;
            }
            if (gameStopped)
                return;

        }

        if (Input.GetKeyDown(KeyCode.Escape) && !paused) {
            SpawnPopup(pausePrefab);
            paused = true;
        }
        if (!isSinglePlayer) {
            if (Input.GetKeyDown(KeyCode.T))
                chatInput.ActivateInputField();
            if (Input.GetKeyDown(KeyCode.Escape))
                chatInput.DeactivateInputField();
            if (Input.GetKeyDown(KeyCode.Return)) {
                if (chatInput.text.Replace(" ", "") != "")
                    ChatAMessage(MyPlayerPrefs.GetString("playerName") + ": " + chatInput.text);
                if (!isMobile)
                    chatInput.text = "";
                chatInput.DeactivateInputField();
            }
            if (!chatInput.isFocused && chatInput.text != "" && !isMobile)
                chatInput.text = "";
            if (chatInput.text == "" && Input.GetKey(KeyCode.Space))
                chatInput.DeactivateInputField();
            chatInput.text = chatInput.text.Replace("|", "");
            chatInput.text = chatInput.text.Replace("=", "");
            chatInput.text = chatInput.text.Replace("+", "");
            chatInput.text = chatInput.text.Replace("&", "");
        }

        if (!isMobile) {
            if (Input.GetKey(KeyCode.Tab)) {
                score1.enabled = true;
                score2.enabled = true;
            } else {
                score1.enabled = false;
                score2.enabled = false;
            }
        }
    }
    int totalScore1 = 0, totalScore2 = 0; //updated from this functions below
    int deltaPlayerScore = 0;
    protected void IterateKeyValuePair(Dictionary<int, int>dic) {
        foreach (KeyValuePair<int, int> k in dic) {
            string nam = "";
            if (k.Key == -1) {
                nam = CustomFunctions.TranslateText("Team 1");
                totalScore1 = k.Value;
            } else if (k.Key == -2) {
                nam = CustomFunctions.TranslateText("Team 2");
                totalScore2 = k.Value;
            } else if (k.Key == playerId) {
                nam = MyPlayerPrefs.GetString("playerName");
            } else {
                try {
                    nam = playerInformation[k.Key].playerName;
                } catch {
                    //print(playerInformation.Count);
                    //print(k.Key);
                }
            }
            if (k.Key == playerId) {
                score1.text += "(" + CustomFunctions.TranslateText("You") + ") " + nam + "\n";
                if (k.Value > deltaPlayerScore && !isSinglePlayer) {
                    //earn experience
                    localData.myData.xp += 5;
                    localData.SaveFile();
                }
                deltaPlayerScore = k.Value;
            } else {
                if (nam != "")
                    score1.text += nam + "\n";

            }
            if (nam != "")
                score2.text += k.Value + "\n";

        }
        score1.text += "\n";
        score2.text += "\n";
    }
    float deltaLatency = 0f;
    [HideInInspector]
    public int gameTimer = 0, deltaGameTimer = 0; //game countdown
    IEnumerator DelayedRespawn() {
        for (float t = 0f; t < (playerId % 2 == 0 ? 0.012f : 0.25f); t += Time.deltaTime)
            yield return null;
        player.RespawnPlayer();
    }
    public void UpdatePlayers(string str) {
//        print(str);
        try {
            deltaGameTimer = gameTimer;
            deltaLatency = 0f;
            if (score1 == null) //for editor
                return;
            score1.text = "";
            score2.text = "";

            string[] cutList = str.Split('+');
            string[] messages = cutList[cutList.Length - 3].Split('|');
            string[] scoreboard = cutList[cutList.Length - 2].Split('|');
            int time = int.Parse(cutList[cutList.Length - 1]);
            gameTimer = time;
            if (deltaGameTimer < 0 && gameTimer > 0) {
                //respawn player
                StartCoroutine(DelayedRespawn());
            }
            string minutes = (Mathf.Abs(time) / 60).ToString();
            if (minutes.Length == 1) {
                minutes = "0" + minutes;
            }
            string seconds = (Mathf.Abs(time) % 60).ToString();
            if (seconds.Length == 1) {
                seconds = "0" + seconds;
            }
            if (time >= 0) {
                timerText.text = minutes + ":" + seconds;
            } else {
                timerText.text = CustomFunctions.TranslateText("Game starts in:") + " " + minutes + ":" + seconds;

            }


            Dictionary<int, int> team1Scores = new Dictionary<int, int>();
            Dictionary<int, int> team2Scores = new Dictionary<int, int>();


            foreach (string s in scoreboard) {
                if (s.Split('=').Length > 1) {
                    if (int.Parse(s.Split('=')[0]) == -1) {
                        team1Scores.Add(int.Parse(s.Split('=')[0]), int.Parse(s.Split('=')[1]));
                    } else if (int.Parse(s.Split('=')[0]) == -2) {
                        team2Scores.Add(int.Parse(s.Split('=')[0]), int.Parse(s.Split('=')[1]));
                    } else {
                        if (int.Parse(s.Split('=')[0]) % 2 == 0) {
                            team1Scores.Add(int.Parse(s.Split('=')[0]), int.Parse(s.Split('=')[1]));
                        } else {
                            team2Scores.Add(int.Parse(s.Split('=')[0]), int.Parse(s.Split('=')[1]));

                        }
                    }

                }
            }
            IterateKeyValuePair(team1Scores);





            IterateKeyValuePair(team2Scores);

            string msg = "";
            foreach (string s in messages) {
                string myS = s;
                if (s.Split(':').Length == 1) {
                    myS = myS.Replace("blew up self", CustomFunctions.TranslateText("blew up self"));
                    myS = myS.Replace("blew up", CustomFunctions.TranslateText("blew up"));

                    myS = myS.Replace("killed", CustomFunctions.TranslateText("killed"));
                    myS = myS.Replace("left the game", CustomFunctions.TranslateText("left the game"));
                    myS = myS.Replace("joined the game", CustomFunctions.TranslateText("joined the game"));



                }
                msg += myS + "\n";
            }

            msg = msg.Substring(0, msg.Length - 1);
            messageBox.text = msg;
            //messageBox.GetComponent<RectTransform>().sizeDelta = new Vector3(300f, messageBox.flexibleHeight);
            cutList[cutList.Length - 2] = ""; //remove messages from list of informations

            foreach (int key in playerInformation.Keys) {
                playerInformation[key].online = false;
            }
            foreach (string s in cutList) {
                string[] infos = s.Split('=');
                int pid = 0;
                try {
                    pid = Convert.ToInt32("0x" + infos[0], 16);
                } catch {
                    //print(infos[0]);
                }
                if (infos.Length > 2 && pid != playerId) { //legitimate information, not player
                    if (playerInformation.ContainsKey(pid)) {
                        playerInformation[pid].position = new Vector3(float.Parse(infos[1]), float.Parse(infos[2]), float.Parse(infos[3]));
                        playerInformation[pid].rotation = new Vector3(float.Parse(infos[4]), float.Parse(infos[5]), float.Parse(infos[6]));
                        playerInformation[pid].moving = int.Parse(infos[7]) == 1;
                        playerInformation[pid].running = int.Parse(infos[9]) == 1;
                        playerInformation[pid].weapon = int.Parse(infos[10].Substring(1));
                        playerInformation[pid].player.attachmentPrefabId = int.Parse(infos[10][0].ToString());
                        //if (playerInformation[pid].player.instantiatedGun != null && playerInformation[pid].player.instantiatedGun.attachment != playerInformation[pid].player.attachmentPrefabId) {
                        //    playerInformation[pid].player.instantiatedGun.attachment = playerInformation[pid].player.attachmentPrefabId;
                        //    playerInformation[pid].player.instantiatedGun.UpdateAttachments(playerInformation[pid].player.instantiatedGun.attachment);
                        //}
                        //print(playerInformation[pid].player.attachmentPrefabId);
                        playerInformation[pid].dying = int.Parse(infos[13]) == 1;
                        playerInformation[pid].spineRotation = float.Parse(infos[14]);
                        playerInformation[pid].crouchRotation = float.Parse(infos[15]);
                        playerInformation[pid].playerName = infos[16];
                        playerInformation[pid].pickupGunRotation = float.Parse(infos[17]);
                        int vehicleType = int.Parse(infos[18]);
                        if (vehicleType > 0) { //is vehicle
                            if (vehicleType == 1) {
                                if (!playerInformation[pid].player.isVehicle) { //put player in tank
                                    playerInformation[pid].player.nearbyTank = map.tanks[int.Parse(infos[19])];
                                    playerInformation[pid].player.RideVehicle();
                                } else {
                                    Transform ht = playerInformation[pid].player.nearbyTank.tankSetUp.tankHead.transform;
                                    ht.eulerAngles = new Vector3(ht.eulerAngles.x, float.Parse(infos[20]), ht.eulerAngles.z);
                                    playerInformation[pid].player.nearbyTank.tankSetUp.tankTurret.transform.localEulerAngles = new Vector3(float.Parse(infos[21]), 0f, 0f);
                                }
                            } else if (vehicleType == 2) {
                                if (!playerInformation[pid].player.isVehicle) { //put player in tank
                                    playerInformation[pid].player.nearbyPlane = map.airplanes[int.Parse(infos[19])];
                                    playerInformation[pid].player.RideVehicle();
                                } else {
                                    playerInformation[pid].player.nearbyPlane.planeSetUp.currentSpeed = float.Parse(infos[20]);
                                }
                            } else if (vehicleType == 3) { //blow up vehicle
                                if (playerInformation[pid].player.isVehicle) { //put player in tank
                                    if (playerInformation[pid].player.isPlane) {
                                        playerInformation[pid].player.nearbyPlane.BlowUp();
                                    } else {
                                        playerInformation[pid].player.nearbyTank.BlowUp();
                                    }
                                    playerInformation[pid].player.ExitVehicle(true);
                                }
                            }
                        } else { //check to get out of tank
                            if (playerInformation[pid].player.isVehicle) {

                                playerInformation[pid].player.ExitVehicle(true);
                            }
                        }

                        if (infos[8] != "0") {
                            playerInformation[pid].shootingCount = int.Parse(infos[8].Split('|')[0]);
                            playerInformation[pid].shootingCode = int.Parse(infos[8].Split('|')[1]);
                        }
                        playerInformation[pid].online = true;
                        playerInformation[pid].UpdatePlayer(callTime * 2.5f, firstTime);


                    } else if (int.Parse(infos[10 + 1]) != -1) {
                        playerInformation.Add(pid, new SoldierData());
                        playerInformation[pid].player = Instantiate(playerPrefab).GetComponent<SoldierAnimator>();
                        playerInformation[pid].player.soldierCountry = (SoldierCountry)int.Parse(infos[10 + 1]);
                        playerInformation[pid].player.playerId = pid; //assign id
                        playerInformation[pid].player.masterController = this;
                        playerInformation[pid].position = new Vector3(float.Parse(infos[1]), float.Parse(infos[2]), float.Parse(infos[3]));
                        playerInformation[pid].player.playerTeam = pid % 2;
                        playerInformation[pid].online = true;
                        playerInformation[pid].UpdatePlayer(callTime * 2.5f, firstTime);
                    }


                } else if (infos.Length > 2) { //player information
                                               //retrieve damage
                    float damage = float.Parse(infos[12].Split('|')[0]);
                    string sender = infos[12].Split('|')[1];
                    
                    int senderId = int.Parse(infos[12].Split('|')[2]);

                    if (damage > 0 && !player.dying && player.immuneTimer <= 0f) {
                        print("damaged: " + damage + " by " + sender);
                        if (player.isVehicle) {
                            if (player.isPlane) {
                                float prevHp = player.nearbyPlane.health;
                                player.nearbyPlane.health -= damage;
                                if (prevHp > 0f && player.nearbyPlane.health <= 0f) {
                                    killerId = senderId;
                                }
                            } else {
                                float prevHp = player.nearbyPlane.health;
                                player.nearbyTank.health -= damage;
                                if (prevHp > 0f && player.nearbyTank.health <= 0f) {
                                    killerId = senderId;
                                }
                            }
                        } else {
                            player.health -= damage;
                            if (player.health <= 0f) {
                                killerId = senderId;
                                latestMessage = sender + " killed " + MyPlayerPrefs.GetString("playerName");
                                cannotChangeLatest = true;
                            }
                        }
                    }

                }
            }
            if (firstTime) {
                Destroy(loadingScreen.gameObject);
                player.playerId = playerId;
                player.StartGame();
                firstTime = false;
            }

            foreach (int key in playerInformation.Keys) { //players that do not exist upon receiving data are destroyed
                if (!playerInformation[key].online) {
                    Destroy(playerInformation[key].player.gameObject);
                    StartCoroutine(RemoveKey(key));
                }
            }
        } catch (Exception e) {
            //piece the strings together?
            print(str);
            print(e);
        }
    }
    IEnumerator RemoveKey(int key) {
        yield return null;
        playerInformation.Remove(key);
    }
    public float FormatFloat(float input, int digits) {
        return (int)(input * Mathf.Pow(10, digits)) / Mathf.Pow(10f, digits);
    }
    public void ResetTriggers() { //don't reset things in getplayerdata because there is a chance for packet to be blocked
        player.shootingCumulationDelta = 0;
        player.damagedTargets = new Dictionary<int, float>();
        latestMessage = "";
        cannotChangeLatest = false;
        killerId = -1;
    }
    //for uploading
    public string GetPlayerData() {

        string s;
        if (player.isVehicle) {
            if (!player.isPlane) {
                s = FormatFloat(player.nearbyTank.transform.position.x, 3) + "=" + FormatFloat(player.nearbyTank.transform.position.y, 3) + "=" + FormatFloat(player.nearbyTank.transform.position.z, 3) + "=";
                s += FormatFloat(player.nearbyTank.transform.eulerAngles.x, 1) + "=" + FormatFloat(player.nearbyTank.transform.eulerAngles.y, 2) + "=" + FormatFloat(player.nearbyTank.transform.eulerAngles.z, 1) + "=";
            } else {
                s = FormatFloat(player.nearbyPlane.transform.position.x, 3) + "=" + FormatFloat(player.nearbyPlane.transform.position.y, 3) + "=" + FormatFloat(player.nearbyPlane.transform.position.z, 3) + "=";
                s += FormatFloat(player.nearbyPlane.transform.eulerAngles.x, 1) + "=" + FormatFloat(player.nearbyPlane.transform.eulerAngles.y, 2) + "=" + FormatFloat(player.nearbyPlane.transform.eulerAngles.z, 1) + "=";
            }
        } else {
            s = FormatFloat(player.transform.position.x, 3) + "=" + FormatFloat(player.transform.position.y, 3) + "=" + FormatFloat(player.transform.position.z, 3) + "=";
            s += FormatFloat(player.gunRotation, 1) + "=" + FormatFloat(player.transform.eulerAngles.y, 2) + "=" + FormatFloat(player.reloadRotation, 3) + "=";
        }
         s  += ((player.moving && (!paused || player.useBotControls)) ? 1 : 0) + "=" + player.shootingCumulationDelta + "=" + (player.running ? 1 : 0) + "=";
        if (player.instantiatedGun == null) {
            s += "00=";
        } else {
            s += MyPlayerPrefs.GetInt("gun" + player.instantiatedGun.gunId + "Attachment").ToString() +  player.instantiatedGun.gunId.ToString() + "=";
        }
        s += (int)player.soldierCountry + "=";
        if (player.damagedTargets.Count == 0) {
            s += "0";
        } else {
            int index = 0;
            foreach (int key in player.damagedTargets.Keys) {
                s += key + "|" + player.damagedTargets[key];
                if (index < player.damagedTargets.Count - 1)
                    s += "|";
                index++;
            }
        }

        s += "=" + (player.dying ? 1 : 0) + "|" + killerId + "=" + latestMessage + "=" + FormatFloat(player.spineRotation, 3) + "=" + FormatFloat(player.crouchRotation, 3) + "=" + FormatFloat(player.pickupGunRotation, 3) + "=";
        if (player.isVehicle) { //whether in vehicle/airplane or not
            if (player.isPlane) {
                s += "2=" + player.nearbyPlane.planeId + "=" + FormatFloat(player.nearbyPlane.planeSetUp.currentSpeed, 1);
            } else {
                s += "1=" + player.nearbyTank.tankId + "=" + FormatFloat(player.nearbyTank.tankSetUp.tankHead.transform.eulerAngles.y, 2) + "=" + FormatFloat(player.nearbyTank.tankSetUp.tankTurret.transform.localEulerAngles.x, 1);
            }
        } else if (player.diedFromVehicle) { //b
            s += "3";
        } else {
            s += "0";
        }

        return s;
    }
    public void ReceivedId(int id, int mapNum, int gameMode) {
        if (id == -2) { //server is maxed out
            if (!gameStopped)
                SpawnPopup(maxoutPrefab);
            gameStopped = true;

            Cursor.lockState = CursorLockMode.None;
            return;
        } else if (id == -3) {
            if (!gameStopped)
                SpawnPopup(oldVersionPrefab);
            gameStopped = true;

            Cursor.lockState = CursorLockMode.None;
            return;
        }
        if (id != playerId) {
            this.gameMode = gameMode;
            playerId = id;
            mapId = mapNum;
            //choose map here
            int index = 0;
            foreach (Map m in maps) {
                if (index != mapId) {
                    Destroy(m.gameObject);
                } else {
                    map = m;
                }
                index++;
            }
        }
       

    }
}
