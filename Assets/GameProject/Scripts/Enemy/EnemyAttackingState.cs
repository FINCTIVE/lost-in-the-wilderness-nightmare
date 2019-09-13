using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackingState : BaseState
{
    public EnemyChasingState enemyChasingState;
    private EnemyController _enemyController;

    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
    }
    private float _explosionTimer = 0f;
    public override void OnStateStart()
    {
        _enemyController.SetBodyFlicker(true);
        _explosionTimer = 0f;
    }
    public override BaseState Tick()
    {
        Vector3 targetPos = PlayerController.playerTransform.position;
        float distanceSqr = (targetPos - transform.position).sqrMagnitude;
        
        if(distanceSqr > _enemyController.enemyInfo.stopAttackingDistance*_enemyController.enemyInfo.stopAttackingDistance)
        {
            return enemyChasingState;
        }

        _explosionTimer += Time.deltaTime;
        if(_explosionTimer > _enemyController.enemyInfo.explosionWaitingTime)
        {
            _enemyController.Explode(); // 该敌人死亡，之后的逻辑由对象池相关代码掌控；而不是状态机代码
        }
        return this;
    }
    public override void OnStateExit()
    {
        _enemyController.SetBodyFlicker(false);
    }
}
