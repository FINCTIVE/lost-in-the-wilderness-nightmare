using UnityEngine;
[System.Serializable]
public class EnemyState : MonoBehaviour
{
    public int hp;
    public float moveSpeed;
    public float rotateSpeed;
    public float startChasingDistance;
    public float stopChasingDistance;
    public float startAttackingDistance;
    public float stopAttackingDistance;
    public float explosionWaitingTime;
    public int explosionDamage;
    public float recoilVelocity;
    public bool isDead = false;
    public bool isExploded = false;
}
