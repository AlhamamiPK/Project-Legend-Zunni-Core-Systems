using System.Collections;
using MoreMountains.Feedbacks;
using UnityEngine;

/// <summary>
/// Player stats: movement, combat, damage intake, and hit feedback.
/// Access globally via Instance.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    #region Constants
    private const float KnockbackDuration = 0.35f;
    private const float SpeedToDamageRatio = 0.05f;
    #endregion
    #region Inspector - Movement
    [Header("Movement")]
    [Tooltip("Base walk speed before sprint multiplier.")]
    public float baseSpeed = 5f;
    [Tooltip("How fast sprint builds while running.")]
    public float accelerationRate = 0.5f;
    [Tooltip("Max sprint multiplier.")]
    [Range(1f,5f)]
    public float maxMultiplier = 3f;
    [Tooltip("Hard cap on movement speed.")]
    public float speedLimit = 50f;

    [Space(4)]
    [Tooltip("Current sprint multiplier (runtime).")]
    public float currentMultiplier = 1f;
    [Tooltip("Speed used for movement this frame.")]
    public float currentMoveSpeed;
    [Tooltip("Speed over the cap - used for bonus damage.")]
    public float virtualSpeed = 0f;

    [Space(10)]
    [Header("Combat")]
    [Tooltip("Max hit points.")]
    public double maxHealth = 100;
    [Tooltip("Current hit points.")]
    public double currentHealth;
    [Tooltip("Base Damage.")]
    public double playerDamage = 10;
    [Tooltip("Adds % extra damage.")]
    [Range(0f,500f)]
    public float critDamageAmp = 20f;
    [Tooltip("Chance of crit.")]
    [Range(0f,100f)]
    public float critChance = 5f;
    [Tooltip("The calculated damage")]
    public double finalDamage;

    [Space(5)]
    [Header("Physical")]
    [SerializeField] private SpriteRenderer playerSprite;
    [Header("Runtime")]
    public bool playerIsDead = false;
    public bool thatDamageWasCrit = false;
    [Header("WhiteFlashThings")]
    public Material whiteMat;
    private Material defaultMat;
    [SerializeField] private float playerFlash = 0.05f;
    [SerializeField] public Transform playerTransform;
    [SerializeField] HealthBar healthBar;
    [Space(5)]
    [Header("References & Hit Juice")]
    [Tooltip("The base push strength. We multiply this by the damage tier.")]
    [SerializeField] public float baseKnockBackForce = 30f;
    [SerializeField] public MMF_Player hitFeedBack;

#endregion

    #region Singleton   
    public static PlayerStats Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Unity Lifecycle
    public void Start()
    {
        if (playerSprite != null)
        { 
            defaultMat = playerSprite.material;
        }
        currentHealth = maxHealth;
        if (healthBar != null) 
        { 
            healthBar.UpdateHealth();
        }

    }
    private void FixedUpdate()
    {
        CheckDeath();
        UpdateMovementSpeed();
    }
    #endregion

    #region Movement
    private void UpdateMovementSpeed()
    {
        if (PlayerController.isPlayerRunningWithZeroInterrputtion)
        {
            currentMultiplier += accelerationRate * Time.fixedDeltaTime;
            currentMultiplier = Mathf.Clamp(currentMultiplier, 1f, maxMultiplier);
        }
        else
        {
            currentMultiplier = 1f;
        }

        float intendedSpeed = baseSpeed * currentMultiplier;

        if (intendedSpeed > speedLimit)
        {

            currentMoveSpeed = speedLimit;
            virtualSpeed = intendedSpeed - speedLimit;
        }
        else
        {
            currentMoveSpeed = intendedSpeed;
            virtualSpeed = 0f;
        }
    }
    #endregion

    #region Damage
    private readonly struct DamageTier
    {
        public readonly float MaxPercentOfMaxHealth;
        public readonly float KnockbackMultiplier;
        public readonly int HurtSoundIndex;

        public DamageTier(float maxPercent, float knockback, int soundIndex)
        {
            MaxPercentOfMaxHealth = maxPercent;
            KnockbackMultiplier = knockback;
            HurtSoundIndex = soundIndex;
        }
    }
        private static readonly DamageTier[] DamageTiers =
        {
            //Each new(10f, 0.5f, 1) means: “up to 10% of max HP → 0.5 knockback → hurt sound 1.”
            new(10f,  0.5f,  1),
            new(20f,  1.0f,  2),
            new(30f,  2.5f,  3),
            new(40f,  3.0f,  4),
            new(50f,  3.5f,  5),
            new(60f,  4.0f,  6),
            new(70f,  4.5f,  7),
            new(80f,  5.0f,  8),
            new(90f,  5.5f,  9),
            new(95f,  6.0f, 10),
            new(99f,  7.5f, 11),
        };
        private static readonly DamageTier BrutalHitTier = new(100f, 10f, 12);

        private static DamageTier ResolveDamageTier(double damagePercent)
        {
            if (damagePercent > 99) return BrutalHitTier;

            for(int i = 0; i < DamageTiers.Length; i++)
            {
                if (damagePercent <= DamageTiers[i].MaxPercentOfMaxHealth) return DamageTiers[i];
            }
            return BrutalHitTier;
        }

    
    public void GettingDamaged(double incomingDamage, Transform enemyTransform)
    {
        // 1. Guards
        if(playerIsDead || incomingDamage<= 0) return;

        // 2. Tier
        double damagePercent = (incomingDamage / maxHealth) * 100;
        DamageTier tier = ResolveDamageTier(damagePercent);

        // 3. Direction
        Vector2 pushDirection = (playerTransform.position - enemyTransform.position).normalized;
        if (pushDirection.sqrMagnitude < 0.0001f) pushDirection = Vector2.left;

        // 4. Sound (one line!)
        AudioPlayer.Instance.PlayerHurt(tier.HurtSoundIndex);

        // 5. Apply damage + reset sprint
        currentHealth -= incomingDamage;
        currentMultiplier = 1f;
        virtualSpeed = 0f;

        // 6. UI
        if (HealthBar.healthBarInstance != null) 
        {
            HealthBar.healthBarInstance.PlayFlash();
            HealthBar.healthBarInstance.PlayerIsDamaged();
        
        }

        // 7. Juice + knockback 
        if(CameraShake.cameraShakeForOtherObjectsToUseAndGrab != null)
        {
            CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
        }

        if (hitFeedBack != null) 
        { 
            hitFeedBack.PlayFeedbacks();
        }

        PlayFlash();

        float knockback = baseKnockBackForce * tier.KnockbackMultiplier;
        PlayerController.instance.TakeKnockBack(pushDirection* knockback, KnockbackDuration);
    }
    #endregion

    #region Combat
    public void CriticalDamage()
    {
        // 1. Get the player's absolute total speed (capped physical + virtual)
        float totalSpeed = currentMoveSpeed + virtualSpeed;
        // 2. THE KINETIC RAM: 5% of their total speed becomes flat Bonus Damage.
        double speedBonusDamage = totalSpeed * SpeedToDamageRatio;
        double effectiveBaseDamage = playerDamage + speedBonusDamage;

        // 3. Calculate Critical Hits using the new Effective Damage
        float roll = Random.Range(0f, 100f);
        if (roll <= critChance)
        {
            thatDamageWasCrit = true;
            finalDamage = effectiveBaseDamage + (effectiveBaseDamage * (critDamageAmp / 100));
        }
        else
        {
            thatDamageWasCrit = false;
            finalDamage = effectiveBaseDamage;
        }

        // 4. THE MOMENTUM STRIKE: Multiply the final result by how fast they are currently running.
        // If they just got hit, it's x1. If they are sprinting, it's up to x2.5!
        finalDamage *= currentMultiplier;
        
    }
    #endregion

    #region Death
    private void CheckDeath()
    {
        if (!playerIsDead && currentHealth <= 0)
        {
            playerIsDead = true;
            AudioPlayer.Instance.PlayerIsDead();
        }
    }
    #endregion

    #region Visual Feedback
    private IEnumerator FlashRoutine()
    {
        playerSprite.material = whiteMat;
        yield return new WaitForSeconds(playerFlash);
        playerSprite.material = defaultMat;
    }
    public void PlayFlash()
    {
        StartCoroutine(FlashRoutine());
    }
    #endregion
}