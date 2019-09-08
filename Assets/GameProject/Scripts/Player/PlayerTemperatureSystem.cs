using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerTemperatureSystem : MonoBehaviour
{
    public float getColdDistanceStart; // 开始计算寒冷伤害的距离
    public float getColdDistanceEnd; // 往后因寒冷受伤的伤害值一样
    public float gettingColdDamagePerSec;
    [Space(10)]
    public Color warmBloodColor;
    public Color coldBloodColor;
    public Image healthFill;
    public Transform heatPoint; //热源中心

    private Transform _transform;
    private PlayerController _playerController;
    private void Awake()
    {
        _transform = transform;
        _playerController = GetComponent<PlayerController>();
    }

    //体温控制
    private void FixedUpdate()
    {
        float distanceFromHeat = Vector3.Distance(_transform.position, heatPoint.position);
        float coldRate = 0f;
        if (distanceFromHeat > getColdDistanceStart)
        {
            if (distanceFromHeat < getColdDistanceEnd)
            {
                coldRate = (distanceFromHeat - getColdDistanceStart) / (10f - getColdDistanceStart);
            }
            else if (distanceFromHeat > getColdDistanceEnd)
            {
                coldRate = 1f;
            }
            int coldDamage = (int)(coldRate * gettingColdDamagePerSec);
            _playerController.HurtByDamagePerSec(coldDamage);
        }
        healthFill.color = Color.Lerp(warmBloodColor, coldBloodColor, coldRate);
    }
}
