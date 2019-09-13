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
    private EnemyStateMachine _enemyStateMachine;
    #endregion

    private void Awake()
    {
        _initialEnemyInfo = new EnemyInfo();
        _initialEnemyInfo.CopyValues(enemyInfo);
        
        _rb = GetComponent<Rigidbody>();
        _enemyStateMachine = GetComponent<EnemyStateMachine>();
        modelAnimator = transform.Find("Model").GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
    }
    
    public void OnObjectSpawn()
    {
        enemyInfo.CopyValues(_initialEnemyInfo);
        _enemyStateMachine.currentState = _enemyStateMachine.defaultState;
    }

    public void OnObjectDestroy()
    {
        
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
            StopCoroutine(_bodyFlickerCoro); // TODO: StopCoroutine后_bodyFlickerCoro还在不在？
            GlowObj.SetActive(false); //防止闪烁时定格在发光状态
        }
    }
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
