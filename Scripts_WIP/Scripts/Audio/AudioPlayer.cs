using UnityEngine;
using UnityEngine.Audio;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using UnityEditor.Experimental.GraphView;
public class AudioPlayer : MonoBehaviour
{
    public static AudioPlayer Instance;
    [Header("DamageRelated")]
    [SerializeField] AudioClip damagingAnEnemy;
    [SerializeField] AudioClip playerDeath;
    [SerializeField] AudioClip playerHurt;
    [SerializeField] AudioClip mainTheme;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sfxSource;
    [SerializeField] AudioSource bossThemeAudioSource;
    [SerializeField][Range(0f, 1f)] float dmgVolume = 1f;

    [Header("Settings")]
    [SerializeField][Range(0f, 1f)] float duckedMusicVolume = 1f;
    [Tooltip("How much time to shave off the end of the audio clip wait time (in seconds).")]
    [SerializeField] float sfxTrimTime = 0.3f;
    [Header("Level Up Related")]
    [Tooltip("Drag your 6 cut level-up sounds here.")]
    [SerializeField] AudioClip[] levelUpSounds;
    [Header("Hurt Sounds")]
    [SerializeField] AudioClip[] hurtSounds;
    [SerializeField][Range(0f, 1f)] float hurtVolume = 0.5f;
    [Header("Spending Money sounds")]
    [SerializeField] private AudioClip[] spendingCoinsSounds;
    [SerializeField][Range(0f,1f)] float coinSpendingSoundsPower;
    [Header("BossTHEMES")]
    [SerializeField] AudioClip bossTheme;

    private float waitTimeForBoss;
    public void Start()
    {
       
    }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);
        if (sfxSource != null)
        {
            sfxSource.enabled = true; // Now this will ALWAYS run
        }

        // Keep the music logic, but checking if it's null first is safer
        if (musicSource != null && !musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }
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
            // Pick a random number between 0 and the amount of sounds in the list
            int randomIndex = Random.Range(0, levelUpSounds.Length);
            AudioClip chosenSound = levelUpSounds[randomIndex];

            if (chosenSound != null)
            {
                // We use your existing coroutine to pause music, play the sound, wait, and resume!
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
            // = Random.Range(0.9f, 1.1f);
            //AudioSource.PlayClipAtPoint(damagingAnEnemy, Camera.main.transform.position, dmgVolume);
            sfxSource.PlayOneShot(damagingAnEnemy,0.4f);
        }
    }
    public void PlayerHurt(int hurtScale)
    {
        if (hurtSounds != null && hurtSounds.Length > 0 )
        {
            // Pick a random number between 0 and the amount of sounds in the list
            int randomIndex = Random.Range(0, hurtSounds.Length);
            AudioClip chosenSound = hurtSounds[randomIndex];

            if (chosenSound != null)
            {
                //sfxSource.pitch = Random.Range(0.90f, 1.10f);
                float finalVolume = hurtVolume;
                if(hurtScale == 1)
                {
                    finalVolume = hurtVolume * 0.2f;
                }else if(hurtScale == 2)
                {
                    finalVolume = (hurtVolume * 0.3f);
                }else if(hurtScale == 3)
                {
                    finalVolume = (hurtVolume * 0.4f);
                }else if (hurtScale == 4)
                {
                    finalVolume = (hurtVolume * 0.4f);
                }else if(hurtScale == 5)
                {
                    finalVolume = (hurtVolume * 0.5f);
                }else if (hurtScale == 6)
                {
                    finalVolume = (hurtVolume * 0.6f);
                }
                else if (hurtScale == 7)
                {
                    finalVolume = (hurtVolume * 1.1f);
                }
                else if (hurtScale == 8)
                {
                    finalVolume = (hurtVolume * 1.3f);
                }
                else if (hurtScale == 9)
                {
                    finalVolume = (hurtVolume * 1.4f);
                }
                else if (hurtScale == 10)
                {
                    finalVolume = (hurtVolume * 1.6f);
                }
                else if (hurtScale == 11)
                {
                    finalVolume = (hurtVolume * 1.8f);
                }else if( hurtScale == 12)
                {
                    finalVolume = (hurtVolume * 2f);
                }
                    sfxSource.PlayOneShot(chosenSound, finalVolume);
            }
        }
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

           // StartCoroutine(PlaySFXAndPauseMusic(playerDeath));
        }
    }
    // Add this to AudioPlayer.cs

    public void PlayBossTheme()
    {
        StartCoroutine(PlayBossMusicRoutine(bossTheme));
        //StartCoroutine(PlaySFXAndPauseMusic(bossTheme));
    }

    IEnumerator PlaySFXAndLowerMusicVolume(AudioClip clipToPlay, float volume =1f)
    {
        float originalVolume = 1f;

        // 1. Lower the music volume (but save the original volume first so we can go back to it)
        if (musicSource != null)
        {
            originalVolume = musicSource.volume;
            musicSource.volume = duckedMusicVolume;
        }

        // 2. Play the sound effect
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }

        // 3. Wait exactly as long as the sound clip is
        float waitTime = Mathf.Max(0f, clipToPlay.length - sfxTrimTime);
        yield return new WaitForSecondsRealtime(waitTime);

        // 4. Restore the music to its normal volume
        if (musicSource != null)
        {
            musicSource.volume = originalVolume;
        }
    }

    IEnumerator PlaySFXAndPauseMusic(AudioClip clipToPlay)
    {
        // 1. Pause the music
        if (musicSource != null) musicSource.Pause();

        // 2. Play the sound effect
        if (sfxSource != null)
        {
            sfxSource.PlayOneShot(clipToPlay);
        }

        // 3. Wait exactly as long as the sound clip is
        float waitTime = Mathf.Max(0f, clipToPlay.length - sfxTrimTime);
        
            yield return new WaitForSecondsRealtime(waitTime);

        // 4. Resume the music
        if (musicSource != null) musicSource.UnPause();
    }
    // NEW COROUTINE JUST FOR THE BOSS
    IEnumerator PlayBossMusicRoutine(AudioClip bossClip)
    {
        // 1. Pause the normal stage music
        if (musicSource != null) musicSource.Pause();
        musicSource.Pause();
        // 2. Play the boss theme!
        // We DO NOT use PlayOneShot here, because we need the ability to stop it early.
        if (bossThemeAudioSource != null && bossClip != null)
        {
            bossThemeAudioSource.clip = bossClip;
            bossThemeAudioSource.loop = true; // Boss music should loop!
            bossThemeAudioSource.Play();
        }

        // 3. THE MAGIC WAIT TIMERS
        // WaitUntil pauses this exact script until the condition inside becomes true.
        // It checks every frame automatically. No crashing!
        yield return new WaitUntil(() => GameManager.isBossDead == true);

        //// 4. The boss is dead (or time ran out)! Stop the boss music.
        if (bossThemeAudioSource != null)
        {
            bossThemeAudioSource.Stop();

            //sfxSource.Pause();

            bossThemeAudioSource.clip = null;
        }
        // 5. Resume the normal stage music
        if (musicSource != null) musicSource.UnPause();
    }
}


