using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponState 
{
    private const int maxWeaponCount = 20;
    public Weapon[] Weapons = new Weapon[maxWeaponCount];
    
    public void Init()
    {
        for(int i=0; i<maxWeaponCount; ++i)
        {
            //Weapons[i] = new Weapon();
            Weapons[i].ammo = 0;
            Weapons[i].weaponType = (Weapon.WeaponType)i;
        }
    }
    public void PickUpWeapon(Weapon weapon_pickup)
    {
        Weapons[(int)weapon_pickup.weaponType].ammo += weapon_pickup.ammo;
    }
}
