using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SanetyChekControler : DialogueBase
{
    [Header("unity references")]
    [SerializeField] private SpriteRenderer portrait;
    [SerializeField] private Sprite BrokenPortrait;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Ending Scene")]
    [SerializeField] private string EndingScene;
    
    [Header("Lines")]
    [SerializeField] private string lineStable;
    [SerializeField] private string lineNeutral;
    [SerializeField] private string lineUnstable;

    private IEnumerator Start()
    {
        var state = HealthBars.Instance.CurrentPsycheState();

        switch (state)
        {
            case HealthBars.PsycheState.Stable:
                PlayLine(lineStable);
                yield return new WaitForSecondsRealtime(lineStable.Length * charDelay + 1f);
                SceneController.Instance.LoadNextOrLoop();
                break;
            
            case HealthBars.PsycheState.Neutral:
                PlayLine(lineNeutral);
                yield return new WaitForSecondsRealtime(lineStable.Length * charDelay + 1f);
                SceneController.Instance.LoadNextOrLoop();
                break;
            
            case HealthBars.PsycheState.Unstable:
                PlayLine(lineUnstable);
                yield return new WaitForSecondsRealtime(lineStable.Length * charDelay + 1f);
                SceneController.Instance.LoadNextOrLoop();
                break;

            case HealthBars.PsycheState.Broken:
                canvasGroup.alpha = 0;
                portrait.sprite = BrokenPortrait; 
                yield return new WaitForSecondsRealtime(3f);
                SceneController.Instance.LoadScene(EndingScene);
                break;
        }
    }
}


