using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    [SerializeField, Min(0f)] private float brokenPortraitHoldSeconds = 12f;
    
    [Header("Lines")]
    [SerializeField] private string lineStable;
    [SerializeField] private string lineNeutral;
    [SerializeField] private string lineUnstable;

    private IEnumerator Start()
    {
        var healthBars = HealthBars.Instance;
        if (!healthBars)
        {
            Debug.LogWarning($"{nameof(SanetyChekControler)} could not find {nameof(HealthBars)}. Falling back to stable psyche state.", this);
        }

        var psycheState = healthBars ? healthBars.CurrentPsycheState() : HealthBars.PsycheState.Stable;

        if (psycheState == HealthBars.PsycheState.Broken)
        {
            if (canvasGroup) canvasGroup.alpha = 0;
            if (portrait && BrokenPortrait) portrait.sprite = BrokenPortrait;

            yield return new WaitForSecondsRealtime(brokenPortraitHoldSeconds);
            LoadConfiguredScene(EndingScene);
            yield break;
        }

        string line = psycheState switch
        {
            HealthBars.PsycheState.Stable => lineStable,
            HealthBars.PsycheState.Neutral => lineNeutral,
            HealthBars.PsycheState.Unstable => lineUnstable,
            _ => lineUnstable
        };
        line ??= string.Empty;

        PlayLine(line);
        while (Typing != null)
        {
            yield return null;
        }
        
        LoadConfiguredScene(NextScene);
    }

    private void LoadConfiguredScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError($"{nameof(SanetyChekControler)} cannot load a blank scene name.", this);
            return;
        }

        if (SceneController.Instance)
        {
            SceneController.Instance.LoadScene(sceneName);
            return;
        }

        Debug.LogWarning($"{nameof(SanetyChekControler)} could not find {nameof(SceneController)}. Loading '{sceneName}' directly.", this);
        SceneManager.LoadScene(sceneName);
    }
}
