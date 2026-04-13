using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("BGM Source")]
    [SerializeField] private AudioSource bgmSource;

    [Header("SFX Source Pool Size")]
    [SerializeField] private int sfxPoolSize = 10;

    [Header("Audio Clips")]
    [SerializeField] private List<AudioClip> audioClips;

    private Dictionary<string, AudioClip> clipDict;
    private List<AudioSource> sfxPool;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitClips();
        InitSFXPool();
    }

    // =========================
    // 初始化音效字典
    // =========================
    private void InitClips()
    {
        clipDict = new Dictionary<string, AudioClip>();

        foreach (var clip in audioClips)
        {
            if (clip != null && !clipDict.ContainsKey(clip.name))
                clipDict.Add(clip.name, clip);
        }
    }

    // =========================
    // 初始化 SFX 池
    // =========================
    private void InitSFXPool()
    {
        sfxPool = new List<AudioSource>();

        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject obj = new GameObject("SFX_Source_" + i);
            obj.transform.SetParent(transform);

            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;

            sfxPool.Add(source);
        }
    }

    // =========================
    // BGM 播放
    // =========================
    public void PlayBGM(string clipName, float volume = 1f, bool loop = true)
    {
        if (!clipDict.TryGetValue(clipName, out AudioClip clip))
        {
            Debug.LogWarning($"BGM not found: {clipName}");
            return;
        }

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = volume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    // =========================
    // SFX 播放（支持并发）
    // =========================
    public void PlaySFX(string clipName, float volume = 1f)
    {
        if (!clipDict.TryGetValue(clipName, out AudioClip clip))
        {
            Debug.LogWarning($"SFX not found: {clipName}");
            return;
        }

        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.volume = volume;
        source.loop = false;
        source.Play();
    }

    // =========================
    // 找一个空闲 AudioSource
    // =========================
    private AudioSource GetAvailableSource()
    {
        foreach (var s in sfxPool)
        {
            if (!s.isPlaying)
                return s;
        }

        // 如果都在播放，就强制复用第一个（避免爆池）
        return sfxPool[0];
    }

    // =========================
    // 直接播放（不依赖名字）
    // =========================
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        AudioSource source = GetAvailableSource();
        source.PlayOneShot(clip, volume);
    }
}