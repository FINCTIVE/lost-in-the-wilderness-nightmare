using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerTemperatureSystem : MonoBehaviour
{
    public int maxColdDamagePerSec;
    [Tooltip("开始受到寒冷伤害的温度")]public float startGetingColdTemperature;
    [Tooltip("受到最大寒冷伤害的温度（低于此的温度都受到一样的伤害）")]public float endGetingColdTemperature;
    private float playerTemperature = 0f;
    
    private Transform _transform;
    private void Awake()
    {
        _transform = transform;
    }

    //体温控制
    private void FixedUpdate()
    {
        playerTemperature = 0f;
        foreach (var campfire in CampfireManager.Instance.campFires)
        {
            float distanceFromHeat = Vector3.Distance(_transform.position, campfire.transform.position);
            playerTemperature += Mathf.Max(0f,campfire.temperature * (1-(distanceFromHeat / campfire.heatRadius)));
        }

        int damage = 0;
        
        float coldRate = Mathf.Max(0f, (playerTemperature - endGetingColdTemperature)
                         / (startGetingColdTemperature - endGetingColdTemperature)); // 一次函数
        if (playerTemperature < endGetingColdTemperature)
        {
            damage = maxColdDamagePerSec;
        }
        else if (playerTemperature < startGetingColdTemperature)
        {
            damage = (int)(maxColdDamagePerSec * coldRate); // 一次函数
        };
        PlayerController.player.HurtByDamagePerSec(damage, DamageFrom.Cold);
        PlayerController.player.playerState.coldRate = coldRate;
    }
}
