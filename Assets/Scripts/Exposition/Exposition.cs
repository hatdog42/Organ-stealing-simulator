using System.Collections;
using TMPro;
using UnityEngine;

public class Exposition : TypewriterBase
{
    [Header("UI")]
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;

    [Header("Dialogue Fade")]
    [SerializeField, Min(0f)] private float textBoxFadeInDuration = 0.12f;
    [SerializeField, Min(0f)] private float textBoxFadeOutDuration = 0.08f;
    [SerializeField] private bool waitForSceneFade = true;
    [SerializeField] private bool hideTextBoxOnAwake = true;
    [SerializeField] private bool hideTextBoxAfterLine = true;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    [Header("Text")]
    [TextArea(5, 10)]
    [SerializeField] private string expositionText =
        "I got a letter in the mail today-\n\n" +
        "It mentioned an opportunity to earn more money... in exchange for some... cruel acts.\n" +
        "Organs. They want me to possibly kill my patients for their organs and put it in the mop bucket.\n" +
        "I do not know who sent it. But my family... We need the money.\n" +
        "But is it right of me to kill for my own survival? I have to make a choice.";

    private bool hiddenByPause;

    private void Awake()
    {
        EnsureDialogueCanvasGroup();

        if (dialogueText) dialogueText.text = "";
        if (hideTextBoxOnAwake) SetTextBoxVisible(false);
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

    private void Start()
    {
        StartCoroutine(PlayExposition());
    }

    private IEnumerator PlayExposition()
    {
        if (waitForSceneFade)
        {
            while (SceneController.Instance && SceneController.Instance.IsTransitioning)
            {
                yield return null;
            }
        }

        Typing = StartCoroutine(PlayExpositionLine(expositionText));
        yield return Typing;
        Typing = null;

        SceneController.Instance.LoadScene(nextSceneName);
    }

    private IEnumerator PlayExpositionLine(string line)
    {
        if (!dialogueText) yield break;

        dialogueText.text = "";
        yield return FadeTextBox(1f, textBoxFadeInDuration);
        yield return TypeText(dialogueText, line);
        yield return WaitForAdvancePromptDelay();
        yield return WaitForAdvanceClick();

        if (hideTextBoxAfterLine) yield return FadeTextBox(0f, textBoxFadeOutDuration);
    }

    private void EnsureDialogueCanvasGroup()
    {
        if (dialogueCanvasGroup || !dialogueText) return;

        dialogueCanvasGroup = dialogueText.GetComponentInParent<CanvasGroup>();
        if (!dialogueCanvasGroup) dialogueCanvasGroup = dialogueText.gameObject.AddComponent<CanvasGroup>();
    }

    private IEnumerator FadeTextBox(float targetAlpha, float duration)
    {
        EnsureDialogueCanvasGroup();

        if (!dialogueCanvasGroup) yield break;

        if (duration <= 0f)
        {
            SetTextBoxVisible(targetAlpha > 0f);
            yield break;
        }

        float startAlpha = dialogueCanvasGroup.alpha;
        float t = 0f;
        dialogueCanvasGroup.blocksRaycasts = targetAlpha > 0f;
        dialogueCanvasGroup.interactable = targetAlpha > 0f;

        while (t < duration)
        {
            yield return WaitWhilePaused();
            t += Time.unscaledDeltaTime;
            dialogueCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
            yield return null;
        }

        SetTextBoxVisible(targetAlpha > 0f);
    }

    private void SetTextBoxVisible(bool visible)
    {
        EnsureDialogueCanvasGroup();

        if (!dialogueCanvasGroup) return;

        dialogueCanvasGroup.alpha = visible ? 1f : 0f;
        dialogueCanvasGroup.interactable = visible;
        dialogueCanvasGroup.blocksRaycasts = visible;
    }

    private void OnPauseChanged(bool paused)
    {
        EnsureDialogueCanvasGroup();
        if (!dialogueCanvasGroup) return;

        if (paused)
        {
            hiddenByPause = dialogueCanvasGroup.alpha > 0.001f;
            if (hiddenByPause) SetTextBoxVisible(false);
            return;
        }

        if (!hiddenByPause) return;

        SetTextBoxVisible(true);
        hiddenByPause = false;
    }
}
