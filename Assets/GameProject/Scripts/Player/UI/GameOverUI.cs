using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class GameOverUI : MonoBehaviour
{
    public Text wordsOfDeath;
    public string wordsDieOfCold = "...好冷...";
    public string wordsDieOfEnemy = "僵尸炸焦了你的脑子";
    public string wordsDieOfFire = "...你沐浴在光芒中远离了黑暗";
    void Start()
    {
        PlayerController.player.OnPlayerDie += _OnPlayerDie;
    }

    void _OnPlayerDie(DamageFrom attacker)
    {
        switch (attacker)
        {
            case DamageFrom.Cold:
                wordsOfDeath.text = wordsDieOfCold;
                break;
            case DamageFrom.Enemy:
                wordsOfDeath.text = wordsDieOfEnemy;
                break;
            case DamageFrom.Fire:
                wordsOfDeath.text = wordsDieOfFire;
                break;
        }
    }
}
