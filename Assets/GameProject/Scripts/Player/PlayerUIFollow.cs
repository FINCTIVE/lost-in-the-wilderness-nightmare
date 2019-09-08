using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerUIFollow : MonoBehaviour
{
    public float t;
    public float height;
    private Transform _player;
    private Transform _transform;
    private void Start()
    {
        _player = PlayerController.playerTransform;
        _transform = GetComponent<Transform>();
    }
    void FixedUpdate()
    {
        Vector3 targetPos = _player.position;
        targetPos.y += height;
        _transform.position = Vector3.Lerp(_transform.position, targetPos, Time.deltaTime * t);
    }
}
