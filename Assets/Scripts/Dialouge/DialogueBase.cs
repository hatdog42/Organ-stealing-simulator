using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueBase : MonoBehaviour
{
    [Header("Prefab Dialogue")]
    [SerializeField] private DialogueBoxUI dialogueBoxPrefab;
    [SerializeField] private bool usePrefabDialogue = true;
    [SerializeField] private string resourceDialogueBoxPath = "DialogueBox";
    [SerializeField] private bool showTextBoxAtTop = true;

    [Header("Legacy UI Fallback")]
    [SerializeField] protected TMP_Text dialogueText;
    [SerializeField] protected CanvasGroup dialogueCanvasGroup;
    [SerializeField] private GameObject legacyDialogueRoot;
    
    [Header("Typing")]
    [SerializeField, Range(0.001f, 0.1f)] protected float charDelay = 0.02f;

    [Header("Dialogue Fade")]
    [SerializeField, Min(0f)] private float textBoxFadeInDuration = 0.12f;
    [SerializeField, Min(0f)] private float textBoxFadeOutDuration = 0.08f;
    [SerializeField, Min(0f)] private float holdAfterTyping = 2f;
    [SerializeField] private bool waitForSceneFade = true;
    [SerializeField] private bool hideTextBoxOnAwake = true;
    [SerializeField] private bool hideTextBoxAfterLine = true;
    
    protected Coroutine Typing;
    private static DialogueBoxUI SharedDialogueBox;
    private bool hiddenByPause;
    protected virtual bool UsePrefabDialogue => usePrefabDialogue;
    protected virtual bool ShowTextBoxAtTop => showTextBoxAtTop;
    protected virtual string DialogueSpeakerName => null;
    protected bool PrefabDialogueEnabled => UsePrefabDialogue;

    protected virtual void Awake()
    {
        EnsureDialogueCanvasGroup();

        if (dialogueText) dialogueText.text = "";
        if (UsePrefabDialogue)
        {
            HideLegacyDialogueRoot();
            return;
        }

        if (hideTextBoxOnAwake) SetTextBoxVisible(false);
    }

    protected virtual void OnEnable()
    {
        PauseMenueControler.PauseChanged += OnPauseChanged;
        if (PauseMenueControler.IsPaused) OnPauseChanged(true);
    }

    protected virtual void OnDisable()
    {
        PauseMenueControler.PauseChanged -= OnPauseChanged;
    }
    
    protected Coroutine PlayLine(string line)
    {
        if (Typing != null) StopCoroutine(Typing);
        Typing = StartCoroutine(PlayLineRoutine(line ?? string.Empty));
        return Typing;
    }

    private IEnumerator PlayLineRoutine(string line)
    {
        if (waitForSceneFade)
        {
            while (SceneController.Instance && SceneController.Instance.IsTransitioning)
            {
                yield return null;
            }
        }

        DialogueBoxUI dialogueBox = GetDialogueBox();
        if (dialogueBox)
        {
            dialogueBox.SetSpeakerName(DialogueSpeakerName);
            yield return dialogueBox.PlayLine(
                line,
                charDelay,
                textBoxFadeInDuration,
                textBoxFadeOutDuration,
                holdAfterTyping,
                hideTextBoxAfterLine,
                ShowTextBoxAtTop);

            Typing = null;
            yield break;
        }

        yield return TypeLegacyRoutine(line);

        Typing = null;
    }

    private IEnumerator TypeLegacyRoutine(string line)
    {
        if (!dialogueText) yield break;

        dialogueText.text = "";
        yield return FadeTextBox(1f, textBoxFadeInDuration);

        int i = 0;

        while (i < line.Length)
        {
            yield return WaitWhilePaused();
            dialogueText.text += line[i];
            
            float delay = charDelay;
            char c = line[i];

            i++;
            yield return WaitForSecondsRealtimeRespectingPause(delay);
        }

        if (holdAfterTyping > 0f) yield return WaitForSecondsRealtimeRespectingPause(holdAfterTyping);
        if (hideTextBoxAfterLine) yield return FadeTextBox(0f, textBoxFadeOutDuration);
    }

    protected IEnumerator HideTextBox()
    {
        DialogueBoxUI dialogueBox = GetDialogueBox();
        if (dialogueBox)
        {
            yield return dialogueBox.Hide(textBoxFadeOutDuration);
            yield break;
        }

        yield return FadeTextBox(0f, textBoxFadeOutDuration);
    }

    protected void SetDialogueSpeakerName(string speakerName)
    {
        DialogueBoxUI dialogueBox = GetDialogueBox();
        if (dialogueBox) dialogueBox.SetSpeakerName(speakerName);
    }

    private DialogueBoxUI GetDialogueBox()
    {
        if (!UsePrefabDialogue) return null;

        if (SharedDialogueBox) return SharedDialogueBox;

        DialogueBoxUI prefab = dialogueBoxPrefab;
        if (!prefab && !string.IsNullOrWhiteSpace(resourceDialogueBoxPath))
        {
            prefab = Resources.Load<DialogueBoxUI>(resourceDialogueBoxPath);
        }

        if (!prefab) return null;

        SharedDialogueBox = Instantiate(prefab);
        DontDestroyOnLoad(SharedDialogueBox.gameObject);
        return SharedDialogueBox;
    }

    private void EnsureDialogueCanvasGroup()
    {
        if (dialogueCanvasGroup || !dialogueText) return;

        dialogueCanvasGroup = dialogueText.GetComponentInParent<CanvasGroup>();
        if (!dialogueCanvasGroup) dialogueCanvasGroup = dialogueText.gameObject.AddComponent<CanvasGroup>();
    }

    private void HideLegacyDialogueRoot()
    {
        if (!legacyDialogueRoot) return;

        if (legacyDialogueRoot.GetComponent<Canvas>())
        {
            Debug.LogWarning(
                $"{nameof(DialogueBase)} will not disable legacy dialogue root '{legacyDialogueRoot.name}' because it has a Canvas component. Assign the old textbox object instead of the whole Canvas.",
                this);
            return;
        }

        legacyDialogueRoot.SetActive(false);
    }

    private IEnumerator FadeTextBox(float targetAlpha, float duration)
    {
        EnsureDialogueCanvasGroup();

        if (!dialogueCanvasGroup)
        {
            yield break;
        }

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
        if (UsePrefabDialogue) return;

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

    private static IEnumerator WaitWhilePaused()
    {
        while (PauseMenueControler.IsPaused)
        {
            yield return null;
        }
    }

    private static IEnumerator WaitForSecondsRealtimeRespectingPause(float seconds)
    {
        float elapsed = 0f;
        while (elapsed < seconds)
        {
            if (!PauseMenueControler.IsPaused)
            {
                elapsed += Time.unscaledDeltaTime;
            }

            yield return null;
        }
    }
}
