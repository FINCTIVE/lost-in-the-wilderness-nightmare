using UnityEngine;
using System.Collections;

public class BulletTrail : MonoBehaviour, IPooledObject
{
    public string objectPoolTag;
    public float speed;
    public float destroyTime = 3f;
    private Transform _transform;
    void Awake()
    {
        _transform = GetComponent<Transform>();
    }
    public void OnObjectSpawn()
    {
//        GetComponent<TrailRenderer>().enabled = true;
        GetComponent<TrailRenderer>().Clear();
        StartCoroutine(ObjectPooler.Instance.DestroyToPoolCoroutine(objectPoolTag, gameObject, destroyTime));
    }

    public void OnObjectDestroy()
    {
    }

    private void FixedUpdate()
    {
        _transform.position += speed * Time.deltaTime * _transform.forward;
    }
}
