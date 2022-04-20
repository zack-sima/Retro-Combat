using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {
    public bool isHead, isBody; //increase damage output
    public SoldierAnimator parent;
    void Start() {
        //if (parent.isPlayer) {
        //    GetComponent<Collider>().enabled = false;
        //}
    }
}