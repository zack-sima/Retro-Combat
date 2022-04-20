using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullets : MonoBehaviour
{
    public SoldierAnimator owner;
    public int sender;
    public float destroyTime;

    Collider[] toIgnore;

    public GameObject particles, particles2;

    [HideInInspector]
    public bool instantiateParticles = false;
    public bool useExplosion;

    void Start()
    {
        StartCoroutine(AutoDestroy(6));
        toIgnore = GetComponentsInChildren<Collider>();
    }

    void OnCollisionEnter(Collision other)
    {
        try {
            for (int i = 0; i < toIgnore.Length; i++) {
                if (other.collider != toIgnore[i]) {
                    if (instantiateParticles && particles != null) {
                        Explosion insItem = Instantiate(useExplosion ? particles : particles2, other.contacts[0].point, new Quaternion(0, 0, 0, 0)).GetComponent<Explosion>();
                        insItem.owner = owner;
                        insItem.sender = sender;
                        Destroy(gameObject);
                    } else {
                        Destroy(gameObject);
                    }

                    return;
                }
            }
        } catch (Exception e) {
            print(e.Message);
        }
    }





    void Update() {
        //transform.Translate(Vector3.forward * Time.deltaTime * 10f);
    }

    IEnumerator ExpiryDate(float deathTime )
    {
        yield return new WaitForSeconds(deathTime);
        Destroy(this.gameObject);
    }

    IEnumerator AutoDestroy(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}
