using System.Collections;
using UnityEngine;

public class DialogueBase : TypewriterBase
{
    [Header("Prefab Dialogue")]
    [SerializeField] private DialogueBoxUI dialogueBoxPrefab;
    [SerializeField] private string resourceDialogueBoxPath = "DialogueBox";
    [SerializeField] private bool showTextBoxAtTop = true;

    [Header("Dialogue Fade")]
    [SerializeField, Min(0f)] private float textBoxFadeInDuration = 0.12f;
    [SerializeField, Min(0f)] private float textBoxFadeOutDuration = 0.08f;
    [SerializeField] private bool waitForSceneFade = true;
    [SerializeField] private bool hideTextBoxAfterLine = true;

    private static DialogueBoxUI SharedDialogueBox;

    protected virtual bool ShowTextBoxAtTop => showTextBoxAtTop;
    protected virtual bool HideTextBoxAfterLine => hideTextBoxAfterLine;
    protected virtual string DialogueSpeakerName => null;
    protected virtual void OnDialogueLineStarted(string line) { }

    protected virtual void Awake()
    {
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
            OnDialogueLineStarted(line);
            yield return dialogueBox.PlayLine(
                line,
                CurrentTypewriterSettings,
                textBoxFadeInDuration,
                textBoxFadeOutDuration,
                HideTextBoxAfterLine,
                ShowTextBoxAtTop);
        }

        Typing = null;
    }

    protected IEnumerator HideTextBox()
    {
        DialogueBoxUI dialogueBox = GetDialogueBox();
        if (dialogueBox)
        {
            yield return dialogueBox.Hide(textBoxFadeOutDuration);
        }
    }

    protected void SetDialogueSpeakerName(string speakerName)
    {
        DialogueBoxUI dialogueBox = GetDialogueBox();
        if (dialogueBox) dialogueBox.SetSpeakerName(speakerName);
    }

    private DialogueBoxUI GetDialogueBox()
    {
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
}
