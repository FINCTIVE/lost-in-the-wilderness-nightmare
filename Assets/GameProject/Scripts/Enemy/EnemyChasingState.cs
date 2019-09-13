using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyChasingState : BaseState
{
    public EnemyAttackingState enemyAttackingState;
    public EnemyIdlingState enemyIdlingState;
    
    private EnemyController _enemyController;
    private NavMeshAgent _navMeshAgent;
    private Rigidbody _rb;
    public Animator modelAnimator;

    private void Awake()
    {
        _enemyController = GetComponent<EnemyController>();
        _rb = GetComponent<Rigidbody>();
        modelAnimator = transform.Find("Model").GetComponent<Animator>();
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
    }
    private static readonly int AnimMove = Animator.StringToHash("move");
    public override BaseState Tick()
    {
        Vector3 targetPos = PlayerController.playerTransform.position;
        
        _navMeshAgent.SetDestination(targetPos);
        var myPosition = transform.position;
        _navMeshAgent.nextPosition = myPosition;

        float distanceSqr = (targetPos - myPosition).sqrMagnitude;
        Vector3 dirXZ = _navMeshAgent.desiredVelocity;
        dirXZ.y = 0;
        if (distanceSqr > 1) { dirXZ = Vector3.Normalize(dirXZ); }
        
        if(distanceSqr < _enemyController.enemyInfo.startAttackingDistance*_enemyController.enemyInfo.startAttackingDistance)
        {
            _enemyController.SetBodyFlicker(true);
            modelAnimator.SetFloat(AnimMove, 0f);
            return enemyAttackingState;
        }
        else if(distanceSqr > _enemyController.enemyInfo.stopChasingDistance*_enemyController.enemyInfo.stopChasingDistance)
        {
            modelAnimator.SetFloat(AnimMove, 0f);
            return enemyIdlingState;
        }

        Quaternion rot = dirXZ.sqrMagnitude > float.Epsilon ? 
            Quaternion.LookRotation(dirXZ, transform.up)
            : Quaternion.identity;
        _rb.MoveRotation(Quaternion.Lerp(_rb.rotation, rot,_enemyController.enemyInfo.rotateSpeed * Time.deltaTime));
        _rb.MovePosition(_rb.position + _enemyController.enemyInfo.moveSpeed*Time.deltaTime*transform.forward);
        modelAnimator.SetFloat(AnimMove, dirXZ.magnitude);

        return this;
    }
}
