using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using System.IO;

public class ShareButton : MonoBehaviour
{
	public Texture2D shareTexture;
	private string shareMessage;
	private IEnumerator TakeScreenshotAndShare() {
		yield return new WaitForEndOfFrame();

		//Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		//ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0); //screenshot
		//ss.Apply();
		Texture2D ss = shareTexture;

		string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
		File.WriteAllBytes(filePath, ss.EncodeToPNG());

		// To avoid memory leaks
// Destroy(ss);

		new NativeShare().AddFile(filePath)
			.SetSubject(CustomFunctions.TranslateText("Retro Combat")).SetText(shareMessage)
			.SetCallback((result, shareTarget) => Debug.Log("Share result: " + result + ", selected app: " + shareTarget))
			.Share();

	}
	public void ClickShareButton() {
		shareMessage = MyPlayerPrefs.GetString("language") == "Chinese" ?
			"听说手机上就可以跟朋友对干？还可以线下跟人机玩？\n https://apps.apple.com/us/app/id1368995698" :
			"Play this shooter with or against your friends! Don't have any yet? Don't worry, there's bots to get you covered :) \n http://app.retrocombat.com"; //https://apps.apple.com/us/app/id1368995698
		if (SystemInfo.deviceType != DeviceType.Handheld) {
			Application.OpenURL("http://retrocombat.com");
		} else {
			StartCoroutine(TakeScreenshotAndShare());
		}

    }
}