using System.Collections;
using UnityEngine;

public class SanetyChekControler : DialogueBase
{
    [Header("unity references")]
    [SerializeField] private SpriteRenderer portrait;
    [SerializeField] private Sprite BrokenPortrait;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Next Scene")]
    [SerializeField] private string NextScene;
    [SerializeField] private string EndingScene = "Credits";

    [Header("Timing")]
    [SerializeField, Min(0f)] private float minimumLineHoldSeconds = 4f;
    [SerializeField, Min(0f)] private float extraLineHoldSeconds = 2f;
    [SerializeField, Min(0f)] private float brokenPortraitHoldSeconds = 4f;
    
    [Header("Lines")]
    [SerializeField] private string lineStable;
    [SerializeField] private string lineNeutral;
    [SerializeField] private string lineUnstable;

    private IEnumerator Start()
    {
        var healthBars = HealthBars.Instance;
        var psycheState = healthBars.CurrentPsycheState();

        if (psycheState == HealthBars.PsycheState.Broken)
        {
            canvasGroup.alpha = 0;
            portrait.sprite = BrokenPortrait;
            yield return new WaitForSecondsRealtime(brokenPortraitHoldSeconds);
            SceneController.Instance.LoadScene(EndingScene);
            yield break;
        }

        string line = psycheState switch
        {
            HealthBars.PsycheState.Stable => lineStable,
            HealthBars.PsycheState.Neutral => lineNeutral,
            HealthBars.PsycheState.Unstable => lineUnstable,
            _ => lineUnstable
        };

        PlayLine(line);
        float lineHoldSeconds = Mathf.Max(minimumLineHoldSeconds, line.Length * charDelay + extraLineHoldSeconds);
        float elapsed = 0f;
        while (elapsed < lineHoldSeconds || Typing != null)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        
        SceneController.Instance.LoadScene(NextScene);
    }
}


