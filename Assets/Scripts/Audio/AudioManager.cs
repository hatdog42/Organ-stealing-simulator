using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private const string MusicVolumeKey = "Audio.MusicVolume";
    private const string SfxVolumeKey = "Audio.SfxVolume";

    [Header("Default Volumes")]
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.5f;
    [SerializeField] private bool rememberVolumeSettings = true;

    [Header("UI Sounds")]
    [SerializeField] private AudioClip defaultButtonClickSound;
    [SerializeField, Range(0f, 1f)] private float defaultButtonClickVolume = 1f;
    [SerializeField] private bool addClickSoundToButtons = true;

    private readonly Dictionary<AudioSource, SourceSettings> _sources = new();
    private AudioSource _musicSource;
    private AudioSource _oneShotSource;
    private Coroutine _musicFadeRoutine;
    private float _currentMusicBaseVolume = 1f;
    private bool _musicPaused;

    public static AudioManager Instance { get; private set; }

    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void BootstrapFallback()
    {
        if (Instance) return;

        GameObject audioManager = new("AudioManager");
        audioManager.AddComponent<AudioManager>();
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

        MusicVolume = rememberVolumeSettings
            ? PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume)
            : defaultMusicVolume;

        SfxVolume = rememberVolumeSettings
            ? PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume)
            : defaultSfxVolume;

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;

        _oneShotSource = gameObject.AddComponent<AudioSource>();
        _oneShotSource.playOnAwake = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
        RegisterSceneSources();
        RegisterSceneButtons();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterSceneSources();
        RegisterSceneButtons();
        ApplyVolumes();
    }

    public void SetMusicVolume(float volume)
    {
        MusicVolume = Mathf.Clamp01(volume);
        SaveVolume(MusicVolumeKey, MusicVolume);
        ApplyVolumes();
    }

    public void SetSfxVolume(float volume)
    {
        SfxVolume = Mathf.Clamp01(volume);
        SaveVolume(SfxVolumeKey, SfxVolume);
        ApplyVolumes();
    }

    [ContextMenu("Reset Saved Volumes")]
    public void ResetSavedVolumes()
    {
        PlayerPrefs.DeleteKey(MusicVolumeKey);
        PlayerPrefs.DeleteKey(SfxVolumeKey);
        MusicVolume = defaultMusicVolume;
        SfxVolume = defaultSfxVolume;
        ApplyVolumes();
    }

    public void RegisterSource(AudioSource source, AudioChannelType channelType)
    {
        RegisterSource(source, channelType, source ? source.volume : 1f);
    }

    public void RegisterSource(AudioSource source, AudioChannelType channelType, float baseVolume)
    {
        if (!source) return;

        if (!_sources.ContainsKey(source))
        {
            _sources.Add(source, new SourceSettings(Mathf.Clamp01(baseVolume), channelType));
        }
        else
        {
            _sources[source] = new SourceSettings(Mathf.Clamp01(baseVolume), channelType);
        }

        ApplyVolume(source);
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (!clip) return;

        _oneShotSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale) * SfxVolume);
    }

    public void PlayButtonClick(AudioClip overrideClip = null, float volumeScale = 1f)
    {
        AudioClip clip = overrideClip ? overrideClip : defaultButtonClickSound;
        PlaySfx(clip, defaultButtonClickVolume * volumeScale);
    }

    public void PauseMusic()
    {
        if (!_musicSource || !_musicSource.isPlaying) return;

        _musicSource.Pause();
        _musicPaused = true;
    }

    public void ResumeMusic()
    {
        if (!_musicSource || !_musicPaused) return;

        _musicSource.UnPause();
        _musicPaused = false;
    }

    public void PlayMusic(AudioClip clip, float baseVolume = 1f, bool loop = true)
    {
        if (!clip) return;

        StopMusicFade();
        _musicPaused = false;
        _currentMusicBaseVolume = baseVolume;
        _musicSource.loop = loop;

        if (_musicSource.clip == clip && _musicSource.isPlaying)
        {
            ApplyMusicVolume();
            return;
        }

        _musicSource.clip = clip;
        _musicSource.time = 0f;
        ApplyMusicVolume();
        _musicSource.Play();
    }

    public void FadeOutMusic(float duration)
    {
        if (!_musicSource || !_musicSource.isPlaying) return;

        StopMusicFade();
        _musicFadeRoutine = StartCoroutine(FadeOutMusicRoutine(duration));
    }

    private void RegisterSceneSources()
    {
        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
        {
            if (!source || IsManagerSource(source) || _sources.ContainsKey(source)) continue;

            AudioChannel channel = source.GetComponent<AudioChannel>();
            AudioChannelType channelType = channel ? channel.ChannelType : GuessChannelType(source);

            if (channelType == AudioChannelType.Music)
            {
                PlayMusic(source.clip, source.volume, source.loop);
                source.Stop();
                source.playOnAwake = false;
                continue;
            }

            _sources.Add(source, new SourceSettings(source.volume, channelType));
        }
    }

    private void RegisterSceneButtons()
    {
        if (!addClickSoundToButtons) return;

        foreach (Button button in FindObjectsByType<Button>(FindObjectsInactive.Include))
        {
            if (!button || button.GetComponent<ButtonClickSound>()) continue;

            button.gameObject.AddComponent<ButtonClickSound>();
        }
    }

    private static AudioChannelType GuessChannelType(AudioSource source)
    {
        string sourceName = source.name.ToLowerInvariant();
        string clipName = source.clip ? source.clip.name.ToLowerInvariant() : string.Empty;

        if (sourceName.Contains("sfx") || sourceName.Contains("sound") || sourceName.Contains("alarm"))
        {
            return AudioChannelType.Sfx;
        }

        if (clipName.Contains("sfx") || clipName.Contains("sound") || clipName.Contains("alarm"))
        {
            return AudioChannelType.Sfx;
        }

        if (sourceName.Contains("music") || sourceName.Contains("theme")) return AudioChannelType.Music;
        if (clipName.Contains("music") || clipName.Contains("theme")) return AudioChannelType.Music;
        if (sourceName.Contains("conflicted") || sourceName.Contains("waiting")) return AudioChannelType.Music;
        if (clipName.Contains("conflicted") || clipName.Contains("waiting")) return AudioChannelType.Music;
        if (sourceName.Contains("gameplay") || sourceName.Contains("menu")) return AudioChannelType.Music;
        if (clipName.Contains("gameplay") || clipName.Contains("menu")) return AudioChannelType.Music;

        return AudioChannelType.Sfx;
    }

    private void SaveVolume(string key, float volume)
    {
        if (!rememberVolumeSettings) return;

        PlayerPrefs.SetFloat(key, volume);
        PlayerPrefs.Save();
    }

    private void ApplyVolumes()
    {
        CleanupMissingSources();

        foreach (AudioSource source in _sources.Keys)
        {
            ApplyVolume(source);
        }

        ApplyMusicVolume();
    }

    private void ApplyVolume(AudioSource source)
    {
        if (!source || !_sources.TryGetValue(source, out SourceSettings settings)) return;

        float channelVolume = settings.ChannelType == AudioChannelType.Music ? MusicVolume : SfxVolume;
        source.volume = settings.BaseVolume * channelVolume;
    }

    private void ApplyMusicVolume()
    {
        if (!_musicSource) return;

        _musicSource.volume = _currentMusicBaseVolume * MusicVolume;
    }

    private IEnumerator FadeOutMusicRoutine(float duration)
    {
        float startVolume = _musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            _musicSource.volume = Mathf.Lerp(startVolume, 0f, progress);
            yield return null;
        }

        _musicSource.volume = 0f;
        _musicSource.Stop();
        _musicFadeRoutine = null;
    }

    private void StopMusicFade()
    {
        if (_musicFadeRoutine == null) return;

        StopCoroutine(_musicFadeRoutine);
        _musicFadeRoutine = null;
    }

    private bool IsManagerSource(AudioSource source)
    {
        return source == _musicSource || source == _oneShotSource;
    }

    private void CleanupMissingSources()
    {
        List<AudioSource> missingSources = null;

        foreach (AudioSource source in _sources.Keys)
        {
            if (source) continue;

            missingSources ??= new List<AudioSource>();
            missingSources.Add(source);
        }

        if (missingSources == null) return;

        foreach (AudioSource source in missingSources)
        {
            _sources.Remove(source);
        }
    }

    private struct SourceSettings
    {
        public readonly float BaseVolume;
        public AudioChannelType ChannelType;

        public SourceSettings(float baseVolume, AudioChannelType channelType)
        {
            BaseVolume = baseVolume;
            ChannelType = channelType;
        }
    }
}
