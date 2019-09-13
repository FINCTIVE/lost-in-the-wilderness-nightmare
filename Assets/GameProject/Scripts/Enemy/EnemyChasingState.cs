using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChasingState : BaseState
{
    public EnemyAttackingState enemyAttackingState;
    public EnemyIdlingState enemyIdlingState;
    
    private EnemyController _enemyController;

    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
    }
    private static readonly int AnimMove = Animator.StringToHash("move");
    public override BaseState Tick()
    {
        Vector3 targetPos = PlayerController.playerTransform.position;
        _enemyController.MoveToTarget(targetPos);
        
        float distanceSqrMag = (targetPos - transform.position).sqrMagnitude;
        if(distanceSqrMag < _enemyController.enemyInfo.startAttackingDistance*_enemyController.enemyInfo.startAttackingDistance)
        {
            return enemyAttackingState;
        }
        if(distanceSqrMag > _enemyController.enemyInfo.stopChasingDistance*_enemyController.enemyInfo.stopChasingDistance)
        {
            return enemyIdlingState;
        }
        return this;
    }

    public override void OnStateExit()
    {
        _enemyController.modelAnimator.SetFloat(AnimMove, 0f);
    }
}
