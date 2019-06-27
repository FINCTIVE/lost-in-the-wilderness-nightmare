using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public TMPro.TMP_InputField nameInputField;
    public void Awake(){
        PlayerSettingsManager.InitPlayerSettings();
        nameInputField.text = PlayerSettingsManager.playerInfo.name;
    }
    public void StartGame(){
        if(PlayerSettingsManager.playerInfo.name == "")
        {
            PlayerSettingsManager.playerInfo.name = "一个懒得改昵称的人";
        }
        PlayerSettingsManager.SavePlayerSettings();
        SceneManager.LoadScene(1);
    }

    public void QuitGame(){
        PlayerSettingsManager.SavePlayerSettings();
        Application.Quit();
    }

    public void ChangePlayerName(string name){
        PlayerSettingsManager.playerInfo.name = name;
    }
}
