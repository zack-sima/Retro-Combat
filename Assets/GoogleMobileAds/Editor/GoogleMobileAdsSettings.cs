using UnityEditor;
using UnityEngine;

namespace GoogleMobileAds.Editor
{

    internal class GoogleMobileAdsSettings : ScriptableObject
    {
        private const string MobileAdsSettingsDir = "Assets/GoogleMobileAds";

        private const string MobileAdsSettingsResDir = "Assets/GoogleMobileAds/Resources";

        private const string MobileAdsSettingsFile =
            "Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset";

        private static GoogleMobileAdsSettings instance;

        [SerializeField]
        private string adMobAndroidAppId = "ca-app-pub-9659065879138366~3662770031";

        [SerializeField]
        private string adMobIOSAppId = /*"ca-app-pub-9659065879138366~7934268095";non-china:*/ "ca-app-pub-9659065879138366~2465238431";

        [SerializeField]
        private bool delayAppMeasurementInit = false;

        public string GoogleMobileAdsAndroidAppId
        {
            get
            {
                return Instance.adMobAndroidAppId;
            }

            set
            {
                Instance.adMobAndroidAppId = value;
            }
        }

        public string GoogleMobileAdsIOSAppId
        {
            get
            {
                return Instance.adMobIOSAppId;
            }

            set
            {
                Instance.adMobIOSAppId = value;
            }
        }

        public bool DelayAppMeasurementInit
        {
            get
            {
                return Instance.delayAppMeasurementInit;
            }

            set
            {
                Instance.delayAppMeasurementInit = value;
            }
        }

        public static GoogleMobileAdsSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    if (!AssetDatabase.IsValidFolder(MobileAdsSettingsDir))
                    {
                        AssetDatabase.CreateFolder("Assets", "GoogleMobileAds");
                    }

                    if (!AssetDatabase.IsValidFolder(MobileAdsSettingsResDir))
                    {
                        AssetDatabase.CreateFolder(MobileAdsSettingsDir, "Resources");
                    }

                    instance = (GoogleMobileAdsSettings) AssetDatabase.LoadAssetAtPath(
                        MobileAdsSettingsFile, typeof(GoogleMobileAdsSettings));

                    if (instance == null)
                    {
                        instance = ScriptableObject.CreateInstance<GoogleMobileAdsSettings>();
                        AssetDatabase.CreateAsset(instance, MobileAdsSettingsFile);
                    }
                }
                return instance;
            }
        }

        internal void WriteSettingsToFile()
        {
            AssetDatabase.SaveAssets();
        }
    }
}
