using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System;

public class Backpack : MonoBehaviour
{
    public Weapon weaponInHand = null;
    private int _weaponIndex = 0;
    public List<Weapon> weapons;
    public event Action<int, Weapon.WeaponType> OnSwapWeapon;
    void Awake()
    {
        foreach (Weapon w in weapons)
        {
            w.gameObject.SetActive(false);
        }
    }
    
    public void ChangeWeaponInHand(bool isNextWeapon)
    {
        int lastWeaponIndex = _weaponIndex;
        do
        {
            if (isNextWeapon) ++_weaponIndex;
            else --_weaponIndex;
            
            _weaponIndex = _weaponIndex % weapons.Count;
            if (_weaponIndex == -1) _weaponIndex = weapons.Count - 1;

            weaponInHand = weapons[_weaponIndex];
            
            if (_weaponIndex == lastWeaponIndex && weaponInHand.ammo == 0)
            {
                //所有的枪都没有子弹了
                weaponInHand = null;
                break;
            }
        } while (weaponInHand.ammo <= 0);
        
        weapons[lastWeaponIndex].gameObject.SetActive(false);
        if (weaponInHand != null)
        {
            weaponInHand.gameObject.SetActive(true);
            if (OnSwapWeapon != null)
                OnSwapWeapon(weaponInHand.ammo, weaponInHand.weaponType);
        }
    }
    
    public void PickUpWeapon(Weapon.WeaponType weaponType, int ammo)
    {
        foreach (Weapon w in weapons)
        {
            if (w.weaponType == weaponType)
            {
                w.ammo += ammo;
                break;
            }
        }
        if (weaponInHand == null)
        {
            PlayerController.player.SwapWeapon(true);
        }
    }
}
