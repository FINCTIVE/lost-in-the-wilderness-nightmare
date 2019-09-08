using UnityEngine;
using System.Collections;

public class Props : MonoBehaviour
{
    public Weapon.WeaponType weaponType;
    public int ammo;

    private void OnTriggerEnter(Collider other)
    {
        Backpack playerBackpack = other.GetComponent<Backpack>();
        if(playerBackpack != null)
        {
            playerBackpack.PickUpWeapon(weaponType, ammo);
            Destroy(gameObject);
        }
    }
}
