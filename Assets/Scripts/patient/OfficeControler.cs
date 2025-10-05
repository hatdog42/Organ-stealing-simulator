using System.Collections;
using UnityEngine;

public class OfficeControler : DialogueBase, IClickable
{
    [Header("Reputation → Opening line")]
    [SerializeField, TextArea] private string stableLine;
    [SerializeField, TextArea] private string neutralLine;
    [SerializeField, TextArea] private string unstableLine;
    [SerializeField, TextArea] private string brokenLine;
    
    [Header("PrisonEnding")]
    [SerializeField] private string brokenPrisonEndingLine;
    
    [Header("Paper UI (assign your panel)")]
    [SerializeField] private CanvasGroup paperCanvas;
    [SerializeField] private CanvasGroup coworkerCanvas;
    
    private bool dialogueActive;
    
    private void Awake()
    {
        if (paperCanvas) Hide(paperCanvas);
        if (coworkerCanvas) Hide(coworkerCanvas);
    }

    private void Start()
    {
        var repState = HealthBars.Instance.CurrentReputationState();
        string line = LineFor(repState);
        StartCoroutine(TypeCoworkerLine(line));
    }
    
    public void OnClick(Vector3 worldPos)
    {
        if (dialogueActive) return;
        
        Show(paperCanvas);
    }

    private string LineFor(HealthBars.ReputationState state) => state switch
    {
        HealthBars.ReputationState.Stable => stableLine,
        HealthBars.ReputationState.Neutral => neutralLine,
        HealthBars.ReputationState.Unstable => unstableLine,
        HealthBars.ReputationState.Broken => brokenLine,
        _ => neutralLine
    };

    private IEnumerator TypeCoworkerLine(string line)
    {
        dialogueActive = true;

        yield return new WaitForSecondsRealtime(1f);
        
        Show(coworkerCanvas);
        PlayLine(line);
        
        while (Typing != null) yield return null;

        yield return new WaitForSecondsRealtime(1f);
        
        Hide(coworkerCanvas);
        if (HealthBars.Instance.CurrentReputationState() == HealthBars.ReputationState.Broken)
        {
            yield return new WaitForSecondsRealtime(1f);
            SceneController.Instance.LoadScene(brokenPrisonEndingLine);
            yield break;
        }
        dialogueActive = false;
    }
    private static void Show(CanvasGroup cg)
    {
        if (!cg) return;
        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;
    }

    private static void Hide(CanvasGroup cg)
    {
        if (!cg) return;
        cg.alpha = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;
    }
}
