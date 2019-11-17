using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public enum DamageFrom
{
    Enemy,
    Fire,
    Cold
}
/// <summary>
/// 组件功能：移动，开火，与武器管理系统、玩家状态系统交互。
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController player;
    public static Transform playerTransform;
    public PlayerInfo playerInfo;
    
    [Tooltip("辅助瞄准的最远距离")]public float aimingDistance; // 
    [Tooltip("辅助瞄准视角大小")]public float aimingAngle;
    
    public GameObject ParticleSystem_Death;
    
    //组件引用缓存
    private Animator _animator;
    private Rigidbody _rigidbody;
    private Transform _transform;
    private Backpack _backpack;

    public event Action<DamageFrom> OnPlayerDie;
    
    void Awake()
    {
        playerTransform = GetComponent<Transform>();
        player = this;

        _rigidbody = GetComponent<Rigidbody>();
        _transform = GetComponent<Transform>();
        _backpack = GetComponent<Backpack>();
        _animator = _transform.Find("Model").GetComponent<Animator>();
    }

    //INPUT
    private Vector3 _iDir = Vector3.zero; //单位向量
    private Vector3 _iRawDir = Vector3.zero; //未经处理的输入向量
    private bool _fire = false; // 当为true时发射一发子弹
    private bool _iLook = false; // 是否站在原地旋转
    private bool _iNextWeapon = false;
    private bool _iLastWeapon = false;
    //State
    private float _fireTimer = 0f;
    void Update()
    {
        // Input操作放在Update，否则类似GetButtonDown这类操作有可能无法捕捉到
        _iDir.x = Input.GetAxis("Horizontal");
        _iDir.z = Input.GetAxis("Vertical");
        _iRawDir.x = Input.GetAxisRaw("Horizontal");
        _iRawDir.z = Input.GetAxisRaw("Vertical");
        //限制向量的模长度小于等于1
        if (_iDir.sqrMagnitude > 1f) { _iDir = Vector3.Normalize(_iDir); }

        if (_backpack.weaponInHand)
        {
            bool iFireHold = Input.GetButton("Fire");
            //点射将会直接开火 第一发子弹不受射速限制 
            if (Input.GetButtonDown("Fire"))
            {
                _fire = true;
            }
            else if (iFireHold && _backpack.weaponInHand.weaponType != Weapon.WeaponType.Pistol)
            {
                _fireTimer += Time.deltaTime;
                if (_fireTimer > _backpack.weaponInHand.rate)
                {
                    _fire = true;
                    _fireTimer = 0f;
                }
            }
        }

        if (Input.GetButtonDown("LastWeapon")) { _iLastWeapon = true; }
        else if (Input.GetButtonDown("NextWeapon")) { _iNextWeapon = true; }

        if (Input.GetButton("Look")) { _iLook = true; }
        else { _iLook = false; }
    }
    private static readonly int AnimHashMove = Animator.StringToHash("move");
    private static readonly int AnimHashHand = Animator.StringToHash("hand");
    void FixedUpdate()
    {
        Move(_iDir, _iRawDir.sqrMagnitude, _iLook);

        if (_fire)
        {
            Vector3 aimingDir = _backpack.weaponInHand.FirePoint.forward;
            aimingDir.y = 0;

            Aim(aimingDistance, aimingAngle, ref aimingDir);
            Fire(aimingDir);
            if (_backpack.weaponInHand.ammo == 0)
            {
                _iNextWeapon = true;
            }
            _fire = false;
        }

        if (_iNextWeapon)
        {
            SwapWeapon(true);
            _iNextWeapon = false;
        }
        else if (_iLastWeapon)
        {
            SwapWeapon(false);
            _iLastWeapon = false;
        }

        // 动画系统
        _animator.SetFloat(AnimHashMove, _iDir.sqrMagnitude);
        _animator.SetFloat(AnimHashHand, Mathf.Lerp(_animator.GetFloat(AnimHashHand), targetAnimValueOfHand, 20f * Time.deltaTime)); // 平滑换枪hhh
    }

    //移动
    private void Move(Vector3 moveDirXz, float rawInputDirSqrMag, bool isLooking)
    {
        if (!isLooking)
        {
            _rigidbody.MovePosition(_transform.position + Time.deltaTime * playerInfo.moveSpeed * moveDirXz);
        }

        // &&左边第一个条件保证玩家松手的时候一定不会转动角色
        // Unity的Input类做了平滑处理
        float rotSpeed = 20f;
        if (isLooking)
        {
            rotSpeed = 5f;
        }

        if (rawInputDirSqrMag > float.Epsilon && moveDirXz.magnitude > float.Epsilon)
        {
            Quaternion rot = Quaternion.LookRotation(moveDirXz, Vector3.up);
            _rigidbody.MoveRotation(Quaternion.Lerp(_rigidbody.rotation, rot, Time.deltaTime * rotSpeed)); //平滑变量硬编码
        }
    }

    // 辅助瞄准 计算出FirePoint指向的位置
    private void Aim(float mAimingDistance, float mAimingAngle, ref Vector3 mFirePointDir)
    {
        Collider[] hitColliders = Physics.OverlapSphere(_transform.position, mAimingDistance);

        Transform targetEnemy = null;
        if (hitColliders.Length > 0)
        {
            //寻找夹角范围内距离最近的敌人
            for (int i = 0; i < hitColliders.Length; ++i)
            {
                if (!hitColliders[i].CompareTag("Enemy")) continue;

                Vector3 ePos = hitColliders[i].GetComponent<Transform>().position;
                if (Mathf.Abs(Vector3.Angle(ePos - _transform.position, _backpack.weaponInHand.FirePoint.forward)) < mAimingAngle)
                {
                    if (targetEnemy == null)
                    {
                        targetEnemy = hitColliders[i].GetComponent<Transform>();
                    }
                    else if ((targetEnemy.position - _transform.position).sqrMagnitude
                        > (ePos - _transform.position).sqrMagnitude)
                    {
                        targetEnemy = hitColliders[i].GetComponent<Transform>();
                    }
                }
            }

            // 如果瞄准范围内有敌人，改变FirePoint朝向
            // 否则保持原来的朝向，这样才能发射子弹到瞄准范围外的东西
            if (targetEnemy != null)
            {
                mFirePointDir = targetEnemy.position - _transform.position;
                mFirePointDir.y = 0;
            }
        }
    }

    // 开枪 射出一发子弹
    private void Fire(Vector3 mAmingDir)
    {
        RaycastHit hit;
        if (Physics.Raycast(_backpack.weaponInHand.FirePoint.position, mAmingDir, out hit))
        {
            EnemyController enemyController = hit.collider.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.GetDamage(_backpack.weaponInHand.damage, _backpack.weaponInHand.recoilForce * 6f);
            }
            //击中物体后产生的火花
            ObjectPooler.Instance.SpawnFromPool(_backpack.weaponInHand.objectPoolTagSpark, hit.point, Quaternion.identity);
        }
        //弹道轨迹(假子弹)
        GameObject mBullet = ObjectPooler.Instance.SpawnFromPool(_backpack.weaponInHand.objectPoolTagBulletTrail, _backpack.weaponInHand.FirePoint.position, Quaternion.LookRotation(mAmingDir, Vector3.up)) as GameObject;
        //枪口的闪光粒子特效
        _backpack.weaponInHand.GunFire.Emit(1);
        //后座力
        _rigidbody.AddForce(_backpack.weaponInHand.recoilForce * (-1f) * _transform.forward, ForceMode.VelocityChange);
        //子弹
        _backpack.weaponInHand.ammo--;
    }

    private float targetAnimValueOfHand;
    public void SwapWeapon(bool isNextWeapon)
    {
        _backpack.ChangeWeaponInHand(isNextWeapon);
        if (_backpack.weaponInHand == null)
        {
            targetAnimValueOfHand = 0f; // 使用BlendTree改变武器切换动画，对应值请看Animator窗口
        }
        else
        {
            if (_backpack.weaponInHand.weaponType == Weapon.WeaponType.Pistol)
            {
                targetAnimValueOfHand = 0.5f;
            }
            else if(_backpack.weaponInHand.weaponType == Weapon.WeaponType.Rifle)
            {
                targetAnimValueOfHand = 1f;
            }
        }
    }
    

    public void Hurt(int damage, DamageFrom attacker)
    {
        if (playerInfo.isDead) return;

        playerInfo.hp -= damage;
        if (playerInfo.hp <= 0)
        {
            playerInfo.hp = 0;
            Die(attacker);
        }
    }

    private float _hurtPerSecTimer = 0f;

    public void HurtByDamagePerSec(int damagePerSec, DamageFrom attacker)
    {
        _hurtPerSecTimer += Time.deltaTime;
        if (_hurtPerSecTimer > 1f)
        {
            _hurtPerSecTimer -= 1f;
            Hurt(damagePerSec, attacker);
        }
    }
    
    public void Die(DamageFrom attacker)
    {
        playerInfo.isDead = true;
        //死亡过程硬编码
        _rigidbody.constraints = RigidbodyConstraints.None; //取消限制 自由倒地
        _rigidbody.AddForce(Vector3.up * (-1f) + Vector3.right * (-2f), ForceMode.Impulse); //推一把
        this.enabled = false;
        Instantiate(ParticleSystem_Death, _transform.position + Vector3.up * 0.2f, ParticleSystem_Death.transform.rotation); // 例子效果 黑暗之门
        if (OnPlayerDie != null)
            OnPlayerDie(attacker);
        LevelManager.singleton.PlayerDie();
    }
}
