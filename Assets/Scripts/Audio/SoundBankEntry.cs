using System;
using UnityEngine;

[Serializable]
public class SoundBankEntry
{
    [SerializeField] private SoundId id = SoundId.None;
    [SerializeField] private AudioClip clip;
    [SerializeField] private AudioChannelType channelType = AudioChannelType.Sfx;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Min(0f)] private float pitch = 1f;
    [SerializeField] private bool loop;

    public SoundId Id => id;
    public AudioClip Clip => clip;
    public AudioChannelType ChannelType => channelType;
    public float Volume => volume;
    public float Pitch => pitch;
    public bool Loop => loop;

    public SoundBankEntry()
    {
    }

    public SoundBankEntry(SoundId id, AudioClip clip, AudioChannelType channelType, bool loop)
    {
        this.id = id;
        this.clip = clip;
        this.channelType = channelType;
        this.loop = loop;
        volume = 1f;
        pitch = 1f;
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
    }
}
