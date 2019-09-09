using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjPooledParticle : MonoBehaviour, IPooledObject
{
    public string objPoolTag;
    public float destroyTime;
    public void OnObjectSpawn()
    {
        StartCoroutine(ObjectPooler.Instance.DestroyToPoolCoroutine(objPoolTag, gameObject, destroyTime));
        GetComponent<ParticleSystem>().Play();
    }

    public void OnObjectDestroy()
    {
        
    }
}
