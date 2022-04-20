using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public PhysicMaterial zeroFriction;
    public Transform[] spawnpoints;

    public TankController[] tanks;
    public PlaneController[] airplanes;

    // Start is called before the first frame update
    void Start() {
        if (zeroFriction != null) {
            zeroFriction.dynamicFriction = 0f;
            zeroFriction.staticFriction = 0f;
        }
        int index = 0;
        foreach (TankController i in tanks) {
            i.tankId = index;
            index++;
        }
        index = 0;
        foreach (PlaneController p in airplanes) {
            p.planeId = index;
            index++;
        }
        //foreach (Collider i in transform.GetComponentsInChildren<Collider>()) {
        //    i.material = zeroFriction;
        //}
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}