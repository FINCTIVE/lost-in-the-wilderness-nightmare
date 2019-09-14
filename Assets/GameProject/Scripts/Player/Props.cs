using System;
using UnityEngine;
using System.Collections;

public class Props : MonoBehaviour
{
    public Weapon.WeaponType weaponType;
    public int ammo;
    public GameObject particleSystemPutBox;
    public GameObject particleSystemPickBox;

    private void Start()
    {
        Instantiate(particleSystemPutBox, transform.position, transform.rotation);
    }

    private void OnTriggerEnter(Collider other)
    {
        Backpack playerBackpack = other.GetComponent<Backpack>();
        if(playerBackpack != null)
        {
            playerBackpack.PickUpWeapon(weaponType, ammo);
            Instantiate(particleSystemPickBox, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
