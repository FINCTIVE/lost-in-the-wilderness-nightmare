using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackPackItemUI : MonoBehaviour
{
    public Text pistalAmmoText;
    public Text rifleAmmoText;
    public Weapon pistal;
    public Weapon rifle;
    void Start()
    {
        pistal.OnAmmoChange += OnPistalAmmoChange;
        rifle.OnAmmoChange += OnRifleAmmoChange;
    }

    private void OnPistalAmmoChange(int ammo)
    {
        pistalAmmoText.text = ammo.ToString();
    }
    private void OnRifleAmmoChange(int ammo)
    {
        rifleAmmoText.text = ammo.ToString();
    }
}
