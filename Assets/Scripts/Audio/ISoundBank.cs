public interface ISoundBank
{
    bool TryGetSound(SoundId id, out SoundBankEntry sound);
    bool TryGetSceneMusic(string sceneName, string scenePath, out SceneMusicEntry sceneMusic);
    float GetVolume(SoundId id);
    void SetVolume(SoundId id, float volume);
}
