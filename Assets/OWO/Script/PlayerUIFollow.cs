using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIFollow : MonoBehaviour
{
    public Transform Player;
    public float t;
    public float height;
    private Transform myTransform;
    private void Awake()
    {
        myTransform = GetComponent<Transform>();
    }
    void FixedUpdate()
    {
        Vector3 targetPos = Player.position;
        targetPos.y += height;
        myTransform.position = Vector3.Lerp(myTransform.position, targetPos, Time.deltaTime * t);
    }
}
