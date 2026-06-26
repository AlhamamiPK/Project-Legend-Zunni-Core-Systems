using UnityEngine;
using UnityEngine.UI;
using MoreMountains.Feedbacks;
using System.Collections;
using TMPro;
/// <summary>
/// Boss enemy controller extending Enemy. Handles cinematic intro, fight timer, UI updates, and win/timeout callbacks.
/// </summary>
public class BossController : Enemy
{
    #region Inspector Fields

    [Header("Boss Timer Settings")]
    [SerializeField] private float timeLimit = 60f;

    [Header("Boss UI")]
    [SerializeField] private Image timerFillBar;
    [SerializeField] private TextMeshProUGUI bossTimer;
    [SerializeField] private GameObject bossHealthBarUI;
    [SerializeField] private Animator bossHealthBarUIAnimator;


    [Header("Boss Feedbacks")]
    [SerializeField] private MMF_Player bossSpawnFeedBack;
    [SerializeField] private MMF_Player bossTimeOutFeedBack;


    [Header("Cinematic Intro Timings")]
    [SerializeField] private float animationDuration = 1.5f;

    [Header("Spawner References")]
    [SerializeField] private Collider2D spawnersCollider;


    #endregion

    #region Runtime State

    public bool timerActive = false;
    private float currentTimer;
    private bool introIsFinished = false;

    #endregion



    #region Unity Lifecycle
    protected override void Start()
    {    
        NormalEnmiesSpanwer.bossIsSpawned = true;
        base.Start();
        InitializeBossStats(GameManager.instance.currentStageIndex);

        GameManager.isBossDead = false;
        timerActive = false;

        if (bossHealthBarUI != null) bossHealthBarUI.SetActive(false);

    }

    private void Update()
    {
        if (!timerActive || isDead || !introIsFinished) return;

        currentTimer -= Time.deltaTime;

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

    #endregion
    #region Boss Intro
    public void WakeUpBossAndStartIntro()
    {
        if(spawnersCollider != null) spawnersCollider.enabled= false;
        StartCoroutine(BossIntroSequence());
    }

    private IEnumerator BossIntroSequence()
    {
        // PHASE 1: FEEDBACK
        if (bossSpawnFeedBack != null) bossSpawnFeedBack.PlayFeedbacks();

        // PHASE 2: ANIMATION
        anim.SetBool("JustSpawned", true);
        if (bossHealthBarUIAnimator != null) bossHealthBarUIAnimator.SetBool("JustSpawned", true);

        yield return new WaitForSeconds(animationDuration);

        anim.SetBool("JustSpawned", false);
        anim.SetBool("IsIdle", true);

        // PHASE 3: THE FIGHT BEGINS
        if (bossHealthBarUI != null) bossHealthBarUI.SetActive(true);

        if (bossHealthBarUIAnimator != null)
        {
            bossHealthBarUIAnimator.SetBool("JustSpawned", false);
        }

        currentTimer = timeLimit;
        timerActive = true;
        introIsFinished = true;


    }
    #endregion

    #region Boss Setup
    private void InitializeBossStats(int stageIndex)
    {
        double scaleFactor = System.Math.Pow(2.0d, stageIndex);
        currentHealth = enemyData.baseHealth * scaleFactor;
        currentDamage = enemyData.baseDamage * scaleFactor;
    }



    #endregion

    #region Visual Feedback

    protected override void SpawnFloatingText(double damage, bool isCrit)
    {
        if (damageTextPrefab == null) return;

        float randomX = Random.Range(-6f, -14f);
        float randomY = Random.Range(0.2f, 2.6f);
        Vector3 randomOffset = new Vector3(randomX, randomY, 0f);

        Vector3 spawnPosition = posOfText.position + randomOffset;

        DamageText textInstance = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);

        textInstance.SetupText(damage, isCrit);
    }

    #endregion

    #region Death & Failure
    protected override void Die()
    {
        timerActive = false;
        GameManager.instance.BossDefeated();
        base.Die();
    }

    private void BossFailed()
    {
        if (isDead) return; 

        timerActive = false;
        isDead = true;

        if (bossTimeOutFeedBack != null)
        {
            bossTimeOutFeedBack.PlayFeedbacks();
        }

        GameManager.instance.BossTimeOut();
        Destroy(gameObject, 0.5f);
    }
    #endregion


}