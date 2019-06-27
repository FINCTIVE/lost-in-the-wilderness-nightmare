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
    public AudioSource audio_shoot;
    public AudioSource audio_pickup;
    public GameObject ParticleSystem_Death;


    public Weapon[] Weapons;
    private int myWeaponId = 0; // 空手
    public float aimingDistance;
    public float aimingAngle;
    
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
    private Vector3 dirXZ = Vector3.zero; //单位向量
    private Vector3 rawDirXZ = Vector3.zero; //未经处理的输入向量
    private bool fire = false;
    private bool nextWeapon = false;
    private bool lastWeapon = false;


    private void Update()
    {
         //INPUT
        dirXZ.x = Input.GetAxis("Horizontal");
        dirXZ.z = Input.GetAxis("Vertical");
        rawDirXZ.x = Input.GetAxisRaw("Horizontal");
        rawDirXZ.z = Input.GetAxisRaw("Vertical");
        if (dirXZ.sqrMagnitude > 1f) {
            dirXZ = Vector3.Normalize(dirXZ);//限制向量的模长度小于等于1
        }
        if(Input.GetButtonDown("Fire"))
        {
            fire = true;
        }

        if (Input.GetButtonDown("LastWeapon"))
        {
            lastWeapon = true;
        }
        else if (Input.GetButtonDown("NextWeapon"))
        {
            nextWeapon = true;
        }
    }

    void FixedUpdate()
    {
        #region 移动 转身
        myRigidbody.MovePosition(myTransform.position + dirXZ * myPlayerState.moveSpeed * Time.deltaTime);

        if (rawDirXZ.magnitude > float.Epsilon && dirXZ.magnitude > float.Epsilon)
        {
            Quaternion rot = Quaternion.LookRotation(dirXZ, Vector3.up);
            myRigidbody.MoveRotation(Quaternion.Lerp(
                myRigidbody.rotation, rot, Time.deltaTime * 20f)); //平滑变量硬编码
        }
        #endregion

        if (fire && Weapons[myWeaponId].weaponType != Weapon.WeaponType.EmptyHands)
        {
            #region 辅助瞄准
            Collider[] hitColliders = Physics.OverlapSphere(myTransform.position,
                aimingDistance);
            Quaternion fireRot = Weapons[myWeaponId].FirePoint.rotation;
            Vector3 aimingDir = Weapons[myWeaponId].FirePoint.forward;
            aimingDir.y = 0;

            if (hitColliders.Length > 0)
            {
                //寻找夹角范围内距离最近的敌人
                Transform targetEnemy = null;
                for (int i = 0; i < hitColliders.Length; ++i)
                {
                    if (hitColliders[i].tag != "Enemy") continue;
                    Vector3 ePos = hitColliders[i].GetComponent<Transform>().position;
                    if (Mathf.Abs(Vector3.Angle(ePos - myTransform.position, Weapons[myWeaponId].FirePoint.forward))
                        < aimingAngle)
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
                if (targetEnemy != null)
                {
                    aimingDir = targetEnemy.position - myTransform.position;
                    aimingDir.y = 0;
                    fireRot = Quaternion.LookRotation(aimingDir, Weapons[myWeaponId].FirePoint.up);

                    //敌人受到打击
                    EnemyController enemyController = targetEnemy.GetComponent<EnemyController>();
                    if (enemyController != null)
                    {
                        enemyController.GetDamage(Weapons[myWeaponId].damage);

                    }
                }
            }
            #endregion
            
            #region 开火
            audio_shoot.Play();
            RaycastHit hit;
            if(Physics.Raycast(Weapons[myWeaponId].FirePoint.position,
                aimingDir, out hit))
            {
                //击中物体后产生的火花
                GameObject spark = Instantiate(Weapons[myWeaponId].Spark, 
                    hit.point, Quaternion.identity);
                Destroy(spark, 10f);
            }
            //弹道轨迹
            GameObject mBullet = Instantiate(Weapons[myWeaponId].Bullet,
                Weapons[myWeaponId].FirePoint.position, 
                fireRot) as GameObject;
            //枪口的闪光粒子特效
            Weapons[myWeaponId].GunFire.Emit(1); 
            //后座力
            myRigidbody.AddForce(myTransform.forward * (-1f) * Weapons[myWeaponId].recoilForce, ForceMode.VelocityChange);
            //子弹
            Weapons[myWeaponId].ammo--;
            if(Weapons[myWeaponId].ammo == 0)
            {
                nextWeapon = true;
            }
            UpdateWeaponUI();
            #endregion
        }
        if (fire) fire = false;

        #region 武器切换
        if (nextWeapon || lastWeapon)
        {
            Weapons[myWeaponId].gameObject.SetActive(false);
            do
            {
                if (nextWeapon) ++myWeaponId;
                else if (lastWeapon) --myWeaponId;

                myWeaponId = myWeaponId % Weapons.Length;
                if (myWeaponId == -1) myWeaponId = Weapons.Length - 1;
            } while (Weapons[myWeaponId].ammo <= 0);
            Weapons[myWeaponId].gameObject.SetActive(true);
            //空手状态 子弹无限大
            nextWeapon = false;
            lastWeapon = false;
        }
        #endregion


        #region 控制动画
        myAnimator.SetFloat("move", dirXZ.sqrMagnitude);

        float targetAnimValue_hand = (float)Weapons[myWeaponId].weaponType/((float)Weapons.Length-1f);
        myAnimator.SetFloat("hand", Mathf.Lerp(myAnimator.GetFloat("hand"), targetAnimValue_hand, 20f*Time.deltaTime)); //使用Blend Tree平滑武器切换 平滑变量硬编码
        #endregion
    }

    private void UpdateWeaponUI(){
        Text_Pistol_Ammo.text = (Weapons[1].ammo).ToString();
        Text_Rifle_Ammo.text = (Weapons[2].ammo).ToString();
    }
    public void PickUpWeapon(Weapon.WeaponType weaponType, int ammo)
    {
        foreach(Weapon w in Weapons)
        {
            if(w.weaponType == weaponType)
            {
                audio_pickup.Play();
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

    public void Die()
    {
        myPlayerState.isDead = true;
        //死亡过程硬编码
        myRigidbody.constraints = RigidbodyConstraints.None; //取消限制 自由倒地
        myRigidbody.AddForce(Vector3.up * (-1f) + Vector3.right * (-2f), ForceMode.Impulse); //推一把
        this.enabled = false;
        Instantiate(ParticleSystem_Death, myTransform.position+Vector3.up*0.2f, ParticleSystem_Death.transform.rotation);

        GameManager.singleton.PlayerDie();
    }
}
