using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class AudioManager : MonoBehaviour
{
    private const string DefaultSoundBankResourcePath = "DefaultSoundBank";
    private const string MusicVolumeKey = "Audio.MusicVolume";
    private const string SfxVolumeKey = "Audio.SfxVolume";
    private const string SoundVolumeKeyPrefix = "Audio.Sound.";

    [Header("Sound Bank")]
    [SerializeField] private SoundBank soundBank;

    [Header("Default Volumes")]
    [SerializeField, Range(0f, 1f)] private float defaultMusicVolume = 0.5f;
    [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.5f;
    [SerializeField] private bool rememberVolumeSettings = true;

    [Header("UI Sounds")]
    [SerializeField] private SoundId defaultButtonClickSoundId = SoundId.ButtonAlt;
    [SerializeField, Range(0f, 1f)] private float defaultButtonClickVolume = 1f;
    [SerializeField] private bool addClickSoundToButtons = true;

    [Header("Dialogue Voices")]
    [SerializeField] private AudioClip[] maleVoiceClips;
    [SerializeField] private AudioClip[] femaleVoiceClips;
    [SerializeField] private AudioClip[] shadImanVoiceClips;
    [SerializeField, Range(0f, 1f)] private float defaultVoiceVolume = 1f;

    [Header("Scene Music")]
    [FormerlySerializedAs("playDefaultSceneMusic")]
    [SerializeField] private bool playSceneMusicFromSoundBank = true;

    [Header("Audio Listeners")]
    [SerializeField] private bool enforceSingleAudioListener = true;

    [Header("Source Priority")]
    [SerializeField, Range(0, 256)] private int musicPriority = 0;
    [SerializeField, Range(0, 256)] private int importantSfxPriority = 32;
    [SerializeField, Range(0, 256)] private int loopingSfxPriority = 48;
    [SerializeField, Range(0, 256)] private int oneShotSfxPriority = 64;
    [SerializeField, Min(1)] private int maxOneShotSources = 64;

    private readonly Dictionary<AudioSource, SourceSettings> _sources = new();
    private readonly Dictionary<SoundId, float> _soundVolumeOverrides = new();
    private readonly List<AudioSource> _oneShotSources = new();
    private AudioSource _musicSource;
    private AudioSource _sfxSource;
    private Coroutine _musicFadeRoutine;
    private SoundId _currentMusicSoundId = SoundId.None;
    private float _currentMusicBaseVolume = 1f;
    private bool _musicPaused;
    private bool _musicShouldKeepPlaying;

    private enum DialogueVoicePool
    {
        MalePatient,
        FemalePatient,
        ShadIman
    }

    public static AudioManager Instance { get; private set; }

    public float MusicVolume { get; private set; }
    public float SfxVolume { get; private set; }
    public SoundBank SoundBank => soundBank;

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

        EnsureSoundBank();
        LoadSavedVolumes();
        CreateOutputSources();
        EnsureSingleAudioListener();

        SceneManager.sceneLoaded += OnSceneLoaded;

        PlaySceneMusic(SceneManager.GetActiveScene());
        RegisterSceneButtons();
        ApplyVolumes();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureSingleAudioListener();
        PlaySceneMusic(scene);
        RegisterSceneButtons();
        ApplyVolumes();
    }

    private void Update()
    {
        KeepExpectedMusicPlaying();
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

    public float GetSoundVolume(SoundId soundId)
    {
        if (soundId == SoundId.None) return 1f;

        return _soundVolumeOverrides.TryGetValue(soundId, out float volume)
            ? volume
            : soundBank ? soundBank.GetVolume(soundId) : 1f;
    }

    public void SetSoundVolume(SoundId soundId, float volume)
    {
        if (soundId == SoundId.None) return;

        volume = Mathf.Clamp01(volume);
        _soundVolumeOverrides[soundId] = volume;

        if (rememberVolumeSettings)
        {
            PlayerPrefs.SetFloat(SoundVolumeKey(soundId), volume);
            PlayerPrefs.Save();
        }

        ApplyVolumes();
    }

    [ContextMenu("Reset Saved Volumes")]
    public void ResetSavedVolumes()
    {
        PlayerPrefs.DeleteKey(MusicVolumeKey);
        PlayerPrefs.DeleteKey(SfxVolumeKey);

        foreach (SoundId soundId in Enum.GetValues(typeof(SoundId)))
        {
            if (soundId == SoundId.None) continue;

            PlayerPrefs.DeleteKey(SoundVolumeKey(soundId));
        }

        _soundVolumeOverrides.Clear();
        MusicVolume = defaultMusicVolume;
        SfxVolume = defaultSfxVolume;
        ApplyVolumes();
    }

    public void RegisterSource(AudioSource source, AudioChannelType channelType)
    {
        RegisterSource(source, channelType, source ? source.volume : 1f, SoundId.None);
    }

    public void RegisterSource(AudioSource source, AudioChannelType channelType, float baseVolume)
    {
        RegisterSource(source, channelType, baseVolume, SoundId.None);
    }

    public void RegisterSource(AudioSource source, AudioChannelType channelType, float baseVolume, SoundId soundId)
    {
        if (!source) return;

        _sources[source] = new SourceSettings(Mathf.Clamp01(baseVolume), channelType, soundId);
        source.priority = channelType == AudioChannelType.Music ? musicPriority : SfxPriority(soundId, source.loop);
        ApplyVolume(source);
    }

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        PlayOneShot(clip, SoundId.None, volumeScale, 1f);
    }

    public AudioClip GetRandomPatientVoice(string patientSex)
    {
        if (IsMaleSex(patientSex)) return GetRandomVoiceClip(DialogueVoicePool.MalePatient);
        if (IsFemaleSex(patientSex)) return GetRandomVoiceClip(DialogueVoicePool.FemalePatient);

        return GetRandomVoiceClip(
            UnityEngine.Random.value < 0.5f
                ? DialogueVoicePool.MalePatient
                : DialogueVoicePool.FemalePatient);
    }

    public AudioClip GetRandomShadImanVoice()
    {
        return GetRandomVoiceClip(DialogueVoicePool.ShadIman);
    }

    public bool PlayPatientVoice(string patientSex, AudioClip voiceClip = null, float volumeScale = 1f)
    {
        return PlayVoice(voiceClip ? voiceClip : GetRandomPatientVoice(patientSex), volumeScale);
    }

    public bool PlayShadImanVoice(float volumeScale = 1f)
    {
        return PlayVoice(GetRandomShadImanVoice(), volumeScale);
    }

    public bool PlayVoice(AudioClip voiceClip, float volumeScale = 1f)
    {
        return PlayOneShot(voiceClip, SoundId.None, defaultVoiceVolume * volumeScale, 1f);
    }

    public bool PlaySfx(SoundId soundId, float volumeScale = 1f, float pitchScale = 1f)
    {
        if (!TryGetSound(soundId, out SoundBankEntry sound)) return false;

        if (sound.ChannelType == AudioChannelType.Music)
        {
            return PlayMusic(soundId, volumeScale);
        }

        return PlayOneShot(sound.Clip, soundId, volumeScale, sound.Pitch * pitchScale);
    }

    public bool PlaySfxOnSource(AudioSource source, SoundId soundId, float volumeScale = 1f, float pitchScale = 1f)
    {
        if (!source || !TryGetSound(soundId, out SoundBankEntry sound)) return false;

        if (!_sources.TryGetValue(source, out SourceSettings settings) || settings.SoundId != soundId)
        {
            RegisterSource(source, AudioChannelType.Sfx, 1f, soundId);
        }

        source.priority = SfxPriority(soundId, source.loop);
        source.clip = sound.Clip;
        source.pitch = Mathf.Max(0f, sound.Pitch * pitchScale);
        ApplyVolume(source, Mathf.Clamp01(volumeScale));
        source.Play();
        return true;
    }

    public void PlayButtonClick(float volumeScale = 1f)
    {
        PlayButtonClick(SoundId.None, volumeScale);
    }

    public void PlayButtonClick(SoundId overrideSoundId, float volumeScale = 1f)
    {
        SoundId soundId = overrideSoundId == SoundId.None ? defaultButtonClickSoundId : overrideSoundId;
        PlaySfx(soundId, defaultButtonClickVolume * volumeScale);
    }

    public void PlayButtonClick(AudioClip overrideClip, float volumeScale = 1f)
    {
        PlaySfx(overrideClip, defaultButtonClickVolume * volumeScale);
    }

    public AudioSource CreateSfxSource(
        SoundId soundId,
        Transform parent = null,
        bool loop = false,
        float baseVolume = 1f,
        float spatialBlend = 0f)
    {
        AudioSource source = CreateManagedAudioSource(SourceName(soundId), parent, loop, spatialBlend);

        if (soundId == SoundId.None)
        {
            RegisterSource(source, AudioChannelType.Sfx, baseVolume);
            return source;
        }

        ConfigureSfxSource(source, soundId, baseVolume, loop, 1f, spatialBlend);
        return source;
    }

    public AudioSource CreateSfxSource(
        string sourceName,
        Transform parent = null,
        bool loop = false,
        float baseVolume = 1f,
        float spatialBlend = 0f)
    {
        AudioSource source = CreateManagedAudioSource(sourceName, parent, loop, spatialBlend);
        RegisterSource(source, AudioChannelType.Sfx, baseVolume);
        return source;
    }

    public bool ConfigureSfxSource(
        AudioSource source,
        SoundId soundId,
        float baseVolume = 1f,
        bool loop = false,
        float pitchScale = 1f,
        float spatialBlend = 0f)
    {
        if (!source || !TryGetSound(soundId, out SoundBankEntry sound)) return false;

        source.playOnAwake = false;
        source.loop = loop || sound.Loop;
        source.clip = sound.Clip;
        source.pitch = Mathf.Max(0f, sound.Pitch * pitchScale);
        source.spatialBlend = Mathf.Clamp01(spatialBlend);
        source.priority = SfxPriority(soundId, source.loop);

        RegisterSource(source, AudioChannelType.Sfx, baseVolume, soundId);
        return true;
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

    public void StopMusic()
    {
        StopMusicFade();
        _musicPaused = false;
        _musicShouldKeepPlaying = false;
        _currentMusicSoundId = SoundId.None;

        if (_musicSource)
        {
            _musicSource.Stop();
        }
    }

    public bool PlayMusic(SoundId soundId, float volumeScale = 1f, bool? loopOverride = null)
    {
        if (!TryGetSound(soundId, out SoundBankEntry sound)) return false;

        StopMusicFade();
        _musicPaused = false;
        _musicShouldKeepPlaying = true;
        _currentMusicSoundId = soundId;
        _currentMusicBaseVolume = Mathf.Clamp01(volumeScale);
        _musicSource.loop = loopOverride ?? sound.Loop;
        _musicSource.priority = musicPriority;

        if (_musicSource.clip == sound.Clip && _musicSource.isPlaying)
        {
            ApplyMusicVolume();
            return true;
        }

        _musicSource.clip = sound.Clip;
        _musicSource.time = 0f;
        ApplyMusicVolume();
        _musicSource.Play();
        return true;
    }

    public void PlayMusic(AudioClip clip, float baseVolume = 1f, bool loop = true)
    {
        if (!clip || !_musicSource) return;

        StopMusicFade();
        _musicPaused = false;
        _musicShouldKeepPlaying = true;
        _currentMusicSoundId = SoundId.None;
        _currentMusicBaseVolume = Mathf.Clamp01(baseVolume);
        _musicSource.loop = loop;
        _musicSource.priority = musicPriority;

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
        _musicShouldKeepPlaying = false;
        _musicFadeRoutine = StartCoroutine(FadeOutMusicRoutine(duration));
    }

    private void EnsureSoundBank()
    {
        if (soundBank) return;

        soundBank = Resources.Load<SoundBank>(DefaultSoundBankResourcePath);
    }

    private void LoadSavedVolumes()
    {
        MusicVolume = rememberVolumeSettings
            ? PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume)
            : defaultMusicVolume;

        SfxVolume = rememberVolumeSettings
            ? PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume)
            : defaultSfxVolume;

        if (!rememberVolumeSettings) return;

        foreach (SoundId soundId in Enum.GetValues(typeof(SoundId)))
        {
            if (soundId == SoundId.None) continue;

            string key = SoundVolumeKey(soundId);
            if (PlayerPrefs.HasKey(key))
            {
                _soundVolumeOverrides[soundId] = PlayerPrefs.GetFloat(key);
            }
        }
    }

    private void CreateOutputSources()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.loop = true;
        _musicSource.priority = musicPriority;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;
        _sfxSource.spatialBlend = 0f;
        _sfxSource.priority = oneShotSfxPriority;

        _oneShotSources.Add(_sfxSource);
    }

    private AudioSource CreateManagedAudioSource(string sourceName, Transform parent, bool loop, float spatialBlend)
    {
        GameObject sourceObject = new(string.IsNullOrWhiteSpace(sourceName) ? "Audio Source" : sourceName);
        sourceObject.transform.SetParent(parent ? parent : transform, false);

        AudioSource source = sourceObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = loop;
        source.spatialBlend = Mathf.Clamp01(spatialBlend);
        source.priority = oneShotSfxPriority;
        return source;
    }

    private bool PlayOneShot(AudioClip clip, SoundId soundId, float volumeScale, float pitch)
    {
        if (!clip) return false;

        AudioSource source = GetAvailableOneShotSource();
        if (!source) return false;

        source.Stop();
        source.playOnAwake = false;
        source.loop = false;
        source.clip = clip;
        source.pitch = Mathf.Max(0f, pitch);
        source.spatialBlend = 0f;
        source.priority = oneShotSfxPriority;

        RegisterSource(source, AudioChannelType.Sfx, volumeScale, soundId);
        source.Play();
        return true;
    }

    private AudioSource GetAvailableOneShotSource()
    {
        foreach (AudioSource source in _oneShotSources)
        {
            if (source && !source.isPlaying) return source;
        }

        if (_oneShotSources.Count >= maxOneShotSources) return null;

        AudioSource newSource = CreateManagedAudioSource("One Shot SFX Source", transform, false, 0f);
        _oneShotSources.Add(newSource);
        return newSource;
    }

    private void PlaySceneMusic(Scene scene)
    {
        if (!playSceneMusicFromSoundBank || !soundBank) return;
        if (!soundBank.TryGetSceneMusic(scene.name, scene.path, out SceneMusicEntry cue)) return;

        if (cue.StopCurrentMusic)
        {
            StopMusic();
            return;
        }

        if (cue.Music == SoundId.None) return;

        PlayMusic(cue.Music, cue.Volume, cue.Loop);
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

    private void EnsureSingleAudioListener()
    {
        if (!enforceSingleAudioListener) return;

        AudioListener[] listeners = FindObjectsByType<AudioListener>(FindObjectsInactive.Include);
        if (listeners.Length == 0) return;

        AudioListener preferredListener = FindPreferredAudioListener(listeners);
        foreach (AudioListener listener in listeners)
        {
            if (!listener) continue;

            bool shouldEnable = listener == preferredListener && listener.gameObject.activeInHierarchy;
            if (listener.enabled != shouldEnable)
            {
                listener.enabled = shouldEnable;
            }
        }
    }

    private static AudioListener FindPreferredAudioListener(AudioListener[] listeners)
    {
        AudioListener preferredListener = FindMainOutputListener(listeners, requireEnabled: true);
        if (preferredListener) return preferredListener;

        foreach (AudioListener listener in listeners)
        {
            if (listener && listener.enabled && IsUsableOutputListener(listener)) return listener;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera
            && !mainCamera.targetTexture
            && mainCamera.TryGetComponent(out AudioListener mainListener)
            && IsUsableOutputListener(mainListener))
        {
            return mainListener;
        }

        preferredListener = FindMainOutputListener(listeners, requireEnabled: false);
        if (preferredListener) return preferredListener;

        foreach (AudioListener listener in listeners)
        {
            if (IsUsableOutputListener(listener)) return listener;
        }

        foreach (AudioListener listener in listeners)
        {
            if (listener && listener.gameObject.activeInHierarchy) return listener;
        }

        return listeners.Length > 0 ? listeners[0] : null;
    }

    private static AudioListener FindMainOutputListener(AudioListener[] listeners, bool requireEnabled)
    {
        foreach (AudioListener listener in listeners)
        {
            if (requireEnabled && (!listener || !listener.enabled)) continue;
            if (!IsUsableOutputListener(listener)) continue;
            if (listener.TryGetComponent(out Camera camera) && camera.CompareTag("MainCamera")) return listener;
        }

        return null;
    }

    private static bool IsUsableOutputListener(AudioListener listener)
    {
        if (!listener || !listener.gameObject.activeInHierarchy) return false;

        return !listener.TryGetComponent(out Camera camera) || !camera.targetTexture;
    }

    private bool TryGetSound(SoundId soundId, out SoundBankEntry sound)
    {
        EnsureSoundBank();
        sound = null;

        if (soundId == SoundId.None || !soundBank) return false;

        return soundBank.TryGetSound(soundId, out sound);
    }

    private AudioClip GetRandomVoiceClip(DialogueVoicePool pool)
    {
        AudioClip[] directPool = pool switch
        {
            DialogueVoicePool.MalePatient => maleVoiceClips,
            DialogueVoicePool.FemalePatient => femaleVoiceClips,
            DialogueVoicePool.ShadIman => shadImanVoiceClips,
            _ => null
        };

        if (TryGetRandomClip(directPool, out AudioClip clip)) return clip;

        return GetRandomSoundBankVoiceClip(pool);
    }

    private AudioClip GetRandomSoundBankVoiceClip(DialogueVoicePool pool)
    {
        EnsureSoundBank();
        if (!soundBank) return null;

        AudioClip selectedClip = null;
        int matchingClipCount = 0;

        foreach (SoundBankEntry entry in soundBank.Entries)
        {
            if (entry == null || entry.ChannelType != AudioChannelType.Sfx || !entry.Clip) continue;
            if (!IsVoicePoolClip(entry.Clip.name, pool)) continue;

            matchingClipCount++;
            if (UnityEngine.Random.Range(0, matchingClipCount) == 0)
            {
                selectedClip = entry.Clip;
            }
        }

        return selectedClip;
    }

    private static bool TryGetRandomClip(AudioClip[] clips, out AudioClip clip)
    {
        clip = null;
        if (clips == null || clips.Length == 0) return false;

        int validClipCount = 0;
        foreach (AudioClip candidate in clips)
        {
            if (candidate) validClipCount++;
        }

        if (validClipCount == 0) return false;

        int targetIndex = UnityEngine.Random.Range(0, validClipCount);
        foreach (AudioClip candidate in clips)
        {
            if (!candidate) continue;
            if (targetIndex-- != 0) continue;

            clip = candidate;
            return true;
        }

        return false;
    }

    private static bool IsVoicePoolClip(string clipName, DialogueVoicePool pool)
    {
        string normalizedName = NormalizeVoiceName(clipName);
        bool isShadImanVoice = normalizedName.Contains("shadimanvoice");
        bool isFemaleVoice = normalizedName.Contains("femalevoice")
                             || normalizedName.Contains("femalevoise")
                             || normalizedName.Contains("femalevoize");
        bool isMaleVoice = !isFemaleVoice
                           && (normalizedName.Contains("malevoice")
                               || normalizedName.Contains("malevoise")
                               || normalizedName.Contains("malevoize")
                               || normalizedName.Contains("mlaevoice"));

        return pool switch
        {
            DialogueVoicePool.MalePatient => isMaleVoice,
            DialogueVoicePool.FemalePatient => isFemaleVoice,
            DialogueVoicePool.ShadIman => isShadImanVoice,
            _ => false
        };
    }

    private static string NormalizeVoiceName(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        char[] buffer = new char[value.Length];
        int count = 0;

        foreach (char character in value)
        {
            if (!char.IsLetterOrDigit(character)) continue;

            buffer[count] = char.ToLowerInvariant(character);
            count++;
        }

        return new string(buffer, 0, count);
    }

    private static bool IsMaleSex(string sex)
    {
        return string.Equals(sex, "M", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sex, "Male", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsFemaleSex(string sex)
    {
        return string.Equals(sex, "F", StringComparison.OrdinalIgnoreCase)
               || string.Equals(sex, "Female", StringComparison.OrdinalIgnoreCase);
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

    private void ApplyVolume(AudioSource source, float volumeScale = 1f)
    {
        if (!source || !_sources.TryGetValue(source, out SourceSettings settings)) return;

        float channelVolume = settings.ChannelType == AudioChannelType.Music ? MusicVolume : SfxVolume;
        float soundVolume = settings.SoundId == SoundId.None ? 1f : GetSoundVolume(settings.SoundId);
        source.volume = settings.BaseVolume * soundVolume * channelVolume * volumeScale;
    }

    private void ApplyMusicVolume()
    {
        if (!_musicSource) return;

        float soundVolume = _currentMusicSoundId == SoundId.None ? 1f : GetSoundVolume(_currentMusicSoundId);
        _musicSource.volume = _currentMusicBaseVolume * soundVolume * MusicVolume;
    }

    private void KeepExpectedMusicPlaying()
    {
        if (!_musicShouldKeepPlaying || _musicPaused || _musicFadeRoutine != null) return;
        if (!_musicSource || !_musicSource.clip || _musicSource.isPlaying) return;
        if (!_musicSource.loop) return;

        ApplyMusicVolume();
        _musicSource.Play();
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
        _currentMusicSoundId = SoundId.None;
        _musicShouldKeepPlaying = false;
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
        return source == _musicSource
               || source == _sfxSource
               || source.transform.IsChildOf(transform);
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

    private static string SoundVolumeKey(SoundId soundId)
    {
        return SoundVolumeKeyPrefix + soundId;
    }

    private static string SourceName(SoundId soundId)
    {
        return soundId == SoundId.None ? "SFX Source" : $"{soundId} SFX Source";
    }

    private int SfxPriority(SoundId soundId, bool loop)
    {
        if (IsImportantSfx(soundId)) return importantSfxPriority;
        return loop ? loopingSfxPriority : oneShotSfxPriority;
    }

    private static bool IsImportantSfx(SoundId soundId)
    {
        return soundId == SoundId.Alarm
               || soundId == SoundId.Flatline
               || soundId == SoundId.Oxygen;
    }

    private struct SourceSettings
    {
        public readonly float BaseVolume;
        public readonly AudioChannelType ChannelType;
        public readonly SoundId SoundId;

        public SourceSettings(float baseVolume, AudioChannelType channelType, SoundId soundId)
        {
            BaseVolume = baseVolume;
            ChannelType = channelType;
            SoundId = soundId;
        }
    }

}
