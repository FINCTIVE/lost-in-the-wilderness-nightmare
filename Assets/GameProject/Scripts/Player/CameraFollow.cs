using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Experimental.PlayerLoop;
using UnityEngine.Serialization;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float speed;
    public Vector3 offset;
    private Transform _transform;
    void Awake()
    {
        _transform = GetComponent<Transform>();
    }

    void FixedUpdate()
    {
        Vector3 targetCameraPos = target.position + offset;
        _transform.position = Vector3.Lerp(_transform.position,targetCameraPos,speed * Time.deltaTime);
    }
}
