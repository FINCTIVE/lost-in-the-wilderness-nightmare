using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjPooledAutoDestroy : MonoBehaviour, IPooledObject
{
    public string objPoolTag;
    public float destroyTime;
    public void OnObjectSpawn()
    {
        StartCoroutine(ObjectPooler.Instance.DestroyToPoolCoroutine(objPoolTag, gameObject, destroyTime));
    }

    public void OnObjectDestroy()
    {
    }
}
