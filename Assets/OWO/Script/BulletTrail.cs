using UnityEngine;
using System.Collections;

public class BulletTrail : MonoBehaviour
{
    public float speed;
    private Transform myTransform;
    void Awake()
    {
        myTransform = GetComponent<Transform>();
    }
    private void Start()
    {
        Destroy(gameObject, 10f); //如果子弹什么都没碰到，将会自动销毁
    }
    private void FixedUpdate()
    {
        myTransform.position += myTransform.forward * speed * Time.deltaTime;
    }
}
