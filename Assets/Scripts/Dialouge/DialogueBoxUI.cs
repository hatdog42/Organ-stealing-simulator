using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueBoxUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private GraphicRaycaster graphicRaycaster;

    [Header("Position")]
    [SerializeField] private Vector2 topAnchoredPosition = new(0f, 181f);
    [SerializeField] private Vector2 bottomAnchoredPosition = new(0f, -181f);

    private RectTransform speakerNameRect;
    private Vector2 speakerNamePrefabAnchoredPosition;
    private bool hasSpeakerNamePrefabAnchoredPosition;

    private void Awake()
    {
        EnsureReferences();
        SetSpeakerName(null);
        SetVisible(false);
    }

    public IEnumerator PlayLine(
        string line,
        float charDelay,
        float fadeInDuration,
        float fadeOutDuration,
        float holdAfterTyping,
        bool hideAfterLine,
        bool showAtTop)
    {
        EnsureReferences();

        if (!dialogueText || !canvasGroup) yield break;

        dialogueText.text = "";
        SetPosition(showAtTop);
        yield return FadeTo(1f, fadeInDuration);

        foreach (char letter in line ?? string.Empty)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(charDelay);
        }

        if (holdAfterTyping > 0f) yield return new WaitForSecondsRealtime(holdAfterTyping);
        if (hideAfterLine) yield return FadeTo(0f, fadeOutDuration);
    }

    public IEnumerator Hide(float fadeOutDuration)
    {
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
            if (text != speakerNameText)
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
    }
}
