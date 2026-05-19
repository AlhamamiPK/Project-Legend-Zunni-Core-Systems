using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [Header("Speed")]
    public float baseSpeed = 5f;
    public float currentMultiplier = 1f;
    public float maxMultiplier = 3f;
    public float accelerationRate = 0.5f;
    public float speedLimit = 50;
    public float virtualSpeed = 0f;
    public float currentMoveSpeed;
    [Header("----CORE STATS ----")]
    public double maxHealth = 100;
    public double currentHealth;
    public double playerDamage = 10;
    //public float moveSpeed = 5.0f;
    public float critDamageAmp = 20f;
    public float critChance = 5f;
    public double finalDamage;
    [Header("Players Phisycal Stuff Fuck my Spelling")]
    [SerializeField] private SpriteRenderer playerSprite;
    [Header("Bools")]
    public bool playerIsDead = false;
    public bool thatDamageWasCrit = false;
    [Header("WhiteFlashThings")]
    public Material whiteMat;
    private Material defultMat;
    [SerializeField] private float playerFlash = 0.05f;
    [SerializeField] public Transform playerTransform;
    [SerializeField] HealthBar healthBar;
    [Header("KnockBackStuff")]
    [Tooltip("The base push strength. We multiply this by the damage tier.")]
    [SerializeField] public float baseKnockBackForce = 30f;

    [Header("MMF FEEDBACKS")]
    [SerializeField] public MMF_Player hitFeedBack;
    #region Making Our Script global   
    public static PlayerStats Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
    }
    #endregion
    public void Start()
    {
        defultMat = playerSprite.material;
        currentHealth = maxHealth;
        healthBar.UpdateHealth();

    }
    private void FixedUpdate()
    {
        Death();
        CalculateSpeed();
    }
    private void CalculateSpeed()
    {
        if(PlayerController.isPlayerRunningWithZeroInterrputtion == true)
        {
            currentMultiplier += accelerationRate * Time.fixedDeltaTime;
            currentMultiplier = Mathf.Clamp(currentMultiplier, 1f, maxMultiplier);
        }
        else
        {
            currentMultiplier = 1f;
        }

        currentMoveSpeed = baseSpeed * currentMultiplier;
        float intendedSpeed = baseSpeed * currentMultiplier;

        if (intendedSpeed > speedLimit) {

            currentMoveSpeed = speedLimit;
            virtualSpeed = intendedSpeed - speedLimit;
        }else
        {
            currentMoveSpeed = intendedSpeed;
            virtualSpeed = 0f;
        }
    }
    //public void GettingDamaged(double incomingDmage)
    //{
    //    double damagePercent = (incomingDmage / currentHealth) * 100;

    //    if (damagePercent <= 10)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce*0.5f, 0.35f);
    //    }
    //    else if (damagePercent <= 20)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce*1f, 0.35f);
    //    }
    //    else if (damagePercent <= 30)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 1.5f, 0.35f);
    //    }
    //    else if (damagePercent <= 40)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 2f, 0.35f);
    //    }
    //    else if (damagePercent <= 50)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 2.5f, 0.35f);
    //    }
    //    else if (damagePercent <= 60)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 3f, 0.35f);
    //    }
    //    else if (damagePercent <= 70)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 3.5f, 0.35f);
    //    }
    //    else if (damagePercent <= 80)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 4f, 0.35f);
    //    }
    //    else if (damagePercent <= 90) 
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 4.5f, 0.35f);
    //    } 
    //    else if (damagePercent <= 95)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 5f, 0.35f);
    //    }
    //    else if (damagePercent <= 99)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 5.5f, 0.35f);
    //    }
    //    else if(damagePercent <= 100)
    //    {
    //        AudioPlayer.Instance.PlayerHurt();
    //        currentMultiplier = 1f;
    //        virtualSpeed = 0f;
    //        currentHealth = currentHealth - incomingDmage;
    //        HealthBar.healthBarInstance.PlayFlash();
    //        HealthBar.healthBarInstance.PlayerIsDamaged();
    //        CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //        PlayerController.instance.TakeKnockBack(knockBackForce * 6f, 0.35f);
    //    }
    //    //AudioPlayer.Instance.PlayerHurt();
    //    //currentMultiplier = 1f;
    //    //virtualSpeed = 0f;
    //    //currentHealth = currentHealth - incomingDmage;
    //    //HealthBar.healthBarInstance.PlayFlash();
    //    //HealthBar.healthBarInstance.PlayerIsDamaged();
    //    //CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
    //    //PlayerController.instance.TakeKnockBack(knockBackForce, 0.35f);

    //}
    // NOTICE: We added 'Transform enemyTransform' so the player knows where the hit came from!
    public void GettingDamaged(double incomingDamage, Transform enemyTransform)
    {
        // 1. Base the tier on MAX health so knockback remains consistent regardless of current health
        double damagePercent = (incomingDamage / maxHealth) * 100;

        // 2. Calculate the direction to push the player (Away from the enemy)
        Vector2 pushDirection = (playerTransform.position - enemyTransform.position).normalized;



        // 3. Determine the Knockback Tier
        float knockbackMultiplier = 0.5f;

        if (damagePercent <= 10)      { knockbackMultiplier = 0.5f; AudioPlayer.Instance.PlayerHurt(1); }
        else if (damagePercent <= 20) { knockbackMultiplier = 1.0f; AudioPlayer.Instance.PlayerHurt(2); }
        else if (damagePercent <= 30) { knockbackMultiplier = 2.5f; AudioPlayer.Instance.PlayerHurt(3); }
        else if (damagePercent <= 40) { knockbackMultiplier = 3.0f; AudioPlayer.Instance.PlayerHurt(4); }
        else if (damagePercent <= 50) { knockbackMultiplier = 3.5f; AudioPlayer.Instance.PlayerHurt(5); }
        else if (damagePercent <= 60) { knockbackMultiplier = 4.0f; AudioPlayer.Instance.PlayerHurt(6); }
        else if (damagePercent <= 70) { knockbackMultiplier = 4.5f; AudioPlayer.Instance.PlayerHurt(7); }
        else if (damagePercent <= 80) { knockbackMultiplier = 5.0f; AudioPlayer.Instance.PlayerHurt(8); }
        else if (damagePercent <= 90) { knockbackMultiplier = 5.5f; AudioPlayer.Instance.PlayerHurt(9); }
        else if (damagePercent <= 95) { knockbackMultiplier = 6.0f; AudioPlayer.Instance.PlayerHurt(10); }
        else if (damagePercent <= 99) { knockbackMultiplier = 7.5f; AudioPlayer.Instance.PlayerHurt(11); }
        else                          { knockbackMultiplier = 10.0f; AudioPlayer.Instance.PlayerHurt(12); } 

        // 4. Apply all shared damage effects ONCE instead of repeating them 11 times
        currentMultiplier = 1f;
        virtualSpeed = 0f;
        currentHealth = currentHealth - incomingDamage;

        if (HealthBar.healthBarInstance != null)
        {
            HealthBar.healthBarInstance.PlayFlash();
            HealthBar.healthBarInstance.PlayerIsDamaged();
        }

        if (CameraShake.cameraShakeForOtherObjectsToUseAndGrab != null)
        {
            CameraShake.cameraShakeForOtherObjectsToUseAndGrab.Play();
        }

        // 5. Apply the dynamic Knockback to the Player Controller!
        hitFeedBack.PlayFeedbacks();
        Vector2 finalKnockbackForce = pushDirection * (baseKnockBackForce * knockbackMultiplier);
        PlayerController.instance.TakeKnockBack(finalKnockbackForce, 0.35f);
    }
    public void CriticalDamage()
    {
        // 1. Get the player's absolute total speed (capped physical + virtual)
        float totalSpeed = currentMoveSpeed + virtualSpeed;

        // 2. THE KINETIC RAM: 5% of their total speed becomes flat Bonus Damage.
        // (You can change 0.1f to 0.5f if you want it to be stronger!)
        double speedBonusDamage = totalSpeed * 0.05f;
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
        //float roll = Random.Range(0f, 100f);
        //if (roll <= critChance)
        //{
        //    thatDamageWasCrit = true;
        //    finalDamage =playerDamage + (playerDamage * (critDamageAmp/100));
        //}
        //else
        //{
        //    thatDamageWasCrit = false;
        //    finalDamage = playerDamage;
        //}
    }

    public void Death()
    {
        if(currentHealth <0 && playerIsDead == false)
        {
            playerIsDead = true;
            //playerControllerSciprt.enabled = false;
            AudioPlayer.Instance.PlayerIsDead();
        }
    }
    private IEnumerator FlashRoutine()
    {
        playerSprite.material = whiteMat;
        yield return new WaitForSeconds(playerFlash);
        playerSprite.material = defultMat;
    }
    public void PlayFlash()
    {
        StartCoroutine(FlashRoutine());
    }
}
