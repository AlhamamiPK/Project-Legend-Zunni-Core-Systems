using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
/// <summary>
/// AudioPlayer: Manages all game audio SFX playback, music ducking, hurt scaling,
/// Level-up sounds, loot sounds, and boss music transitions.
/// </summary>
public class AudioPlayer : MonoBehaviour
{
    #region Inspector Fields

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bossThemeAudioSource;

    [Header("Music")]
    [SerializeField] private AudioClip bossTheme;

    [Header("SFX - Combat")]
    [SerializeField] private AudioClip damagingAnEnemy;
    [SerializeField] private AudioClip playerDeath;
    [SerializeField][Range(0f, 1f)] private float dmgVolume = 0.4f;

    [Header("SFX - Hurt")]
    [Tooltip("Pool of hurt sounds — one is picked randomly per hit.")]
    [SerializeField] private AudioClip[] hurtSounds;
    [SerializeField][Range(0f, 1f)] private float hurtVolume = 0.5f;

    [Header("SFX - Level Up")]
    [Tooltip("Pool of level-up sounds — one is picked randomly.")]
    [SerializeField] private AudioClip[] levelUpSounds;

    [Header("SFX - Loot")]
    [SerializeField] private AudioClip[] spendingCoinsSounds;
    [SerializeField][Range(0f, 1f)] private float coinSpendingSoundsPower = 0.8f;

    [Header("Settings")]
    [SerializeField][Range(0f, 1f)] private float duckedMusicVolume = 0.3f;
    [Tooltip("Seconds trimmed from the end of a clip's wait time.")]
    [SerializeField] private float sfxTrimTime = 0.3f;

    #endregion

    #region Singleton
    public static AudioPlayer Instance { get; private set; }
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
    private void Start()
    {
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
    #endregion

    #region SFX
    public void PlayGenericSFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    public void PlayLevelUpSound()
    {
        if (levelUpSounds != null && levelUpSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, levelUpSounds.Length);
            AudioClip chosenSound = levelUpSounds[randomIndex];
            if (chosenSound != null)
            {
                StartCoroutine(PlaySFXAndPauseMusic(chosenSound));
            }
        }
        else
        {
            Debug.LogWarning("No Level Up sounds assigned in the AudioPlayer!");
        }
    }
    public void DamagingAnEnemy()
    {
        if (damagingAnEnemy != null)
        {
            sfxSource.PlayOneShot(damagingAnEnemy,dmgVolume);
        }
    }
    public void PlayerHurt(int hurtScale)
    {
        if (hurtSounds == null || hurtSounds.Length == 0 || hurtScale <= 0) return;

        float multiplier = hurtScale < hurtVolumeMultipliers.Length ? hurtVolumeMultipliers[hurtScale] : 2f;
        int randomIndex = Random.Range(0,hurtSounds.Length);
        sfxSource.PlayOneShot(hurtSounds[randomIndex],hurtVolume*multiplier);        
    }
    public void SpendMoney()
    {
        if(spendingCoinsSounds != null &&  spendingCoinsSounds.Length > 0)
        {
            int randomIndex = Random.Range(0, spendingCoinsSounds.Length);
            AudioClip chosenSound = spendingCoinsSounds[randomIndex];
            if(chosenSound != null)
            {
                sfxSource.PlayOneShot(chosenSound, coinSpendingSoundsPower);
            }
        }
    }
    public void PlayerIsDead()
    {
        if(playerDeath != null)
        {
            // TODO: implement death sting + music stop
        }
    }
    #endregion
    #region Hurt Volume Data
    private static readonly float[] hurtVolumeMultipliers =
        { 0f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 1.0f, 1.4f, 1.6f, 1.7f, 2f };
    #endregion

    #region Music & Boss
    public void PlayBossTheme()
    {
        StartCoroutine(PlayBossMusicRoutine(bossTheme));
    }

    private IEnumerator PlaySFXAndLowerMusicVolume(AudioClip clipToPlay, float volume =1f)
    {
        float originalVolume = 1f;
        if (musicSource != null)
        {
            originalVolume = musicSource.volume;
            musicSource.volume = duckedMusicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }

        float waitTime = Mathf.Max(0f, clipToPlay.length - sfxTrimTime);
        yield return new WaitForSecondsRealtime(waitTime);

        if (musicSource != null)
        {
            musicSource.volume = originalVolume;
        }
    }

     private IEnumerator PlaySFXAndPauseMusic(AudioClip clipToPlay)
    {
        if (musicSource != null) musicSource.Pause();

        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }
        //  Wait exactly as long as the sound clip is
        float waitTime = Mathf.Max(0f, clipToPlay.length - sfxTrimTime);
        
            yield return new WaitForSecondsRealtime(waitTime);

        if (musicSource != null) musicSource.UnPause();
    }
    private IEnumerator PlayBossMusicRoutine(AudioClip bossClip)
    {
        // 1. Pause the normal stage music
        if (musicSource != null) musicSource.Pause();
        // 2. Play the boss theme!
        if (bossThemeAudioSource != null && bossClip != null)
        {
            bossThemeAudioSource.clip = bossClip;
            bossThemeAudioSource.loop = true; 
            bossThemeAudioSource.Play();
        }
        // 3. THE MAGIC WAIT TIMERS
        // WaitUntil pauses this exact script until the condition inside becomes true.
        // It checks every frame automatically. No crashing!
        yield return new WaitUntil(() => GameManager.isBossDead);

        // 4. The boss is dead (or time ran out)! Stop the boss music.
        if (bossThemeAudioSource != null)
        {
            bossThemeAudioSource.Stop();
            bossThemeAudioSource.clip = null;
        }
        // 5. Resume the normal stage music
        if (musicSource != null) musicSource.UnPause();
    }
    #endregion
}


