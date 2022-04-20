using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
//this class exists to import data from version 2.0 of retro combat
public static class LegacyBinarySave {
    public static void SaveData(BinaryData data) {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(Application.persistentDataPath + "/playerdata.sav", FileMode.Create);

        bf.Serialize(stream, data);
        stream.Close();
    }
    //add integer parameter for different maps
    public static BinaryData LoadData() {
        if (File.Exists(Application.persistentDataPath + "/playerdata.sav")) {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(Application.persistentDataPath + "/playerdata.sav", FileMode.Open);
            BinaryData data = bf.Deserialize(stream) as BinaryData;
            stream.Close();
            return data;
        } else {
            return null;
        }
    }
    public static SaveData MigrateToNewData() {
        BinaryData oldData = LoadData();
        if (oldData != null) {
            SaveData newData = new SaveData();
            newData.money = oldData.money + oldData.moneySpent;
            newData.xp = oldData.xp;
            newData.xpLevel = oldData.xpLevel;
            return newData;
        } else {
            return null;
        }
    }
}
[System.Serializable]
public class BinaryData {
    public int xp, xpLevel, money, kills, headshots, damage, bulletsUsed, tanksDestroyed, deaths, gunsOwned, gamesPlayed, moneySpent;
    public float timePlayed;
    public bool[] purchasedPrimaryWeapons, purchasedSecondaryWeapons;
    public int[][] gunLevels;

    //initial use only 
    public BinaryData() {
        xp = 0;
        money = 100;
        xpLevel = 1;
        //purchased nonconsumable items from shop are added here
        purchasedPrimaryWeapons = new bool[30];
        purchasedSecondaryWeapons = new bool[30];
        gunLevels = new int[100][];
        for (int i = 0; i < gunLevels.Length; i++)
            gunLevels[i] = new int[5];
    }
}




