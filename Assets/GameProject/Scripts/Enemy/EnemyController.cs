using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AI;

public class EnemyController : MonoBehaviour, IPooledObject
{
    public EnemyInfo enemyInfo;
    private EnemyInfo _initialEnemyInfo;
    
    #region GameObject
    public GameObject GlowObj;
    public string objectPoolTagParticleSystemExplosion = "ParticleSystemExplosion";
    public string objectPoolTagParticleSystemEnemyDeath = "ParticleSystemEnemyDeath";
    #endregion
    #region Component
    public Animator modelAnimator;
    private Rigidbody _rb;
    private NavMeshAgent _navMeshAgent;
    private StateMachine _stateMachine;
    #endregion

    private void Awake()
    {
        _initialEnemyInfo = new EnemyInfo();
        _initialEnemyInfo.CopyValues(enemyInfo);
        
        _rb = GetComponent<Rigidbody>();
        _stateMachine = GetComponent<StateMachine>();
//        modelAnimator = transform.Find("Model").GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
    }
    
    public void OnObjectSpawn()
    {
        enemyInfo.CopyValues(_initialEnemyInfo);
        _stateMachine.currentState = _stateMachine.defaultState;
        SetBodyFlicker(false);
    }

    public void OnObjectDestroy(){}
    
    private static readonly int AnimMove = Animator.StringToHash("move");
    public void MoveToTarget(Vector3 targetPos)
    {
        _navMeshAgent.SetDestination(targetPos); // TODO:降低调用频率优化性能
        var myPosition = transform.position;
        _navMeshAgent.nextPosition = myPosition;

        float distanceSqr = (targetPos - myPosition).sqrMagnitude;
        Vector3 dirXZ = _navMeshAgent.desiredVelocity;
        dirXZ.y = 0;
        if (distanceSqr > 1) { dirXZ = Vector3.Normalize(dirXZ); }
        
        Quaternion rot = dirXZ.sqrMagnitude > float.Epsilon ? 
            Quaternion.LookRotation(dirXZ, transform.up)
            : Quaternion.identity;
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, rot,enemyInfo.rotateSpeed * Time.deltaTime));
        _rb.MovePosition(_rb.position + enemyInfo.moveSpeed*Time.deltaTime*transform.forward);
        modelAnimator.SetFloat(AnimMove, dirXZ.magnitude);
    }
    private Coroutine _bodyFlickerCoro;
    public void SetBodyFlicker(bool isFlickering)
    {
        if (isFlickering)
        {
            _bodyFlickerCoro = StartCoroutine(BodyFlicker());
        }
        else
        {
            GlowObj.SetActive(false); //防止闪烁时定格在发光状态
            if(_bodyFlickerCoro != null)
                StopCoroutine(_bodyFlickerCoro); // TODO: StopCoroutine后_bodyFlickerCoro还在不在？
        }
    }
    private float flickerGapTime = 0.2f;
    private IEnumerator BodyFlicker()
    {
        bool isLighted = false;
        while(true)
        {
            GlowObj.SetActive(isLighted); // TODO:这里可能可以通过修改material参数来实现淡入淡出，SetActive可能会有多余的性能损耗
            isLighted = !isLighted;
            yield return new WaitForSeconds(flickerGapTime);
        }
    }
    
    public void Explode()
    {
        var myPosition = transform.position;
        ObjectPooler.Instance.SpawnFromPool(objectPoolTagParticleSystemExplosion, myPosition + Vector3.up, Quaternion.identity);

        Collider[] hitColliders = Physics.OverlapSphere(myPosition, enemyInfo.startAttackingDistance);
        for(int i=0; i<hitColliders.Length; ++i)
        {
            PlayerController player = hitColliders[i].GetComponent<PlayerController>();
            if(player != null)
            {
                int actualDamage = (int)((float)enemyInfo.explosionDamage *
                (1 - Vector3.Distance(transform.position, hitColliders[i].GetComponent<Transform>().position)/enemyInfo.explosionDistance));
                player.Hurt(actualDamage, DamageFrom.Enemy);
            }
        }
        enemyInfo.hp = 0;
        enemyInfo.isExploded = true;
        Die();
    }
    public void GetDamage(int damage, float recoilVelocity)
    {
        _rb.AddForce(recoilVelocity * (-1f) * transform.forward, ForceMode.VelocityChange);
        if (enemyInfo.isDead) { return; }
        enemyInfo.hp -= damage;
        if (enemyInfo.hp <= 0)
        {
            enemyInfo.hp = 0;
            Die();
        }
    }
    public void Die()
    {
        enemyInfo.isDead = true;
        if (enemyInfo.isExploded == false)
        {
            ObjectPooler.Instance.SpawnFromPool(objectPoolTagParticleSystemEnemyDeath,transform.position + Vector3.up * 0.2f, Quaternion.identity);
        }
        LevelManager.singleton.KillEnemy();
        ObjectPooler.Instance.DestroyToPool("EnemyExplosion", gameObject);
    }
}
