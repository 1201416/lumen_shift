using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// AudioManager - simple singleton for background music and SFX.
/// Attach to a GameObject in the first scene, or it will create itself on demand.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    public AudioClip backgroundMusic;
    public AudioClip dayMusic;
    public AudioClip nightMusic;
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    public bool musicLoop = true;
    public bool playMusicOnStart = true;
    [Tooltip("Seconds to crossfade between day/night tracks")]
    public float musicCrossfadeSeconds = 0.5f;

    [Header("SFX")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private AudioSource musicSource;
    private AudioSource musicSourceB; // second source for crossfades
    private AudioSource sfxSource;
    private Coroutine crossfadeCoroutine;
    private GameManager cachedGameManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Debug.Log("AudioManager: Awake() - initializing");
        
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Create AudioSources
        musicSource = gameObject.GetComponent<AudioSource>();
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();

        // Secondary source for crossfading
        musicSourceB = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSourceB.playOnAwake = false;

        musicSource.loop = musicLoop;
        musicSource.volume = musicVolume;
        musicSourceB.loop = musicLoop;
        musicSourceB.volume = 0f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        // Load default day/night music from Resources if none assigned
        if (dayMusic == null)
            dayMusic = Resources.Load<AudioClip>("Audio/day");
        if (nightMusic == null)
            nightMusic = Resources.Load<AudioClip>("Audio/night");

        // Subscribe to GameManager day/night changes if available (handle late binding)
        TryBindToGameManager();

        // Listen for scene loads to attempt binding if GameManager is created later
        SceneManager.sceneLoaded += OnSceneLoaded;

        // If still not bound, play fallback backgroundMusic if requested
        if (cachedGameManager == null && playMusicOnStart && backgroundMusic != null)
        {
            PlayMusic(backgroundMusic, true);
        }
    }

    void OnDestroy()
    {
        if (cachedGameManager != null)
            cachedGameManager.OnTimeOfDayChanged -= OnTimeOfDayChanged;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryBindToGameManager();
    }

    void TryBindToGameManager()
    {
        if (cachedGameManager == null)
        {
            cachedGameManager = FindFirstObjectByType<GameManager>();
            if (cachedGameManager != null)
            {
                cachedGameManager.OnTimeOfDayChanged += OnTimeOfDayChanged;
                Debug.Log($"AudioManager: bound to GameManager, initial isDay={cachedGameManager.isDayTime}");
                PlayDayNightMusic(cachedGameManager.isDayTime, true);
            }
        }
    }

    void OnTimeOfDayChanged(bool isDay)
    {
        Debug.Log($"AudioManager: OnTimeOfDayChanged received isDay={isDay}");
        PlayDayNightMusic(isDay, false);
    }

    /// <summary>
    /// Play day or night music depending on `isDay`.
    /// If `instant` is true, switch immediately without crossfade.
    /// </summary>
    public void PlayDayNightMusic(bool isDay, bool instant = false)
    {
        AudioClip target = isDay ? dayMusic : nightMusic;

        // If target is missing try reloading from Resources (in case assets were moved)
        if (target == null)
        {
            Debug.LogWarning($"AudioManager: {(isDay ? "dayMusic" : "nightMusic")} is null - attempting to load from Resources/Audio");
            string resName = isDay ? "Audio/day" : "Audio/night";
            AudioClip loaded = Resources.Load<AudioClip>(resName);
            if (loaded != null)
            {
                Debug.Log($"AudioManager: loaded {resName} from Resources");
                target = loaded;
                if (isDay) dayMusic = loaded; else nightMusic = loaded;
            }
            else
            {
                // Fallback to generic backgroundMusic
                target = backgroundMusic;
            }
        }

        Debug.Log($"AudioManager: switching music -> {(target != null ? target.name : "(null)")}, instant={instant}");

        if (target == null)
        {
            Debug.LogWarning("AudioManager: No music clip available to play for this time of day.");
            return;
        }

        if (instant || musicCrossfadeSeconds <= 0f)
        {
            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            musicSource.clip = target;
            musicSource.volume = musicVolume;
            musicSource.loop = musicLoop;
            musicSource.Play();
            musicSourceB.Stop();
        }
        else
        {
            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            crossfadeCoroutine = StartCoroutine(CrossfadeTo(musicSource, musicSourceB, target, musicCrossfadeSeconds));
        }
    }

    /// <summary>
    /// Play background music. If 'clip' is null, uses the inspector-assigned `backgroundMusic`.
    /// If `forceRestart` is false and the same clip is already playing, does nothing.
    /// </summary>
    public void PlayMusic(AudioClip clip = null, bool forceRestart = false)
    {
        if (clip == null) clip = backgroundMusic;
        if (clip == null) return;

        if (!forceRestart && (musicSource.isPlaying && musicSource.clip == clip || musicSourceB.isPlaying && musicSourceB.clip == clip)) return;

        // Use PlayDayNightMusic to respect crossfade if clip matches day/night, otherwise play immediately on primary source
        if (forceRestart)
        {
            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.loop = musicLoop;
            musicSource.Play();
            musicSourceB.Stop();
        }
        else
        {
            musicSource.clip = clip;
            musicSource.volume = musicVolume;
            musicSource.loop = musicLoop;
            musicSource.Play();
            musicSourceB.Stop();
        }
    }

    IEnumerator CrossfadeTo(AudioSource a, AudioSource b, AudioClip targetClip, float duration)
    {
        // Ensure 'b' will play the target clip while 'a' fades out
        b.clip = targetClip;
        b.loop = musicLoop;
        b.volume = 0f;
        b.Play();

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            a.volume = Mathf.Lerp(musicVolume, 0f, p);
            b.volume = Mathf.Lerp(0f, musicVolume, p);
            yield return null;
        }

        a.Stop();
        a.volume = musicVolume;

        // Swap sources so next crossfade uses the other direction
        AudioSource tmp = musicSource;
        musicSource = musicSourceB;
        musicSourceB = tmp;
        crossfadeCoroutine = null;
    }

    /// <summary>
    /// Stop background music immediately.
    /// </summary>
    public void StopMusic()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }

    /// <summary>
    /// Play a one-shot SFX via the SFX source.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * sfxVolume);
    }

    /// <summary>
    /// Set music volume at runtime (0..1).
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null) musicSource.volume = musicVolume;
    }

    /// <summary>
    /// Set SFX volume at runtime (0..1).
    /// </summary>
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null) sfxSource.volume = sfxVolume;
    }
}
