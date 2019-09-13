using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyIdlingState : BaseState
{
    public EnemyChasingState enemyChasingState;
    
    private EnemyController _enemyController;

    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
    }
    
    public override BaseState Tick()
    {
        Vector3 targetPos = PlayerController.playerTransform.position;
        float distanceSqr = (targetPos - transform.position).sqrMagnitude;
        if (distanceSqr < _enemyController.enemyInfo.startChasingDistance * _enemyController.enemyInfo.startChasingDistance)
        {
            return enemyChasingState;
        }

        return this;
    }
}
