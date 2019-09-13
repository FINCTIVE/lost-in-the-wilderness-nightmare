using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseState : MonoBehaviour
{
    public abstract BaseState Tick();
    /// <summary>
    /// 与*本状态*有关的初始化代码
    /// </summary>
    public virtual void OnStateStart(){}
    /// <summary>
    /// 与*本状态*有关的退出代码
    /// </summary>
    public virtual void OnStateExit(){}
}
