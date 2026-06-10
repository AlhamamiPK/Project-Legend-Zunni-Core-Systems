using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;
/// <summary>
/// CurrencyPickUp: Handles the movement, behavior, appearance of the currency
/// </summary>
public class CurrencyPickUp : MonoBehaviour
{
    #region Inspector Fields
    [Header("Currency Attributes")]
    [SerializeField] private CurrencyData currencyData;

    [Header("Feel Feedbacks")]
    [SerializeField] private MMF_Player spawnFeedback;
    [SerializeField] private MMF_Player collectFeedback;
    
    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Magnetic Settings")]
    [Tooltip("How close the player needs to be to suck up the coin")]
    [SerializeField] private float magneticRadius = 3f;
    [Tooltip("How long before the coin can be sucked up (so it can burst first)")]
    [SerializeField] private float magnetDelay = 0.3f;

    [Header("Visual Settings")]
    [SerializeField] private float minScale = 1.0f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private float flightAcceleration = 35f;
    [SerializeField] private bool addSparkleParticles = true;
    [SerializeField] private Color sparkleColor = Color.yellow;

    #endregion

    #region Runtime State
    private SpriteRenderer spriteRenderer;
    private ParticleSystem sparkleParticles;
    private bool isCollected = false;
    private Transform playerTransform;
    private float baseFlySpeed;
    private float currentFlySpeed;
    private bool isFlyingToPlayer = false;
    private bool canBeMagnetized = false;
    private Vector3 initialScale;

    #endregion

    #region Unity Lifecycle
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spawnFeedback != null) spawnFeedback.PlayFeedbacks();
        // 1. Read the data and set the visual sprite
        if (currencyData != null)
        {
            spriteRenderer.sprite = currencyData.sprite;
        }

        // 2. Randomize Scale slightly for variety
        float randomScaleMultiplier = Random.Range(minScale, maxScale);
        initialScale = new Vector3(randomScaleMultiplier, randomScaleMultiplier, 1f);
        transform.localScale = initialScale;

        // 3. The "Loot Pop" - Shoot the coin upwards and randomly left/right
        if (rb != null)
        {
            Vector2 burstForce = new Vector2(Random.Range(-5f, 5f), Random.Range(4f, 7f));
            rb.AddForce(burstForce, ForceMode2D.Impulse);
        }

        // 4. Setup Homing Variables
        playerTransform = PlayerStats.Instance.transform;
        baseFlySpeed = Random.Range(12f, 20f);

        // 5. Setup Particles
        if (addSparkleParticles)
        {
           SetupParticleSystem();
        }
        
        SetupAnimator();
        StartCoroutine(DelayToTurnOnMagnet());
    }
    void Update()
    {
        if (canBeMagnetized && !isFlyingToPlayer && playerTransform != null && !isCollected)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // 1. If the player steps inside the radius, trigger the homing missile!
            if (distanceToPlayer <= magneticRadius)
            {
                StartFlying();
            }
        }

        // 2. Homing — accelerates toward player and shrinks on arrival
        if (isFlyingToPlayer && playerTransform != null && !isCollected)
        {
            currentFlySpeed += flightAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, currentFlySpeed * Time.deltaTime);

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance < 1.5f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, initialScale * 0.2f, Time.deltaTime * 15f);
            }
        }
    }
    #endregion

    #region Magnet & Flight
   
    private IEnumerator DelayToTurnOnMagnet()
    {
        yield return new WaitForSeconds(magnetDelay);
        TurnOnMagnet();
    }

    private void TurnOnMagnet()
    {
        canBeMagnetized = true;
    }

    private void StartFlying()
    {
        isFlyingToPlayer = true;
        currentFlySpeed = baseFlySpeed;

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero; // Stop it from bouncing
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
 
    }
    #endregion

    #region Collection
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isCollected)
        {
            isCollected = true;

            CurrencyManager.Instance.AddCurrency((int)currencyData.currencyValue);

            if (currencyData.pickUpSoundForTheCurrency != null)
            {
                AudioPlayer.Instance.PlayGenericSFX(currencyData.pickUpSoundForTheCurrency);
            }
            collectFeedback.PlayFeedbacks();
            spriteRenderer.enabled = false;
            if (sparkleParticles != null)
            {
                sparkleParticles.transform.SetParent(null); // Detach particles
                sparkleParticles.Stop(); // Stop emitting
                Destroy(sparkleParticles.gameObject, 1f); // Clean up the leftover trail after 1 second
            }

        }
    }
    #endregion

    #region Setup
    private void SetupAnimator()
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null && currencyData != null && currencyData.animation != null)
        {
            // Create an override controller to swap the default animation with the one in your ScriptableObject
            AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

            // We assume you have a default state in your Animator. This swaps the clips.
            var clipOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
            overrideController.GetOverrides(clipOverrides);

            if (clipOverrides.Count > 0)
            {
                // Swap the first animation clip with the one from your ScriptableObject
                clipOverrides[0] = new KeyValuePair<AnimationClip, AnimationClip>(clipOverrides[0].Key, currencyData.animation);
                overrideController.ApplyOverrides(clipOverrides);
                animator.runtimeAnimatorController = overrideController;
            }
        }
    }
    private void SetupParticleSystem()
    {
        GameObject particleObj = new GameObject("CurrencySparkles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;

        sparkleParticles = particleObj.AddComponent<ParticleSystem>();
        sparkleParticles.Stop();

        ParticleSystemRenderer psr = particleObj.GetComponent<ParticleSystemRenderer>();
        psr.material = spriteRenderer.material;

        var main = sparkleParticles.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 0.5f;
        main.startSpeed = 0f;
        main.startSize = 0.5f;
        main.startColor = sparkleColor; 
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = sparkleParticles.emission;
        emission.rateOverTime = 20f;

        var shape = sparkleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        var sizeOverLifetime = sparkleParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);

        sparkleParticles.Play();
    }
    #endregion

    #region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow; 
        Gizmos.DrawWireSphere(transform.position, magneticRadius);
    }
    #endregion
}