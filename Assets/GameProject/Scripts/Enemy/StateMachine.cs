using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine : MonoBehaviour
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
        if (nextStateType != currentState)
        {
            nextStateType.OnStateStart();
            currentState.OnStateExit();
        }
        currentState = nextStateType;
    }
}
