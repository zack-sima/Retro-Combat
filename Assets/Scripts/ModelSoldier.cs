using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//this script is for displaying the soldier with the specific skin/gun
public class ModelSoldier : MonoBehaviour {
    public SoldierWeaponsManager weaponsManager;
    public int weapon;//, armRotation = 20;
    public SoldierCountry soldierCountry;
    void Awake() {
        ResetAmericanBones();
    }
    void Start(){

        
        UpdateSkinAndWeapon();
        transform.Rotate(0f, 7f * 1.75f, 0f, Space.Self);
        transform.eulerAngles = new Vector3(-7f, transform.eulerAngles.y, 0f);
    }
    public MeshOrganizer russianRootbone, americanRootbone, meshRoot;
    public Transform bodyPartsParent;
    GameObject instantiatedGun;

    IEnumerator CallInFrame() {
        yield return null;
        UpdateSkinAndWeapon();
    }

    public void UpdateSkinAndWeapon() {
        if (instantiatedGun != null) {
            Destroy(instantiatedGun);
            StartCoroutine(CallInFrame());
            return;
        }
        switch (soldierCountry) {
        case SoldierCountry.American:
            meshRoot = americanRootbone;
            russianRootbone.gameObject.SetActive(false);
            americanRootbone.gameObject.SetActive(true);

            break;
        case SoldierCountry.Russian:
            meshRoot = russianRootbone;
            russianRootbone.gameObject.SetActive(true);
            americanRootbone.gameObject.SetActive(false);
            break;
        }
        float originalRotationY = transform.eulerAngles.y;
        transform.eulerAngles = Vector3.zero;
        bodyPartsParent = meshRoot.rootSpine;


        GameObject insItem = Instantiate(weaponsManager.weapons[weapon], meshRoot.gunAnchor);
        insItem.transform.localPosition = Vector3.zero;
        insItem.transform.localRotation = Quaternion.identity;
        insItem.transform.localScale = Vector3.one * Vector3.one.y;
        instantiatedGun = insItem;
        ResetBones(instantiatedGun);
        meshRoot.rootSpine.localPosition = Vector3.zero;

        transform.eulerAngles = new Vector3(-7f, originalRotationY, 0f);

        Update();
    }

    public void ResetBones(GameObject gun) {
        Gun gunController = gun.GetComponent<Gun>();
        List<Transform> bodyParts = new List<Transform>(bodyPartsParent.GetComponentsInChildren<Transform>());
        List<Transform> gunBodyParts = new List<Transform>(gunController.poseWielderBodyPartsParent.GetComponentsInChildren<Transform>());
        for (int i = 0, j = 0; i < bodyParts.Count; i++) {
            if (!bodyParts[i].CompareTag("MainCamera") && !bodyParts[i].CompareTag("Gun") &&
                !bodyParts[i].CompareTag("Player") && !bodyParts[i].CompareTag("Magazine")) {
                if (!bodyParts[i].CompareTag("NobodyChange")) {
                    try {
                        if (!bodyParts[i].CompareTag("ModelHead"))
                            bodyParts[i].localPosition = gunBodyParts[j].localPosition;
                        bodyParts[i].localRotation = gunBodyParts[j].localRotation;
                    } catch {
                        print(bodyParts[i].name);
                    }
                }

                j++;
            }

        }
        headRotation = 0f;
        meshRoot.head.parent.localRotation = Quaternion.identity;
    }
    public void ResetAmericanBones() {
        List<Transform> americanBodyParts = new List<Transform>(americanRootbone.GetComponentsInChildren<Transform>());
        List<Transform> russianBodyParts = new List<Transform>(russianRootbone.GetComponentsInChildren<Transform>());

        for (int i = 0; i < americanBodyParts.Count && i < russianBodyParts.Count; i++) {
            americanBodyParts[i].position = russianBodyParts[i].position;
            americanBodyParts[i].rotation = russianBodyParts[i].rotation;
        }
        americanRootbone.gameObject.SetActive(false);
    }


    bool deltaMouseDown = false;
    // Update is called once per frame

    float headRotation = 0f;
    void Update() {
        float deg = Mathf.Abs(Input.GetAxis("Mouse X")) < 2.5d ? Input.GetAxis("Mouse X") : 2.5d * Input.GetAxis("Mouse X") > 0 ? 2.5f : -2.5f;
        if (deltaMouseDown) {
            transform.Rotate(0f, 7f * -deg, 0f, Space.Self);
            transform.eulerAngles = new Vector3(-7f, transform.eulerAngles.y, 0f);
        }
        headRotation += Time.deltaTime / 1.5f;
        meshRoot.head.parent.Rotate(0f, Mathf.Cos(headRotation - 1.5f) / 15f, 0.00000001f);

        deltaMouseDown = Input.GetMouseButton(0);
    }
}


