using UnityEngine;
using System.Collections;

public class KillingZoneContinuously : MonoBehaviour
{
    public int damagePerSec;
    private float _timer = 0f;
    private void OnTriggerStay(Collider other)
    {
        _timer += Time.deltaTime;
        if(_timer > 1f)
        {
            _timer -= 1f;
            if(other.CompareTag("Enemy"))
            {
                other.GetComponent<EnemyController>().GetDamage(damagePerSec, 3f);
            }
            else if(other.CompareTag("Player"))
            {
                other.GetComponent<PlayerController>().Hurt(damagePerSec);
            }
        }
    }
}
