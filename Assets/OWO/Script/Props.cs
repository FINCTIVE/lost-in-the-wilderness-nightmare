using UnityEngine;
using System.Collections;

public class Props : MonoBehaviour
{
    public enum PropsType
    {
        Weapon,
        Boost
    }
    public PropsType propsType;
    public Weapon.WeaponType weaponType;
    public int ammo;


    private void OnTriggerEnter(Collider other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if(player != null)
        {
            if(propsType == PropsType.Weapon)
            {
                player.PickUpWeapon(weaponType, ammo);
            }

            Destroy(gameObject);
        }
        
    }
}
