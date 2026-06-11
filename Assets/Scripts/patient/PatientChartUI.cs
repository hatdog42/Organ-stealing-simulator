using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PatientChartUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text nameText;
    public TMP_Text ageText;
    public TMP_Text jobText;
    public TMP_Text traitText;
    public TMP_Text personalityText;
    public TMP_Text sexText;
    public TMP_Text majorMiniGameText;
    public Image patientImage;
    public Button selectButton; 

    [Header("Paper Movement")]
    [SerializeField] private float hoverLift = 20f;
    [SerializeField] private float selectDropDistance = 700f;
    [SerializeField] private float hoverMoveDuration = 0.08f;
    [SerializeField] private float selectMoveDuration = 0.25f;

    [Header("Paper Audio")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip selectSound;
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float selectVolume = 1f;
    
    private Patient _shownPatient;
    private System.Action<Patient> _onSelect;
    private RectTransform _rectTransform;
    private Vector2 _restingPosition;
    private Coroutine _moveRoutine;
    private bool _isSelected;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
        if (_rectTransform) _restingPosition = _rectTransform.anchoredPosition;
    }

    public void Bind(Patient patient, System.Action<Patient> onSelect)
    {
        _shownPatient = patient;
        _onSelect = onSelect;
        _isSelected = false;

        if (!_rectTransform) _rectTransform = transform as RectTransform;
        if (_rectTransform) _rectTransform.anchoredPosition = _restingPosition;

        nameText.text = patient.FullName;
        ageText.text = patient.age.ToString();
        jobText.text = patient.job;
        traitText.text = patient.trait;
        personalityText.text = patient.personality.ToString();
        sexText.text = patient.sex;
        if (majorMiniGameText) majorMiniGameText.text = patient.majorMiniGameName;
        patientImage.sprite = patient.face;
        
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(SelectPatient);

        ButtonClickSound buttonClickSound = selectButton.GetComponent<ButtonClickSound>();
        if (buttonClickSound) buttonClickSound.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSelected) return;

        PlaySfx(hoverSound, hoverVolume);
        MoveTo(_restingPosition + Vector2.up * hoverLift, hoverMoveDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected) return;

        MoveTo(_restingPosition, hoverMoveDuration);
    }

    private void SelectPatient()
    {
        if (_isSelected) return;

        _isSelected = true;
        selectButton.interactable = false;
        PlaySfx(selectSound, selectVolume);
        MoveTo(_restingPosition + Vector2.down * selectDropDistance, selectMoveDuration, InvokeSelect);
    }

    private void MoveTo(Vector2 targetPosition, float duration, System.Action onComplete = null)
    {
        if (!_rectTransform)
        {
            onComplete?.Invoke();
            return;
        }

        if (_moveRoutine != null) StopCoroutine(_moveRoutine);
        _moveRoutine = StartCoroutine(MoveRoutine(targetPosition, duration, onComplete));
    }

    private IEnumerator MoveRoutine(Vector2 targetPosition, float duration, System.Action onComplete)
    {
        Vector2 startPosition = _rectTransform.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);
            _rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, targetPosition, t);
            yield return null;
        }

        _rectTransform.anchoredPosition = targetPosition;
        _moveRoutine = null;
        onComplete?.Invoke();
    }

    private void InvokeSelect()
    {
        _onSelect?.Invoke(_shownPatient);
    }

    private static void PlaySfx(AudioClip clip, float volume)
    {
        if (!AudioManager.Instance || !clip) return;

        AudioManager.Instance.PlaySfx(clip, volume);
    }
}
