using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class LevelManager : MonoBehaviour
{
    #region GameObject
    public GameObject EnemyPrefab;
    public GameObject PropPrefab;
    public Transform[] EnemySpawnPoints;
    public Transform[] PropSpawnPoints;
    #endregion
    #region UI GameObj
    public Text Text_Time_Over;
    public Text Text_PlayerName;
    public GameObject StartHintMenu;
    public GameObject PauseMenu;
    public GameObject GameOverMenu;
    public GameObject GameUI;
    #endregion

    public static LevelManager singleton;
    public int killNumber = 0;

    [SerializeField]private GameRoundInfo[] gameRoundInfos = null;
    [System.Serializable]
    private class GameRoundInfo
    {
        public float roundTime = 1f;
        public float enemySpawnTime = 1f;
        public float propsSpawnTime = 1f;
        public int propsAmmoPistal = 10;
        public int propsAmmoRifle = 10;
    }
    void Awake(){
        singleton = this;
        //直接从Game.unity场景启动测试时调用
        if(PlayerDataManager.singleton == null){
            PlayerDataManager.InitPlayerSettings();
        }
    }
    private Coroutine gameLoopCoro;
    private Coroutine gameRoundCoro;
    IEnumerator Start()
    {
        yield return StartCoroutine(StartGame());
        gameLoopCoro = StartCoroutine(GameLoop());
    }
    IEnumerator StartGame(){
        StartHintMenu.SetActive(true);
        Text_PlayerName.text = PlayerDataManager.singleton.name;
        Time.timeScale = 0f;
        while(!Input.anyKey)
        {
            yield return null;
        }
        StartHintMenu.SetActive(false);
        GameUI.SetActive(true);
        Time.timeScale = 1f;
    }
    IEnumerator GameLoop(){
        int gameRoundIndex = 0;
        while(gameRoundIndex < gameRoundInfos.Length)
        {
            gameRoundCoro = StartCoroutine(GameRound(gameRoundInfos[gameRoundIndex]));
            yield return gameRoundCoro;             
            ++gameRoundIndex;
        }
    }
    private Coroutine spawnEnemyCoro;
    private Coroutine spawnPropsCoro;
    IEnumerator GameRound(GameRoundInfo info)
    {
        spawnEnemyCoro  = StartCoroutine(SpawnEnemy(info.enemySpawnTime, 2f));
        spawnPropsCoro  = StartCoroutine(SpawnProps(info.propsSpawnTime, 2f, info.propsAmmoPistal, info.propsAmmoRifle));
        yield return new WaitForSeconds(info.roundTime);
        StopCoroutine(spawnEnemyCoro);
        StopCoroutine(spawnPropsCoro);
    }
    IEnumerator SpawnEnemy(float waitingTimeBase, float randomTime)
    {
        yield return new WaitForSeconds(waitingTimeBase + Random.Range(-1f*randomTime, randomTime));

        int index = Random.Range(0, EnemySpawnPoints.Length);
        Quaternion rot = Quaternion.LookRotation(PlayerController.playerTransform.position - EnemySpawnPoints[index].position, Vector3.up);
        Vector3 euler = rot.eulerAngles;
        rot.eulerAngles = new Vector3(0f, euler.y, 0f);

        GameObject enmey = Instantiate(EnemyPrefab,EnemySpawnPoints[index].position + Vector3.up * 5f, rot);
        enmey.GetComponent<Rigidbody>().AddForce(Vector3.up *(-30f), ForceMode.VelocityChange);
        // GameObject enmey = Instantiate(Enemy,EnemySpawnPoints[index].position + Vector3.up, rot);

        spawnEnemyCoro = StartCoroutine(SpawnEnemy(waitingTimeBase, randomTime));
    }
    IEnumerator SpawnProps(float waitingTimeBase, float randomTime, int propsAmmoPistal, int propsAmmoRifle)
    {
        yield return new WaitForSeconds(waitingTimeBase + Random.Range(-1f*randomTime, randomTime));

        int index = Random.Range(0, PropSpawnPoints.Length);
        GameObject prop = Instantiate(PropPrefab, PropSpawnPoints[index].position, Quaternion.identity);
        Props p = prop.GetComponent<Props>();
        p.propsType = Props.PropsType.Weapon;

        if (Random.Range(1, 3) == 1) 
        {
            p.weaponType = Weapon.WeaponType.Pistol; 
            p.ammo = propsAmmoPistal;
        }
        else 
        {
            p.weaponType = Weapon.WeaponType.Rifle; 
            p.ammo = propsAmmoRifle;
        }

        spawnPropsCoro = StartCoroutine(SpawnProps(waitingTimeBase, randomTime, propsAmmoPistal, propsAmmoRifle));
    }
    public void GameOver()
    {
        Text_Time_Over.text = string.Format("{0:D2}:{1:D2}", (totalTimeSeconds / 60), (totalTimeSeconds % 60));
        GameOverMenu.SetActive(true);
        Time.timeScale = 0f;
    }
    public void PlayerDie()
    {
        Invoke("GameOver", 5f);
        isGameOver = true;
    }
    public bool isGameStopped=false, isGameOver=false;
    private int totalTimeSeconds = 0;
    private float timerRemainSecond = 0f;
    void Update()
    {
        if(isGameOver) return;

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SetGameState(!isGameStopped);
        }
        
        timerRemainSecond += Time.deltaTime;
        if (timerRemainSecond > 1f)
        {
            totalTimeSeconds += 1;
            timerRemainSecond -= 1f;
        }
    }
    public void KillEnemy()
    {
        ++killNumber;
    }

    #region UI调用的方法
    public void Restart()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
    public void SetGameState(bool isStopped)
    {
        Time.timeScale = isStopped ? 0f : 1f;
        isGameStopped = isStopped;
        PauseMenu.SetActive(isStopped);
    }
    public void MenuQuitGame()
    {
        Application.Quit();
    }
    #endregion
}