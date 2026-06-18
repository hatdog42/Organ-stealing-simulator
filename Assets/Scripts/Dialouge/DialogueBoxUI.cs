using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxUI : TypewriterBase
{
    [Header("References")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text nextText;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("Position")]
    [SerializeField] private Vector2 topAnchoredPosition = new(0f, 181f);
    [SerializeField] private Vector2 bottomAnchoredPosition = new(0f, -181f);

    private RectTransform speakerNameRect;
    private Vector2 speakerNamePrefabAnchoredPosition;
    private bool hasSpeakerNamePrefabAnchoredPosition;
    private bool hiddenByPause;

    private void Awake()
    {
        EnsureReferences();
        SetSpeakerName(null);
        SetNextTextVisible(false);
        SetVisible(false);
    }

    private void OnEnable()
    {
        PauseMenueControler.PauseChanged += OnPauseChanged;
        if (PauseMenueControler.IsPaused) OnPauseChanged(true);
    }

    private void OnDisable()
    {
        PauseMenueControler.PauseChanged -= OnPauseChanged;
    }

    public IEnumerator PlayLine(
        string line,
        TypewriterSettings typewriterSettings,
        float fadeInDuration,
        float fadeOutDuration,
        bool hideAfterLine,
        bool showAtTop)
    {
        EnsureReferences();

        if (!dialogueText || !canvasGroup) yield break;

        SetNextTextVisible(false);
        dialogueText.text = "";
        SetPosition(showAtTop);
        yield return FadeTo(1f, fadeInDuration);

        string text = line ?? string.Empty;
        yield return TypeText(dialogueText, text, typewriterSettings);
        yield return WaitForAdvancePromptDelay(typewriterSettings.AdvancePromptDelay);
        SetNextTextVisible(true);
        yield return WaitForAdvanceClick();
        SetNextTextVisible(false);

        if (hideAfterLine) yield return FadeTo(0f, fadeOutDuration);
    }

    public IEnumerator Hide(float fadeOutDuration)
    {
        SetNextTextVisible(false);
        yield return FadeTo(0f, fadeOutDuration);
    }

    public void SetSpeakerName(string speakerName)
    {
        EnsureReferences();

        if (!speakerNameText) return;

        bool hasName = !string.IsNullOrWhiteSpace(speakerName);
        speakerNameText.text = hasName ? speakerName : "";
        speakerNameText.gameObject.SetActive(hasName);
    }

    private void EnsureReferences()
    {
        if (!uiCanvas) uiCanvas = GetComponent<Canvas>();
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!graphicRaycaster) graphicRaycaster = GetComponent<GraphicRaycaster>();
        FindTextReferences();
        if (!dialoguePanel && dialogueText) dialoguePanel = dialogueText.transform.parent as RectTransform;
        if (!dialoguePanel && transform.childCount > 0) dialoguePanel = transform.GetChild(0) as RectTransform;
        CacheSpeakerNamePosition();
    }

    private void FindTextReferences()
    {
        TMP_Text[] textComponents = GetComponentsInChildren<TMP_Text>(true);

        if (!nextText)
        {
            foreach (TMP_Text text in textComponents)
            {
                if (text.name.Equals("NextText", System.StringComparison.OrdinalIgnoreCase) ||
                    text.name.Equals("Next", System.StringComparison.OrdinalIgnoreCase))
                {
                    nextText = text;
                    break;
                }
            }
        }

        if (nextText)
        {
            nextText.raycastTarget = false;
        }

        if (!speakerNameText)
        {
            foreach (TMP_Text text in textComponents)
            {
                if (text.name.Equals("name", System.StringComparison.OrdinalIgnoreCase) ||
                    text.name.Equals("SpeakerName", System.StringComparison.OrdinalIgnoreCase))
                {
                    speakerNameText = text;
                    break;
                }
            }
        }

        if (dialogueText) return;

        foreach (TMP_Text text in textComponents)
        {
            if (text == speakerNameText || text == nextText)
            {
                continue;
            }

            if (text.name.Equals("Text", System.StringComparison.OrdinalIgnoreCase) ||
                text.name.Equals("DialogueText", System.StringComparison.OrdinalIgnoreCase))
            {
                dialogueText = text;
                return;
            }
        }

        foreach (TMP_Text text in textComponents)
        {
            if (text != speakerNameText && text != nextText)
            {
                dialogueText = text;
                return;
            }
        }
    }

    private void SetPosition(bool showAtTop)
    {
        if (!dialoguePanel) return;

        dialoguePanel.anchoredPosition = showAtTop ? topAnchoredPosition : bottomAnchoredPosition;
        SetSpeakerNamePosition(showAtTop);
    }

    private void CacheSpeakerNamePosition()
    {
        if (hasSpeakerNamePrefabAnchoredPosition || !speakerNameText) return;

        speakerNameRect = speakerNameText.rectTransform;
        speakerNamePrefabAnchoredPosition = speakerNameRect.anchoredPosition;
        hasSpeakerNamePrefabAnchoredPosition = true;
    }

    private void SetSpeakerNamePosition(bool showAtTop)
    {
        if (!speakerNameText) return;

        if (!speakerNameRect) speakerNameRect = speakerNameText.rectTransform;
        if (!hasSpeakerNamePrefabAnchoredPosition) CacheSpeakerNamePosition();

        Vector2 anchoredPosition = speakerNamePrefabAnchoredPosition;
        if (!showAtTop)
        {
            anchoredPosition += topAnchoredPosition - bottomAnchoredPosition;
        }

        speakerNameRect.anchoredPosition = anchoredPosition;
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        if (!canvasGroup) yield break;

        bool becomingVisible = targetAlpha > 0f;
        SetInteraction(becomingVisible);
        if (becomingVisible && uiCanvas) uiCanvas.enabled = true;

        if (duration <= 0f)
        {
            SetVisible(targetAlpha > 0f);
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        float t = 0f;

        while (t < duration)
        {
            yield return WaitWhilePaused();
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            yield return null;
        }

        SetVisible(targetAlpha > 0f);
    }

    private void SetVisible(bool visible)
    {
        if (!canvasGroup) return;

        canvasGroup.alpha = visible ? 1f : 0f;
        SetInteraction(visible);
        if (uiCanvas) uiCanvas.enabled = visible;
    }

    private void SetInteraction(bool enabled)
    {
        if (canvasGroup)
        {
            canvasGroup.interactable = enabled;
            canvasGroup.blocksRaycasts = enabled;
        }

        if (graphicRaycaster) graphicRaycaster.enabled = enabled;
        if (nextText) nextText.raycastTarget = false;
    }

    private void OnPauseChanged(bool paused)
    {
        EnsureReferences();

        if (paused)
        {
            hiddenByPause = canvasGroup && canvasGroup.alpha > 0.001f;
            if (hiddenByPause) SetVisible(false);
            return;
        }

        if (!hiddenByPause) return;

        SetVisible(true);
        hiddenByPause = false;
    }

    private void SetNextTextVisible(bool visible)
    {
        if (!nextText) return;

        nextText.gameObject.SetActive(visible);
        nextText.raycastTarget = false;
    }
}
