using UnityEngine;
using System.Collections;

public class KillingZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            other.GetComponent<EnemyController>().GetDamage(10000, 0f);
        }
        else if(other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Hurt(10000);
        }
    }
}
