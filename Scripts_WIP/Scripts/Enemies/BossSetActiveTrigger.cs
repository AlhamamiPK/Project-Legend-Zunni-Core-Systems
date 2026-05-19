using UnityEngine;

public class BossSetActiveTrigger : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Drag your Boss PREFAB here from the project folder")]
    [SerializeField] private GameObject bossPrefab;

    [Tooltip("Create an Empty GameObject where you want the boss to spawn, and drag it here")]
    [SerializeField] private Transform spawnLocation;
    private GameObject spawnedBoss;
    public static bool playMusicOnce = true;

    private void Start()
    {
        
        // 1. Instantiates the Boss Prefab at the exact place you chose
        spawnedBoss = Instantiate(bossPrefab, spawnLocation.position, Quaternion.identity);
        //PlayBossThemeNow();
        // 2. Grabs the script to feed it the "bullshit from the GameManager"
        BossController bossScript = spawnedBoss.GetComponent<BossController>();
        if (bossScript != null)
        {
           
            bossScript.Initialize(GameManager.instance.currentStageIndex);
        }
      //  AudioPlayer.Instance.PlayBossTheme();
        // 3. Immediately turns the boss OFF so it waits for the player
        spawnedBoss.SetActive(false);
    }
    private void Awake()
    {
        //if (playMusicOnce == true)
        //{
        //    AudioPlayer.Instance.PlayBossTheme();
        //    playMusicOnce = false;
        //}
        //else
        AudioPlayer.Instance.PlayBossTheme();


        //AudioPlayer.Instance.PlayBossTheme();
    }
    public static void PlayBossThemeNow()
    {
        AudioPlayer.Instance.PlayBossTheme();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        
        if(collision.CompareTag("Player"))
        {
            bossPrefab.SetActive(true);
            BossController bossScript = FindAnyObjectByType<BossController>();
            if (bossScript != null) { bossScript.WakeUpBossAndStartIntro(); }
            //gameObject.SetActive(false);
        }
        //GetComponent<Collider2D>().enabled = false;
    }
}
