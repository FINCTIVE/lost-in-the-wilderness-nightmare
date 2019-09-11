using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class EnemyController : MonoBehaviour, IPooledObject
{
    public EnemyState enemyState;
    private EnemyState _initialEnemyState;
    
    #region GameObject
    public GameObject GlowObj;
    public string objectPoolTagParticleSystemExplosion = "ParticleSystemExplosion";
    public string objectPoolTagParticleSystemEnemyDeath = "ParticleSystemEnemyDeath";
    #endregion
    #region Component
    public Animator modelAnimator;
    private Rigidbody _rb;
    private NavMeshAgent _navMeshAgent;
    #endregion

    private enum EnemyBehaviour{ Idle, Chase, Attack }
    private EnemyBehaviour _enemyBehaviour = EnemyBehaviour.Idle;
    private float _explosionTimer = 0f;

    private void Awake()
    {
        _initialEnemyState = new EnemyState();
        _initialEnemyState.CopyValues(enemyState);
        
        _rb = GetComponent<Rigidbody>();
        modelAnimator = transform.Find("Model").GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
    }
    
    public void OnObjectSpawn()
    {
        enemyState.CopyValues(_initialEnemyState);
    }

    public void OnObjectDestroy()
    {
        
    }
    private void FixedUpdate()
    {
        Vector3 targetPos = PlayerController.playerTransform.position;
        
        _navMeshAgent.SetDestination(targetPos);
        var myPosition = transform.position;
        _navMeshAgent.nextPosition = myPosition;

        float distanceSqr = (targetPos - myPosition).sqrMagnitude;
        // Vector3 dirXZ = targetPos - myTransform.position;
        Vector3 dirXZ = _navMeshAgent.desiredVelocity;
        dirXZ.y = 0;
        if (distanceSqr > 1) { dirXZ = Vector3.Normalize(dirXZ); }

        #region 状态机
        // idle <----> chase <----> attck
        if(_enemyBehaviour == EnemyBehaviour.Idle)
        {
            if (distanceSqr < enemyState.startChasingDistance*enemyState.startChasingDistance)
            {
                _enemyBehaviour = EnemyBehaviour.Chase;
                return;
            }
        }
        else if(_enemyBehaviour == EnemyBehaviour.Chase)
        {
            if(distanceSqr < enemyState.startAttackingDistance*enemyState.startAttackingDistance)
            {
                _enemyBehaviour = EnemyBehaviour.Attack;
                _bodyFlickerCoro = StartCoroutine(BodyFlicker());
                modelAnimator.SetFloat("move", 0f);
                return;
            }
            else if(distanceSqr > enemyState.stopChasingDistance*enemyState.stopChasingDistance)
            {
                _enemyBehaviour = EnemyBehaviour.Idle;
                modelAnimator.SetFloat("move", 0f);
                return;
            }

            Quaternion rot = dirXZ.sqrMagnitude > float.Epsilon ? 
                Quaternion.LookRotation(dirXZ, transform.up)
                : Quaternion.identity;
            _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, rot,enemyState.rotateSpeed * Time.deltaTime));
            // myRigidbody.MovePosition(myRigidbody.position + dirXZ*Time.deltaTime*myEnemyState.moveSpeed);
            _rb.MovePosition(_rb.position + enemyState.moveSpeed*Time.deltaTime*transform.forward);

            modelAnimator.SetFloat("move", dirXZ.magnitude);
        }
        else if(_enemyBehaviour == EnemyBehaviour.Attack)
        {
            if(distanceSqr > enemyState.stopAttackingDistance*enemyState.stopAttackingDistance)
            {
                _enemyBehaviour = EnemyBehaviour.Chase;
                StopCoroutine(_bodyFlickerCoro);
                GlowObj.SetActive(false); //防止闪烁时定格在发光状态
                _explosionTimer = 0f;
                return;
            }

            _explosionTimer += Time.deltaTime;
            if(_explosionTimer > enemyState.explosionWaitingTime)
            {
                Explode();
            }
        }
        #endregion
    }
    private Coroutine _bodyFlickerCoro;
    private float flickerGapTime = 0.2f;
    IEnumerator BodyFlicker()
    {
        bool isLighted = false;
        while(true)
        {
            GlowObj.SetActive(isLighted); // 这里可能可以通过修改material参数来实现淡入淡出，SetActive可能会有多余的性能损耗
            isLighted = !isLighted;
            yield return new WaitForSeconds(flickerGapTime);
        }
    }
    public void Explode()
    {
        var myPosition = transform.position;
        ObjectPooler.Instance.SpawnFromPool(objectPoolTagParticleSystemExplosion, myPosition + Vector3.up*1f, Quaternion.identity);

        Collider[] hitColliders = Physics.OverlapSphere(myPosition, enemyState.startAttackingDistance);
        for(int i=0; i<hitColliders.Length; ++i)
        {
            PlayerController player = hitColliders[i].GetComponent<PlayerController>();
            if(player != null)
            {
                int actualDamage = (int)((float)enemyState.explosionDamage *
                (1 - Vector3.Distance(transform.position, hitColliders[i].GetComponent<Transform>().position)/enemyState.explosionDistance));
                player.Hurt(actualDamage, DamageFrom.Enemy);
            }
        }
        enemyState.hp = 0;
        enemyState.isExploded = true;
        Die();
    }
    public void GetDamage(int damage, float recoilVelocity)
    {
        _rb.AddForce(recoilVelocity * (-1f) * transform.forward, ForceMode.VelocityChange);
        if (enemyState.isDead) { return; }
        enemyState.hp -= damage;
        if (enemyState.hp <= 0)
        {
            enemyState.hp = 0;
            Die();
        }
    }
    public void Die()
    {
        enemyState.isDead = true;
        if (enemyState.isExploded == false)
        {
            ObjectPooler.Instance.SpawnFromPool(objectPoolTagParticleSystemEnemyDeath,transform.position + Vector3.up * 0.2f, Quaternion.identity);
        }
        LevelManager.singleton.KillEnemy();
        ObjectPooler.Instance.DestroyToPool("EnemyExplosion", gameObject);
    }
}