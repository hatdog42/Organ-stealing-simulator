using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
    }

    private void SetAlpha(float a)
    {
        _canvasGroup.alpha = a;
        _canvasGroup.blocksRaycasts = a > 0.001f;
        _canvasGroup.interactable = a > 0.001f;
    }
    private IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0;
        SetAlpha(0);
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        SetAlpha(to);
    }
    
    public IEnumerator FadeOut(float duration) => Fade(0, 1, duration);
    public IEnumerator FadeIn(float duration) => Fade(1, 0, duration);
}
