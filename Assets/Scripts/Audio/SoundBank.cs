using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "SoundBank", menuName = "Scriptable Objects/Audio/Sound Bank")]
public class SoundBank : ScriptableObject, ISoundBank
{
    private const string MusicFolder = "Assets/music";
    private const string SfxFolder = "Assets/Sfx";

    [SerializeField] private bool autoPopulateWhenEmpty = true;

    [Header("Music")]
    [SerializeField] private List<SoundBankEntry> musicEntries = new();

    [Header("SFX")]
    [SerializeField] private List<SoundBankEntry> sfxEntries = new();

    [Header("Scene Music")]
    [SerializeField] private List<SceneMusicEntry> sceneMusicEntries = new();

    [SerializeField, HideInInspector] private List<SoundBankEntry> entries = new();

    private readonly Dictionary<SoundId, SoundBankEntry> _lookup = new();
    private readonly List<SoundBankEntry> _allEntries = new();

    public IReadOnlyList<SoundBankEntry> Entries
    {
        get
        {
            EnsureLookup();
            return _allEntries;
        }
    }

    public IReadOnlyList<SceneMusicEntry> SceneMusicEntries => sceneMusicEntries;

    private void OnEnable()
    {
#if UNITY_EDITOR
        MigrateLegacyEntries();
#endif
        RebuildLookup();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        MigrateLegacyEntries();

        if (autoPopulateWhenEmpty && musicEntries.Count == 0 && sfxEntries.Count == 0)
        {
            PopulateFromDefaultFolders();
        }

        ClampEntryVolumes(musicEntries);
        ClampEntryVolumes(sfxEntries);
        RebuildLookup();
    }

    public void PopulateFromDefaultFolders(bool replaceExisting = false)
    {
        if (replaceExisting)
        {
            musicEntries.Clear();
            sfxEntries.Clear();
        }

        AddClipsFromFolder(MusicFolder, AudioChannelType.Music, musicEntries);
        AddClipsFromFolder(SfxFolder, AudioChannelType.Sfx, sfxEntries);
        RebuildLookup();
        EditorUtility.SetDirty(this);
    }

    public void SyncSceneMusicFromBuildSettings(bool removeScenesNotInBuild = true)
    {
        sceneMusicEntries ??= new List<SceneMusicEntry>();
        List<SceneMusicEntry> orderedEntries = new();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!scene.enabled) continue;

            string scenePath = scene.path;
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            SceneMusicEntry entry = FindSceneMusicEntry(scenePath, sceneName) ?? new SceneMusicEntry(scenePath, sceneName);
            entry.SetScene(scenePath, sceneName);
            orderedEntries.Add(entry);
        }

        if (!removeScenesNotInBuild)
        {
            foreach (SceneMusicEntry entry in sceneMusicEntries)
            {
                if (entry == null || ContainsSceneEntry(orderedEntries, entry)) continue;
                orderedEntries.Add(entry);
            }
        }

        sceneMusicEntries = orderedEntries;
        EditorUtility.SetDirty(this);
    }

    public void ImportSceneMusicFromBuildSettings(bool removeScenesNotInBuild = true)
    {
        SyncSceneMusicFromBuildSettings(removeScenesNotInBuild);

        Dictionary<string, SoundId> musicByGuid = BuildMusicGuidLookup();
        foreach (SceneMusicEntry entry in sceneMusicEntries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.ScenePath)) continue;
            if (!TryFindSceneMusic(entry.ScenePath, musicByGuid, out ImportedSceneMusic importedMusic)) continue;

            entry.SetMusic(importedMusic.Music, importedMusic.Volume, importedMusic.Loop);
        }

        EditorUtility.SetDirty(this);
    }
#endif

    public bool TryGetSound(SoundId id, out SoundBankEntry sound)
    {
        EnsureLookup();
        return _lookup.TryGetValue(id, out sound) && sound != null && sound.Clip;
    }

    public bool TryGetSceneMusic(string sceneName, string scenePath, out SceneMusicEntry sceneMusic)
    {
        if (sceneMusicEntries == null)
        {
            sceneMusic = null;
            return false;
        }

        foreach (SceneMusicEntry entry in sceneMusicEntries)
        {
            if (entry == null || !entry.MatchesScene(sceneName, scenePath)) continue;

            sceneMusic = entry;
            return true;
        }

        sceneMusic = null;
        return false;
    }

    public float GetVolume(SoundId id)
    {
        return TryGetEntry(id, out SoundBankEntry sound) ? sound.Volume : 1f;
    }

    public void SetVolume(SoundId id, float volume)
    {
        if (!TryGetEntry(id, out SoundBankEntry sound)) return;

        sound.SetVolume(volume);
    }

    private bool TryGetEntry(SoundId id, out SoundBankEntry sound)
    {
        EnsureLookup();
        return _lookup.TryGetValue(id, out sound) && sound != null;
    }

    private void EnsureLookup()
    {
        if (_allEntries.Count == musicEntries.Count + sfxEntries.Count) return;

        RebuildLookup();
    }

    private void RebuildLookup()
    {
        _lookup.Clear();
        _allEntries.Clear();

        AddEntriesToLookup(musicEntries);
        AddEntriesToLookup(sfxEntries);
    }

    private void AddEntriesToLookup(List<SoundBankEntry> sourceEntries)
    {
        foreach (SoundBankEntry entry in sourceEntries)
        {
            if (entry == null) continue;

            _allEntries.Add(entry);
            if (entry.Id != SoundId.None)
            {
                _lookup[entry.Id] = entry;
            }
        }
    }

#if UNITY_EDITOR
    private void MigrateLegacyEntries()
    {
        if (entries == null || entries.Count == 0) return;

        foreach (SoundBankEntry entry in entries)
        {
            if (entry == null) continue;

            if (entry.ChannelType == AudioChannelType.Music)
            {
                musicEntries.Add(entry);
            }
            else
            {
                sfxEntries.Add(entry);
            }
        }

        entries.Clear();
    }

    private static void ClampEntryVolumes(List<SoundBankEntry> sourceEntries)
    {
        foreach (SoundBankEntry entry in sourceEntries)
        {
            entry?.SetVolume(entry.Volume);
        }
    }

    private void AddClipsFromFolder(string folder, AudioChannelType channelType, List<SoundBankEntry> targetEntries)
    {
        if (!AssetDatabase.IsValidFolder(folder)) return;

        string[] clipGuids = AssetDatabase.FindAssets("t:AudioClip", new[] { folder });
        Array.Sort(clipGuids, CompareAssetPaths);

        foreach (string guid in clipGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if (!clip || HasClip(clip)) continue;

            SoundId soundId = InferSoundId(clip.name);
            targetEntries.Add(new SoundBankEntry(soundId, clip, channelType, ShouldLoop(soundId, channelType)));
        }
    }

    private static int CompareAssetPaths(string leftGuid, string rightGuid)
    {
        return string.Compare(
            AssetDatabase.GUIDToAssetPath(leftGuid),
            AssetDatabase.GUIDToAssetPath(rightGuid),
            StringComparison.OrdinalIgnoreCase);
    }

    private bool HasClip(AudioClip clip)
    {
        return HasClip(musicEntries, clip) || HasClip(sfxEntries, clip);
    }

    private static bool HasClip(List<SoundBankEntry> sourceEntries, AudioClip clip)
    {
        foreach (SoundBankEntry entry in sourceEntries)
        {
            if (entry != null && entry.Clip == clip) return true;
        }

        return false;
    }

    private SceneMusicEntry FindSceneMusicEntry(string scenePath, string sceneName)
    {
        if (sceneMusicEntries == null) return null;

        foreach (SceneMusicEntry entry in sceneMusicEntries)
        {
            if (entry != null && entry.MatchesScene(sceneName, scenePath)) return entry;
        }

        return null;
    }

    private static bool ContainsSceneEntry(List<SceneMusicEntry> entriesToSearch, SceneMusicEntry targetEntry)
    {
        foreach (SceneMusicEntry entry in entriesToSearch)
        {
            if (entry != null && entry.MatchesScene(targetEntry.SceneName, targetEntry.ScenePath)) return true;
        }

        return false;
    }

    private Dictionary<string, SoundId> BuildMusicGuidLookup()
    {
        Dictionary<string, SoundId> musicByGuid = new(StringComparer.OrdinalIgnoreCase);

        foreach (SoundBankEntry entry in musicEntries)
        {
            if (entry == null || entry.Id == SoundId.None || !entry.Clip) continue;

            string clipPath = AssetDatabase.GetAssetPath(entry.Clip);
            string guid = AssetDatabase.AssetPathToGUID(clipPath);
            if (string.IsNullOrWhiteSpace(guid)) continue;

            musicByGuid[guid] = entry.Id;
        }

        return musicByGuid;
    }

    private static bool TryFindSceneMusic(
        string scenePath,
        Dictionary<string, SoundId> musicByGuid,
        out ImportedSceneMusic importedMusic)
    {
        importedMusic = default;
        if (string.IsNullOrWhiteSpace(scenePath) || !File.Exists(scenePath)) return false;

        string sceneText = File.ReadAllText(scenePath);
        Dictionary<string, string> objectNames = ParseGameObjectNames(sceneText);
        ImportedSceneMusic bestMusic = default;
        int bestScore = int.MinValue;

        foreach (Match sourceMatch in Regex.Matches(sceneText, @"--- !u!82 &[^\n\r]+(?<block>[\s\S]*?)(?=\r?\n--- !u!|\z)"))
        {
            string block = sourceMatch.Groups["block"].Value;
            string guid = MatchValue(block, @"m_Resource: \{fileID: 8300000, guid: (?<value>[0-9a-f]+), type: 3\}");
            if (string.IsNullOrWhiteSpace(guid) || !musicByGuid.TryGetValue(guid, out SoundId music)) continue;

            float volume = ParseFloat(MatchValue(block, @"m_Volume: (?<value>[0-9.]+)"), 1f);
            bool loop = MatchValue(block, @"Loop: (?<value>\d+)") == "1";
            bool playOnAwake = MatchValue(block, @"m_PlayOnAwake: (?<value>\d+)") == "1";
            string gameObjectId = MatchValue(block, @"m_GameObject: \{fileID: (?<value>-?\d+)\}");
            objectNames.TryGetValue(gameObjectId, out string gameObjectName);

            int score = ScoreSceneMusicCandidate(gameObjectName, playOnAwake, loop);
            if (score <= bestScore) continue;

            bestScore = score;
            bestMusic = new ImportedSceneMusic(music, volume, loop);
        }

        if (bestScore == int.MinValue) return false;

        importedMusic = bestMusic;
        return true;
    }

    private static Dictionary<string, string> ParseGameObjectNames(string sceneText)
    {
        Dictionary<string, string> objectNames = new();

        foreach (Match objectMatch in Regex.Matches(sceneText, @"--- !u!1 &(?<id>-?\d+)[\s\S]*?\r?\n  m_Name: (?<name>[^\r\n]+)"))
        {
            objectNames[objectMatch.Groups["id"].Value] = objectMatch.Groups["name"].Value;
        }

        return objectNames;
    }

    private static string MatchValue(string text, string pattern)
    {
        Match match = Regex.Match(text, pattern);
        return match.Success ? match.Groups["value"].Value : string.Empty;
    }

    private static float ParseFloat(string value, float fallback)
    {
        return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed)
            ? parsed
            : fallback;
    }

    private static int ScoreSceneMusicCandidate(string objectName, bool playOnAwake, bool loop)
    {
        int score = 0;
        if (playOnAwake) score += 2;
        if (loop) score += 2;
        if (IsSceneMusicName(objectName)) score += 4;
        return score;
    }

    private static bool IsSceneMusicName(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        string normalized = NormalizeName(value);
        return normalized.Contains("music")
               || normalized.Contains("theme")
               || normalized.Contains("waiting")
               || normalized.Contains("conflicted")
               || normalized.Contains("gameplay")
               || normalized.Contains("menu");
    }

    private static bool ShouldLoop(SoundId soundId, AudioChannelType channelType)
    {
        if (channelType == AudioChannelType.Music) return true;

        return soundId == SoundId.Alarm
               || soundId == SoundId.Flatline
               || soundId == SoundId.Oxygen;
    }

    private static SoundId InferSoundId(string clipName)
    {
        string normalizedClipName = NormalizeName(clipName);

        switch (normalizedClipName)
        {
            case "button2":
            case "sfxbutton2":
                return SoundId.ButtonAlt;
            case "swoshblob":
            case "sfxswoshblob":
                return SoundId.SloshBlob;
        }

        foreach (SoundId soundId in Enum.GetValues(typeof(SoundId)))
        {
            if (soundId == SoundId.None) continue;
            if (NormalizeName(soundId.ToString()) == normalizedClipName) return soundId;
            if ("sfx" + NormalizeName(soundId.ToString()) == normalizedClipName) return soundId;
        }

        return SoundId.None;
    }

    private static string NormalizeName(string value)
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

    private readonly struct ImportedSceneMusic
    {
        public readonly SoundId Music;
        public readonly float Volume;
        public readonly bool Loop;

        public ImportedSceneMusic(SoundId music, float volume, bool loop)
        {
            Music = music;
            Volume = volume;
            Loop = loop;
        }
    }
#endif
}
