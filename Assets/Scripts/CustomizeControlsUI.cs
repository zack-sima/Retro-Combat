using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CustomizeControlsUI : MonoBehaviour {
    public bool isEditable; //turn off in actual play
    public Transform[] objects;
    void Start() {
        if (MyPlayerPrefs.GetInt("resetMobile") == 0) {
            if (isEditable)
                MyPlayerPrefs.SetInt("resetMobile", 1);
        } else {
            for (int i = 0; i < objects.Length; i++) {
                try {
                    objects[i].localPosition = new Vector2(MyPlayerPrefs.GetFloat("mobileui" + i + "x"), MyPlayerPrefs.GetFloat("mobileui" + i + "y"));
                    if (isEditable && objects[i].GetComponent<Button>() != null) {
                        objects[i].GetComponent<Button>().interactable = false;
                    }
                } catch {
                }
            }
        }
    }

    Vector2 dragOffset;
    int dragObject = -1;
    void Update() {


        if (isEditable) {
            if (Input.GetMouseButtonUp(0)) {
                dragObject = -1;
            }
            for (int i = objects.Length - 1; i >= 0; i--) {
                if (dragObject == -1 && Vector2.Distance(objects[i].position, Input.mousePosition) < objects[i].GetComponent<RectTransform>().sizeDelta.y / 1.9f * Screen.width / 1200f && Input.GetMouseButtonDown(0)) {
                    dragOffset = Input.mousePosition - objects[i].position;
                    dragObject = i;
                }
                MyPlayerPrefs.SetFloat("mobileui" + i + "x", objects[i].localPosition.x);
                MyPlayerPrefs.SetFloat("mobileui" + i + "y", objects[i].localPosition.y);
            }

            if (dragObject > -1) {
                if ((dragObject != 1 || Input.mousePosition.x < Screen.width / 2f) && (dragObject != 2 || Input.mousePosition.x > Screen.width / 2f))
                    objects[dragObject].position = (Vector2)Input.mousePosition - dragOffset;
            }
        }
    }

    public void ResetDefault() {
        MyPlayerPrefs.SetInt("resetMobile", 0);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }



    public void ChangeScene(int index) {
        SceneManager.LoadScene(index);
    }
}
