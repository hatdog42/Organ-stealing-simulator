using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AudioVolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioChannelType channelType = AudioChannelType.Music;

    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (!_slider) _slider = GetComponent<Slider>();

        _slider.SetValueWithoutNotify(CurrentVolume());
        _slider.onValueChanged.AddListener(SetVolume);
    }

    private void OnDisable()
    {
        if (_slider) _slider.onValueChanged.RemoveListener(SetVolume);
    }

    private float CurrentVolume()
    {
        if (!AudioManager.Instance) return 1f;

        return channelType == AudioChannelType.Music
            ? AudioManager.Instance.MusicVolume
            : AudioManager.Instance.SfxVolume;
    }

    private void SetVolume(float volume)
    {
        if (!AudioManager.Instance) return;

        if (channelType == AudioChannelType.Music)
        {
            AudioManager.Instance.SetMusicVolume(volume);
        }
        else
        {
            AudioManager.Instance.SetSfxVolume(volume);
        }
    }
}
