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
}