using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    #region GameObject
    public GameObject GlowObj;
    public GameObject ExplosionParticleObj;
    public GameObject DeathParticleObj;
    #endregion
    #region Component
    public Animator modelAnimator;
    private Transform myTransform;
    private Rigidbody myRigidbody;
    private EnemyState myEnemyState;
    #endregion

    private enum EnemyBehaviour{ Idle, Chase, Attack }
    private EnemyBehaviour enemyBehaviour = EnemyBehaviour.Idle;
    private float explosionTimer = 0f;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
        myTransform = GetComponent<Transform>();
        modelAnimator = myTransform.Find("Model").GetComponent<Animator>();
        myEnemyState = GetComponent<EnemyState>();
    }

    private void FixedUpdate()
    {
        Vector3 targetPos = PlayerController.playerTransform.position;
        float distanceSqr = (targetPos - myTransform.position).sqrMagnitude;
        Vector3 dirXZ = targetPos - myTransform.position;
        dirXZ.y = 0;
        if (distanceSqr > 1) { dirXZ = Vector3.Normalize(dirXZ); }

        #region 状态机
        // idle <----> chase <----> attck
        if(enemyBehaviour == EnemyBehaviour.Idle)
        {
            if (distanceSqr < myEnemyState.startChasingDistance*myEnemyState.startChasingDistance)
            {
                enemyBehaviour = EnemyBehaviour.Chase;
                return;
            }
        }
        else if(enemyBehaviour == EnemyBehaviour.Chase)
        {
            if(distanceSqr < myEnemyState.startAttackingDistance*myEnemyState.startAttackingDistance)
            {
                enemyBehaviour = EnemyBehaviour.Attack;
                BodyFlickerCoro = StartCoroutine(BodyFlicker());
                modelAnimator.SetFloat("move", 0f);
                return;
            }
            else if(distanceSqr > myEnemyState.stopChasingDistance*myEnemyState.stopChasingDistance)
            {
                enemyBehaviour = EnemyBehaviour.Idle;
                modelAnimator.SetFloat("move", 0f);
                return;
            }

            myRigidbody.MovePosition(myRigidbody.position + dirXZ*Time.deltaTime*myEnemyState.moveSpeed);
            Quaternion rot = Quaternion.LookRotation(dirXZ, myTransform.up);
            myRigidbody.MoveRotation(Quaternion.Lerp(myRigidbody.rotation, rot,       myEnemyState.rotateSpeed * Time.deltaTime));

            modelAnimator.SetFloat("move", dirXZ.magnitude);
        }
        else if(enemyBehaviour == EnemyBehaviour.Attack)
        {
            if(distanceSqr > myEnemyState.stopAttackingDistance*myEnemyState.stopAttackingDistance)
            {
                enemyBehaviour = EnemyBehaviour.Chase;
                StopCoroutine(BodyFlickerCoro);
                GlowObj.SetActive(false); //防止闪烁时定格在发光状态
                explosionTimer = 0f;
                return;
            }

            explosionTimer += Time.deltaTime;
            if(explosionTimer > myEnemyState.explosionWaitingTime)
            {
                Explode();
            }
        }
        #endregion
    }
    private Coroutine BodyFlickerCoro;
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
        Instantiate(ExplosionParticleObj, 
            myTransform.position + Vector3.up*1f, Quaternion.identity);
        Collider[] hitColliders = Physics.OverlapSphere(myTransform.position, myEnemyState.startAttackingDistance);
        for(int i=0; i<hitColliders.Length; ++i)
        {
            PlayerController player = hitColliders[i].GetComponent<PlayerController>();
            if(player != null)
            {
                int actualDamage = (int)((float)myEnemyState.explosionDamage *(1 - 
                    (myTransform.position - hitColliders[i].GetComponent<Transform>().position).sqrMagnitude
                    / (myEnemyState.startAttackingDistance * myEnemyState.startAttackingDistance)));
                player.GetDamage(actualDamage);
            }
        }
        myEnemyState.hp = 0;
        myEnemyState.isExploded = true;
        Die();
    }
    public void GetDamage(int damage)
    {
        myRigidbody.AddForce(myTransform.forward * (-1f) * myEnemyState.recoilVelocity, ForceMode.VelocityChange);
        if (myEnemyState.isDead) { return; }
        myEnemyState.hp -= damage;
        if (myEnemyState.hp <= 0)
        {
            myEnemyState.hp = 0;
            Die();
        }
    }
    public void Die()
    {
        myEnemyState.isDead = true;
        if (myEnemyState.isExploded == false)
        {
            GameObject deathParticle = Instantiate(DeathParticleObj,
                myTransform.position + Vector3.up * 0.2f, Quaternion.identity);
            Destroy(deathParticle, 4f);
        }
        GameManager.singleton.KillEnemy();
        Destroy(gameObject);
    }
}