using UnityEngine;
public class Weapon : MonoBehaviour
{
    public enum WeaponType
    {
        Pistol,
        Rifle
    }

    public WeaponType weaponType = WeaponType.Pistol;
    public int ammo = 0;
    public int damage = 0;
    public float rate;
    public float recoilForce;
    public Transform FirePoint;
    public string objectPoolTagBulletTrail;
    public string objectPoolTagSpark;
    public ParticleSystem GunFire;
}
