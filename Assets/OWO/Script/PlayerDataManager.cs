using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;

public class PlayerDataManager
{
    public static PlayerData singleton;
    private static string path = Application.dataPath + "/PlayerData.gameData";
    //将在游戏刚运行时调用
    public static void InitPlayerSettings(){
        if (File.Exists(path)){
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = new FileStream(path, FileMode.Open);
            singleton = formatter.Deserialize(stream) as PlayerData;
            stream.Close();
        }else{
            singleton = new PlayerData();
            singleton.name = "";
        }
    }
    public static void SavePlayerSettings(){
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create);
        formatter.Serialize(stream, singleton);
        stream.Close();
    }
}
