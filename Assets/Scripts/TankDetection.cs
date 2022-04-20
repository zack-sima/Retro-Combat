using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankDetection : MonoBehaviour
{
    public TankController target;
    Vector3 colSize;
    // Start is called before the first frame update
    void Start()
    {
        colSize = GetComponent<BoxCollider>().size / 2f;

        colls = new Collider[0];
    }
    void OnTriggerEnter(Collider other) {
        if (other != null && other.GetComponent<SoldierAnimator>() != null) {
            SoldierAnimator sa = other.GetComponent<SoldierAnimator>();
            if (!sa.isVehicle && !target.interactingWith) {
                sa.nearbyTank = target;
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other != null && other.GetComponent<SoldierAnimator>() != null) {
            SoldierAnimator sa = other.GetComponent<SoldierAnimator>();
            if (sa.nearbyTank == target && !sa.isVehicle)
                sa.nearbyTank = null;
        }
    }
    Collider[] colls = null;
    void Update()
    {
        Collider[] newColls = Physics.OverlapBox(transform.position, colSize, transform.rotation);
        foreach (Collider i in newColls) {
            OnTriggerEnter(i);
        }
        foreach (Collider i in colls) {
            bool didNotExit = false;
            foreach (Collider j in newColls) {
                if (i == j) {
                    didNotExit = true;
                    break;
                }
            }
            if (!didNotExit) {
                OnTriggerExit(i);
            }
        }
        colls = newColls;
    }
}
