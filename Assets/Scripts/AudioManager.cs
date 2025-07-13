using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource sfxSource;
    public AudioSource musicSource;

    [Header("Sound Clips")]
    public AudioClip jumpClip;
    public AudioClip obstacleHitClip;
    public AudioClip cloudClip;
    public AudioClip springboardClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayJump()
    {
        PlaySFX(jumpClip);
    }

    public void PlayObstacleHit()
    {
        PlaySFX(obstacleHitClip);
    }

    public void PlayCloud()
    {
        PlaySFX(cloudClip);
    }

    public void PlaySpringboard()
    {
        PlaySFX(springboardClip);
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayMusic(AudioClip music, bool loop = true)
    {
        if (musicSource != null)
        {
            musicSource.clip = music;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }
} 