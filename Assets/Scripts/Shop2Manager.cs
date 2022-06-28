using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Shop2Manager : MonoBehaviour {
    public void ToScene(int index) {
        SceneManager.LoadScene(index);
    }
    void Start() {

    }
    public List<Button> disableButtons;

    public void loadScene(int index) {
        SceneManager.LoadScene(index);
    }
    public void ToShop1() {
        SceneManager.LoadScene(5);
    }
    public void BeginPurchase() {
        foreach (Button b in disableButtons)
            b.interactable = false;
    }
    public void PurchaseComplete() {
        StartCoroutine(EnableButtons());
    }
    private IEnumerator EnableButtons() {
        yield return null; yield return null;
        foreach (Button b in disableButtons)
            b.interactable = true;
    }
    public void Purchase1000Coins() {
        PlayerDatas.instance.myData.money += 1000;

        PlayerDatas.instance.SaveFile();

    }
    public void Purchase3000Coins() {
        PlayerDatas.instance.myData.money += 3000;

        PlayerDatas.instance.SaveFile();
    }
    public void Purchase10000Coins() {
        PlayerDatas.instance.myData.money += 10000;

        PlayerDatas.instance.SaveFile();
    }
}