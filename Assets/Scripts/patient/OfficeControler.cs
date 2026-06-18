using System.Collections;
using UnityEngine;

public class OfficeControler : DialogueBase, IClickable
{
    
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite hoverSprite;
    
    [Header("Reputation → Opening line")]
    [SerializeField, TextArea] private string stableLine;
    [SerializeField, TextArea] private string neutralLine;
    [SerializeField, TextArea] private string unstableLine;
    [SerializeField, TextArea] private string brokenLine;
    
    [Header("Paper UI (assign your panel)")]
    [SerializeField] private CanvasGroup paperCanvas;
    [SerializeField] private CanvasGroup coworkerCanvas;
    
    private bool dialogueActive;
    
    protected override void Awake()
    {
        base.Awake();

        spriteRenderer = GetComponent<SpriteRenderer>();
        
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
        if (!isActiveAndEnabled) return;
        if (dialogueActive) return;
        
        Show(paperCanvas);
    }

    public void OnHoverEnter()
    {
        SetSprite(hoverSprite);
    }

    public void OnHoverExit()
    {
        SetSprite(baseSprite);
    }

    private void SetSprite(Sprite sprite)
    {
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer && sprite) spriteRenderer.sprite = sprite;
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

        PlayLine(line);
        
        while (Typing != null) yield return null;
        
        if (HealthBars.Instance.CurrentReputationState() == HealthBars.ReputationState.Broken)
        {
            SceneController.Instance.LoadScene("PrisonScene");
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
