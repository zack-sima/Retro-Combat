using System.Collections;
using UnityEngine;
public class ReviewButton : MonoBehaviour{
    public void ReviewGame() {
#if UNITY_IOS
        Application.OpenURL("https://itunes.apple.com/us/app/retro-combat/id1368995698?ls=1&mt=8");
#elif UNITY_ANDROID
        //android link here
#endif
    }
    void Start() {
        if (SystemInfo.deviceType != DeviceType.Handheld)
            Destroy(gameObject);
    }


}
