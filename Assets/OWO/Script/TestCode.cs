﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TestCode : MonoBehaviour
{
    public Text testText;
    public bool isTesting = false;
    void Start(){
        testText.text = "已进入测试模式，按Y退出\n";
        testText.text += "R: 敌人全部静止 \n";
        testText.text += "I: 玩家受伤100点 \n";
        testText.text += "U: 子弹无限 \n";
        testText.text += "P: 满血 \n";
    }
    void testP(){
        PlayerController.player.GetComponent<PlayerState>().hp = 10000;
    }
    void testR(){
        GameObject[] enemys = GameObject.FindGameObjectsWithTag("Enemy");
        foreach(GameObject enemy in enemys)
        {
            enemy.GetComponent<EnemyState>().moveSpeed = 0;
        }
    }

    void testI(){
        GameObject.Find("Player").GetComponent<PlayerController>().GetDamage(1000);
    }

    void testU(){
        PlayerController.player.PickUpWeapon(Weapon.WeaponType.Pistol, 9000);
        PlayerController.player.PickUpWeapon(Weapon.WeaponType.Rifle, 9000);
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Y))
        {
            isTesting = !isTesting;
            
            testText.gameObject.SetActive(isTesting);
        }

        if (!isTesting) { return; }

        if (Input.GetKeyDown(KeyCode.R))
        {
            testR();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            testI();
        }
        if(Input.GetKeyDown(KeyCode.U))
        {
            testU();
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            testP();
        }
    }
}