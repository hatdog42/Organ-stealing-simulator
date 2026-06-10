using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ButtonClickSound : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private bool playClickSound = true;
    [SerializeField] private AudioClip overrideClickSound;
    [SerializeField, Range(0f, 1f)] private float volumeScale = 1f;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_button) _button = GetComponent<Button>();
        if (!_button.interactable) return;
        if (!playClickSound || !AudioManager.Instance) return;

        AudioManager.Instance.PlayButtonClick(overrideClickSound, volumeScale);
    }
}
