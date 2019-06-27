using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    #region GameObject
    public GameObject Enemy;
    public GameObject Prop;
    public Transform[] EnemySpawnPoints;
    public Transform[] PropSpawnPoints;
    #endregion
    #region UI GameObj
    public TMPro.TextMeshProUGUI Text_PlayerName;
    public Text Text_Kill;
    public Text Text_Time;
    public Text Text_Time_Over;
    public Text Text_Kill_Over;
    public GameObject MenuStart;
    public GameObject MenuPause;
    public GameObject MenuOver;
    public GameObject GameUI;
    #endregion

    public static GameManager singleton;
    public int killNumber = 0;

    [SerializeField]private GameRoundInfo[] gameRoundInfos = null;
    [System.Serializable]
    private class GameRoundInfo
    {
        public float roundTime = 1f;
        public float enemySpawnTime = 1f;
        public float propsSpawnTime = 1f;
    }
    void Awake(){
        singleton = this;
        //直接从Game.unity场景启动测试时调用
        if(PlayerSettingsManager.playerInfo == null){
            PlayerSettingsManager.InitPlayerSettings();
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
        MenuStart.SetActive(true);
        Text_PlayerName.text = PlayerSettingsManager.playerInfo.name;
        Time.timeScale = 0f;
        while(!Input.anyKey)
        {
            yield return null;
        }
        MenuStart.SetActive(false);
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
        spawnPropsCoro  = StartCoroutine(SpawnProps(info.propsSpawnTime, 2f));
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

        GameObject enmey = Instantiate(Enemy, 
            EnemySpawnPoints[index].position + Vector3.up * 35f,
            rot);

        spawnEnemyCoro = StartCoroutine(SpawnEnemy(waitingTimeBase, randomTime));
    }
    IEnumerator SpawnProps(float waitingTimeBase, float randomTime)
    {
        yield return new WaitForSeconds(waitingTimeBase + Random.Range(-1f*randomTime, randomTime));

        int index = Random.Range(0, PropSpawnPoints.Length);
        GameObject prop = Instantiate(Prop, PropSpawnPoints[index].position, Quaternion.identity);
        Props p = prop.GetComponent<Props>();
        p.propsType = Props.PropsType.Weapon;

        p.ammo = Random.Range(5, 30); //子弹数量 硬编码
        if (Random.Range(1, 3) == 1) { p.weaponType = Weapon.WeaponType.Pistol; }
        else { p.weaponType = Weapon.WeaponType.Rifle; }

        spawnPropsCoro = StartCoroutine(SpawnProps(waitingTimeBase, randomTime));
    }
    public void GameOver()
    {
        Text_Time_Over.text = string.Format("{0:D2}:{1:D2}", (totalTimeSeconds / 60), (totalTimeSeconds % 60));
        Text_Kill_Over.text = killNumber.ToString();
        MenuOver.SetActive(true);
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
            Text_Time.text = string.Format("{0:D2}:{1:D2}", (totalTimeSeconds / 60), (totalTimeSeconds % 60)); 
        }
    }
    public void KillEnemy()
    {
        ++killNumber;
        Text_Kill.text = killNumber.ToString();
    }

    #region UI Methods
    public void Restart()
    {
        SceneManager.LoadScene("Game", LoadSceneMode.Single);
    }
    public void SetGameState(bool isStopped)
    {
        Time.timeScale = isStopped ? 0f : 1f;
        isGameStopped = isStopped;
        MenuPause.SetActive(isStopped);
    }
    public void MenuQuitGame()
    {
        Application.Quit();
    }
    #endregion
}