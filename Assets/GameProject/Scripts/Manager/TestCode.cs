using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;

public class TestCode : MonoBehaviour
{
    public bool isTesting = false;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && isTesting)
        {
            var _weapons = PlayerController.player.GetComponent<Backpack>().weapons;
            foreach (var w in _weapons)
            {
                w.ammo = 1000;
            }

            PlayerController.player.playerInfo.hp = 10000000;
        }
    }
}
