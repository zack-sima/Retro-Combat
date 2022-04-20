using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using System.Text;
public static class CustomFunctions {
    public static string TranslateText(string s) {
        Dictionary<string, string> chineseTranslations = new Dictionary<string, string>() { };
        chineseTranslations.Add("Join", "进入");
        chineseTranslations.Add("Online", "在线");
        //翻译



        chineseTranslations.Add("bot", "电脑");
        chineseTranslations.Add("Retro Combat", "经典战争");
        chineseTranslations.Add("Streak: ", "签到：");
        chineseTranslations.Add(" day(s)", "天");
        chineseTranslations.Add("Unknown", "未知");
        chineseTranslations.Add("Shipment", "集装箱");
        chineseTranslations.Add("Warehouse", "仓库");
        chineseTranslations.Add("Desert", "沙漠");
        chineseTranslations.Add("Silencer", "消音");
        chineseTranslations.Add("Extended Mag", "扩容弹匣");


        chineseTranslations.Add("Town", "小城");
        chineseTranslations.Add("No Attachment", "无配件");
        chineseTranslations.Add("Default Scope", "高倍镜");
        chineseTranslations.Add("Red Dot", "红点");
        chineseTranslations.Add("ACOG", "小倍镜");
        chineseTranslations.Add("Village", "村庄");
        chineseTranslations.Add("TDM", "团战");
        chineseTranslations.Add("TDM (Hard)", "团战（困难）");
        chineseTranslations.Add("blew up", "炸死了");
        chineseTranslations.Add("blew up self", "炸死了自己");

        chineseTranslations.Add("killed", "击杀了");
        chineseTranslations.Add("joined the game", "进入了游戏");
        chineseTranslations.Add("left the game", "离开了游戏");
        chineseTranslations.Add("You", "你");
        chineseTranslations.Add("Team 1", "团队1");
        chineseTranslations.Add("Team 2", "团队2");
        chineseTranslations.Add("Level", "等级");
        chineseTranslations.Add("Victory!", "胜利！");
        chineseTranslations.Add("Defeat", "失败");
        chineseTranslations.Add("Draw match", "平局");
        chineseTranslations.Add("Game starts in:", "游戏即将开始：");
        chineseTranslations.Add("FREE COINS!", "免费金币！");
        chineseTranslations.Add("for", "配件给");
        chineseTranslations.Add("You received ", "你获得了");
        chineseTranslations.Add(" coins!", "金币！");
        chineseTranslations.Add("No perks selected", "无技能");
        chineseTranslations.Add("Sleight of hand", "快速换弹夹");
        chineseTranslations.Add("Juggernaut", "增加防护");
        chineseTranslations.Add("Tighter grip", "狙击高手");
        chineseTranslations.Add("Sprinter", "跑步高手");
        chineseTranslations.Add("Deep impact", "大口径弹药");
        chineseTranslations.Add("Extra frag", "手榴弹玩家");
        chineseTranslations.Add("Respiration", "深度呼吸");
        chineseTranslations.Add("Double tap", "加速师");
        chineseTranslations.Add("P17C", "格洛克");
        chineseTranslations.Add("Deagle", "沙漠之鹰");








        chineseTranslations.Add("perk", "技能");



        chineseTranslations.Add("CHECK LATER", "明天再来看！");




        chineseTranslations.Add("Shop", "商店");











        if (MyPlayerPrefs.GetString("language") == "Chinese") {
            if (chineseTranslations.ContainsKey(s))
                return chineseTranslations[s];
        }
        return s;
    }
    public static string GetServerVersion() {
        return "2.5";
    }
    public static bool GetIsMobile() { 
        //change this based on device
        if (SystemInfo.deviceType == DeviceType.Handheld)
            return true;
        else
            return false; 
    }
    public static void CopyToClipboard(string s) {
        TextEditor te = new TextEditor();
        te.text = s;
        te.SelectAll();
        te.Copy();
    }
    public static string ConvertToUtf8(string str) {
        UTF8Encoding encodes = new UTF8Encoding();
        return encodes.GetString(encodes.GetBytes(str));
    }
    public static string PasteFromClipboard() {
        TextEditor te = new TextEditor();
        te.Paste();
        return te.text;
    }
}
