using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampfireManager : MonoBehaviour
{
    public List<Campfire> campFires;
    public static CampfireManager Instance;

    private void Awake()
    {
        Instance = this;
    }
}
