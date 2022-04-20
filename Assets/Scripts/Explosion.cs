using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float damage;
    public SoldierAnimator owner;
    public int sender;
    public float radius = 8.2f;
    void Start()
    {
        GetComponent<AudioSource>().Play();
        foreach (Collider i in Physics.OverlapSphere(transform.position, radius)) {
            if (owner.masterController.gameTimer < 0f) //game is over
                break;
            if (i.GetComponent<PlaneController>() != null && i.GetComponent<PlaneController>().interactingWith == true) {
                PlaneController pa = i.GetComponent<PlaneController>();
                SoldierAnimator s = null;
                if (pa.interactingWith == true)
                    s = pa.planeSetUp.player.GetComponent<SoldierAnimator>();
                if (s != null) {
                    float prevHealth = pa.health;
                    pa.health = pa.health - damage * (12.7f - Vector3.Distance(s.transform.position, transform.position)) * 0.035f;
                    if (owner.masterController.isSinglePlayer) {
                        if (pa.health <= 0f && prevHealth > 0f && s.playerId % 2 != owner.playerId % 2) {
                            owner.masterController.playerInformation[owner.playerId].score += 2;
                        }
                    }
                }
            } else if (i.GetComponent<TankController>() != null && i.GetComponent<TankController>().interactingWith == true) {
                TankController ta = i.GetComponent<TankController>();
                SoldierAnimator s = null;
                if (ta.interactingWith == true)
                    s = ta.tankSetUp.player.GetComponent<SoldierAnimator>();
                if (s != null) {
                    float prevHealth = ta.health;
                    ta.health = ta.health - damage * (12.7f - Vector3.Distance(s.transform.position, transform.position)) * 0.035f;
                    if (owner.masterController.isSinglePlayer) {
                        if (ta.health <= 0f && prevHealth > 0f && s.playerId % 2 != owner.playerId % 2) {
                            owner.masterController.playerInformation[owner.playerId].score  += 2;
                        }
                    }
                }
            } else if (i.GetComponent<SoldierAnimator>() != null && !i.GetComponent<SoldierAnimator>().isVehicle && (i.GetComponent<SoldierAnimator>().isPlayer || i.GetComponent<SoldierAnimator>().masterController.isSinglePlayer) && !i.GetComponent<SoldierAnimator>().dying) {
                SoldierAnimator s = i.GetComponent<SoldierAnimator>();
                float prevHealth = s.health;
                s.health = s.health - damage * (12.7f - Vector3.Distance(s.transform.position, transform.position)) * 0.035f;
                if (owner.masterController.isSinglePlayer) {
                    if (s.health <= 0f && prevHealth > 0f && s.playerId % 2 != owner.playerId % 2) {
                        owner.masterController.playerInformation[owner.playerId].score++;
                    }
                }

                if (s.health <= 0f) {
                    if (!owner.masterController.isSinglePlayer) {
                        if (owner.isPlayer) { //player blew up
                            owner.masterController.latestMessage = MyPlayerPrefs.GetString("playerName") + " blew up self";
                        } else {
                            s.masterController.latestMessage = s.masterController.playerInformation[sender].playerName + " blew up " + MyPlayerPrefs.GetString("playerName");
                        }

                        s.masterController.killerId = sender;

                        s.masterController.cannotChangeLatest = true;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
