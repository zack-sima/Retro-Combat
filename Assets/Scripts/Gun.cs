using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine; //uzi gun sound by jarzxe


public class Gun : MonoBehaviour {
    public string gunName; //use this to display in armory
    [HideInInspector]
    public SoldierAnimator owner;
    public float damage, spread; //etak y aigroeg atsug et em
    public float recoil, caliber, aimTime;
    public Transform poseWielderBodyPartsParent; //only assign this if it is an assault rifle that needs to be displayed
    public Transform wielderBodyPartsParent; //this will update the bones of the soldier carrying the weapon so that hands are in the right place
    //public Vector3 aimingRightArmRotation, aimingLeftArmRotation, aimingLowerRightArmRotation, aimingLowerLeftArmRotation, aimingRightHandRotation, aimingLeftHandRotation;
    public Transform aimAnchor, magazine; //camera will interpolate to this position when aiming
    public Transform silencer;
    public Transform acogAimAnchor, redDotAimAnchor;
    [HideInInspector]
    public Transform magAnchor;
    public float shootCooldown, reloadTime;
    public float effectiveDistance;
    int originalMagSize;
    [Range(1f, 15f)]
    public float zoomingTimes; //for default configuration
    public bool isAuto, isMelee, isGrenade;
    public bool scoped;
    public Sprite[] muzzles;
    public SpriteRenderer muzzleAnchor;
    public int magSize, totalBullets;
    public GameObject bulletHolePrefab, firingMapIconPrefab; //red dot appears on map if not player's team fired ammunitions

    public GameObject explosionPrefab;
    [HideInInspector]
    public float cookingTimer; //grenade cook timer
    [HideInInspector]
    public bool thrown; //grenade cooks time
    public int extraBurstBullets; //shotguns will have more than 0
    [HideInInspector]
    public int magBullets;

    [HideInInspector]
    public float shootDelay;
    [HideInInspector]
    public int originalTotalBullets;
    int sender;
    public void EjectProjectile() {
        thrown = true;
        print("thrown");
        owner.grenades -= 1;


        transform.parent = GameObject.Find("NetworkMaster").transform;
        transform.position = owner.gunAnchor.position;




        StartCoroutine(SyncPos());
        //throw the grenade!
        GetComponent<Rigidbody>().useGravity = true;
        GetComponent<BoxCollider>().isTrigger = false;





        GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0f, 0.62f, 0.7f) * 8f, ForceMode.Impulse);
    }
    IEnumerator SyncPos() {
        yield return null;
        yield return null;
        transform.position = owner.gunAnchor.position;
    }
    void Awake()
    {
        originalMagSize = magSize;
        originalTotalBullets = totalBullets;
        magBullets = magSize;
        shootDelay = shootCooldown;
        if (!isMelee && !isGrenade) {
            Transform insitem = new GameObject().transform;
            //Transform insitem = Instantiate(sampleObject, magazine.parent).transform;
            insitem.parent = transform;
            insitem.position = magazine.position;
            insitem.rotation = magazine.rotation;
            magAnchor = insitem;
            magAnchor.tag = "Gun";
            //Destroy(sampleObject);
        }
    }
    public void UpdateAttachments(int attachmentId) {
        updatedAttachment = true;
        switch (attachmentId) {
        case 5:
            if (silencer != null) {
                Destroy(silencer.gameObject);
            }
            if (acogAimAnchor != null) {
                Destroy(acogAimAnchor.gameObject);
            }
            if (redDotAimAnchor != null) {
                Destroy(redDotAimAnchor.gameObject);
            }
            magSize = (int)(originalMagSize * 1.330f + 0.5f);
            break;
        case 4:
            if (redDotAimAnchor != null) {
                Destroy(redDotAimAnchor.gameObject);
            }
            if (acogAimAnchor != null) {
                Destroy(acogAimAnchor.gameObject);
            }
            muzzleAnchor.transform.Translate(Vector3.forward * 0.3f);
            aimTime = 0.25f;
            break;
        case 2:
            Destroy(aimAnchor.gameObject);
            if (redDotAimAnchor != null) {
                Destroy(redDotAimAnchor.gameObject);
            }
            if (silencer != null) {
                Destroy(silencer.gameObject);
            }
            aimAnchor = acogAimAnchor;
            zoomingTimes = 3.8f;
            aimTime = 0.25f;
            scoped = false;
            break;
        case 1:
            Destroy(aimAnchor.gameObject);
            if (silencer != null) {
                Destroy(silencer.gameObject);
            }
            if (acogAimAnchor != null) {
                Destroy(acogAimAnchor.gameObject);
            }
            aimAnchor = redDotAimAnchor;
            zoomingTimes = 1.35f;
            aimTime = 0.25f;
            scoped = false;
            break;
        default:
            if (silencer != null) {
                Destroy(silencer.gameObject);
            }
            if (acogAimAnchor != null) {
                Destroy(acogAimAnchor.gameObject);
            }
            if (redDotAimAnchor != null) {
                Destroy(redDotAimAnchor.gameObject);
            }
            break;
        }
    }
    bool updatedAttachment = false;
    void Start() {
        if (owner != null && owner.isLocalBot && owner.useBotControls) {
            totalBullets = 100000;
            originalTotalBullets = 100000;
        }
        //if (!isMelee && !isGrenade)
        //    recoil *= 1f;
        if (owner == null || owner.isPlayer) {
            UpdateAttachments(MyPlayerPrefs.GetInt("gun" + gunId + "Attachment"));
        } else if (owner != null && !owner.isPlayer && (owner.masterController.isSinglePlayer || owner.attachmentPrefabId != -1)) {
            UpdateAttachments(owner.attachmentPrefabId);
        }
        if (owner != null && owner.isPlayer) {
            foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
                r.gameObject.layer = 12;
            }
            if (owner.isPlayer && (Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreGrenadesLessAmmo && owner.weaponsManager.weapons[gunId].GetComponent<Gun>().totalBullets == totalBullets) {
                originalTotalBullets = (int)(originalTotalBullets * 0.8f);
                totalBullets = originalTotalBullets;
            }

        }
        
        if (isGrenade) {
            cookingTimer = 3.5f;
            sender = owner.playerId;
        }
    }
    public void ResetMagazines() {
        magBullets = magSize;
        totalBullets = originalTotalBullets;
    }



    public int gunId;
    float muzzleTimer;
    void LateUpdate()
    {
        if (!updatedAttachment) {
            if (owner != null && !owner.isPlayer && (owner.masterController.isSinglePlayer || owner.attachmentPrefabId != -1)) {
                UpdateAttachments(owner.attachmentPrefabId);
            }
        }
        if (isGrenade) {
            if (owner == null) {
                Destroy(gameObject);
            }
            if (!thrown) {
                transform.position = owner.gunAnchor.position;
                transform.rotation = owner.gunAnchor.rotation;
            }
            cookingTimer -= Time.deltaTime;
            if (cookingTimer < 0f) {
                GameObject insItem = Instantiate(firingMapIconPrefab, new Vector3(transform.position.x, 52.1f, transform.position.z), firingMapIconPrefab.transform.rotation);
                insItem.transform.parent = GameObject.Find("NetworkMaster").transform;
                //explode here
                
                insItem = Instantiate(explosionPrefab, transform.position, explosionPrefab.transform.rotation);
                insItem.GetComponent<Explosion>().owner = owner;
                
                

                insItem.GetComponent<Explosion>().sender = sender;
                Destroy(gameObject);
            }
        }
        if (muzzleTimer > 0f) {
            muzzleAnchor.enabled = true;
            muzzleTimer -= Time.deltaTime;
        } else
            muzzleAnchor.enabled = false;

        if (shootDelay > 0f)
            shootDelay -= Time.deltaTime;
    }
    GameObject instantiatedMapIcon; //each player has one mapicon at one time max_
    public void ShootAmmo(bool isPlayer) { //isPlayer determines whether this shot actually calculates damage and is sent to server
        if (shootDelay < -0.07f) {
            shootDelay = shootCooldown * ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.FasterShooting ? 0.879f : 1f);
        } else
            shootDelay += shootCooldown * ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.FasterShooting ? 0.879f : 1f); //shootdelay is negative by a tiny bit so it takes less time to shoot again
        if (isPlayer) {
            owner.shootingCumulationDelta++;
        } else {
            owner.shootingCumulatedStorage--;
        }
        if (isGrenade) {
            if (!isPlayer) {
                print("cooking incoming grenade");
            }
            EjectProjectile();
            return;
        }
        if (!isGrenade && !isMelee && owner.playerId % 2 != owner.masterController.playerId % 2 && silencer == null) {
            //display on map
            if (instantiatedMapIcon != null)
                Destroy(instantiatedMapIcon);
            instantiatedMapIcon = Instantiate(firingMapIconPrefab, new Vector3(transform.position.x, 52f, transform.position.z), firingMapIconPrefab.transform.rotation);
            instantiatedMapIcon.transform.parent = GameObject.Find("NetworkMaster").transform;
        }

        if (owner.gunRotation > owner.minGunRotation - 7f || owner.masterController.isMobile) {
            owner.recoil += recoil;
            if (owner.recoilDelta < recoil)
                owner.recoilDelta += recoil;
            else {
                owner.recoilDelta = recoil;
            }
        }
        if (GetComponent<AudioSource>() != null) {
            AudioSource a = GameObject.Find("NetworkMaster").GetComponent<AudioSource>();
            float p = Mathf.Pow(0.952f, Vector3.Distance(transform.position, owner.masterController.player.transform.position)) * 3f;
            if (p > 1f)
                p = 1f;
            a.pitch = GetComponent<AudioSource>().pitch;
            if (silencer != null) {
                a.pitch *= 2f;
            }
            float scopedMultiple = ((scoped && owner.aiming) || (acogAimAnchor != null && owner.aiming)) ? 0.25f : 1f;
            a.PlayOneShot(GetComponent<AudioSource>().clip, GetComponent<AudioSource>().volume * p);
            if (!isMelee && (!scoped || !owner.aiming) && owner.isPlayer) {
                //owner.mainCam.Translate(Vector3.forward * (recoil / 30f * scopedMultiple - owner.lengthRecoil / 2f));
                owner.lengthRecoilDelta = recoil / 30f * scopedMultiple - owner.lengthRecoil;
                owner.lengthRecoil = recoil / 30f * scopedMultiple;
            }
        }
        if (!isMelee) {
            muzzleAnchor.sprite = muzzles[Random.Range(0, muzzles.Length)];
            muzzleTimer = 0.052f;
            owner.sideRecoilDir = Random.Range(0, 2) == 1;
            if (isPlayer)
                magBullets--;
        }
        for (int i = 0; i < extraBurstBullets + 1; i++) {
            //scan for target
            RaycastHit hit;
            Transform raycaster = isPlayer ? aimAnchor : owner.head;
            Quaternion originalRotation; // = Quaternion.identity;
            originalRotation = aimAnchor.rotation;
            float mySpread = spread;
            if (isPlayer && owner.useBotControls) {
                mySpread *= 2.5f / Mathf.Sqrt(zoomingTimes);
            }
            if (isPlayer || extraBurstBullets > 0) {
                if (!owner.aiming || extraBurstBullets > 0) {
                    if (extraBurstBullets <= 0 || owner.aiming)
                        aimAnchor.Rotate(Random.Range(-mySpread, mySpread), Random.Range(-mySpread, mySpread), Random.Range(-mySpread, mySpread));
                    else {
                        aimAnchor.Rotate(Random.Range(-mySpread * 1.2f, mySpread * 1.2f), Random.Range(-mySpread * 1.2f, mySpread * 1.2f), Random.Range(-mySpread * 2f, mySpread * 2f));

                    }
                } else {
                    if (isPlayer && owner.useBotControls) { //if bot aims still need to be a bit off
                        aimAnchor.Rotate(Random.Range(-mySpread / 3.7f, mySpread / 3.7f), Random.Range(-mySpread / 3.81f, mySpread / 3.31f), Random.Range(-mySpread / 3.8f, mySpread / 3.6f));

                    } else
                    raycaster = owner.mainCam; //bullets will go to center of camera regardless of gun's orientation
                }
            }
            if (Physics.Raycast(raycaster.position, raycaster.forward, out hit, effectiveDistance)) {
                if (owner.masterController.gameTimer < 0f) //game is over
                    break;
                if (hit.collider.GetComponent<PlaneController>() != null) {
                    PlaneController hitPlane = hit.collider.GetComponent<PlaneController>();

                    SoldierAnimator hitSoldier = null;
                    if (hitPlane.interactingWith == true)
                        hitSoldier = hitPlane.planeSetUp.player.GetComponent<SoldierAnimator>();
                    if (hitSoldier != null) {
                        if (!hitSoldier.dying && hitSoldier.playerTeam != owner.playerTeam) { //make sure not on same team
                            int hitPlayer = hitSoldier.playerId;
                            if (!owner.masterController.isSinglePlayer && !owner.damagedTargets.ContainsKey(hitPlayer)) {
                                owner.damagedTargets.Add(hitPlayer, 0f);
                            }
                            float myDamage = damage;
                            if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreDamageLessHealth) {
                                myDamage *= 1.12f;
                            }
                            if (owner.useBotControls) {

                                if (owner.masterController.isSinglePlayer && hitSoldier.isPlayer) {
                                    myDamage *= 0.25f + 0.2f * MyPlayerPrefs.GetInt("difficulty");
                                } else {
                                    myDamage *= 0.62f;
                                }
                            }
                            if (!owner.masterController.isSinglePlayer)
                                owner.damagedTargets[hitPlayer] += myDamage;
                            else if (hitPlane.health > 0f) {
                                hitPlane.health -= myDamage;
                                if (hitPlane.health <= 0f) {
                                    owner.masterController.playerInformation[owner.playerId].score++;
                                    if (owner.isPlayer) {
                                        owner.masterController.localData.myData.xp += 1 + MyPlayerPrefs.GetInt("difficulty");
                                        owner.masterController.localData.SaveFile();
                                    }
                                }
                            }
                            if (owner.isPlayer)
                                owner.masterController.TurnOnMarker();
                        }
                    }
                } else if (hit.collider.GetComponent<TankController>() != null) {
                    TankController hitTank = hit.collider.GetComponent<TankController>();

                    SoldierAnimator hitSoldier = null;
                    if (hitTank.interactingWith == true)
                        hitSoldier = hitTank.tankSetUp.player.GetComponent<SoldierAnimator>();
                    if (hitSoldier != null) {
                        if (!hitSoldier.dying && hitSoldier.playerTeam != owner.playerTeam) { //make sure not on same team
                            int hitPlayer = hitSoldier.playerId;
                            if (!owner.masterController.isSinglePlayer && !owner.damagedTargets.ContainsKey(hitPlayer)) {
                                try {
                                    owner.damagedTargets.Add(hitPlayer, 0f);
                                } catch(System.Exception e) {
                                    print(e);
                                }
                            }
                            float myDamage = damage;
                            if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreDamageLessHealth) {
                                myDamage *= 1.12f;
                            }
                            if (owner.useBotControls) {

                                if (owner.masterController.isSinglePlayer && hitSoldier.isPlayer) {
                                    myDamage *= 0.25f + 0.2f * MyPlayerPrefs.GetInt("difficulty");
                                } else {
                                    myDamage *= 0.62f;
                                }
                            }
                            if (!owner.masterController.isSinglePlayer)
                                owner.damagedTargets[hitPlayer] += myDamage;
                            else if (hitTank.health > 0f) {
                                hitTank.health -= myDamage;
                                if (hitTank.health <= 0f) {
                                    owner.masterController.playerInformation[owner.playerId].score++;
                                    if (owner.isPlayer) {
                                        owner.masterController.localData.myData.xp += 1 + MyPlayerPrefs.GetInt("difficulty");
                                        owner.masterController.localData.SaveFile();
                                    }
                                }
                            }
                            if (owner.isPlayer)
                                owner.masterController.TurnOnMarker();
                        }
                    }
                } else if (hit.collider.GetComponent<Hitbox>() != null) {
                    if (isPlayer) {
                        Hitbox hitbox = hit.collider.GetComponent<Hitbox>();
                        SoldierAnimator hitSoldier = hitbox.parent;
                        if (!hitSoldier.dying && !hitSoldier.isVehicle && hitSoldier.playerTeam != owner.playerTeam) { //make sure not on same team
                            int hitPlayer = hitSoldier.playerId;
                            if (!owner.masterController.isSinglePlayer && !owner.damagedTargets.ContainsKey(hitPlayer)) {
                                owner.damagedTargets.Add(hitPlayer, 0f);
                            }
                            float myDamage = damage;
                            if ((Perks)MyPlayerPrefs.GetInt("chosenPerk") == Perks.MoreDamageLessHealth) {
                                myDamage *= 1.12f;
                            }
                            if (owner.useBotControls) {

                                if (owner.masterController.isSinglePlayer && hitSoldier.isPlayer) {
                                    myDamage *= 0.25f + 0.2f * MyPlayerPrefs.GetInt("difficulty");
                                } else {
                                    myDamage *= 0.62f;
                                }
                            }

                            if (hitbox.isHead) {
                                myDamage *= 1.97f; //add to damage
                                if (i == 0 && isPlayer) { //not shotgun
                                    owner.masterController.localData.myData.xp += 1;
                                    owner.masterController.localData.SaveFile();
                                }
                            } else if (hitbox.isBody) {
                                myDamage *= 1.25f; //add to damage
                            } else {
                                myDamage *= 0.85f; //add to damage
                            }
                            if (!owner.masterController.isSinglePlayer)
                                owner.damagedTargets[hitPlayer] += myDamage;
                            else if (hitSoldier.health > 0f) {
                                hitSoldier.health -= myDamage;
                                if (hitSoldier.health <= 0f) {
                                    owner.masterController.playerInformation[owner.playerId].score++;
                                    if (owner.isPlayer) {
                                        owner.masterController.localData.myData.xp += 1 + MyPlayerPrefs.GetInt("difficulty");
                                        owner.masterController.localData.SaveFile();
                                    }
                                }
                            }
                            if (owner.isPlayer)
                                owner.masterController.TurnOnMarker();
                        }
                    }
                } else if (!hit.collider.CompareTag("Gun")) {
                    GameObject ins = Instantiate(bulletHolePrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    ins.transform.localScale = new Vector3(ins.transform.localScale.x * caliber, ins.transform.localScale.y * caliber, 1f);
                    ins.transform.Translate(Vector3.forward * 0.01f);
                    ins.transform.Rotate(0f, 0f, Random.Range(-30f, 30f));
                }
            }
            if (!owner.aiming || extraBurstBullets > 0) {
                aimAnchor.rotation = originalRotation;
            }
        }

    }
}
