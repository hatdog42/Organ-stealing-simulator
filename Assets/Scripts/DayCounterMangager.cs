using System.Collections;
using TMPro;
using UnityEngine;

public class DayCounterMangager : MonoBehaviour
{
    [SerializeField, Min(0f)] private float textFadeDuration = 1f;
    [SerializeField, Min(0f)] private float pauseAfterTextFade = 1f;
    [SerializeField, Min(0f)] private float waitAfterBell = 2f;
    [SerializeField, Min(0f)] private float musicFadeOutDuration = 1.5f;
    [SerializeField] private AudioClip bellSound;
    [SerializeField, Range(0f, 1f)] private float bellVolume = 1f;
    [SerializeField] private string nextSceneName = "ChosePatient";

    private static int dayCount = 0;
    private TextMeshProUGUI _text;

    public static void ResetDayCount()
    {
        dayCount = 0;
    }

    IEnumerator Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
        if (!_text)
        {
            Debug.LogError($"{nameof(DayCounterMangager)} needs a TextMeshProUGUI component.", this);
            yield break;
        }

        _text.text = "DAY " + dayCount.ToString();
        SetTextAlpha(0f);
        _text.ForceMeshUpdate();

        while (SceneController.Instance && SceneController.Instance.IsTransitioning)
        {
            yield return null;
        }

        if (AudioManager.Instance)
        {
            AudioManager.Instance.FadeOutMusic(musicFadeOutDuration);
        }

        yield return FadeText(1f);
        yield return new WaitForSecondsRealtime(pauseAfterTextFade);

        dayCount++;
        _text.text = "DAY " + dayCount.ToString();
        _text.ForceMeshUpdate();

        if (AudioManager.Instance)
        {
            AudioManager.Instance.PlaySfx(bellSound, bellVolume);
        }

        yield return new WaitForSecondsRealtime(waitAfterBell);

        if (SceneController.Instance)
        {
            SceneController.Instance.LoadScene(nextSceneName);
        }
    }

    private IEnumerator FadeText(float targetAlpha)
    {
        float startAlpha = _text.color.a;
        float elapsed = 0f;

        while (elapsed < textFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = textFadeDuration <= 0f ? 1f : Mathf.Clamp01(elapsed / textFadeDuration);
            SetTextAlpha(Mathf.Lerp(startAlpha, targetAlpha, progress));
            yield return null;
        }

        SetTextAlpha(targetAlpha);
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = _text.color;
        color.a = alpha;
        _text.color = color;
    }
}
