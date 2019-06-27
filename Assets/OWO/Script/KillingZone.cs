using UnityEngine;
using System.Collections;

public class KillingZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            other.GetComponent<EnemyController>().GetDamage(10000);
        }
        else if(other.tag == "Player")
        {
            other.GetComponent<PlayerController>().GetDamage(10000);
        }
    }
}
