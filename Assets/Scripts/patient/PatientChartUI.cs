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
    [SerializeField] private SoundId hoverSound = SoundId.PaperHover;
    [SerializeField] private SoundId selectSound = SoundId.PaperSelect;
    [SerializeField, Range(0f, 1f)] private float hoverVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float selectVolume = 1f;
    
    private Patient _shownPatient;
    private System.Action<Patient> _onSelect;
    private System.Action<Patient> _onSelectionStarted;
    private RectTransform _rectTransform;
    private Vector2 _restingPosition;
    private Coroutine _moveRoutine;
    private bool _isSelected;
    private bool _selectionLocked;

    private void Awake()
    {
        _rectTransform = transform as RectTransform;
        if (_rectTransform) _restingPosition = _rectTransform.anchoredPosition;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ValidatePaperReferences();
    }
#endif

    public void Bind(Patient patient, System.Action<Patient> onSelect, System.Action<Patient> onSelectionStarted = null)
    {
        ValidatePaperReferences();

        _shownPatient = patient;
        _onSelect = onSelect;
        _onSelectionStarted = onSelectionStarted;
        _isSelected = false;
        _selectionLocked = false;

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
        selectButton.interactable = true;

        ButtonClickSound buttonClickSound = selectButton.GetComponent<ButtonClickSound>();
        if (buttonClickSound) buttonClickSound.enabled = false;
    }

    private void ValidatePaperReferences()
    {
        WarnIfReferenceOutsidePaper(nameText, nameof(nameText));
        WarnIfReferenceOutsidePaper(ageText, nameof(ageText));
        WarnIfReferenceOutsidePaper(jobText, nameof(jobText));
        WarnIfReferenceOutsidePaper(traitText, nameof(traitText));
        WarnIfReferenceOutsidePaper(personalityText, nameof(personalityText));
        WarnIfReferenceOutsidePaper(sexText, nameof(sexText));
        WarnIfReferenceOutsidePaper(majorMiniGameText, nameof(majorMiniGameText));
        WarnIfReferenceOutsidePaper(patientImage, nameof(patientImage));
        WarnIfReferenceOutsidePaper(selectButton, nameof(selectButton));
    }

    private void WarnIfReferenceOutsidePaper(Component component, string fieldName)
    {
        if (!component || component.transform == transform || component.transform.IsChildOf(transform)) return;

        Debug.LogWarning($"{nameof(PatientChartUI)} on '{name}' has {fieldName} assigned outside its paper. This can display another patient's data.", this);
    }

    public void SetSelectionEnabled(bool enabled)
    {
        _selectionLocked = !enabled;

        if (_selectionLocked && !_isSelected)
        {
            MoveTo(_restingPosition, hoverMoveDuration);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isSelected || _selectionLocked) return;

        PlaySfx(hoverSound, hoverVolume);
        MoveTo(_restingPosition + Vector2.up * hoverLift, hoverMoveDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isSelected || _selectionLocked) return;

        MoveTo(_restingPosition, hoverMoveDuration);
    }

    private void SelectPatient()
    {
        if (_isSelected || _selectionLocked) return;

        _isSelected = true;
        _onSelectionStarted?.Invoke(_shownPatient);
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

    private static void PlaySfx(SoundId soundId, float volume)
    {
        if (!AudioManager.Instance || soundId == SoundId.None) return;

        AudioManager.Instance.PlaySfx(soundId, volume);
    }
}
