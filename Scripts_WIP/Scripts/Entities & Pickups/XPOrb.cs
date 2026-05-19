using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class XPOrb : MonoBehaviour
{
    [Header("Randomization Settings")]
    public Sprite[] orbSprites;
    public AudioClip[] pickUpSounds;
    [Range(0f, 1f)] public float soundVolume = 0.2f;

    [Header("Scale & Polish Settings")]
    public float minScale = 5.0f;
    public float maxScale = 7.6f;
    public float flightAcceleration = 35f;
    public bool addSparkleParticles = true;

    [Header("Glow Color Settings")]
    public Color[] glowColors = new Color[3]; // Set your 3 colors in the Inspector!
    public float colorPulseSpeed = 3f; // How fast it switches colors

    // Lighting Variables
    public Light2D orbLight;
    public float colorTimer = 0f;
    public int currentColorIndex = 0;
    public int nextColorIndex = 1;

    public Transform playerTransform;
    public float baseFlySpeed;
    public float currentFlySpeed;
    public float flySpeed;
    public bool isFlyingToPlayer = false;
    public Vector3 initialScale;
    public SpriteRenderer spriteRenderer;
    public Rigidbody2D rb;
    public ParticleSystem sparkleParticles;
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (orbSprites.Length > 0)
        {
            int randomSpriteIndex = Random.Range(0, orbSprites.Length);
            spriteRenderer.sprite = orbSprites[randomSpriteIndex];
        }
        float randomScaleMultiplier = Random.Range(minScale, maxScale);
        initialScale = new Vector3(randomScaleMultiplier, randomScaleMultiplier, 1f);
        transform.localScale = initialScale;
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Shoot the orb upwards and randomly left or right!
            Vector2 burstForce = new Vector2(Random.Range(0f, 10f), Random.Range(2f, 3f));
            rb.AddForce(burstForce, ForceMode2D.Impulse);
        }
        playerTransform = PlayerStats.Instance.transform;
        baseFlySpeed = Random.Range(12f, 20f); // Randomize flight speed
        //float randomWaitTime = Random.Range(0.6f, 2f); // Wait between 0.6 and 1.8 seconds
        if (addSparkleParticles && GetComponentInChildren<ParticleSystem>() == null)
        {
           SetupParticleSystem();
        }
        orbLight = GetComponent<Light2D>();
        // Use the random wait time instead of 0.2f
        //new WaitForSeconds(randomWaitTime);
        //StartFlying();
        //Invoke("StartFlying", randomWaitTime);
        StopAllCoroutines();
        StartCoroutine(DelyStartFlying());
    }
    void Update()
    {
        if (orbLight != null && glowColors.Length > 0)
        {
            colorTimer += Time.deltaTime * colorPulseSpeed;

            // Smoothly blend between the current color and the next color
            orbLight.color = Color.Lerp(glowColors[currentColorIndex], glowColors[nextColorIndex], colorTimer);

            if (sparkleParticles != null)
            {
                var main = sparkleParticles.main;
                main.startColor = orbLight.color;
            }

            // Once it fully reaches the next color, swap targets
            if (colorTimer >= 1f)
            {
                colorTimer = 0f;
                currentColorIndex = nextColorIndex;
                // Move to the next color, and loop back to 0 if we hit the end of the list
                nextColorIndex = (nextColorIndex + 1) % glowColors.Length;
            }


        }
        if (isFlyingToPlayer && playerTransform != null)
        {
            currentFlySpeed += flightAcceleration * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, currentFlySpeed * Time.deltaTime);
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            if (distance < 1.5f)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, initialScale * 0.4f, Time.deltaTime * 15f);
            }

        }
    }

    IEnumerator DelyStartFlying()
    {
        float randomWaitTime = Random.Range(0.6f, 2f);
        yield return new WaitForSeconds(randomWaitTime);
        StartFlying();

    }

    void StartFlying()
    {
        isFlyingToPlayer = true;
        currentFlySpeed = baseFlySpeed;
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            //PlayerStats.Instance.GainExperience(5);
            XPBarUI.XPBARGlobalScript.FlashBar();
            if (pickUpSounds.Length > 0)
            {
                int randomSoundIndex = Random.Range(0, pickUpSounds.Length);
                AudioClip randomSound = pickUpSounds[randomSoundIndex];
                AudioPlayer.Instance.PlayGenericSFX(randomSound, soundVolume);


            }
            if (sparkleParticles != null)
            {
                sparkleParticles.transform.SetParent(null); // Detach particles
                sparkleParticles.Stop(); // Stop emitting
                Destroy(sparkleParticles.gameObject, 1f); // Clean up
            }
                Destroy(gameObject);
        }
    }
    private void SetupParticleSystem()
    {
        // Create a child object to hold the particles
        GameObject particleObj = new GameObject("OrbSparkles");
        particleObj.transform.SetParent(transform);
        particleObj.transform.localPosition = Vector3.zero;

        // NEW: Store the reference so Update() can change its color
        sparkleParticles = particleObj.AddComponent<ParticleSystem>();
        sparkleParticles.Stop();
        ParticleSystemRenderer psr = particleObj.GetComponent<ParticleSystemRenderer>();

        // Makes the particles glow with your bloom!
        psr.material = spriteRenderer.material;

        // Main particle settings
        var main = sparkleParticles.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 0.5f; // Short life so they disappear quickly
        main.startSpeed = 0f; // They stay exactly where they spawn in the air
        main.startSize = 0.76f; // Small pixel dots

        // Removed the hardcoded white color here, it's handled dynamically in Update() now!
        main.simulationSpace = ParticleSystemSimulationSpace.World; // Makes them trail behind the orb!

        // Emission rate
        var emission = sparkleParticles.emission;
        emission.rateOverTime = 25f; // Drops 25 particles per second

        // Shape of the emitter
        var shape = sparkleParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.5f; // Spawns randomly within this radius

        // Make them shrink to 0 size before disappearing
        var sizeOverLifetime = sparkleParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve curve = new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f));
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, curve);
        sparkleParticles.Play();
    }
}
