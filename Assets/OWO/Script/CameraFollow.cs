using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public float speed;
    public Vector3 offset;
    private Transform myTransform;
    void Awake()
    {
        myTransform = GetComponent<Transform>();
    }

    void FixedUpdate()
    {
        Vector3 targetCameraPos = Target.position + offset;
        myTransform.position = Vector3.Lerp(myTransform.position,
            targetCameraPos, speed * Time.deltaTime);
    }
}
