using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using MoreMountains.Feedbacks;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy's Stats")]
    [SerializeField] public double currentHealth;
    [SerializeField] public double currentDamage;
    [SerializeField] EnemyHealthBar enemyHealthBarScript;

    [Header("Enemy's physical Parts")]
    public SpriteRenderer enemySprite;
    public Rigidbody2D enemyRigidBody;
    public ParticleSystem enemyBlood;
    public Transform enemyMoverPart;
    [SerializeField] public Animator anim;
    public MMF_Player hitFeedback;
    public MMF_Player deathFeedBack;
    [Header("Drops")]
    [Header("CurrencyTypes")]
    [SerializeField] private List<currencyType> currencyTypes = new List<currencyType>();

    [Header("lootDropWeightTweeks")]
    [SerializeField] public float HealthWeight;
    [SerializeField] public float DamageWeight;
    [SerializeField] public float ExpWeight;

    [Header("Bonus Loot Settings")]
    [Tooltip("Minimum amount of bonus items to drop")]
    [SerializeField] public int minBonusDrops = 1;
    [Tooltip("Maximum amount of bonus items to drop")]
    [SerializeField] public int maxBonusDrops = 3;

    [Header("Death Knockback Settings")]
    [Tooltip("How fast the enemy flies to the right/left when killed.")]
    [SerializeField] private float deathFlyForceX = 45f;
    [Tooltip("Minimum upward force applied on death.")]
    [SerializeField] private float minDeathFlyForceY = 2f;
    [Tooltip("Maximum upward force applied on death.")]
    [SerializeField] private float maxDeathFlyForceY = 7f;
    [Tooltip("Adds a spinning effect when they die. Set to 0 if you don't want them to spin.")]
    [SerializeField] private float deathSpinTorque = 15f;

    [Header("WhiteFlashThings")]
    public Material whiteMat;
    private Material defultMat;
    [SerializeField] private float enemyFlash = 0.05f;


    public bool isDead = false;
    public float lastHitTime = 0f;
    public float hitCooldown = 0.1f;
    public float cachedPowerScore;
    public EnemyData enemyData;
    [SerializeField] public Transform posOfText;
    [Header("Floating Text")]
    public DamageText damageTextPrefab;
    [System.Serializable]
    public class currencyType
    {
        public string currencyTypeName;
        public int currencyValue;
        public float baseWeight;
        public bool isRareloot;
        public GameObject prefabForCurrencys;

    }
    protected virtual void Start()
    {
        currentHealth = enemyData.baseHealth;
        defultMat = enemySprite.material;
        if (enemyRigidBody != null)
        {
            enemyRigidBody.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    
    public void Initialize(int StageIndex)
    {
        //[Tooltip("Base Stats * (1.1 ^ Level)This means enemies get 10% stronger every level.")]
        double scaleFactor = System.Math.Pow(1.3d, StageIndex);
        if (StageIndex != 0)
        {
            currentHealth = enemyData.baseHealth * scaleFactor;
            currentDamage = enemyData.baseDamage * scaleFactor;
            Vector3 baseSize = transform.localScale;
            //transform.localScale = baseSize * (1f + (StageIndex * 0.05f));
            cachedPowerScore = CalculatePowerScore();
        }else {
            currentHealth = enemyData.baseHealth ;
            currentDamage = enemyData.baseDamage ;
            Vector3 baseSize = transform.localScale;
            //transform.localScale = baseSize * (1f + (StageIndex * 0.05f));
            cachedPowerScore = CalculatePowerScore();
        }
    }
        
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
                // The player hit the trigger but didn't get the kill. Punish them.
                if (playerController != null)
                {
                    // Push them back with heavy force, and lock their inputs for 0.25 seconds
                    anim.SetBool("IsIdle",false);
                    anim.SetBool("IsAttacking", true);
                    //Vector2 knockBackForce = new Vector2(-30f, 0f);
                    //playerController.TakeKnockBack(knockBackForce, 0.35f);
                }
                //PlayerStats.Instance.GettingDamaged(currentDamage,transform);
                PlayerStats.Instance.GettingDamaged(currentDamage, transform);
                Invoke("ResetAttackAnimation", 0.25f);
                //anim.SetBool("IsAttacking",false);
                //anim.SetBool("IsIdle", true);
            }
        }
    }
    private void ResetAttackAnimation()
    {
        anim.SetBool("IsAttacking", false);
        anim.SetBool("IsIdle", true); // Move it here
    }

    [System.Obsolete]
    protected virtual void Die()
    {// Find the spawner in the scene and trigger the minus 1 counter!

        NormalEnmiesSpanwer spawner = FindObjectOfType<NormalEnmiesSpanwer>();
        if (spawner != null)
        {
            spawner.EnemyDefeated();
        }

        deathFeedBack.PlayFeedbacks();
        anim.SetBool("IsDead", true);
        SpawnCurrencyDrop();
        // --- NEW LOOT LOGIC ---
        // We use your new Inspector variables! 
        // Notice the "+ 1" on the max. This is a special Unity trick.
        int randomAmountOfBonusLoot = Random.Range(minBonusDrops, maxBonusDrops + 1);
        SpawnBonusLoot(randomAmountOfBonusLoot);
        enemyBlood.startSize = 1f;
        enemyBlood.Play();

        //float hieghtEnemyDeath = Random.Range(5f, 0f);
        //enemyRigidBody.AddForce(new Vector2(45f, hieghtEnemyDeath), ForceMode2D.Impulse);
        if (enemyHealthBarScript != null)
        {
            enemyHealthBarScript.EnemyIsDamged();
        }
        
        StopAllCoroutines();
        StartCoroutine(EnemyIsDead());
    }

    public void SpawnCurrencyDrop()
    {
        int enemyReward = (int)enemyData.currencyReward;

        foreach (currencyType coin in currencyTypes)
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

    private float CalculatePowerScore()
    {
        // NEW: We are using currentHealth and currentDamage now!
        // This means when the enemy scales up in Initialize(), the loot gets better too!
        float powerScore = (float)(currentHealth * HealthWeight) +
                           (float)(currentDamage * DamageWeight) 
                          ;
        return powerScore;
    }
    // Rolls the dice to drop one bonus item based on the Power Score
    // Rolls the dice to drop one bonus item based on the Power Score
    public void SpawnBonusLoot(int numberOfRolls)
    {

        float totalWeight = 0f;

        // We create a temporary list to hold the math for this specific dice roll
        List<float> modifiedWeights = new List<float>();

        // NEW: Print the Power Score to the console so you can see how big it is!
        Debug.Log($"--- ENEMY DIED! Power Score: {cachedPowerScore} ---");

        // 1. THE DYNAMIC MODIFIER (We only need to calculate the math once)
        foreach (currencyType item in currencyTypes)
        {
            float currentItemWeight = item.baseWeight;

            if (item.isRareloot)
            {
                // MODIFIED: Changed 0.5f to 0.01f so the bonus grows MUCH slower. 
                // You can tweak this number until it feels right!
                currentItemWeight += (cachedPowerScore * 0.01f);
            }
            else
            {
                // MODIFIED: Changed 0.2f to 0.01f so common loot isn't destroyed as fast.
                currentItemWeight -= (cachedPowerScore * 0.01f);
                if (currentItemWeight < 1f) currentItemWeight = 1f;
            }

            // NEW: This prints the exact math for this item to your Unity Console!
            Debug.Log($"Item: {item.currencyTypeName} | Base: {item.baseWeight} | Final Tickets: {currentItemWeight}");

            modifiedWeights.Add(currentItemWeight);
            totalWeight += currentItemWeight;
        }
        Debug.Log($"Total Tickets in the Hat: {totalWeight}");
        // 2. THE MULTIPLE DICE ROLLS
        // We wrap the dice roll in a loop so it happens as many times as we want!
        for (int roll = 0; roll < numberOfRolls; roll++)
        {
            float randomRoll = Random.Range(0f, totalWeight);

            for (int i = 0; i < currencyTypes.Count; i++)
            {
                randomRoll -= modifiedWeights[i];

                if (randomRoll <= 0f)
                {
                    // I made the random offset slightly bigger so the coins spread out more!
                    Vector3 randomOffset = new Vector3(Random.Range(5f, 10f), Random.Range(1f, 3f), 0);
                    Instantiate(currencyTypes[i].prefabForCurrencys, transform.position + randomOffset, Quaternion.identity);
                    break;
                }
            }
        }
    }

    IEnumerator EnemyIsDead()
    {
        
        anim.SetBool("IsDead", true);
        yield return new WaitForSeconds(0.65f);
        DestroySelf();
    }

    void DestroySelf()
        {
           // Destroy(gameObject);
        }

    private IEnumerator FlashRoutine()
    {
        enemySprite.material = whiteMat;
        yield return new WaitForSeconds(enemyFlash);
        enemySprite.material = defultMat;
    }
    public void PlayFlash()
    {
        StartCoroutine(FlashRoutine());
    }
    protected virtual void SpawnFloatingText(double damage, bool isCrit)
    {
        if (damageTextPrefab == null) return;

        // X between 0 and -3 (Left side), Y between 0.5 and 3 (Above)
        float randomX = Random.Range(0f, -8f);
        float randomY = Random.Range(0.2f, 1.6f);
        Vector3 randomOffset = new Vector3(randomX, randomY, 0f);

        Vector3 spawnPosition = posOfText.position + randomOffset;

        // Instantiate the text
        DamageText textInstance = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity);

        // Pass your values from PlayerStats into our new script
        textInstance.SetupText(damage, isCrit);
    }
}

 