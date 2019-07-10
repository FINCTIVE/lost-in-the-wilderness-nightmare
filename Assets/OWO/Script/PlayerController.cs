using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PlayerController : MonoBehaviour
{
    public static PlayerController player;
    public static Transform playerTransform;
   
    public Text Text_Pistol_Ammo;
    public Text Text_Rifle_Ammo;
    public Slider Slider_Health;
    public Image HealthFill;
    public Color WarmBloodColor;
    public Color ColdBloodColor;
    public Transform HeatPoint; //热源中心
    public GameObject ParticleSystem_Death;
    public Weapon[] Weapons;
    private int myWeaponId = 0; // 空手
    public float aimingDistance; // 辅助瞄准的最远距离
    public float aimingAngle; // 辅助瞄准视角大小
    public float getColdDistanceStart; // 开始计算寒冷伤害的距离
    public float getColdDistanceEnd; // 往后因寒冷受伤的伤害值一样
    public float gettingColdDamagePerSec;

    private Animator myAnimator;
    private Rigidbody myRigidbody;
    private Transform myTransform;
    private PlayerState myPlayerState;
    
    void Awake()
    {
        playerTransform = GetComponent<Transform>();
        player = this;

        foreach(Weapon w in Weapons)
        {
            w.gameObject.SetActive(false);
        }
        Weapons[0].gameObject.SetActive(true); //设置成空手状态

        myPlayerState = GetComponent<PlayerState>();
        myRigidbody = GetComponent<Rigidbody>();
        myTransform = GetComponent<Transform>();
        myAnimator = myTransform.Find("Model").GetComponent<Animator>();
    }

    //INPUT
    Vector3 dirXZ = Vector3.zero; //单位向量
    Vector3 rawDirXZ = Vector3.zero; //未经处理的输入向量
    bool fire = false; // 当为true时发射一发子弹
    bool look = false; // 是否站在原地旋转
    bool nextWeapon = false;
    bool lastWeapon = false;

    void Update()
    {
        // Input操作建议放在Update，否则类似GetButtonDown这类操作有可能无法捕捉到
        dirXZ.x = Input.GetAxis("Horizontal");
        dirXZ.z = Input.GetAxis("Vertical");
        rawDirXZ.x = Input.GetAxisRaw("Horizontal");
        rawDirXZ.z = Input.GetAxisRaw("Vertical");
        //限制向量的模长度小于等于1
        if (dirXZ.sqrMagnitude > 1f) {dirXZ = Vector3.Normalize(dirXZ);}  
        
        // 手枪：设置rate为无穷大值，只能单发
        // 空手状态：因为换枪的时候不会换到无子弹的抢，空手状态子弹无限大
        // 这个办法好像不是那么优雅...会影响到其他部分的代码，甚至有隐藏的bug
        bool fireHold = false;
        if(Input.GetButton("Fire")) { fireHold = true; }
        else { fireHold = false; }
        //点射将会直接开火 第一发子弹不受射速限制 
        if(Input.GetButtonDown("Fire")) { fire = true; }
        if(fireHold)
        {
            fireTimer += Time.deltaTime;
            // 当rate非常高时，只能点射(手枪)
            if(fireTimer > Weapons[myWeaponId].rate)
            {
                fire = true;
                fireTimer = 0f;
            }
        }
        //防止空手时fire为true,切换枪后会自动发射一发子弹
        if(Weapons[myWeaponId].weaponType == Weapon.WeaponType.EmptyHands){ fire = false;}

        if (Input.GetButtonDown("LastWeapon")) { lastWeapon = true; }
        else if (Input.GetButtonDown("NextWeapon")) { nextWeapon = true; }
        
        if(Input.GetButton("Look")) { look = true; }
        else{ look = false; }
    }

    //State
    private float fireTimer=0f;
    void FixedUpdate()
    {

        Move(dirXZ, rawDirXZ.sqrMagnitude, look);


        if (fire)
        {
            Vector3 aimingDir = Weapons[myWeaponId].FirePoint.forward;
            aimingDir.y = 0;

            Aim(aimingDistance, aimingAngle, ref aimingDir);
            Fire(aimingDir); 
            if(Weapons[myWeaponId].ammo == 0)
            {
                nextWeapon = true;
            }
            UpdateWeaponUI();

            fire = false;
        }

        if(nextWeapon)
        {
            SwapWeapon(true);
            nextWeapon = false;
        }
        else if(lastWeapon)
        {
            SwapWeapon(false);
            lastWeapon = false;
        }

        TemperatureControl();

        // 动画系统
        myAnimator.SetFloat("move", dirXZ.sqrMagnitude);
        float targetAnimValue_hand = (float)Weapons[myWeaponId].weaponType/((float)Weapons.Length-1f);
        //使用Blend Tree平滑武器切换 平滑变量硬编码
        myAnimator.SetFloat("hand", Mathf.Lerp(myAnimator.GetFloat("hand"), targetAnimValue_hand, 20f*Time.deltaTime)); 
    }

    //移动
    private void Move(Vector3 moveDirXZ, float rawInputDirSqrMag, bool isLooking)
    {
        if(!isLooking)
        {
            myRigidbody.MovePosition(myTransform.position + moveDirXZ * myPlayerState.moveSpeed * Time.deltaTime);
        }

        // &&左边第一个条件保证玩家松手的时候一定不会转动角色
        // Unity的Input类做了平滑处理
        float rotSpeed = 20f;
        if(isLooking)
        {
            rotSpeed = 5f;
        }

        if (rawInputDirSqrMag > float.Epsilon && moveDirXZ.magnitude > float.Epsilon)
        {
            Quaternion rot = Quaternion.LookRotation(moveDirXZ, Vector3.up);
            myRigidbody.MoveRotation(Quaternion.Lerp(myRigidbody.rotation, rot, Time.deltaTime * rotSpeed)); //平滑变量硬编码
        }
    }

    // 辅助瞄准 计算出FirePoint指向的位置
    private void Aim(float mAimingDistance, float mAimingAngle, ref Vector3 mFirePointDir)
    {
        Collider[] hitColliders = Physics.OverlapSphere(myTransform.position, mAimingDistance);

        Transform targetEnemy = null;
        if (hitColliders.Length > 0)
        {
            //寻找夹角范围内距离最近的敌人
            for (int i = 0; i < hitColliders.Length; ++i)
            {
                if (hitColliders[i].tag != "Enemy") continue;

                Vector3 ePos = hitColliders[i].GetComponent<Transform>().position;
                if (Mathf.Abs(Vector3.Angle(ePos - myTransform.position, Weapons[myWeaponId].FirePoint.forward)) < mAimingAngle)
                {
                    if (targetEnemy == null)
                    {
                        targetEnemy = hitColliders[i].GetComponent<Transform>();
                    }
                    else if ((targetEnemy.position - myTransform.position).sqrMagnitude
                        > (ePos - myTransform.position).sqrMagnitude)
                    {
                        targetEnemy = hitColliders[i].GetComponent<Transform>();
                    }
                }
            }

            // 如果瞄准范围内有敌人，改变FirePoint朝向
            // 否则保持原来的朝向，这样才能发射子弹到瞄准范围外的东西
            if (targetEnemy != null)
            {
                mFirePointDir = targetEnemy.position - myTransform.position;
                mFirePointDir.y = 0;
            }
        }
    }
    
    // 开枪 射出一发子弹
    private void Fire(Vector3 mAmingDir)
    {
        RaycastHit hit;
        if(Physics.Raycast(Weapons[myWeaponId].FirePoint.position, mAmingDir, out hit))
        {
            EnemyController enemyController = hit.collider.GetComponent<EnemyController>();
            if(enemyController != null)
            {
                enemyController.GetDamage(Weapons[myWeaponId].damage, Weapons[myWeaponId].recoilForce*6f);
            }
            //击中物体后产生的火花
            GameObject spark = Instantiate(Weapons[myWeaponId].Spark, hit.point, Quaternion.identity);
            Destroy(spark, 10f);
        }
        //弹道轨迹
        GameObject mBullet = Instantiate(Weapons[myWeaponId].Bullet, Weapons[myWeaponId].FirePoint.position, Quaternion.LookRotation(mAmingDir, Vector3.up)) as GameObject;
        //枪口的闪光粒子特效
        Weapons[myWeaponId].GunFire.Emit(1); 
        //后座力
        myRigidbody.AddForce(myTransform.forward * (-1f) * Weapons[myWeaponId].recoilForce, ForceMode.VelocityChange);
        //子弹
        Weapons[myWeaponId].ammo--;
    }

    //切换武器 一定会换到有子弹的武器（空手状态下子弹无限多）
    private void SwapWeapon(bool isNextWeapon)
    {
        Weapons[myWeaponId].gameObject.SetActive(false);
        do
        {
            if (isNextWeapon) ++myWeaponId;
            else  --myWeaponId;

            myWeaponId = myWeaponId % Weapons.Length;
            if (myWeaponId == -1) myWeaponId = Weapons.Length - 1;
        } while (Weapons[myWeaponId].ammo <= 0);
        Weapons[myWeaponId].gameObject.SetActive(true);
        //空手状态下子弹无限多 所以不会死循环
    }
    //体温控制
    private void TemperatureControl()
    {
        float distanceFromHeat = Vector3.Distance(myTransform.position, HeatPoint.position);
        float coldRate = 0f;
        if(distanceFromHeat > getColdDistanceStart)
        {
            if(distanceFromHeat < getColdDistanceEnd)
            {
                coldRate = (distanceFromHeat - getColdDistanceStart)/(10f-getColdDistanceStart);
            }
            else if(distanceFromHeat > getColdDistanceEnd)
            {
                coldRate = 1f;
            }
            int coldDamage = (int)(coldRate*gettingColdDamagePerSec);
            GetDamageContinuously(coldDamage);
        }
        HealthFill.color = Color.Lerp(WarmBloodColor, ColdBloodColor, coldRate);
    }

    public void PickUpWeapon(Weapon.WeaponType weaponType, int ammo)
    {
        foreach(Weapon w in Weapons)
        {
            if(w.weaponType == weaponType)
            {
                w.ammo += ammo;
                break;
            }
        }
        UpdateWeaponUI();
    }

    public void GetDamage(int damage)
    {
        if (myPlayerState.isDead) return;

        myPlayerState.hp -= damage;
        if(myPlayerState.hp <= 0)
        {
            myPlayerState.hp = 0;
            Die();
        }
        Slider_Health.value = (float)myPlayerState.hp/100f;
    }

    private float getDamageContinuouslyTimer = 0f;
    public void GetDamageContinuously(int damagePerSec)
    {
        getDamageContinuouslyTimer += Time.deltaTime;
        if(getDamageContinuouslyTimer > 1f)
        {
            getDamageContinuouslyTimer -= 1f;
            GetDamage(damagePerSec);
        }
    }
    public void Die()
    {
        myPlayerState.isDead = true;
        //死亡过程硬编码
        myRigidbody.constraints = RigidbodyConstraints.None; //取消限制 自由倒地
        myRigidbody.AddForce(Vector3.up * (-1f) + Vector3.right * (-2f), ForceMode.Impulse); //推一把
        this.enabled = false;
        Instantiate(ParticleSystem_Death, myTransform.position+Vector3.up*0.2f, ParticleSystem_Death.transform.rotation); // 例子效果 黑暗之门

        LevelManager.singleton.PlayerDie();
    }

    private void UpdateWeaponUI(){
        Text_Pistol_Ammo.text = (Weapons[1].ammo).ToString();
        Text_Rifle_Ammo.text = (Weapons[2].ammo).ToString();
    }
}
