using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using System.Collections;
using TMPro;

public class BossController : Enemy
{
    [Header("Boss Timer Settings")]
    public float timeLimit = 60f;
    public float currentTimer;
    public bool timerActive = false;

    [Header("Boss UI")]
    public Image timerFillBar;
    public TextMeshProUGUI bossTimer;
    public GameObject bossHealthBarUI;
    public Animator bossHealthBarAnimationUi;

    [Header("Boss FeedBack(JUUIICE)")]
    public MMF_Player bossSpawnFeedBack;
    public MMF_Player bossTimeOutFeedBack;

    [Header("Cinematic Intro Timings")]
    public float animationDuration = 1.5f;
    private bool introIsFinished = false;

    [Header("Spawner Related Stuff")]
    [SerializeField] private Collider2D spawnersCollider;
    public void Awake()
    {
       // SetTheStatsUpIfNotSpawned(GameManager.instance.currentStageIndex);
        //BossSetActiveTrigger.PlayBossThemeNow();

    }
    protected override void Start()
    {       // Initialize(GameManager.instance.currentStageIndex);
        SetTheStatsUpIfNotSpawned(GameManager.instance.currentStageIndex);
        // CALL THE PARENT START! This sets up your materials for the White Flash
        NormalEnmiesSpanwer.bossIsSpawned = true;
        base.Start();

        GameManager.isBossDead = false;
        timerActive = false;

        if (bossHealthBarUI != null) bossHealthBarUI.SetActive(false);

        // Optional: Initialize stats based on current stage
    }

    public void SetTheStatsUpIfNotSpawned(int Stage)
    {
        double scaleFactor = System.Math.Pow(2.1d, Stage);
        currentHealth = enemyData.baseHealth * scaleFactor;
        currentDamage = enemyData.baseDamage * scaleFactor;
        Vector3 baseSize = transform.localScale;
    }
    public void WakeUpBossAndStartIntro()
    {
        // Don't initialize twice, just start the sequence
        spawnersCollider.enabled = false;
        StartCoroutine(BossIntroSequence());
    }
    protected override void SpawnFloatingText(double damage, bool isCrit)
    {
        if (damageTextPrefab == null) return;

        // X between 0 and -3 (Left side), Y between 0.5 and 3 (Above)
        float randomX = Random.Range(-6f, -14f);
        float randomY = Random.Range(0.2f, 2.6f);
        Vector3 randomOffset = new Vector3(randomX, randomY, 0f);

        Vector3 spawnPosition = posOfText.position + randomOffset;

        // Instantiate the text
        DamageText textInstance = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);

        // Pass your values from PlayerStats into our new script
        textInstance.SetupText(damage, isCrit);
    }
    private IEnumerator BossIntroSequence()
    {
        // PHASE 1: FEEDBACK
        if (bossSpawnFeedBack != null) bossSpawnFeedBack.PlayFeedbacks();

        // PHASE 2: ANIMATION
        anim.SetBool("JustSpawned", true);
        if (bossHealthBarAnimationUi != null) bossHealthBarAnimationUi.SetBool("JustSpawned", true);

        yield return new WaitForSeconds(animationDuration);

        anim.SetBool("JustSpawned", false);
        anim.SetBool("IsIdle", true);

        // PHASE 3: THE FIGHT BEGINS
        if (bossHealthBarUI != null) bossHealthBarUI.SetActive(true);

        if (bossHealthBarAnimationUi != null)
        {
            bossHealthBarAnimationUi.SetBool("JustSpawned", false);
            //bossHealthBarAnimationUi.enabled = false;
        }

        currentTimer = timeLimit;
        timerActive = true;
        introIsFinished = true;

        // Start the music when the fight actually begins
   
    }

    private void Update()
    {
        // We handle the timer here. Health and Hits are handled by Enemy.cs automatically!
        if (!timerActive || isDead || !introIsFinished) return;

        currentTimer -= Time.deltaTime;

        // UI Updates
        if (timerFillBar != null)
        {
            timerFillBar.fillAmount = currentTimer / timeLimit;
        }

        if (bossTimer != null)
        {
            bossTimer.text = Mathf.Ceil(currentTimer).ToString();
        }

        if (currentTimer <= 0)
        {
            BossFailed();
        }
    }

    protected override void Die()
    {
        timerActive = false;
        GameManager.instance.BossDefeated();
        // This calls the Parent Die() which handles loot, death animation, and destroying the object
        base.Die();
    }

    private void BossFailed()
    {
        if (isDead) return; // Prevents double-triggering

        timerActive = false;
        isDead = true;

        if (bossTimeOutFeedBack != null)
        {
            bossTimeOutFeedBack.PlayFeedbacks();
        }

        GameManager.instance.BossTimeOut();
        Destroy(gameObject, 0.5f);
    }
}