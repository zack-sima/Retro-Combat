using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class FeedbackUI : Popup {
    public InputField couponInput;
    bool checking = false;
    public void CheckRedeem() {
        if (!checking) {
            StartCoroutine(RedeemCode(couponInput.text));
            checking = true;
        }
    }
    IEnumerator RedeemCode(string code) {
        UnityWebRequest r = UnityWebRequest.Get("http://www.retrocombat.com:8002/redeem_coupon?coupon_code=" + code);
        yield return r.SendWebRequest();

        int money;
        if (int.TryParse(r.downloadHandler.text, out money)) {
            PlayerDatas.instance.myData.money += money;
            PlayerDatas.instance.SaveFile();
        }
        DismissPopup();
    }
}
