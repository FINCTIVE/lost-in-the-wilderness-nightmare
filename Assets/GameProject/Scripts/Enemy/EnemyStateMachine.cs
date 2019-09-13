using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    public BaseState defaultState;
    [HideInInspector]public BaseState currentState;

    private void Awake()
    {
        currentState = defaultState;
    }

    void FixedUpdate()
    {
        BaseState nextStateType = currentState.Tick();
        currentState = nextStateType;
    }
}
