using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerState : MonoBehaviour
{
    public int hp;
    public float moveSpeed;
    //public float jumpForce;
    public bool isDead = false;
}
