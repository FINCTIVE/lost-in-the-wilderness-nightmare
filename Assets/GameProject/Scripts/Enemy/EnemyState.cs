[System.Serializable]
public class EnemyState
{
    public int hp = 100;
    public float moveSpeed = 2f;
    public float rotateSpeed = 3f;
    public float startChasingDistance = 100f;
    public float stopChasingDistance = 100f;
    public float startAttackingDistance = 2f;
    public float stopAttackingDistance = 3f;
    public float explosionWaitingTime = 1f;
    public int explosionDamage = 25;
    public float explosionDistance = 4f;
    public bool isDead = false;
    public bool isExploded = false;

    public void CopyValues(EnemyState copyFrom)
    {
        hp = copyFrom.hp;
        moveSpeed = copyFrom.moveSpeed;
        rotateSpeed = copyFrom.rotateSpeed;
        startChasingDistance = copyFrom.startChasingDistance;
        stopChasingDistance = copyFrom.stopChasingDistance;
        startAttackingDistance = copyFrom.startAttackingDistance;
        stopAttackingDistance = copyFrom.stopAttackingDistance;
        explosionWaitingTime = copyFrom.explosionWaitingTime;
        explosionDamage = copyFrom.explosionDamage;
        explosionDistance = copyFrom.explosionDistance;
        isDead = copyFrom.isDead;
        isExploded = copyFrom.isExploded;
    }
}