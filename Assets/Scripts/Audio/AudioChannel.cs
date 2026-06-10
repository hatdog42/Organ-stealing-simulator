using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioChannel : MonoBehaviour
{
    [SerializeField] private AudioChannelType channelType = AudioChannelType.Sfx;

    public AudioChannelType ChannelType => channelType;

    private void OnEnable()
    {
        if (AudioManager.Instance)
        {
            AudioManager.Instance.RegisterSource(GetComponent<AudioSource>(), channelType);
        }
    }
}
