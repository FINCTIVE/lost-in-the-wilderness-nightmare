using System;
using UnityEngine;
public class Weapon : MonoBehaviour
{
    public enum WeaponType
    {
        Pistol,
        Rifle
    }

    public WeaponType weaponType = WeaponType.Pistol;
    private int _ammo = 0;
    public event Action<int> OnAmmoChange; 
    public int ammo
    {
        get => _ammo;
        set
        { 
            _ammo = value;
            if (OnAmmoChange != null)
                OnAmmoChange(_ammo);
        }
    }

    public int damage = 0;
    public float rate;
    public float recoilForce;
    public Transform FirePoint;
    public string objectPoolTagBulletTrail;
    public string objectPoolTagSpark;
    public ParticleSystem GunFire;
}
