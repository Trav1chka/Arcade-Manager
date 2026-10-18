using System.Collections;
using UnityEngine;

// Общий менеджер звука. Повесьте на один объект сцены (например, AudioManager).
// Другие скрипты вызывают его через статические методы, например AudioManager.PlayNextDay().
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Звуки")]
    [SerializeField] private AudioClip guestArrivedClip;       // гость пришёл к кассе
    [SerializeField] private AudioClip machinePurchasedClip;   // куплен автомат
    [SerializeField] private AudioClip nextDayClip;            // нажата кнопка следующего дня
    [SerializeField] private AudioClip replyNegativeClip;      // выбран негативный ответ
    [SerializeField] private AudioClip replyNeutralClip;       // выбран нейтральный ответ
    [SerializeField] private AudioClip replyPositiveClip;      // выбран позитивный ответ

    [Header("Музыка")]
    [SerializeField] private AudioClip dayMusic;               // песня, пока идёт день (повторяется)
    [SerializeField] private float fadeTime = 1f;              // плавное включение и выключение, секунд

    [Header("Громкость")]
    [Range(0f, 1f)] [SerializeField] private float sfxVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;

    private AudioSource sfxSource;
    private AudioSource musicSource;
    private Coroutine musicRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
    }

    // ---------- Статические методы, которые вызывают другие скрипты ----------

    public static void PlayGuestArrived()
    {
        if (Instance != null) Instance.PlaySfx(Instance.guestArrivedClip);
    }

    public static void PlayMachinePurchased()
    {
        if (Instance != null) Instance.PlaySfx(Instance.machinePurchasedClip);
    }

    public static void PlayNextDay()
    {
        if (Instance != null) Instance.PlaySfx(Instance.nextDayClip);
    }

    public static void PlayReply(ReplyType type)
    {
        if (Instance == null) return;

        switch (type)
        {
            case ReplyType.Negative: Instance.PlaySfx(Instance.replyNegativeClip); break;
            case ReplyType.Neutral: Instance.PlaySfx(Instance.replyNeutralClip); break;
            case ReplyType.Positive: Instance.PlaySfx(Instance.replyPositiveClip); break;
        }
    }

    public static void PlayDayMusic()
    {
        if (Instance != null) Instance.StartMusic();
    }

    public static void StopDayMusic()
    {
        if (Instance != null) Instance.EndMusic();
    }

    // ---------- Внутренняя логика ----------

    private void PlaySfx(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    private void StartMusic()
    {
        if (dayMusic == null) return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);

        musicSource.clip = dayMusic;
        musicSource.volume = 0f;
        musicSource.Play();

        musicRoutine = StartCoroutine(FadeMusic(musicVolume, false));
    }

    private void EndMusic()
    {
        if (!musicSource.isPlaying) return;

        if (musicRoutine != null) StopCoroutine(musicRoutine);
        musicRoutine = StartCoroutine(FadeMusic(0f, true));
    }

    private IEnumerator FadeMusic(float target, bool stopAtEnd)
    {
        float start = musicSource.volume;
        float t = 0f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }

        musicSource.volume = target;
        if (stopAtEnd) musicSource.Stop();

        musicRoutine = null;
    }
}
