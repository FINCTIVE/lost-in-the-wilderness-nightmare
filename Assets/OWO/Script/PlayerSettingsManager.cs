using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerSettingsManager
{
    public static PlayerSettings playerInfo;
    private static string path = Application.dataPath + "/PlayerSettings.settings";
    //将在游戏刚运行时调用
    public static void InitPlayerSettings(){
        if (File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            playerInfo = formatter.Deserialize(stream) as PlayerSettings;
            stream.Close();
        }else{
            playerInfo = new PlayerSettings();
            playerInfo.name = "";
        }
    }
    public static void SavePlayerSettings(){
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, playerInfo);
        stream.Close();
    }
}
