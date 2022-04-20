using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class StationaryGun : MonoBehaviour{
    public SoldierAnimator toolsUser;
    void Start()
    {
        
    }

    void Update(){
        foreach (Collider i in Physics.OverlapSphere(transform.position, 1.7f)) {
            if (i.GetComponent<SoldierAnimator>() != null) {
                SoldierAnimator so = i.GetComponent<SoldierAnimator>();
                if ((so.isPlayer || !so.masterController.isSinglePlayer)&& toolsUser == null) {
                    toolsUser = so;
                    if (toolsUser.isPlayer && toolsUser.instantiatedGun != null && toolsUser.instantiatedGun.gunId != 16) {
                        print("set gun");
                        toolsUser.SetGun(toolsUser.weapons[16]);
                        
                    }

                    break;
                }
            }
        }
        if (toolsUser != null && Vector3.Distance(toolsUser.transform.position, transform.position) > 2.7f) {
            if (toolsUser.isPlayer && toolsUser.instantiatedGun != null && toolsUser.instantiatedGun.gunId == 16)
                toolsUser.SetGun(toolsUser.weapons[toolsUser.gunPrefab]);

            toolsUser = null;
        }
        if (toolsUser != null) {
            transform.GetChild(0).gameObject.SetActive(false);
        } else {
            transform.GetChild(0).gameObject.SetActive(true);
        }

    }
}
