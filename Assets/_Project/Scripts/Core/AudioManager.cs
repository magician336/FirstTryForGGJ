using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[DefaultExecutionOrder(-90)]
[DisallowMultipleComponent]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("音乐设置")]
    [SerializeField] private AudioMixerGroup musicMixerGroup;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.8f;
    [SerializeField] private float defaultMusicFade = 0.75f;

    [Header("音效设置")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup;
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1.0f;
    [SerializeField] private int sfxSourcePoolSize = 8;
    [SerializeField] private float defaultSfxPitchVariance = 0.0f; // 例如 0.05f

    [Header("音频资源 (按键索引)")]
    [SerializeField] private NamedClip[] musicBank = Array.Empty<NamedClip>();
    [SerializeField] private NamedClip[] sfxBank = Array.Empty<NamedClip>();

    private readonly Dictionary<string, AudioClip> _musicDict = new();
    private readonly Dictionary<string, AudioClip> _sfxDict = new();

    // 两个音乐声源用于跨淡入淡出
    private AudioSource _musicA;
    private AudioSource _musicB;
    private bool _isMusicAActive = true;

    // 音效声源池
    private readonly List<AudioSource> _sfxPool = new();

    public bool IsMuted { get; private set; }

    [Serializable]
    public struct NamedClip
    {
        public string key;
        public AudioClip clip;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildBanks();
        SetupMusicSources();
        SetupSfxPool();
    }

    private void BuildBanks()
    {
        _musicDict.Clear();
        _sfxDict.Clear();
        foreach (var nc in musicBank)
        {
            if (!string.IsNullOrWhiteSpace(nc.key) && nc.clip != null)
                _musicDict[nc.key] = nc.clip;
        }
        foreach (var nc in sfxBank)
        {
            if (!string.IsNullOrWhiteSpace(nc.key) && nc.clip != null)
                _sfxDict[nc.key] = nc.clip;
        }
    }

    private void SetupMusicSources()
    {
        _musicA = CreateChildAudioSource("Music_A", musicMixerGroup, true);
        _musicB = CreateChildAudioSource("Music_B", musicMixerGroup, true);
        _musicA.volume = musicVolume;
        _musicB.volume = 0f;
        _isMusicAActive = true;
    }

    private void SetupSfxPool()
    {
        _sfxPool.Clear();
        for (int i = 0; i < Mathf.Max(1, sfxSourcePoolSize); i++)
        {
            var s = CreateChildAudioSource($"SFX_{i}", sfxMixerGroup, false);
            s.volume = sfxVolume;
            _sfxPool.Add(s);
        }
    }

    private AudioSource CreateChildAudioSource(string name, AudioMixerGroup mixer, bool isMusic)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = isMusic;
        src.spatialBlend = 0f; // 2D 音频
        src.outputAudioMixerGroup = mixer;
        return src;
    }

    // 音乐接口
    public void PlayMusicByKey(string key, float fadeDuration = -1f, bool loop = true)
    {
        if (!_musicDict.TryGetValue(key, out var clip) || clip == null)
            return;
        PlayMusic(clip, fadeDuration, loop);
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = -1f, bool loop = true)
    {
        if (clip == null) return;
        if (fadeDuration < 0f) fadeDuration = defaultMusicFade;

        var active = _isMusicAActive ? _musicA : _musicB;
        var inactive = _isMusicAActive ? _musicB : _musicA;

        inactive.clip = clip;
        inactive.loop = loop;
        inactive.volume = 0f;
        inactive.Play();
        StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(active, inactive, fadeDuration));
        _isMusicAActive = !_isMusicAActive;
    }

    public void StopMusic(float fadeDuration = -1f)
    {
        if (fadeDuration < 0f) fadeDuration = defaultMusicFade;
        var active = _isMusicAActive ? _musicA : _musicB;
        StopAllCoroutines();
        StartCoroutine(FadeOutAndStop(active, fadeDuration));
    }

    private IEnumerator CrossfadeMusic(AudioSource from, AudioSource to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime; // 受暂停影响的淡入淡出用 unscaled
            float k = Mathf.Clamp01(t / duration);
            from.volume = Mathf.Lerp(musicVolume, 0f, k);
            to.volume = Mathf.Lerp(0f, musicVolume, k);
            yield return null;
        }
        from.Stop();
        to.volume = musicVolume;
    }

    private IEnumerator FadeOutAndStop(AudioSource src, float duration)
    {
        float start = src.volume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            src.volume = Mathf.Lerp(start, 0f, k);
            yield return null;
        }
        src.Stop();
        src.clip = null;
        src.volume = musicVolume;
    }

    // 音效接口
    public void PlaySFXByKey(string key, float volumeScale = 1f, float pitchVariance = float.NaN)
    {
        if (!_sfxDict.TryGetValue(key, out var clip) || clip == null)
            return;
        PlaySFX(clip, volumeScale, pitchVariance);
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f, float pitchVariance = float.NaN)
    {
        if (clip == null) return;
        var src = GetAvailableSfxSource();
        ConfigureSfxSource(src, volumeScale, pitchVariance);
        src.clip = clip;
        src.Play();
    }

    public void PlaySFXAt(AudioClip clip, Vector3 position, float volumeScale = 1f, float pitchVariance = float.NaN)
    {
        if (clip == null) return;
        var src = GetAvailableSfxSource();
        src.transform.position = position;
        src.spatialBlend = 0f; // 保持 2D；如需 3D，可改为 >0 并配置 rolloff
        ConfigureSfxSource(src, volumeScale, pitchVariance);
        src.clip = clip;
        src.Play();
    }

    private AudioSource GetAvailableSfxSource()
    {
        foreach (var s in _sfxPool)
        {
            if (!s.isPlaying) return s;
        }
        // 若全部占用，复用第一个（避免无声）
        return _sfxPool.Count > 0 ? _sfxPool[0] : CreateChildAudioSource("SFX_Extra", sfxMixerGroup, false);
    }

    private void ConfigureSfxSource(AudioSource src, float volumeScale, float pitchVariance)
    {
        float pv = float.IsNaN(pitchVariance) ? defaultSfxPitchVariance : pitchVariance;
        src.volume = sfxVolume * Mathf.Clamp01(volumeScale);
        src.pitch = 1f + UnityEngine.Random.Range(-pv, pv);
        src.loop = false;
        src.spatialBlend = 0f;
    }

    // 音量 / 静音
    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        var active = _isMusicAActive ? _musicA : _musicB;
        var inactive = _isMusicAActive ? _musicB : _musicA;
        if (active != null) active.volume = musicVolume;
        if (inactive != null && !inactive.isPlaying) inactive.volume = 0f;
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        foreach (var s in _sfxPool)
        {
            if (!s.isPlaying) s.volume = sfxVolume;
        }
    }

    public void SetMuted(bool muted)
    {
        IsMuted = muted;
        AudioListener.pause = muted; // 简化：静音即暂停所有音频
    }

    public void ToggleMute() => SetMuted(!IsMuted);
}
