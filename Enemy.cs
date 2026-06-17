using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
/// <summary>
/// Base enemy controller: handles combat, scaling, loot drops, death sequence, and hit feedback.
/// Uses a weighted loot system to decide which currency types drop on death.
/// </summary>
public class Enemy : MonoBehaviour
{
    #region Inspector Fields


    [Header("Stats")]
    [SerializeField] protected EnemyData enemyData;


    [Header("References")]
    [SerializeField] protected EnemyHealthBar enemyHealthBarScript;
    [SerializeField] protected SpriteRenderer enemySprite;
    [SerializeField] protected Rigidbody2D enemyRigidBody;
    [SerializeField] protected Animator anim;
    [SerializeField] protected MMF_Player hitFeedback;
    [SerializeField] protected MMF_Player deathFeedback;
    [Header("Drops")]
    [SerializeField] protected List<CurrencyType> currencyTypes = new List<CurrencyType>();


    [Header("Loot Weights")]
    [SerializeField] protected float healthWeight;
    [SerializeField] protected float damageWeight;


    [Header("Bonus Loot Settings")]
    [Tooltip("Minimum amount of bonus items to drop")]
    [SerializeField] protected int minBonusDrops = 1;
    [Tooltip("Maximum amount of bonus items to drop")]
    [SerializeField] protected int maxBonusDrops = 3;


    [Header("Hit Flash")]
    [SerializeField] protected Material whiteMat;
    [SerializeField] protected float enemyFlash = 0.05f;


    [Header("Floating Text")]
    [SerializeField] protected Transform posOfText;
    [SerializeField] protected DamageText damageTextPrefab;

    #endregion

    #region Runtime State


    public double currentHealth;
    public double currentDamage;
    protected bool isDead = false;
    protected float cachedPowerScore;


    protected Material defaultMat;
    protected float lastHitTime = 0f;
    protected float hitCooldown = 0.1f;


    #endregion

    #region Unity Lifecycle

    protected virtual void Start()
    {
        currentHealth = enemyData.baseHealth;
        defaultMat = enemySprite.material;
        if (enemyRigidBody != null)
        {
            enemyRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    #endregion

    #region Initialization 
    public void Initialize(int stageIndex)
    {
        double scaleFactor = stageIndex > 0? System.Math.Pow(1.3d, stageIndex): 1d;
        currentHealth = enemyData.baseHealth * scaleFactor;
        currentDamage = enemyData.baseDamage * scaleFactor;
        cachedPowerScore = CalculatePowerScore();
    }
    #endregion

    #region Combat 
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (isDead) return;
        if (collider.CompareTag("Player"))
        {
            PlayerController playerController = collider.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.TriggerAttackAnimation();
            }
            if (Time.time < lastHitTime + hitCooldown) return;
            lastHitTime = Time.time;

            AudioPlayer.Instance.DamagingAnEnemy();
            PlayerStats.Instance.CriticalDamage();
            currentHealth = currentHealth - PlayerStats.Instance.finalDamage;
            SpawnFloatingText(PlayerStats.Instance.finalDamage, PlayerStats.Instance.thatDamageWasCrit);
            if (enemyHealthBarScript != null)
            {
                enemyHealthBarScript.EnemyIsDamged();
            }
            if (hitFeedback != null)
            {
                hitFeedback.PlayFeedbacks();
            }

            if (currentHealth <= 0)
            {
                isDead = true;

                Collider2D[] allColliders = GetComponentsInChildren<Collider2D>();
                foreach (Collider2D col in allColliders)
                {
                    col.enabled = false;
                }

                Die();
            }
            else
            {
                if (playerController != null)
                {
                    anim.SetBool("IsIdle", false);
                    anim.SetBool("IsAttacking", true);

                }
                PlayerStats.Instance.GettingDamaged(currentDamage, transform);
                Invoke(nameof(ResetAttackAnimation), 0.25f);

            }
        }
    }
    protected void ResetAttackAnimation()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsIdle", true); 
    }
    #endregion

    #region Death & Loot 
    protected virtual void Die()
    {

        NormalEnmiesSpanwer spawner = FindObjectOfType<NormalEnmiesSpanwer>();
        if (spawner != null)
        {
            spawner.EnemyDefeated();
        }

        if(deathFeedback != null) deathFeedback.PlayFeedbacks();
        SpawnCurrencyDrop();
        // The "+ 1" on the max. This is a Unity trick.
        int randomAmountOfBonusLoot = Random.Range(minBonusDrops, maxBonusDrops + 1);
        SpawnBonusLoot(randomAmountOfBonusLoot);



        if (enemyHealthBarScript != null)
        {
            enemyHealthBarScript.EnemyIsDamged();
        }
        
        StopAllCoroutines();
        StartCoroutine(EnemyIsDead());
    }

    protected void SpawnCurrencyDrop()
    {
        int enemyReward = (int)enemyData.currencyReward;

        foreach (CurrencyType coin in currencyTypes)
        {
            if (coin.currencyValue <= 0) continue;
            int amountToSpawn = enemyReward / coin.currencyValue;
            enemyReward = enemyReward % coin.currencyValue;
            for (int i = 0; i < amountToSpawn; i++)
            {
                Vector3 randomOffset = new Vector3(Random.Range(6f, 14f), Random.Range(-0.5f, 2f), 0);
                Instantiate(coin.prefabForCurrencys, transform.position + randomOffset, Quaternion.identity);

            }
            if (enemyReward <= 0) break;
        }
    }

    protected float CalculatePowerScore()
    {

        float powerScore = (float)(currentHealth * healthWeight) +
                           (float)(currentDamage * damageWeight) 
                          ;
        return powerScore;
    }

    protected void SpawnBonusLoot(int numberOfRolls)
    {

        float totalWeight = 0f;

        List<float> modifiedWeights = new List<float>();


        // 1. THE DYNAMIC MODIFIER (We only need to calculate the math once)
        foreach (CurrencyType item in currencyTypes)
        {
            float currentItemWeight = item.baseWeight;

            if (item.isRareloot)
            {
                currentItemWeight += (cachedPowerScore * 0.01f);
            }
            else
            {
                currentItemWeight -= (cachedPowerScore * 0.01f);
                if (currentItemWeight < 1f) currentItemWeight = 1f;
            }


            modifiedWeights.Add(currentItemWeight);
            totalWeight += currentItemWeight;
        }
        // 2. THE MULTIPLE DICE ROLLS
        for (int roll = 0; roll < numberOfRolls; roll++)
        {
            float randomRoll = Random.Range(0f, totalWeight);

            for (int i = 0; i < currencyTypes.Count; i++)
            {
                randomRoll -= modifiedWeights[i];

                if (randomRoll <= 0f)
                {
                    Vector3 randomOffset = new Vector3(Random.Range(5f, 10f), Random.Range(1f, 3f), 0);
                    Instantiate(currencyTypes[i].prefabForCurrencys, transform.position + randomOffset, Quaternion.identity);
                    break;
                }
            }
        }
    }

    protected IEnumerator EnemyIsDead()
    {
        
        anim.SetBool("IsDead", true);
        PlayFlash();
        yield return new WaitForSeconds(0.65f);
        DestroySelf();
    }

    protected void DestroySelf()
    {
        Destroy(gameObject);
    }
    #endregion

    #region Visual Feedback
    protected IEnumerator FlashRoutine()
    {
        enemySprite.material = whiteMat;
        yield return new WaitForSeconds(enemyFlash);
        enemySprite.material = defaultMat;
    }
    protected void PlayFlash()
    {
        StartCoroutine(FlashRoutine());
    }
    protected virtual void SpawnFloatingText(double damage, bool isCrit)
    {
        if (damageTextPrefab == null) return;

        float randomX = Random.Range(0f, -8f);
        float randomY = Random.Range(0.2f, 1.6f);
        Vector3 randomOffset = new Vector3(randomX, randomY, 0f);

        Vector3 spawnPosition = posOfText.position + randomOffset;

        DamageText textInstance = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);

        textInstance.SetupText(damage, isCrit);

    }
    #endregion
    #region Inner Classes
    [System.Serializable]
    public class CurrencyType
    {
        public string currencyTypeName;
        public int currencyValue;
        public float baseWeight;
        public bool isRareloot;
        public GameObject prefabForCurrencys;

    }
    #endregion
}

