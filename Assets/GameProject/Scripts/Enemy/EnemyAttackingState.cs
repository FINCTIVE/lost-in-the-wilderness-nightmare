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
    public override BaseState Tick()
    {
        Vector3 targetPos = PlayerController.playerTransform.position;
        float distanceSqr = (targetPos - transform.position).sqrMagnitude;
        
        if(distanceSqr > _enemyController.enemyInfo.stopAttackingDistance*_enemyController.enemyInfo.stopAttackingDistance)
        {
            _enemyController.SetBodyFlicker(false);
            _explosionTimer = 0f;
            return enemyChasingState;
        }

        _explosionTimer += Time.deltaTime;
        if(_explosionTimer > _enemyController.enemyInfo.explosionWaitingTime)
        {
            _enemyController.Explode();
        }

        return this;
    }
}
