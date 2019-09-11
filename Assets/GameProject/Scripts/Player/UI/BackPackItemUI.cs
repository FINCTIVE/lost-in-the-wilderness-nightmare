using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackPackItemUI : MonoBehaviour
{
    public Text ammoText;
    public Weapon pistal;
    public Weapon rifle;
    private Weapon.WeaponType _weaponType;
    void Start()
    {
        pistal.OnAmmoChange += OnPistalAmmoChange;
        rifle.OnAmmoChange += OnRifleAmmoChange;
        PlayerController.player.GetComponent<Backpack>().OnSwapWeapon += _OnSwapWeapon;
    }

    private void OnPistalAmmoChange(int ammo)
    {
        if (_weaponType != Weapon.WeaponType.Pistol) return;
        ammoText.text = ammo.ToString();
    }
    private void OnRifleAmmoChange(int ammo)
    {
        if(_weaponType != Weapon.WeaponType.Rifle) return;
        ammoText.text = ammo.ToString();
    }

    private void _OnSwapWeapon(int ammo, Weapon.WeaponType m_weaponType)
    {
        _weaponType = m_weaponType;
        ammoText.text = ammo.ToString();
    }
}
