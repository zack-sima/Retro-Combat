using System.Collections.Generic;
using System.IO; //input output
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.Scripting;

//playerprefs for local storage (not based on bundle id)
public static class MyPlayerPrefs {
    public static void SetInt(string key, int value) {
        PlayerPrefsData d = LoadData();
        d.SetInt(key, value);
        SaveData(d);
    }
    public static void SetFloat(string key, float value) {
        PlayerPrefsData d = LoadData();
        d.SetFloat(key, value);
        SaveData(d);
    }
    public static void SetString(string key, string value) {
        PlayerPrefsData d = LoadData();
        d.SetString(key, value);
        SaveData(d);
    }
    public static int GetInt(string key) {
        PlayerPrefsData d = LoadData();
        return d.GetInt(key);
    }
    public static float GetFloat(string key) {
        PlayerPrefsData d = LoadData();
        return d.GetFloat(key);
    }
    public static string GetString(string key) {
        PlayerPrefsData d = LoadData();
        return d.GetString(key);
    }

    static void SaveData(PlayerPrefsData data) {

        BinaryFormatter bf = new BinaryFormatter(); //use persistentdatapath on mobiles?
        FileStream stream = new FileStream(GetDataPath() + "/myPlayerPrefs.sav", FileMode.Create);

        bf.Serialize(stream, data);
        stream.Close();
    }
    static string GetDataPath() {
        string dataPath = Application.persistentDataPath;
#if !UNITY_WEBGL
        if (SystemInfo.deviceType == DeviceType.Desktop && !Application.isEditor) {
            dataPath = Application.dataPath;

        }
#endif
        return dataPath;

    }


    //add integer parameter for different maps
    static PlayerPrefsData LoadData() {
        if (File.Exists(GetDataPath() + "/myPlayerPrefs.sav")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(GetDataPath() + "/myPlayerPrefs.sav", FileMode.Open);
            PlayerPrefsData data;
            try {
                data = bf.Deserialize(stream) as PlayerPrefsData;
                stream.Close();
            } catch {
                stream.Close();
                return new PlayerPrefsData();
            }
            return data;

        } else {
            return new PlayerPrefsData();
        }
    }
}
[System.Serializable]
public class PlayerPrefsData {
    public Dictionary<string, int> intDict;
    public Dictionary<string, float> floatDict;
    public Dictionary<string, string> stringDict;
    public PlayerPrefsData() {
        intDict = new Dictionary<string, int>();
        floatDict = new Dictionary<string, float>();
        stringDict = new Dictionary<string, string>();
    }
    public void SetInt(string key, int value) {
        if (intDict.ContainsKey(key)) {
            intDict[key] = value;
        } else {
            intDict.Add(key, value);
        }
    }
    public void SetFloat(string key, float value) {
        if (floatDict.ContainsKey(key)) {
            floatDict[key] = value;
        } else {
            floatDict.Add(key, value);
        }
    }
    public void SetString(string key, string value) {
        if (stringDict.ContainsKey(key)) {
            stringDict[key] = value;
        } else {
            stringDict.Add(key, value);
        }
    }

    public int GetInt(string key) {
        if (intDict.ContainsKey(key)) {
            return intDict[key];
        } else {
            return 0;
        }
    }
    public float GetFloat(string key) {
        if (floatDict.ContainsKey(key)) {
            return floatDict[key];
        } else {
            return 0f;
        }
    }
    public string GetString(string key) {
        if (stringDict.ContainsKey(key)) {
            return stringDict[key];
        } else {
            return "";
        }
    }

}


