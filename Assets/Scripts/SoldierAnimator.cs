using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum SoldierCountry { Russian, American, Chinese, British, French }
public class SoldierAnimator : MonoBehaviour {
    [HideInInspector]
    public Animator animator;
    public float maxGunRotation, minGunRotation, spineRotation, crouchRotation; //spinerotation for leaning
 //[HideInInspector] //recoilDelta is used to record the max recoil that should be brought back down
    public float armRotation, gunRotation, recoilDelta; //armrotation is the actual rotation whilst gunrotation is the one the player controls
                                                        //when reloading or picking up a different weapon, armrotation goes below gunrotation
    public float pickupGunRotation; //is added to armrotation (interpolated if picking up gun or running)
    public float reloadRotation; //added to armrotation, represents gun reload
    [HideInInspector]
    public Transform rightArm, leftArm, rightLeg, leftLeg, rightFoot, leftFoot, gunAnchor;
    [HideInInspector]
    public Transform head, mainCam, mainCamAnchor;
    [HideInInspector]
    public Transform bodyPartsParent; //parts where position and rotation are replaced by guns

    public Transform avatar;
    
    [HideInInspector]
    public GameObject[] weapons;
    public SoldierWeaponsManager weaponsManager;
    public int gunPrefab, gun2Prefab, meleePrefab, grenadePrefab; //replace with different guns
    [HideInInspector]
    public int attachmentPrefabId; //not for player
    //[HideInInspector]
    public Gun instantiatedGun;
    float runningSpeedMult = 1.8f;
    public bool movingForward, movingBackward, running;
    [HideInInspector]
    public bool moving;
    public float sensitivity = 0.7f;
    public bool aiming, dying, dyingAnimating; //is in the animation
    [HideInInspector]
    public Dictionary<int, float> damagedTargets;
    float speed, runSpeed;
    public bool isPlayer;
    [HideInInspector]
    public int playerId, playerTeam;
    public bool useBotControls, isLocalBot; //localbot is single player

    
    public int gun1Mag, gun1Total;
    public int gun2Mag, gun2Total;
    
    public int grenades = 1;
    public float maxHealth;
    //[HideInInspector]
    public float health;
    public SpriteRenderer mapIcon;
    public NetworkManager masterController;
    public TankController nearbyTank;
    public PlaneController nearbyPlane;
    public bool isPlane;

    [HideInInspector]
    public bool diedFromVehicle;
    Vector3 originalRightArmRotation, originalRightLowerArmRotation, originalLeftArmRotation, originalLeftLowerArmRotation, originalRightHandRotation, originalLeftHandRotation, moveDir;
    //these rotations are set when a gun is assigned to player. They are used for rotating arms to specific angles
    public MeshOrganizer russianRootbone, americanRootbone;
    
    public SoldierCountry soldierCountry;

    public bool isVehicle;
    //ride and exit vehicle buttons need to be
    public void RideVehicle() {
        if (dying/* || !masterController.isSinglePlayer*/)
            return;
        if (isVehicle) {
            ExitVehicle(false);
            return;
        }
        if (nearbyTank != null) {
            if (!nearbyTank.interactingWith) {
                nearbyTank.SortPlayer(gameObject);
                isVehicle = true;
            }
            nearbyPlane = null;
        } else {
            if (!nearbyPlane.interactingWith) {
                nearbyPlane.SortPlayer(gameObject);
                isVehicle = true;
                isPlane = true;
            }
            nearbyTank = null;
        }
    }
    public void ExitVehicle(bool canExit) {
        if (nearbyPlane != null && isVehicle && !canExit) {
            nearbyPlane.BlowUp();
            return;
        }
        animator.transform.localPosition = Vector3.zero;
        animator.transform.localRotation = Quaternion.identity;
        if (nearbyTank != null) {
            nearbyTank.tankSetUp.Cam.enabled = false;
            nearbyTank.GetOutOfTank();
        } else {
            try {
                nearbyPlane.GetOutOfPlane();
            } catch(System.Exception e) {
                print(e);
            }
        }
        isVehicle = false;
        isPlane = false;
    }
    void Start() {
        attachmentPrefabId = -1;
        damagedTargets = new Dictionary<int, float>();
        turnRight = Random.Range(0, 2) == 0;
        botRandomMultiplier = Random.Range(0.9f, 1.2f);
        if (isPlayer) {
            gunPrefab = MyPlayerPrefs.GetInt("mainGun");
            gun2Prefab = MyPlayerPrefs.GetInt("pistol");
            if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreHealthLessSpeed) {
                maxHealth *= 1.15f;
            } else if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreDamageLessHealth) {
                maxHealth *= 0.95f;
            }
        }
        if (!isPlayer && !isLocalBot) {
            GetComponent<Rigidbody>().useGravity = false;
        } else {
            GetComponent<Rigidbody>().useGravity = true;
            GetComponent<Collider>().isTrigger = false;
            if (isLocalBot || useBotControls) {
                GetComponent<Rigidbody>().drag = 3.5f;
            }
        }
        touchesStartInShoot = new ArrayList();
        touchesStartOutShoot = new ArrayList();
        weapons = weaponsManager.weapons;
        if (!isPlayer) {
            if (playerId % 2 != masterController.player.playerId % 2) { //not the same team
                Destroy(avatar.gameObject);
            }
            AssignSkin();
        } else {
            Destroy(avatar.gameObject);
        }
    }
    public void AssignSkin() {
        MeshOrganizer meshRoot = null;
        soldierCountry = (SoldierCountry)playerTeam;
        switch (soldierCountry) {
        case SoldierCountry.American:
            meshRoot = americanRootbone;
            Destroy(russianRootbone.gameObject);
            break;
        case SoldierCountry.Russian:
            meshRoot = russianRootbone;
            Destroy(americanRootbone.gameObject);
            break;
        }
        rightArm = meshRoot.rightArm;
        leftArm = meshRoot.leftArm;
        rightLeg = meshRoot.rightLeg;
        rightFoot = meshRoot.rightFoot;
        leftLeg = meshRoot.leftLeg;
        leftFoot = meshRoot.leftFoot;
        gunAnchor = meshRoot.gunAnchor;
        head = meshRoot.head;
        if (!isLocalBot) {
            mainCam = meshRoot.mainCam;
        }
        mainCamAnchor = meshRoot.mainCamAnchor;
        bodyPartsParent = meshRoot.rootSpine;
        animator = meshRoot.animator;
        SetGun(weapons[gunPrefab]);
    }
    //call by networker when joined game
    public void StartGame() {
        if (!masterController.isSinglePlayer) {
            //foreach (TankController i in masterController.map.tanks)
            //    Destroy(i.gameObject);
        }
        
        ResetAllBullets();
        Cursor.lockState = CursorLockMode.Locked;

        if (masterController.gameMode == 1) { //hard mode here
            Destroy(masterController.crossHair.gameObject);

            masterController.bigMap.gameObject.SetActive(false);
            masterController.smallMap.gameObject.SetActive(false);
            masterController.ammoTex.gameObject.SetActive(false);

            if (masterController.isMobile)
                Destroy(masterController.mapicon);
        }

        GetComponent<Rigidbody>().useGravity = true;
        print("startgame");
        if (isPlayer) {
            gunPrefab = MyPlayerPrefs.GetInt("mainGun");
            gun2Prefab = MyPlayerPrefs.GetInt("pistol");
        }
        playerTeam = playerId % 2;
        health = maxHealth;



        mainCam.GetComponent<Camera>().enabled = true;
        print("camenabled");

        //damagedTargets = new Dictionary<int, float>();
        AssignSkin();
        RespawnPlayer();
        
    }
    bool stopReloading;
    bool reloading;
    IEnumerator Reload() {
        if (!instantiatedGun.isMelee && !instantiatedGun.isGrenade) {
            stopReloading = false;
            reloading = true;
            float reloadMultiplier = ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.FastReload && isPlayer) ? 0.85f : 1f;
            float dropTime = instantiatedGun.reloadTime * 0.34f * reloadMultiplier, pickupTime = instantiatedGun.reloadTime * 0.67f * reloadMultiplier;
            instantiatedGun.magazine.transform.parent = leftArm;
            for (float i = 0; i < dropTime; i += Time.deltaTime) {
                reloadRotation += Time.deltaTime * 30f;
                if (stopReloading)
                    break;
                yield return null;
            }
            for (float i = 0; i < pickupTime; i += Time.deltaTime) {
                reloadRotation -= Time.deltaTime * dropTime / pickupTime * 30f;
                if (stopReloading)
                    break;
                yield return null;
            }

            instantiatedGun.magazine.transform.parent = instantiatedGun.transform;
            instantiatedGun.magazine.transform.position = instantiatedGun.magAnchor.position;
            instantiatedGun.magazine.transform.rotation = instantiatedGun.magAnchor.rotation;
            reloadRotation = 0f;

            if (!stopReloading && (isPlayer || isLocalBot)) {
                //reload gun
                if (instantiatedGun.totalBullets >= instantiatedGun.magSize - instantiatedGun.magBullets) {
                    instantiatedGun.totalBullets -= instantiatedGun.magSize - instantiatedGun.magBullets;
                    instantiatedGun.magBullets = instantiatedGun.magSize;
                    if (isPlayer) {
                        mainCam.localRotation = Quaternion.identity;
                        mainCam.localPosition = Vector3.zero;
                    }
                } else {
                    instantiatedGun.magBullets += instantiatedGun.totalBullets;
                    instantiatedGun.totalBullets = 0;
                }
            }

            reloading = false;
            stopReloading = false;
        }
    }
    Coroutine lastAimCoroutine = null;
    float aimingClamped = 1f;
    bool aimingAnimating = false;
    IEnumerator ToggleAim(float aimTime) {
        if (!instantiatedGun.isMelee && !instantiatedGun.isGrenade) {
            if (isPlayer) {
                if (aiming && instantiatedGun.scoped) { //instant snapping out of zooming states if scoped weapons
                    mainCam.GetComponent<Camera>().fieldOfView = 62.0f;
                    masterController.scopeDisplay.enabled = false;
                }
            }
            aimingAnimating = true;
            bool changedClips = false; //change clipping plane in the middle of the transition

            for (float i = aimTime - aimingClamped * aimTime; i < aimTime; i += Time.deltaTime) {
                
                if (i > aimTime / 2f && !changedClips) {
                    changedClips = true;
                }
                float interpolateClamped = i / aimTime;
                if (isPlayer) {
                    if (!aiming) {
                        if (!instantiatedGun.scoped)
                            mainCam.GetComponent<Camera>().fieldOfView = 62.0f - (62f - 62.0f / instantiatedGun.zoomingTimes) * interpolateClamped;
                        mainCam.position = Vector3.Lerp(mainCamAnchor.position, instantiatedGun.GetComponent<Gun>().aimAnchor.position, interpolateClamped);
                    } else {
                        if (!instantiatedGun.scoped)
                            mainCam.GetComponent<Camera>().fieldOfView = 62.0f - (62f - 62.0f / instantiatedGun.zoomingTimes) * (1 - interpolateClamped);
                        mainCam.position = Vector3.Lerp(instantiatedGun.GetComponent<Gun>().aimAnchor.position, mainCamAnchor.position, interpolateClamped);
                    }
                }
                aimingClamped = interpolateClamped;
                yield return null;
            }
            aimingClamped = 1f;
            if (isPlayer) {
                if (!aiming) {
                    mainCam.position = instantiatedGun.GetComponent<Gun>().aimAnchor.position;
                    mainCam.parent = instantiatedGun.transform;
                    mainCam.GetComponent<Camera>().fieldOfView = 62.0f / instantiatedGun.zoomingTimes;
                    if (instantiatedGun.scoped) {
                        masterController.scopeDisplay.enabled = true;
                        mainCam.GetComponent<Camera>().nearClipPlane = 0.12f;
                    }

                } else {
                    masterController.scopeDisplay.enabled = false;
                    mainCam.position = mainCamAnchor.position;
                    mainCam.localRotation = Quaternion.identity;
                    mainCam.parent = mainCamAnchor;
                    mainCam.GetComponent<Camera>().fieldOfView = 62.0f;
                    mainCam.GetComponent<Camera>().nearClipPlane = 0.07f;
                }
            }


            if (aiming && isPlayer) {
                mainCam.localRotation = Quaternion.identity;
                mainCam.localPosition = Vector3.zero;
            }
            aiming = !aiming;
            aimingAnimating = false;
        }
    }
    bool changingWeapon;
    IEnumerator DelayedSetGun(GameObject gun, float timer=-1f, bool resetBullets=false) {
        changingWeapon = true;
        for (float it = 0; it < timer; it += Time.deltaTime) {
            yield return null;
        }
        yield return null;
        if (!dyingAnimating)
            SetGun(gun, resetBullets);
    }
    IEnumerator EnableCamera(Transform parent) {
        yield return null;
        mainCam.transform.parent = parent;
        mainCam.transform.localRotation = Quaternion.identity;
        mainCam.transform.localPosition = Vector3.zero;
    }
    public Vector3 magLocalPosition, magLocalRotation;
    public void SetGun(GameObject gun, bool resetAmmo=false) {
        if (reloading) {
            stopReloading = true;
            StartCoroutine(DelayedSetGun(gun, resetBullets:resetAmmo));
            return;
        }
        if (aiming) {
            if (!aimingAnimating) {
                StartCoroutine(ToggleAim(0.1f));
                aimingAnimating = true;
            }
            StartCoroutine(DelayedSetGun(gun, resetBullets:resetAmmo));
            return;
        } else if (aimingAnimating)
            return;
        if (gun == null) { //could not find gun
            gun = weapons[0]; //placeholder guns so game can still continue without error
        }
        try {
            if (weapons[grenadePrefab] != gun && isPlayer)
                grenading = false;
        } catch {
        }
        shootingCumulatedStorage = 0;
        shootingCumulationDelta = 0;
        changingWeapon = true;
        if (isPlayer) {
            mainCam.parent = null;
        }
        recoil = 0;
        recoilDelta = 0;
        if (!isLocalBot) {
            mainCam.position = mainCamAnchor.position;
            CheckThirdPerson();
        }
        if (instantiatedGun != null && !instantiatedGun.thrown) /*deal with changing weapon after throwing grenads*/ {
            
            if (weapons[gunPrefab].GetComponent<Gun>().gunId == instantiatedGun.gunId) {
                gun1Mag = instantiatedGun.magBullets;
                gun1Total = instantiatedGun.totalBullets;
                print("changeweapon " + instantiatedGun.gunId);
            } else if (weapons[gun2Prefab].GetComponent<Gun>().gunId == instantiatedGun.gunId) {
                gun2Mag = instantiatedGun.magBullets;
                gun2Total = instantiatedGun.totalBullets;
                print("changeweapon");
            } //if not any of these, don't record bullets
            Destroy(instantiatedGun.gameObject);
            instantiatedGun = null;
            StartCoroutine(DelayedSetGun(gun, resetBullets:resetAmmo));
            return;
        }
        //resets rotation to 0 for rotation establishment **important**
        float originalRotationY = transform.eulerAngles.y;
        transform.eulerAngles = Vector3.zero;
        ResetBones(gun);
        originalRightArmRotation = rightArm.eulerAngles;
        originalLeftArmRotation = leftArm.eulerAngles;
        originalRightLowerArmRotation = rightArm.GetChild(0).localEulerAngles;
        originalLeftLowerArmRotation = leftArm.transform.GetChild(0).localEulerAngles;
        originalRightHandRotation = rightArm.GetChild(0).GetChild(0).localEulerAngles;
        originalLeftHandRotation = leftArm.transform.GetChild(0).GetChild(0).localEulerAngles;
        GameObject insItem = Instantiate(gun, gunAnchor);
        insItem.transform.localPosition = Vector3.zero;
        print("setting gun");
        insItem.transform.localRotation = Quaternion.identity;
        insItem.transform.localScale = Vector3.one * Vector3.one.y;
        instantiatedGun = insItem.GetComponent<Gun>();
        instantiatedGun.owner = this;
        if (!instantiatedGun.isMelee && !instantiatedGun.isGrenade && instantiatedGun.gunId != 16) {
            Transform originalParent = instantiatedGun.magazine.parent;
            instantiatedGun.magazine.parent = leftArm;
            magLocalPosition = instantiatedGun.magazine.localPosition;
            magLocalRotation = instantiatedGun.magazine.localEulerAngles;
            instantiatedGun.magazine.parent = originalParent;
        }

        if (weapons[gunPrefab].GetComponent<Gun>().gunId == instantiatedGun.gunId) {
            instantiatedGun.magBullets = gun1Mag;
            instantiatedGun.totalBullets = gun1Total;
        } else if (weapons[gun2Prefab].GetComponent<Gun>().gunId == instantiatedGun.gunId) {
            instantiatedGun.magBullets = gun2Mag;
            instantiatedGun.totalBullets = gun2Total;
        } else if (instantiatedGun.gunId == 16) {
            instantiatedGun.magBullets = 7500;
        }

        //reverts rotation to previous state **important*
        transform.eulerAngles = new Vector3(0f, originalRotationY, 0f);

        if (isPlayer || isLocalBot) {
            if (resetAmmo && instantiatedGun.gunId != 16)
                ResetAllBullets();
            UpdateRotations();
            if (isPlayer)
                StartCoroutine(EnableCamera(mainCamAnchor));
        }
        changingWeapon = false;
        CheckThirdPerson();
    }
    public float lengthRecoil = 0f, lengthRecoilDelta = 0f;
    //called when setting up gun or respawning player
    //resets the player's bones to when suitable for action
    public void ResetBones(GameObject gun) {
        Gun gunController = gun.GetComponent<Gun>();
        List<Transform> bodyParts = new List<Transform>(bodyPartsParent.GetComponentsInChildren<Transform>());
        List<Transform> gunBodyParts = new List<Transform>(gunController.wielderBodyPartsParent.GetComponentsInChildren<Transform>());
        for (int i = 0, j = 0; i < bodyParts.Count; i++) {
            if (bodyParts[i].GetComponent<Magazine>() != null && gunController.magazine != bodyParts[i]) {
                Destroy(bodyParts[i].gameObject, 0.1f);
            }
            if (!bodyParts[i].CompareTag("MainCamera") && !bodyParts[i].CompareTag("Gun") && !bodyParts[i].CompareTag("Player")) {
                if (!bodyParts[i].CompareTag("SkipSkinAssign")) {
                    bodyParts[i].localPosition = gunBodyParts[j].localPosition;
                    bodyParts[i].localRotation = gunBodyParts[j].localRotation;
                }
                j++;
            }
        }
        animator.transform.localRotation = Quaternion.identity;
    }
    void ResetAllBullets() {
        gun1Mag = weapons[gunPrefab].GetComponent<Gun>().magSize;
        gun2Mag = weapons[gun2Prefab].GetComponent<Gun>().magSize;

        if (MyPlayerPrefs.GetInt("gun" + gunPrefab + "Attachment") == 5) {
            gun1Mag = (int)(gun1Mag * 1.330f + 0.5f);
            
        }
        if (MyPlayerPrefs.GetInt("gun" + gun2Prefab + "Attachment") == 5) {
            gun2Mag = (int)(gun2Mag * 1.330f + 0.5f);
        }
        gun1Total = weapons[gunPrefab].GetComponent<Gun>().totalBullets;
        gun2Total = weapons[gun2Prefab].GetComponent<Gun>().totalBullets;


        if (instantiatedGun != null) {
            instantiatedGun.totalBullets = weapons[instantiatedGun.gunId].GetComponent<Gun>().totalBullets;
            if (instantiatedGun.gunId == gunPrefab) {
                instantiatedGun.magBullets = gun1Mag;
            } else if (instantiatedGun.gunId == gun2Prefab) {
                instantiatedGun.magBullets = gun2Mag;
            }
        }
    }
    bool respawningInProgress = false;
    IEnumerator RespawnTimer() {
        respawningInProgress = true;
        for (float i = 0; i < 1.2f; i += Time.deltaTime)
            yield return null;

        
        respawningInProgress = false;
        animator.ResetTrigger("Respawn");
    }
    public void PickRandomWeapon() {
        if (isLocalBot) {
            int[] botAvailableGuns = new int[] { 0, 8, 12, 7, 10, 1, 3, 11, 13, 15 };
            gunPrefab = botAvailableGuns[Random.Range(0, botAvailableGuns.Length)];
        }
    }
    public void RespawnPlayer() {
        diedFromVehicle = false;
        transform.Rotate(0f, Random.Range(-30f, 30f), 0f);
        GetComponent<Rigidbody>().velocity = Vector3.zero;
        StartCoroutine(RespawnTimer());
        if (aiming && !isLocalBot) {
            StartCoroutine(ToggleAim(0f));
            mainCam.localRotation = Quaternion.identity;
        }

        

        StartCoroutine(DelayedSetGun(weapons[gunPrefab], -1f, true));
        if (leftLeg != null)
            PickRandomWeapon();
        spineRotation = 0f;
        crouchRotation = 0f;
        crouching = false;
        leaningRight = false;
        leaningLeft = false;
        grenades = 1;
        if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreGrenadesLessAmmo) {
            grenades = 2;
        }
        
        //respawn
        health = maxHealth;
        animator.SetTrigger("Respawn");

        if (instantiatedGun != null)
            instantiatedGun.ResetMagazines();

        //search for the enemies and spawn as far as possible
        float furthestAwaySpawnpointDistance = 0f;
        List<Vector3> furthestAwaySpawnpoints = new List<Vector3>();
        Vector3 furthestAwaySpawnpoint = masterController.map.spawnpoints[Random.Range(0, masterController.map.spawnpoints.Length)].position;
        bool noEnemiesAlive = true;
        foreach (SoldierData p in masterController.playerInformation.Values) {
            if (!p.dying) {
                noEnemiesAlive = false;
                break;
            }
        }
        if (!noEnemiesAlive) {
            foreach (Transform t in masterController.map.spawnpoints) {
                float closestPoint = Mathf.Infinity;
                foreach (SoldierData p in masterController.playerInformation.Values) {
                    if (p.player.playerTeam != playerTeam && Vector3.Distance(p.player.transform.position, t.position) < closestPoint)
                        closestPoint = Vector3.Distance(p.player.transform.position, t.position);
                }
                if (closestPoint > furthestAwaySpawnpointDistance) {
                    furthestAwaySpawnpointDistance = closestPoint;
                }
            }
            foreach (Transform t in masterController.map.spawnpoints) {
                float closestPoint = Mathf.Infinity;
                foreach (SoldierData p in masterController.playerInformation.Values) {
                    if (p.player.playerTeam != playerTeam && Vector3.Distance(p.player.transform.position, t.position) < closestPoint)
                        closestPoint = Vector3.Distance(p.player.transform.position, t.position);
                }
                if (closestPoint > furthestAwaySpawnpointDistance / 1.7f) {
                    furthestAwaySpawnpoints.Add(t.position);
                }
            }
            if (furthestAwaySpawnpoints.Count != 0) {
                furthestAwaySpawnpoint = furthestAwaySpawnpoints[Random.Range(0, furthestAwaySpawnpoints.Count)];
            }
        }
        

        transform.position = furthestAwaySpawnpoint;
        transform.Translate(Random.Range(-0.35f, 0.25f), 0f, Random.Range(-0.25f, 0.331f));
        dying = false;
        dyingAnimating = false;
    }
    IEnumerator RespawnPlayerTimer(float delay) {
        yield return null;
        respawnCoroutine = null;
        for (float i = 0f; i < delay; i += Time.deltaTime) {
            yield return null;
        }
        if (isPlayer || isLocalBot) {
            RespawnPlayer();
        }
        

    }
    public GameObject botCol, farBotCols;
    public bool turnRight = false; //determines random rotation direction
    bool botMoving = false, deltabm = false;
    public float immuneTimer = 0f;

    float moveDistanceNoRotate = 0f, totalMoveDistance = 0f; //if bot moves enough in one direction it can change dir
    void BotMovingDetect() {
        botMoving = true;
        if (!isVehicle) {
            foreach (Collider i in Physics.OverlapBox(botCol.transform.position, botCol.transform.localScale / 2f, botCol.transform.rotation)) {
                if (!i.isTrigger && i.GetComponent<SoldierAnimator>() == null) {
                    botMoving = false;
                    break;
                }
            }
        }
    }
    GameObject closestVehicle = null;
    float vehicleCheckDelay = 0f; //don't check every frame to not clog things up

    bool hasEnemy = false;
    float botRandomMultiplier = 1f;
    public float randomBotRotation = 0f; //set at respawn
    void BotMovements() { //this controls player automatically
        if (instantiatedGun == null || dying || stopMovements)
            return;

        //check bounds
        bool farDetection = false;
        if (isVehicle) {
            foreach (Collider i in Physics.OverlapBox(nearbyTank.tankBoxTracer.transform.position, nearbyTank.tankBoxTracer.transform.localScale / 2f, nearbyTank.tankBoxTracer.transform.rotation)) {
                if (!i.isTrigger && i.GetComponent<SoldierAnimator>() == null) {
                    farDetection = true;
                    break;
                }
            }
        } else {
            foreach (Collider i in Physics.OverlapBox(farBotCols.transform.position, farBotCols.transform.localScale / 2f, farBotCols.transform.rotation)) {
                if (!i.isTrigger && i.GetComponent<SoldierAnimator>() == null) {
                    farDetection = true;
                    break;
                }
            }
        }
        vehicleCheckDelay -= Time.deltaTime;
        if (!isVehicle && closestVehicle == null && vehicleCheckDelay <= 0f) {
            Quaternion orRotation = transform.rotation;
            vehicleCheckDelay = Random.Range(0.9f, 1.2f);
            Vector3 orPosition = transform.position;
            transform.Translate(Vector3.up);
            foreach (TankController i in masterController.map.tanks) {
                if (i != null && !i.interactingWith && i.requestedBot == null) {
                    transform.LookAt(i.transform);
                    RaycastHit hit;
                    if (Physics.Raycast(transform.position, transform.forward, out hit, 25f)) {
                        if (hit.collider.GetComponent<TankController>() == i) {
                            i.requestedBot = this;
                            closestVehicle = hit.collider.gameObject;
                            print("found tank");
                            break;
                        }
                    }
                }
            }
            if (closestVehicle == null) {
                transform.rotation = orRotation;
            } else {
                transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
            }
            transform.position = orPosition;
        }
        //don't combine!
        if (!isVehicle && closestVehicle != null && !isPlayer && !closestVehicle.GetComponent<TankController>().interactingWith) {
            transform.LookAt(closestVehicle.transform);
            transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
            movingForward = true;
            moving = true;
            transform.Translate(0, 0f, 2.5f * botRandomMultiplier * Time.deltaTime);
            animator.SetFloat("ForwardSpeed", 2.0f);
            animator.SetBool("MovingForward", true);
            if (nearbyTank != null && !nearbyTank.interactingWith) {
                RideVehicle();
            }
        } else {
            SoldierAnimator closestEnemy = null;
            float closestDistance = Mathf.Infinity;
            foreach (SoldierData d in masterController.playerInformation.Values) {
                SoldierAnimator s = d.player;
                if (!s.isPlane && s.playerId % 2 != playerId % 2 && Vector3.Distance(transform.position, s.transform.position) < closestDistance && Vector3.Distance(transform.position, s.transform.position) > 1.5f) {
                    closestDistance = Vector3.Distance(transform.position, s.transform.position);
                    closestEnemy = s;
                }
            }
            hasEnemy = false;
            BotMovingDetect();
            Vector3 originalPosition = instantiatedGun.aimAnchor.localPosition;
            Quaternion originalRotation = instantiatedGun.aimAnchor.localRotation;
            //if (isVehicle) {
            //    nearbyTank.rotateTarget = null;
            //}

            if (closestEnemy != null) {
                Quaternion originalTransformRotation = transform.rotation;
                transform.LookAt(new Vector3(closestEnemy.transform.position.x, closestEnemy.transform.position.y, closestEnemy.transform.position.z));
                transform.rotation = Quaternion.Euler(new Vector3(0f, transform.eulerAngles.y, 0f));
                instantiatedGun.aimAnchor.LookAt(new Vector3(closestEnemy.transform.position.x, closestEnemy.isVehicle ? closestEnemy.transform.position.y : closestEnemy.head.position.y, closestEnemy.transform.position.z));
                if (isVehicle || closestDistance < 30f * Mathf.Sqrt(instantiatedGun.zoomingTimes)) {
                    RaycastHit hit;
                    if (Physics.Raycast(instantiatedGun.aimAnchor.transform.position, instantiatedGun.aimAnchor.forward, out hit, 100f)) {
                        Hitbox h = hit.collider.GetComponent<Hitbox>();
                        TankController hc = hit.collider.GetComponent<TankController>();
                        SoldierAnimator hitSoldier = null;
                        if (h != null)
                            hitSoldier = h.parent;
                        else if (hc != null && hc.interactingWith == true) {
                            hitSoldier = hc.tankSetUp.player.GetComponent<SoldierAnimator>();
                        }
                        if (hitSoldier != null && hitSoldier.playerId % 2 != playerId % 2 &&
                            !hitSoldier.dying && !hitSoldier.dyingAnimating && !hitSoldier.respawningInProgress && hitSoldier.immuneTimer <= 0f) {
                            if (isVehicle) {
                                nearbyTank.rotateTarget = hitSoldier.transform;
                                nearbyTank.rotateTargetDelay = 0.52f;
                            } else {
                                if (closestDistance < 15f && aiming && !aimingAnimating) {
                                    StartCoroutine(ToggleAim(0.25f));
                                }
                                gunRotation = instantiatedGun.aimAnchor.eulerAngles.x;
                                if (gunRotation > maxGunRotation * 2f && gunRotation > 180f) {
                                    gunRotation -= 360f;
                                } else if (gunRotation < minGunRotation * 2f && gunRotation < -180f) {
                                    gunRotation += 360f;
                                }
                                if (gunRotation > maxGunRotation * 2f)
                                    gunRotation = maxGunRotation * 2f;
                                else if (gunRotation < minGunRotation * 2f)
                                    gunRotation = minGunRotation * 2f;

                                transform.rotation = (Quaternion.Euler(new Vector3(0f, instantiatedGun.aimAnchor.transform.eulerAngles.y, 0f)));
                                hasEnemy = true;
                                if (instantiatedGun != null && instantiatedGun.isGrenade) {

                                } else if (!dying && instantiatedGun != null && (instantiatedGun.isMelee || instantiatedGun.magBullets > 0) && instantiatedGun.shootDelay < 0.001f) {
                                    if (closestDistance > 15f && !aiming && !aimingAnimating) {
                                        StartCoroutine(ToggleAim(0.27f));
                                    }
                                    instantiatedGun.ShootAmmo(true);

                                }
                            }
                        }
                    }
                }

                transform.rotation = originalTransformRotation;
                instantiatedGun.aimAnchor.localPosition = originalPosition;
                instantiatedGun.aimAnchor.localRotation = originalRotation;

            }
            
            if ((moveDistanceNoRotate > 12.5f || totalMoveDistance > 75f)) {
                moveDistanceNoRotate = Random.Range(-2f, 7f);
                totalMoveDistance = Random.Range(-12f, 0f);
                if (Random.Range(0, 7) == 1) {
                } else {
                    turnRight = !turnRight;
                }
            }
            float deg = 61f;
            if (!botMoving) {
            } else {
                deg = 10f;
            }
            if (Random.Range(0f, 1f) < 0.1f) { //randomized added movement
                if (turnRight) {
                    if (isVehicle) {
                        nearbyTank.Turn(-deg / 3f * 3.31f * Time.deltaTime * botRandomMultiplier);
                    } else
                        transform.Rotate(0, -deg / 3f * 3.31f * Time.deltaTime * botRandomMultiplier, 0);
                } else {
                    if (isVehicle) {
                        nearbyTank.Turn(deg / 3.92f * 3.51f * Time.deltaTime * botRandomMultiplier);

                    } else {
                        transform.Rotate(0, --deg / 3.92f * 3.51f * Time.deltaTime * botRandomMultiplier, 0);
                    }
                }
            }

            if (!hasEnemy || isVehicle) {
                if (!botMoving || farDetection) {
                    if (turnRight) {
                        if (isVehicle) {
                            if (nearbyTank.rotateTarget == null)

                                nearbyTank.Turn(deg * 5.20f * Time.deltaTime);

                        } else {
                            transform.Rotate(0, deg * 3.31f * Time.deltaTime, 0);
                        }
                        
                    } else {
                        if (isVehicle) {
                            if (nearbyTank.rotateTarget == null)
                                nearbyTank.Turn(deg *-5.1f * Time.deltaTime);

                        } else {
                            transform.Rotate(0, deg * -3.51f * Time.deltaTime, 0);
                        }
                    }
                }
            }

            if (botMoving) {
                if (isGrounded || isLocalBot || isVehicle) {
                    if (hasEnemy) {
                        transform.LookAt(closestEnemy.transform);
                        transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0.02f);
                    }
                    if (!stopMovements) {
                        if (isVehicle) {
                            if (nearbyTank.rotateTarget == null) {
                                nearbyTank.Move(-0.35f);
                            }
                        } else
                        transform.Translate(0, 0f, 2.5f * (!hasEnemy ? 1 : 0.7f) * botRandomMultiplier * Time.deltaTime, Space.Self);

                    }
                }
                totalMoveDistance += Time.deltaTime * 7f;
                moveDistanceNoRotate += Time.deltaTime * 2.5f;

                movingForward = true;
                moving = true;
            } else {
                movingForward = false;
                moving = false;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
            deltabm = botMoving;
            if (isLocalBot && dying) {
                //disable movement animation
                animator.SetBool("MovingForward", false);
                if (!stopMovements)
                GetComponent<Rigidbody>().velocity = new Vector3(0f, GetComponent<Rigidbody>().velocity.y, 0f);

                moveDir = Vector3.zero;
                running = false;
                movingForward = false;
            }
            if (randomBotRotation > 0f) {
                randomBotRotation = 0f;
                StartCoroutine(DelayedSetRandom());
            }

            if (movingForward) {
                animator.SetFloat("ForwardSpeed", 2.0f);
                animator.SetBool("MovingForward", true);
            } else {
                animator.SetBool("MovingForward", false);
            }
        }
    }
    public bool stopMovements;
    IEnumerator DelayedSetRandom() {
        for (float i = 0; i < 20; i++)
            yield return null;
        Random.InitState((int)(Time.time * 10f) + playerId);
        float randomized = Random.Range(-90f, 90f);
        //r.MoveRotation(Quaternion.Euler(0, Random.Range(-90f, 90f), 0));
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, randomized, transform.eulerAngles.z);
        //randomBotRotation -= Time.deltaTime * 221.28f;
    }
    bool crouching = false;
    bool leaningRight = false, leaningLeft = false;
    void PlayerMovements() {
        bool canChangeDir = !animator.GetCurrentAnimatorStateInfo(0).IsTag("motion") && isGrounded; //player will keep going in direction until finished walking the run
        movingForward = false;
        movingBackward = false;
        speed = 2.18f; //initial speed

        if (crouching)
            speed = 1.25f;

        runSpeed = speed;

        
        Vector3 myMoveDir = Vector3.zero; //direction player will be moving in

        
        if (Input.GetKey(KeyCode.LeftShift) && !grenading /*add parameters to prevent running*/) {
            runSpeed *= runningSpeedMult; //speed is multiplied if player is running
        }
        
        if (!dying && !masterController.paused) {
            if (Input.GetKeyDown(KeyCode.C) && !masterController.chatInput.isFocused)
                crouching = !crouching;


            if (MyPlayerPrefs.GetInt("leaning") == 1) {
                if (Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.E) && !masterController.chatInput.isFocused) {
                    leaningLeft = true;
                    leaningRight = false;
                } else if (Input.GetKey(KeyCode.E) && !Input.GetKey(KeyCode.Q) && !masterController.chatInput.isFocused) {
                    leaningLeft = false;
                    leaningRight = true;
                } else {
                    leaningLeft = false;
                    leaningRight = false;
                }
            } else {
                if (Input.GetKeyDown(KeyCode.Q) && !masterController.chatInput.isFocused) {
                    leaningLeft = !leaningLeft;
                    leaningRight = false;
                }
                if (Input.GetKeyDown(KeyCode.E) && !masterController.chatInput.isFocused) {
                    leaningLeft = false;
                    leaningRight = !leaningRight;
                }
            }



            if (Input.GetKey(KeyCode.W) && (isGrounded) && !Input.GetKey(KeyCode.S) && !masterController.chatInput.isFocused) {
                movingForward = true;
                animator.SetFloat("ForwardSpeed", runSpeed != speed ? 2f : 1.33f);
                if (animator.GetFloat("ForwardSpeed") > 1.5f && !aiming && !changingWeapon && !aimingAnimating && !reloading && isGrounded && (!Input.GetMouseButton(0) || instantiatedGun == null || instantiatedGun.isMelee)) {
                    running = true;
                    myMoveDir += Vector3.forward * runSpeed;
                } else {
                    running = false;
                    myMoveDir += Vector3.forward * speed;

                }

            } else if (!canChangeDir) {
                running = false;
            }
            if (canChangeDir) {
                moving = movingForward || movingBackward;
            } else
                moving = true;
            if (!running)
                animator.SetFloat("ForwardSpeed", speed);

            if (Input.GetKey(KeyCode.S) && (isGrounded) && !Input.GetKey(KeyCode.W) && !masterController.chatInput.isFocused) {
                movingForward = false;
                movingBackward = true;
                myMoveDir += Vector3.back * speed;
            }

            if (Input.GetKey(KeyCode.A) && (isGrounded) && !Input.GetKey(KeyCode.D) && !masterController.chatInput.isFocused) {
                if (!movingBackward) {
                    movingForward = true;
                }
                myMoveDir += Vector3.left * 0.7f * speed;
            }
            if (Input.GetKey(KeyCode.D) && (isGrounded) && !Input.GetKey(KeyCode.A) && !masterController.chatInput.isFocused) {
                if (!movingBackward)
                    movingForward = true;
                myMoveDir += Vector3.right * 0.7f * speed;
            }
            if (movingForward || movingBackward) {
                animator.SetBool("MovingForward", true);
            } else {
                animator.SetBool("MovingForward", false);
            }
            //move soldier by player input
            if (myMoveDir != Vector3.zero || canChangeDir)
                moveDir = myMoveDir;


            //set gun rotation based on mouse position deltas
            float deltaRot = -Input.GetAxis("Mouse Y") * 2f * sensitivity;
            if (instantiatedGun != null && instantiatedGun.scoped && aiming)
                deltaRot *= 0.52f;
            if (deltaRot > 10f)
                deltaRot = 10f;
            else if (deltaRot < -12f)
                deltaRot = -12f;

            if (deltaRot > 0 && gunRotation + deltaRot < maxGunRotation || deltaRot < 0 && gunRotation + deltaRot > minGunRotation) {
                //if (deltaRot > 0f && gunRotation < maxGunRotation || deltaRot < 0f && gunRotation > minGunRotation) {
                gunRotation += deltaRot;
                //if (deltaRot < 0f && gunRotation - deltaRot < minGunRotation) {
                //    gunRotation = minGunRotation;
                //} else if (deltaRot > 0f && gunRotation + deltaRot > maxGunRotation) {
                //    gunRotation = maxGunRotation;
                //} else {
                //    gunRotation += deltaRot;
                //}
            }

            
            float multiplier = 1f;
            if (instantiatedGun != null && instantiatedGun.scoped && aiming)
                multiplier *= 0.52f;
            //rotate body based on mouse position deltas (horizontal)
            GetComponent<Rigidbody>().MoveRotation(Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + Input.GetAxis("Mouse X") * 6.3f * multiplier * sensitivity, transform.eulerAngles.z));
        } else {
            //disable movement animation
            animator.SetBool("MovingForward", false);
            //if (!masterController.paused)

            if (!stopMovements)
            GetComponent<Rigidbody>().velocity = new Vector3(0f, GetComponent<Rigidbody>().velocity.y, 0f);
            if (canChangeDir || true) {
                moveDir = Vector3.zero;
                running = false;
                movingForward = false;
            }
        }
        if (!dying) {
            Vector3 locVel = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);



            //temporary solution to getting rid of the walking effect after stopping

            locVel = new Vector3(moveDir.x/* * Time.deltaTime*/ * 0.016f * 80, locVel.y, moveDir.z/* * Time.deltaTime*/* 0.016f * 80);
            if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.D)) {
                locVel = new Vector3(0f, locVel.y, 0f);
            }
            if (!dying && !dyingAnimating && (isGrounded))
                GetComponent<Rigidbody>().velocity = transform.TransformDirection(locVel);
        }


        if (Input.GetMouseButtonDown(0) && !masterController.paused && !masterController.gameStopped) {
            Cursor.lockState = CursorLockMode.Locked;
        }
        if (Input.GetKey(KeyCode.Alpha0))
            Cursor.lockState = CursorLockMode.None;

        if (Input.GetKey(KeyCode.Escape) && !masterController.chatInput.isFocused)
            Cursor.lockState = CursorLockMode.None;

        if (!masterController.paused) {
            
            if (Input.GetKeyDown(KeyCode.Space) && masterController.chatInput.text == "")
                Jump();


            if (Input.GetKeyDown((KeyCode.R)) && instantiatedGun != null && instantiatedGun.totalBullets != 0 && instantiatedGun.magBullets != instantiatedGun.magSize && !instantiatedGun.isMelee && !instantiatedGun.isGrenade  && !masterController.chatInput.isFocused && !reloading && !dying && !dyingAnimating)
                StartCoroutine(Reload());

            if ((MyPlayerPrefs.GetInt("aiming") != 1 && Input.GetMouseButtonDown(1) || (MyPlayerPrefs.GetInt("aiming") == 1 && (Input.GetMouseButtonDown(1) || Input.GetMouseButtonUp(1) && (aimingClamped != 1f || aiming))))
                && !respawningInProgress && !reloading && !changingWeapon && !dying && !dyingAnimating && !masterController.chatInput.isFocused) {
                if (lastAimCoroutine != null && aimingClamped != 1f) {
                    StopCoroutine(lastAimCoroutine);
                    aiming = !aiming;
                }
                lastAimCoroutine = StartCoroutine(ToggleAim(aiming ? 0.2f : instantiatedGun.aimTime));
            }
            if (Input.GetKeyDown(KeyCode.Alpha1) && !grenading && !masterController.chatInput.isFocused && instantiatedGun != null && instantiatedGun.gunId != gunPrefab && !dying && !dyingAnimating) {
                SetGun(weapons[gunPrefab]);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && !grenading && !masterController.chatInput.isFocused && instantiatedGun != null && instantiatedGun.gunId != gun2Prefab && !dying && !dyingAnimating) {
                SetGun(weapons[gun2Prefab]);
            }
            if ((Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.V)) && !grenading && !masterController.chatInput.isFocused && instantiatedGun != null && instantiatedGun.gunId != meleePrefab && !dying && !dyingAnimating) {
                SetGun(weapons[meleePrefab]);
            }
            if ((Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.G)) && grenades > 0 && !masterController.chatInput.isFocused && instantiatedGun != null && instantiatedGun.gunId != grenadePrefab && !dying && !dyingAnimating) {
                if (reloading)
                    stopReloading = true;
                SetGun(weapons[grenadePrefab]);
                grenading = true;
            }
        }
        if (Input.GetMouseButtonDown(0) && !masterController.paused && (reloading || instantiatedGun != null && instantiatedGun.magBullets == 0 && !instantiatedGun.isMelee && !instantiatedGun.isGrenade) && playerId != -1 && !changingWeapon)
            GetComponent<AudioSource>().Play();
        if (!reloading && !changingWeapon) {
            if (instantiatedGun != null && instantiatedGun.isGrenade) {
                
            } else if (!useBotControls && !masterController.paused && !dying && !masterController.chatInput.isFocused &&!dyingAnimating && instantiatedGun != null && (instantiatedGun.isMelee || instantiatedGun.magBullets > 0) && (Input.GetMouseButton(0) && (instantiatedGun.isAuto && !isVehicle || isVehicle && isPlane) || Input.GetMouseButtonDown(0) && !instantiatedGun.isAuto) && instantiatedGun.shootDelay < 0.001f) {
                instantiatedGun.ShootAmmo(true);
            }
        }
    }
    public void CrouchMobile() {
        if (!dying && !masterController.chatInput.isFocused)
            crouching = !crouching;
    }
    public void JumpUpMobile() {
        Jump();
    }
    public void ReloadMobile() {
        if ((!isVehicle && instantiatedGun != null && !instantiatedGun.isMelee && !instantiatedGun.isGrenade) && instantiatedGun.totalBullets != 0 && instantiatedGun.magBullets != instantiatedGun.magSize && !masterController.chatInput.isFocused && !reloading && !dying && !dyingAnimating && !reloading)
            StartCoroutine(Reload());
    }
    public void LeanLeftMobile() {
        leaningLeft = !leaningLeft;
        leaningRight = false;
    }
    public void LeanRightMobile() {
        leaningRight = !leaningRight;
        leaningLeft = false;
    }
    public void SetWeapon1() {
        if (!grenading && !masterController.chatInput.isFocused && instantiatedGun != null && instantiatedGun.gunId != gunPrefab && !dying && !dyingAnimating)
            SetGun(weapons[gunPrefab]);

    }
    public void SetWeapon2() {
        if (!grenading && !masterController.chatInput.isFocused && instantiatedGun != null && instantiatedGun.gunId != gun2Prefab && !dying && !dyingAnimating) {
            SetGun(weapons[gun2Prefab]);
        }
    }
    public void SetWeaponMelee() {
        if (!grenading && !masterController.chatInput.isFocused && instantiatedGun != null && instantiatedGun.gunId != meleePrefab && !dying && !dyingAnimating) {
            SetGun(weapons[meleePrefab]);
        }
    }

    public void SetWeaponGrenade() {
        if (grenades > 0 && !masterController.chatInput.isFocused && instantiatedGun != null && instantiatedGun.gunId != grenadePrefab && !dying && !dyingAnimating) {
            if (reloading)
                stopReloading = true;
            SetGun(weapons[grenadePrefab]);
            grenading = true;
        }
    }
    public void SetAiming() {
        if (!respawningInProgress && !reloading && !changingWeapon && !dying && !dyingAnimating && !masterController.chatInput.isFocused) {
            if (lastAimCoroutine != null && aimingClamped != 1f) {
                StopCoroutine(lastAimCoroutine);
                aiming = !aiming;
            }
            lastAimCoroutine = StartCoroutine(ToggleAim(aiming ? 0.2f : instantiatedGun.aimTime));
        }
    }
    
    public void ToggleBigMap() {
        masterController.bigMap.enabled = !masterController.bigMap.enabled;
        masterController.smallMap.enabled = !masterController.bigMap.enabled;
    }
    public bool isGrounded = false;
    void Jump() {
        if (isGrounded) {
            float jumpValue = 29.3f;
            
            GetComponent<Rigidbody>().AddForce(Vector3.up * jumpValue, ForceMode.Impulse);
        }
    }
    public void InterpolatePlayer(Vector3 position, Vector3 rotation, float spineRotations, float crouchRotations, float p, float time) {
        StartCoroutine(InterpolateTransforms(position, rotation, spineRotations, crouchRotations, p, time));
    }
    IEnumerator InterpolateTransforms(Vector3 position, Vector3 rotation, float spineRotations, float crouchRotation, float ph, float time) {
        if (isVehicle) {
            if (isPlane) {
                Vector3 currentPosition = nearbyPlane.transform.position;
                Vector3 currentRotation = nearbyPlane.transform.eulerAngles;
                if (time == 0f || Vector3.Distance(currentPosition, position) > 25f) {
                    nearbyPlane.transform.position = position;
                    nearbyPlane.transform.eulerAngles = rotation;
                    yield return null;
                } else {
                    for (float i = 0f; i < time; i += Time.deltaTime) {
                        if (nearbyPlane != null) {
                            nearbyPlane.transform.position = Vector3.Lerp(currentPosition, position, i / time);
                            nearbyPlane.transform.rotation = Quaternion.Lerp(Quaternion.Euler(currentRotation), Quaternion.Euler(rotation), i / time);
                        }
                        yield return null;
                    }
                }
            } else {
                Vector3 currentPosition = nearbyTank.transform.position;
                Vector3 currentRotation = nearbyTank.transform.eulerAngles;
                if (time == 0f || Vector3.Distance(currentPosition, position) > 9f) {
                    nearbyTank.transform.position = position;
                    nearbyTank.transform.eulerAngles = rotation;
                    yield return null;
                } else {
                    for (float i = 0f; i < time; i += Time.deltaTime) {
                        if (nearbyTank != null) {
                            nearbyTank.transform.position = Vector3.Lerp(currentPosition, position, i / time);
                            nearbyTank.transform.rotation = Quaternion.Lerp(Quaternion.Euler(currentRotation), Quaternion.Euler(rotation), i / time);
                        }
                        yield return null;
                    }
                }
            }
        } else {
            Vector3 currentPosition = transform.position;
            Vector3 currentRotation = new Vector3(gunRotation, transform.eulerAngles.y, reloadRotation);
            float currentSpineRotation = spineRotation;
            float currentPh = pickupGunRotation;
            float currentCrouchRotation = this.crouchRotation;
            if (time == 0f || Vector3.Distance(currentPosition, position) > 9f) {
                transform.position = position;
                transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
                gunRotation = rotation.x;
                reloadRotation = rotation.z;
                yield return null;
            } else {
                for (float i = 0f; i < time; i += Time.deltaTime) {
                    transform.position = Vector3.Lerp(currentPosition, position, i / time);
                    transform.rotation = Quaternion.Lerp(Quaternion.Euler(0f, currentRotation.y, 0f), Quaternion.Euler(0f, rotation.y, 0f), i / time);
                    gunRotation = currentRotation.x + (rotation.x - currentRotation.x) * (i / time);
                    reloadRotation = currentRotation.z + (rotation.z - currentRotation.z) * (i / time);
                    spineRotation = currentSpineRotation + (spineRotations - currentSpineRotation) * (i / time);
                    pickupGunRotation = currentPh + (ph - currentPh) * (i / time);

                    this.crouchRotation = currentCrouchRotation + (crouchRotation - currentCrouchRotation) * (i / time);
                    yield return null;
                }
            }
        }
    }
    [HideInInspector]
    public int shootingCumulatedStorage; //the number of times the client should should based on the number given by the server (tallied by local player)

    [HideInInspector]
    public int shootingCumulationDelta; //number of bullets shot since last packet was messaged; reset on delivery
    [HideInInspector]
    public float recoil;
    [HideInInspector]
    public bool sideRecoilDir; //this is set every time a shot a fired (a direction gun will move to)
    void UpdateRotations() {
        bodyPartsParent.parent.localPosition = new Vector3(0f, -crouchRotation / 201f, 0f);
        armRotation = gunRotation + pickupGunRotation;
        leftLeg.Rotate(crouchRotation, 0f, 0f);
        rightLeg.Rotate(crouchRotation, 0f, 0f);
        leftLeg.GetChild(0).Rotate(-crouchRotation * 1.92f, 0f, 0f);
        rightLeg.GetChild(0).Rotate(-crouchRotation * 1.92f, 0f, 0f);
        leftFoot.Rotate(crouchRotation, 0f, 0f);
        rightFoot.Rotate(crouchRotation, 0f, 0f);

            Transform t = bodyPartsParent.GetChild(2).GetChild(0).transform;
            t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, 0f);

            //orient arms
            head.localEulerAngles = new Vector3(gunRotation, 0f, 0f);
            rightArm.eulerAngles = originalRightArmRotation;
            
            rightArm.Rotate(armRotation, 0f, 0f, Space.World);
            rightArm.eulerAngles += (transform.eulerAngles);
            leftArm.eulerAngles = originalLeftArmRotation;
        leftArm.Rotate((gunRotation + pickupGunRotation + reloadRotation) * (instantiatedGun == null || !instantiatedGun.isMelee && !instantiatedGun.isGrenade ? 1.2f : 1f), 0f, 0f, Space.World);
            leftArm.eulerAngles += (transform.eulerAngles);
            rightArm.GetChild(0).localEulerAngles = originalRightLowerArmRotation;
            leftArm.GetChild(0).localEulerAngles = originalLeftLowerArmRotation;
            rightArm.GetChild(0).GetChild(0).localEulerAngles = originalRightHandRotation;
            leftArm.GetChild(0).GetChild(0).localEulerAngles = originalLeftHandRotation;


            t.localEulerAngles = new Vector3(t.localEulerAngles.x, t.localEulerAngles.y, spineRotation);
    }

    public bool grenading = false;
    //Vector3 magLocalPosition;
    bool deltaDying;
    float dyingTime = 0f; //if dying for too long manually turn off dying
    bool CheckObstacle() {
        foreach (Collider c in Physics.OverlapSphere(mainCam.position, 0.25f)) {
            if (c.gameObject.tag == "Ground") {
                return true;
            }
        }
        return false;
    }
    void CheckThirdPerson() {
        if (MyPlayerPrefs.GetInt("thirdPerson") == 1) {
            if (!aiming && !aimingAnimating && !dying) {
                if (mainCam != null && (Vector3.Distance(mainCam.position, mainCamAnchor.position) < 1.52f || CheckObstacle())) {
                    mainCam.transform.position = mainCamAnchor.position;
                    for (int i = 0; i < 25; i++) {
                        mainCam.transform.Translate(new Vector3(0.3f, 0.25f, -1.5f) / 25f); //distance maxed out is approx 1.55m
                        if (i > 5f && CheckObstacle()) {
                            break;
                        }
                    }
                }
            }
        }
    }
    void LateUpdate() {
        if (isVehicle && !isPlayer && !isLocalBot) {
            if (shootingCumulatedStorage > 0) {
                if (isPlane) {
                    nearbyPlane.FireGun();
                } else {
                    nearbyTank.FireGun();
                }
                if (0 == 0) { }
            }
            try { } catch { }
        }
        if (isPlayer) {

            CheckThirdPerson();
            if (playerId != -1 &&
                Input.GetKeyDown(KeyCode.M) && !masterController.chatInput.isFocused && !masterController.paused)
                ToggleBigMap();
            if (nearbyTank != null || nearbyPlane != null) {
                //edit text as well later
                if (masterController.vehicleRidingButton != null)
                    masterController.vehicleRidingButton.gameObject.SetActive(true);
                if (Input.GetKeyDown(KeyCode.F)) {
                    RideVehicle();
                }
            } else {
                if (masterController.vehicleRidingButton != null)
                    masterController.vehicleRidingButton.gameObject.SetActive(false);
            }
            if (isVehicle) {
                if (masterController.isMobile)
                    MobileControls();
                if (nearbyTank != null) {
                    masterController.healthDisplay.color = new Color(1f, 2f, 3f, (1f - nearbyTank.health / nearbyTank.maxHealth) * 0.7f);
                } else {
                    masterController.healthDisplay.color = new Color(1f, 2f, 3f, (1f - nearbyPlane.health / nearbyPlane.maxHealth) * 0.7f);

                }
                masterController.crossHair.gameObject.SetActive(false);
                masterController.ammoTex.gameObject.SetActive(false);
                if (useBotControls && !dying) {
                    BotMovements();
                }
            } else {
                masterController.healthDisplay.color = new Color(1f, 2f, 3f, (1f - health / maxHealth) * 0.7f);

                if (masterController.crossHair != null) {
                    masterController.crossHair.gameObject.SetActive(true);
                    masterController.ammoTex.gameObject.SetActive(true);
                }

            }
        } else if (isVehicle) {
            if (!isPlane) {
                if (avatar != null)
                    avatar.gameObject.SetActive(false);
                mapIcon.transform.position = transform.position + Vector3.up * 53.1f;
                mapIcon.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
                if (useBotControls && !dying) {
                    BotMovements();
                }
            }
        }
        //game is not ready yet or in vehicle
        if (playerId == -1 || isVehicle) {
            return;
        }

        if (isPlayer || isLocalBot) {
            if (dying) {
                dyingTime += Time.deltaTime;
                if (dyingTime > 3.5f) { //this is a bug that needs to be fixed
                    RespawnPlayer();
                    dyingTime = 0f;
                }
            } else {
                dyingTime = 0f;
            }
        }
        if (immuneTimer > 0f)
            immuneTimer -= Time.deltaTime;
        else if (!dyingAnimating) {
            if (immuneTimer < 0f)
                immuneTimer = 0f;
        }

        if (avatar != null) {
            avatar.gameObject.SetActive(true);
            if (masterController.player.isVehicle) {
                if (masterController.player.isPlane) {
                    avatar.rotation = masterController.player.nearbyPlane.planeSetUp.cam.transform.rotation;

                } else {
                    avatar.rotation = masterController.player.nearbyTank.tankSetUp.Cam.transform.rotation;
                }
            } else {
                avatar.rotation = masterController.player.mainCam.rotation;
            }
            avatar.GetChild(1).GetComponent<Text>().text = masterController.playerInformation[playerId].playerName;
        }
        if (!isPlayer && playerId % 2 != masterController.playerId % 2) {
            mapIcon.enabled = false;
        } else {
            mapIcon.transform.position = transform.position + Vector3.up * 53.1f;
            mapIcon.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        }


        if (recoil < 0f)
            recoil = 0f;
        if (recoil > 0f) {
            if (instantiatedGun.isMelee) {
                recoil -= Time.deltaTime * 150f;
                gunRotation -= Time.deltaTime * 152f;
            } else {
                float change = Time.deltaTime * 90f;
                if (recoil - change < 0f) {
                    change += recoil - change;
                }
                recoil -= change;
                gunRotation -= change;
                if (aiming) {
                    //bool for one side
                    if (sideRecoilDir)
                        transform.Rotate(0f, change * 0.152f, 0f);
                    else
                        transform.Rotate(0f, -change * 0.15f, 0f);
                }
            }

        } else
        if (recoilDelta > 0f || gunRotation < minGunRotation) {
            if (instantiatedGun != null && !instantiatedGun.isMelee)
                if (recoilDelta > 6f)
                    recoilDelta = 6f;
            if (instantiatedGun != null) {
                float change = Time.deltaTime * (instantiatedGun.isMelee ? 25f : 7.5f);
                if (recoilDelta - change < 0f) {
                    change += recoilDelta - change;
                }
                if (gunRotation < maxGunRotation) {
                    gunRotation += change;
                }
                recoilDelta -= change;
            }
        }
        
        if (!isPlayer && !isLocalBot) {
            if (instantiatedGun != null) {
                
            }
            if (deltaDying && !dying) {
                animator.SetTrigger("Respawn");
            }
            
            if (movingForward) {
                animator.SetBool("MovingForward", true);
            } else
                animator.SetBool("MovingForward", false);

            animator.SetFloat("ForwardSpeed", running ? 2f : 1.33f);
            if (instantiatedGun != null && !instantiatedGun.isMelee && !instantiatedGun.isGrenade) {
                if (reloadRotation > 0.1f && !dying) {
                    instantiatedGun.magazine.transform.SetParent(leftArm, true);
                    instantiatedGun.magazine.transform.localPosition = magLocalPosition;
                    instantiatedGun.magazine.transform.localEulerAngles = magLocalRotation;

                } else if (instantiatedGun != null && instantiatedGun.magAnchor != null/* && instantiatedGun.magazine.transform.parent == leftArm*/) {
                    if (instantiatedGun.magazine.transform.parent == leftArm)
                        instantiatedGun.magazine.transform.parent = instantiatedGun.transform;
                    instantiatedGun.magazine.transform.localPosition = instantiatedGun.magAnchor.localPosition;
                    instantiatedGun.magazine.transform.localRotation = instantiatedGun.magAnchor.localRotation;

                }
            }


        }
        

        if (isPlayer) {
            if (!aimingAnimating && !changingWeapon && (aiming || MyPlayerPrefs.GetInt("thirdPerson") == 0)) {
                if (lengthRecoilDelta > 0f) {
                    lengthRecoilDelta -= Time.deltaTime * 0.75f;
                    mainCam.Translate(Vector3.forward * Time.deltaTime * 0.75f);
                    if (lengthRecoilDelta < 0f) {
                        mainCam.Translate(Vector3.forward * lengthRecoilDelta * 0.75f);
                        lengthRecoilDelta = 0f;
                    }
                } else if (lengthRecoil > 0f) {
                    lengthRecoil -= Time.deltaTime * 0.7f;
                    mainCam.Translate(Vector3.back * Time.deltaTime * 0.7f);
                    if (lengthRecoil < 0f) {
                        if (aiming) {
                            mainCam.position = instantiatedGun.aimAnchor.position;
                        } else {
                            mainCam.localPosition = Vector3.zero;
                        }
                        lengthRecoil = 0f;
                    }
                }
            } else {
                lengthRecoil = 0;
                lengthRecoilDelta = 0;
            }
            if (aiming && !aimingAnimating &&
                instantiatedGun != null) {
                mainCam.localRotation = Quaternion.identity;
            }
            if (!mainCam.GetComponent<Camera>().enabled && instantiatedGun != null && !dying && !changingWeapon && !dyingAnimating && !reloading)
                mainCam.GetComponent<Camera>().enabled = true;
            if (instantiatedGun != null && aiming &&
                instantiatedGun.scoped && !aimingAnimating) {
                mainCam.GetComponent<Camera>().cullingMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3 + 1) | (1 << 5) | (1 << 8);
            } else {
                mainCam.GetComponent<Camera>().cullingMask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3 + 1) | (1 << 5) | (1 << 8) | (1 << 12);
            }
            if (instantiatedGun != null && !masterController.paused && masterController.gameMode != 1) {
                if (aiming && !aimingAnimating || instantiatedGun.isMelee || instantiatedGun.isGrenade) {
                    masterController.crossHair.position = new Vector2(-100000, 0);//.sizeDelta = new Vector2(9999f, 10789f);
                } else {
                    masterController.crossHair.position = new Vector2(Screen.width / 2f, Screen.height / 2);


                    float croSize = 80f * instantiatedGun.spread;
                    if (!isGrounded)
                        croSize *= 1.7f;
                    else if (running)
                        croSize *= 1.5f;
                    else if (Mathf.Abs(GetComponent<Rigidbody>().velocity.x) + Mathf.Abs(GetComponent<Rigidbody>().velocity.z) > 0.07f)
                        croSize *= 1.2f;
                    if (!reloading && !dying && (masterController.isMobile && localShooting || (!masterController.isMobile && Input.GetMouseButton(0))))
                        croSize *= 1.35f;

                    if (masterController.crossHair.sizeDelta.y < croSize) {
                        masterController.crossHair.sizeDelta = new Vector2(masterController.crossHair.sizeDelta.x + Time.deltaTime * 1700f, masterController.crossHair.sizeDelta.y + Time.deltaTime * 1700f);
                        if (masterController.crossHair.sizeDelta.y > croSize) {
                            masterController.crossHair.sizeDelta = new Vector2(croSize, croSize);
                        }
                    } else if (masterController.crossHair.sizeDelta.y > croSize) {
                        masterController.crossHair.sizeDelta = new Vector2(masterController.crossHair.sizeDelta.x - Time.deltaTime * 1700f, masterController.crossHair.sizeDelta.y - Time.deltaTime * 1701f);
                        if (masterController.crossHair.sizeDelta.y < croSize) {
                            masterController.crossHair.sizeDelta = new Vector2(croSize, croSize);
                        }
                    }
                    if (masterController.crossHair.sizeDelta.x > 2000f) //snap back
                        masterController.crossHair.sizeDelta = new Vector2(croSize, croSize);
                }
            }

            if (running) {
                if (pickupGunRotation < 12f)
                    pickupGunRotation += Time.deltaTime * 52f;
                if (pickupGunRotation > 12f)
                    pickupGunRotation = 12f;
            } else if (!grenading) {
                if (pickupGunRotation > 0f)
                    pickupGunRotation -= Time.deltaTime * 52f;
                if (pickupGunRotation < 0f)
                    pickupGunRotation = 0f;
            }


        }
        
        if (!animator.GetCurrentAnimatorStateInfo(0).IsTag("respawn"))
            UpdateRotations();
        if (!isPlayer && !isLocalBot) {
            if (instantiatedGun != null && shootingCumulatedStorage > 0 && instantiatedGun.shootDelay < 0.0001f) {
                instantiatedGun.ShootAmmo(false);
            }
        }

        if ((isPlayer || isLocalBot) && !masterController.gameStopped) {
            if (!dying && !masterController.paused) {
                if (isPlayer) {
                    //determine if grounded
                    isGrounded = false;
                    List<Collider> colliders1 = new List<Collider>(Physics.OverlapBox(transform.position, new Vector3(0.35f, 0.25f, 0.52f), transform.rotation));

                    GetComponent<Rigidbody>().useGravity = true;

                    foreach (Collider i in colliders1) {
                        if (i.CompareTag("Ladder") && Vector3.Distance(

                            i.ClosestPoint(transform.position), GetComponent<Collider>().ClosestPoint(i.ClosestPoint(transform.position))) < 0.2f) {
                            isGrounded = true;
                            if (!masterController.paused)
                                GetComponent<Rigidbody>().useGravity = false;
                            if (GetComponent<Rigidbody>().velocity.y > 2.1f)
                                GetComponent<Rigidbody>().velocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 2.09f, GetComponent<Rigidbody>().velocity.z);
                            break;
                        }
                    }
                    List<Collider> colliders2 = new List<Collider>(Physics.OverlapBox(transform.position, new Vector3(0.17f, 0.1f, 0.17f), transform.rotation));
                    foreach (Collider i in colliders2) {

                        if (i.CompareTag("Ground")) {
                            isGrounded = true;
                            break;
                        }
                    }
                }




                if (leaningLeft && spineRotation < 25f) {
                    spineRotation += Time.deltaTime * 70f;
                    if (spineRotation > 25f)
                        spineRotation = 25f;
                } else if (leaningRight && spineRotation > -25f) {
                    spineRotation -= Time.deltaTime * 70f;
                    if (spineRotation < -25f)
                        spineRotation = -25f;
                } else if (!leaningLeft && !leaningRight) {
                    if (Mathf.Abs(spineRotation) < 0.8f)
                        spineRotation = 0.0f;
                    if (spineRotation > 0f)
                        spineRotation -= Time.deltaTime * 70f;
                    else if (spineRotation < 0f)
                        spineRotation += Time.deltaTime * 68f;
                }
                if (crouching) {
                    crouchRotation += Time.deltaTime * 175f;
                    if (crouchRotation > 52f)
                        crouchRotation = 51.9f;
                } else if (crouchRotation > 0f) {
                    crouchRotation -= Time.deltaTime * 176f;
                    if (crouchRotation < 0f)
                        crouchRotation = 0f;
                }
            }
            if (isLocalBot && dying) {
                animator.SetBool("MovingForward", false);
                moving = false;
                GetComponent<Rigidbody>().velocity = new Vector3(0f, GetComponent<Rigidbody>().velocity.y, 0f);

            }
            if (grenading && !dying && !isLocalBot) {
                if (instantiatedGun != null && !instantiatedGun.thrown) {
                    if (pickupGunRotation < 18f * 0.7f) {
                        pickupGunRotation += Time.deltaTime * 35f;
                        if (pickupGunRotation > 18f * 0.7f)
                            pickupGunRotation = (float)18d * 0.7f;

                    } else if (masterController.isMobile || (!Input.GetKey(KeyCode.G) && !Input.GetKey(KeyCode.Alpha4)) || masterController.chatInput.isFocused || masterController.paused || instantiatedGun.cookingTimer < 0.2f) {
                        //throw grenade if not on hold for cooking
                        instantiatedGun.thrown = true;
                        instantiatedGun.ShootAmmo(isPlayer);
                    }
                } else {
                    if (pickupGunRotation > 0f) {
                        pickupGunRotation -= Time.deltaTime * 90f;
                        if (pickupGunRotation < 0f) {
                            pickupGunRotation = 0f;
                            StartCoroutine(DelayedSetGun(weapons[gunPrefab], 0.7f));
                        }
                    }
                }
            }
            if (reloading && aiming && !aimingAnimating) {
                StartCoroutine(ToggleAim(0.2f));
            }
            if (instantiatedGun != null && (instantiatedGun.magBullets == 0 && (masterController.gameMode != 1 || useBotControls) && instantiatedGun.totalBullets > 0f && instantiatedGun != null && !instantiatedGun.isMelee && !instantiatedGun.isGrenade && !changingWeapon) && !masterController.chatInput.isFocused && !reloading && !dying && !dyingAnimating)
                StartCoroutine(Reload());

            if (health <= 0 && !dyingAnimating && !respawningInProgress &&
                 immuneTimer <= 0f) {
                dying = true;
                stopReloading = true;
                immuneTimer = 3.5f;
            }
            if (isPlayer) {
                if (instantiatedGun != null && (instantiatedGun.isMelee || instantiatedGun.isGrenade)) {
                    masterController.ammoTex.enabled = false;
                } else {
                    masterController.ammoTex.enabled = true;
                }
                //if (!hidden && MyPlayerPrefs.GetInt("hidecam") == 1) {
                //    hidden = true;
                //    foreach (Camera c in GameObject.FindObjectsOfType<Camera>()) {
                //        c.enabled = false;
                //    }
                //    bodyPartsParent.parent.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
                //}
            }


            if (isPlayer) {
                if (masterController.isMobile) {
                    MobileControls();
                } else
                    PlayerMovements(); //only call if this is the local player
            }
            if (useBotControls && !dying) {
                BotMovements();
            }

            

            if (instantiatedGun != null && isPlayer) {
                masterController.ammoTex.text = instantiatedGun.magBullets + "/ " + instantiatedGun.totalBullets;
            }
            if (health < maxHealth && !dying) {
                if (masterController.gameMode != 1) {
                    health += Time.deltaTime * 5.020f;
                } else {
                    health += Time.deltaTime * 1.920f;

                }
            }
        }
        
        if (!dyingAnimating && dying && respawnCoroutine == null && (!changingWeapon || isLocalBot)) {
            bodyPartsParent.parent.localPosition = new Vector3(0f, 0f, 0f);
            float delay = Random.Range(2.9f, 3.7f);
            immuneTimer = 2.5f + delay;
            respawnCoroutine = StartCoroutine(RespawnPlayerTimer(delay));
            dyingAnimating = true;
        }
        if (dying && !animator.GetCurrentAnimatorStateInfo(0).IsTag("respawn") && !respawningInProgress) {
            animator.SetTrigger("SoldierDie");
            animator.ResetTrigger("Respawn");
            //dyingAnimating = true;

        }

        deltaDying = dying;
    }
    private bool isSliding, isRolling, deltaSliding;
    Coroutine respawnCoroutine = null;

    private int sliderIndex, rollerIndex;







    Vector2 startMovingPosition;
    Vector2 deltaSlidingPosition;
    private ArrayList touchesStartInShoot, touchesStartOutShoot;

    private float fireLeftDetectDistance, fireRightDetectDistance;


    private Vector2 slidingAxis;



    private float rollSpeed;

    //mobile controls
    void StartRollingConditionsCheck(bool allowOutside) {
        for (int i = 0; i < Input.touchCount; i++) {
            if (Input.GetTouch(i).position.x < Screen.width / 2f && (isRolling || Vector2.Distance(masterController.fireLeftPoint, Input.GetTouch(i).position) > fireLeftDetectDistance)) {
                isRolling = true;
                rollerIndex = i;

                if (!allowOutside) {
                    startMovingPosition = Input.GetTouch(i).position;
                }
                break;
            }
        }
    }

    bool localShooting = false;
    void MobileControls() {
        if (dying) {
            //disable movement animation
            animator.SetBool("MovingForward", false);
            GetComponent<Rigidbody>().velocity = new Vector3(0f, GetComponent<Rigidbody>().velocity.y, 0f);
            moveDir = Vector3.zero;
            running = false;
            movingForward = false;
            speed = 1f;
        } else {
            if (instantiatedGun != null && !isVehicle) {
                if (!instantiatedGun.isMelee && !instantiatedGun.isGrenade &&
                    instantiatedGun.gunId != gunPrefab && instantiatedGun.gunId != gun2Prefab && instantiatedGun.gunId != 16) {
                    SetGun(weapons[gunPrefab]);
                }
            }
            speed = 1f;
            localShooting = false;
            rollSpeed = 0f;

            bool firstShot = false;
            for (int i = 0; i < Input.touchCount; i++) {
                if (Input.GetTouch(i).phase == TouchPhase.Began && (Vector2.Distance(masterController.fireRightPoint, Input.GetTouch(i).position) < fireRightDetectDistance || Vector2.Distance(masterController.fireLeftPoint, Input.GetTouch(i).position) < fireLeftDetectDistance)) {
                    touchesStartInShoot.Add(Input.GetTouch(i).fingerId);
                    if (touchesStartOutShoot.Contains(Input.GetTouch(i).fingerId))
                        touchesStartOutShoot.Remove(Input.GetTouch(i).fingerId);
                } else if (Input.GetTouch(i).phase == TouchPhase.Began) {
                    touchesStartOutShoot.Add(Input.GetTouch(i).fingerId);
                    if (touchesStartInShoot.Contains(Input.GetTouch(i).fingerId))
                        touchesStartInShoot.Remove(Input.GetTouch(i).fingerId);
                }

                if ((Input.GetTouch(i).phase == TouchPhase.Began || instantiatedGun != null && (instantiatedGun.isAuto && !isVehicle || isVehicle && isPlane)) && touchesStartInShoot.Contains(Input.GetTouch(i).fingerId)) {
                    if (Vector2.Distance(masterController.fireRightPoint, Input.GetTouch(i).position) < fireRightDetectDistance || (Vector2.Distance(masterController.fireLeftPoint, Input.GetTouch(i).position) < fireLeftDetectDistance && !isRolling)) {
                        localShooting = true;
                        if (Input.GetTouch(i).phase == TouchPhase.Began)
                            firstShot = true;
                        if (Vector2.Distance(masterController.fireRightPoint, Input.GetTouch(i).position) < fireRightDetectDistance) {
                            //masterController.fireRight.GetComponent<RectTransform>().localScale = new Vector2(1.35f, 1.34f);
                            masterController.fireRight.GetChild(0).GetComponent<Image>().color = Color.yellow;
                            fireRightDetectDistance = 250f * Screen.width / 1200f;

                        } else {
                            //masterController.fireLeft.GetComponent<RectTransform>().localScale = new Vector2(1.33f, 1.34f);
                            masterController.fireLeft.GetChild(0).GetComponent<Image>().color = Color.yellow;
                            fireLeftDetectDistance = 200f * Screen.width / 1200f;
                        }
                        break;
                    }
                }
            }
            if (!localShooting) {
                fireRightDetectDistance = 95f;
                fireLeftDetectDistance = 71f;
                masterController.fireRight.GetChild(0).GetComponent<Image>().color = Color.white;
                masterController.fireLeft.GetChild(0).GetComponent<Image>().color = Color.white;

                //masterController.fireLeft.GetComponent<RectTransform>().localScale = new Vector2(1.0f, 1.0f);
                //masterController.fireRight.GetComponent<RectTransform>().localScale = new Vector2(1.0f, 1.0f);
            } else {
                if (!useBotControls && !masterController.paused && !dying && !reloading && !masterController.chatInput.isFocused && !dyingAnimating && instantiatedGun != null && (instantiatedGun.isMelee || instantiatedGun.magBullets > 0) && instantiatedGun.shootDelay < 0.001f) {
                    if (isVehicle) {
                        if (isPlane) {
                            nearbyPlane.FireGun();
                        } else {
                            nearbyTank.FireGun();
                        }
                    } else {
                        instantiatedGun.ShootAmmo(true);
                    }
                } else if (firstShot && !masterController.paused && !dyingAnimating && !masterController.chatInput.isFocused && instantiatedGun != null && !instantiatedGun.isMelee && !instantiatedGun.isGrenade && !changingWeapon && instantiatedGun.shootDelay < 0.001f && playerId != -1) {
                    GetComponent<AudioSource>().Play();
                }
            }
            if (!isSliding) {
                for (int i = 0; i < Input.touchCount; i++) {
                    if (Input.GetTouch(i).position.x > Screen.width / 2f) {
                        isSliding = true;
                        sliderIndex = i;

                        StartRollingConditionsCheck(true);
                        break;
                    }
                }
            }
            float airplaneHorizontal = 0f, airplaneVertical = 0f, airplaneTurning = 0f; //for airplane rotations
            //don't combine!
            if (isSliding) {
                if (Input.touchCount <= sliderIndex || Input.GetTouch(sliderIndex).phase == TouchPhase.Ended || Input.GetTouch(sliderIndex).position.x < Screen.width / 2f * 0.5f) {
                    isSliding = false;
                } else {
                    if (deltaSlidingPosition == Vector2.zero || !deltaSliding)
                        slidingAxis = Input.GetTouch(sliderIndex).deltaPosition;
                    else
                        slidingAxis = Input.GetTouch(sliderIndex).position - deltaSlidingPosition;

                    float multiplier = 1f;
                    if (instantiatedGun != null && instantiatedGun.scoped && aiming)
                        multiplier *= 1f / instantiatedGun.zoomingTimes;
                    if (isVehicle) {
                        if (isPlane) {
                            airplaneHorizontal = slidingAxis.x / 7f * sensitivity * multiplier;
                        } else {
                            nearbyTank.RotateXTankHead(slidingAxis.x / 7f * sensitivity * multiplier);
                        }
                    } else {
                        transform.Rotate(0, slidingAxis.x / 7f * sensitivity * multiplier, 0); //moverotationsher
                    }
                    float r = -slidingAxis.y / 7f * sensitivity * multiplier;
                    if (isVehicle) {
                        if (isPlane) {
                            airplaneVertical = -r;
                        } else {
                            nearbyTank.RotateYTankTurret(-r);
                        }
                    } else {
                        if (r > 0 && gunRotation + r < maxGunRotation || r < 0 && gunRotation + r > minGunRotation) {
                            gunRotation += r;
                        }
                    }

                    deltaSlidingPosition = Input.GetTouch(sliderIndex).position;
                }
            }


            moving = false;
            deltaSliding = isSliding;
            if (Input.touchCount == 0) {
                isRolling = false;
                isSliding = false;
            }
            if (!isRolling) {
                masterController.ballRoller.transform.GetChild(0).GetComponent<Image>().color = Color.white;
                Vector3 locVel = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
                float mySpeed = 1f;
                if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreHealthLessSpeed) {
                    mySpeed *= 0.9f;
                } else if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreSwift) {
                    mySpeed *= 1.15f;
                }
                if (isGrounded)
                    locVel = new Vector3(moveDir.x, locVel.y, 0f) * mySpeed * (crouching ? 0.7f : 1f); //60 fps assumption
                GetComponent<Rigidbody>().velocity = transform.TransformDirection(locVel);
                running = false;
                StartRollingConditionsCheck(false);

            } else {
                if (Input.touchCount <= rollerIndex) {

                    StartRollingConditionsCheck(true);

                }
                masterController.ballRoller.transform.GetChild(0).GetComponent<Image>().color = Color.yellow;
                if (Input.touchCount > rollerIndex && Input.GetTouch(rollerIndex).phase == TouchPhase.Ended || Input.GetTouch(rollerIndex).position.x > Screen.width / 2f) {
                    isRolling = false;
                } else if (Input.GetTouch(rollerIndex).position.x < Screen.width / 2f) {
                    moving = true;

                    if (Vector2.Distance(startMovingPosition, Input.GetTouch(rollerIndex).position) > 7f) {
                        Vector3 originalPos = masterController.ballRoller.transform.position;
                        masterController.ballRoller.transform.position = new Vector3(startMovingPosition.x, startMovingPosition.y, 0);
                        masterController.ballRoller.transform.LookAt(new Vector3(Input.GetTouch(rollerIndex).position.x, Input.GetTouch(rollerIndex).position.y, 0));
                        masterController.ballRoller.transform.Rotate(0, 90, 90);
                        masterController.ballRoller.transform.position = originalPos;
                    }

                    rollSpeed = Vector2.Distance(Input.GetTouch(rollerIndex).position, startMovingPosition) / (52f * 1.62f * Screen.width / 1200f);
                    float y = masterController.ballRoller.transform.eulerAngles.z;
                    if (Input.GetTouch(rollerIndex).position.x < startMovingPosition.x)
                        y = 360f - y;


                    if (y > 90f && y < 270f) {
                        movingBackward = true;
                        movingForward = false;
                    } else {
                        movingForward = true;
                        movingBackward = false;
                    }
                    if (isVehicle) {
                        rollSpeed = 2f;
                    } else {
                        if (rollSpeed >= 2.7f && (isGrounded)) {
                            if (movingForward && !aiming &&
                                !aimingAnimating && !reloading && !grenading &&
                                isGrounded && (!localShooting || instantiatedGun == null || instantiatedGun.isMelee) && !changingWeapon) {
                                running = true;
                                rollSpeed = 3.6f;
                            } else {
                                running = false;
                                rollSpeed = 2.320f;
                            }
                        } else
                            running = false;
                    }

                    moving = true;
                    Quaternion deltaRotation = transform.rotation;
                    transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + y, transform.eulerAngles.z);

                    float mySpeed = 1f;
                    if (isVehicle) {
                        if (!movingForward)
                            mySpeed = -1f;
                        

                        if (movingForward) {
                            if (y > 10f && y < 180f) {
                                if (isPlane) {
                                    airplaneTurning = (y - 10) * 0.01f;
                                } else {
                                    nearbyTank.Turn((y - 10) * 0.01f);
                                }
                            } else if (y < 350f && y > 180f) {
                                if (isPlane) {
                                    airplaneTurning = (350 - y) * -0.01f;
                                } else {
                                    nearbyTank.Turn((350 - y) * -0.01f);
                                }

                            }
                        } else {
                            if (y < 170f) {
                                if (isPlane) {
                                    airplaneTurning = (170 - y) * -0.01f;
                                } else {
                                    nearbyTank.Turn((170 - y) * -0.01f);
                                }
                            } else if (y > 190) {
                                if (isPlane) {
                                    airplaneTurning = (y - 190f) * 0.01f;
                                } else {
                                    nearbyTank.Turn((y - 190f) * 0.01f);
                                }
                            }
                        }
                        if (isPlane) {
                            
                        } else {
                            nearbyTank.Move(-rollSpeed * 43.2f * (1 / 170f) * speed * mySpeed);
                        }
                    } else {
                        if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreHealthLessSpeed) {
                            mySpeed *= 0.9f;
                        } else if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreSwift) {
                            mySpeed *= 1.15f;
                        }
                        Vector3 locVel = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
                        locVel = new Vector3(moveDir.x, locVel.y, rollSpeed * 43.2f * 1.25f */**/ (1 / 38f) * speed * mySpeed * (crouching ? 0.7f : 1f)); //60 fps assumption
                        if (isGrounded)
                            GetComponent<Rigidbody>().velocity = transform.TransformDirection(locVel);
                    }
                    transform.rotation = deltaRotation;
                } else {
                    isRolling = false;



                }
            }
            if (isVehicle && isPlane) {
                nearbyPlane.Move(airplaneHorizontal * 0.35f, airplaneVertical * 0.35f, -airplaneTurning * 1.5f);
            }
            if (moving) {
                animator.SetFloat("ForwardSpeed", rollSpeed / 2f);
            } else {
                animator.SetFloat("ForwardSpeed", 2f);

            }
            animator.SetBool("MovingForward", moving);
        }
        
    }
    //mobile controls ends here
}


