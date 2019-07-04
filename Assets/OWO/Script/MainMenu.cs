using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public InputField nameInputField;
    public void Awake(){
        PlayerDataManager.InitPlayerSettings();
        nameInputField.text = PlayerDataManager.singleton.name;
    }
    public void StartGame(){
        if(PlayerDataManager.singleton.name == "")
        {
            PlayerDataManager.singleton.name = "一个懒得改昵称的人";
        }
        PlayerDataManager.SavePlayerSettings();
        SceneManager.LoadScene(1);
    }

    public void QuitGame(){
        PlayerDataManager.SavePlayerSettings();
        Application.Quit();
    }

    public void ChangePlayerName(string name){
        PlayerDataManager.singleton.name = name;
    }
}
