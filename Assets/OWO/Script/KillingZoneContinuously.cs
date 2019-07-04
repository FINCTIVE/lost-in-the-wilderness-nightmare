using UnityEngine;
using System.Collections;

public class KillingZoneContinuously : MonoBehaviour
{
    public int damagePerSec;
    private float timer = 0f;
    private void OnTriggerStay(Collider other)
    {
        timer += Time.deltaTime;
        if(timer > 1f)
        {
            timer -= 1f;
            if(other.tag == "Enemy")
            {
                other.GetComponent<EnemyController>().GetDamage(damagePerSec, 3f);
            }
            else if(other.tag == "Player")
            {
                other.GetComponent<PlayerController>().GetDamage(damagePerSec);
            }
        }
    }
}
