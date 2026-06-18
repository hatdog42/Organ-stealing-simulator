using System;
using UnityEngine;

[Serializable]
public class SceneMusicEntry
{
    [SerializeField] private string scenePath;
    [SerializeField] private string sceneName;
    [SerializeField] private SoundId music = SoundId.None;
    [SerializeField, Range(0f, 1f)] private float volume = 0.4f;
    [SerializeField] private bool loop = true;
    [SerializeField] private bool stopCurrentMusic;

    public string ScenePath => scenePath;
    public string SceneName => sceneName;
    public SoundId Music => music;
    public float Volume => volume;
    public bool Loop => loop;
    public bool StopCurrentMusic => stopCurrentMusic;

    public SceneMusicEntry()
    {
    }

    public SceneMusicEntry(string scenePath, string sceneName)
    {
        SetScene(scenePath, sceneName);
    }

    public bool MatchesScene(string targetSceneName, string targetScenePath)
    {
        if (!string.IsNullOrWhiteSpace(scenePath)
            && !string.IsNullOrWhiteSpace(targetScenePath)
            && string.Equals(scenePath, targetScenePath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(sceneName)
               && string.Equals(sceneName, targetSceneName, StringComparison.OrdinalIgnoreCase);
    }

    public void SetScene(string newScenePath, string newSceneName)
    {
        scenePath = NormalizePath(newScenePath);
        sceneName = newSceneName;
    }

    public void SetMusic(SoundId newMusic, float newVolume, bool newLoop)
    {
        music = newMusic;
        volume = Mathf.Clamp01(newVolume);
        loop = newLoop;
        stopCurrentMusic = false;
    }

    private static string NormalizePath(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Replace('\\', '/');
    }
}
