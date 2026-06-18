using System.Collections;
using TMPro;
using UnityEngine;

public class OutsideControler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private bill billFixed;   
    [SerializeField] private bill billOrgan;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text totalMoneyText;
    [SerializeField] private TMP_Text addedMoneyText;
    [SerializeField] private float revealDelay = 3f;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Money Reveal")]
    [SerializeField] private float moneyStageDelay = 0.5f;
    [SerializeField] private float addedMoneyFadeDuration = 0.12f;
    [SerializeField] private float addedMoneyMoveDuration = 0.45f;
    [SerializeField, Range(0f, 1f)] private float totalUpdateMoveProgress = 0.7f;
    [SerializeField] private SoundId moneySound = SoundId.Money;
    [SerializeField, Range(0f, 1f)] private float moneySoundVolume = 1f;
    
    [Header("NextScene")]
    [SerializeField] private string NextScene;

    private RectTransform _addedMoneyRect;
    private Vector2 _addedMoneyStartPosition;
    private float _addedMoneyVisibleAlpha = 1f;
    private int _stagedMoneyTotal;
    private Coroutine _addedMoneyFadeRoutine;

    private void Awake()
    {
        CacheAddedMoneyText();
        ResetAddedMoneyText();
    }

    void Start()
    {
        canvasGroup.alpha = 0;
        billFixed.ClearMoney();
        billOrgan.ClearMoney();
        totalMoneyText.text = HealthBars.Instance.money.ToString();
        StartCoroutine(RevealBillsRoutine());
    }

    private IEnumerator RevealBillsRoutine()
    {
        yield return new WaitForSecondsRealtime(revealDelay);
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;
        
        var healthBars = HealthBars.Instance;
        int[] wageIncomeStages = healthBars.CollectWageMoneyStages();
        int[] organIncomeStages = healthBars.CollectOrganMoneyStages();
        
        yield return billFixed.ShowMoneyRoutine(wageIncomeStages, moneyStageDelay, moneySound, moneySoundVolume, AddMoneyStage);
        yield return billOrgan.ShowMoneyRoutine(organIncomeStages, moneyStageDelay, moneySound, moneySoundVolume, AddMoneyStage);
        
        healthBars.money += _stagedMoneyTotal;
        yield return MoveAddedMoneyToTotalRoutine();

        totalMoneyText.text = healthBars.money.ToString();
        yield return new WaitForSecondsRealtime(2f);
        
        SceneController.Instance.LoadScene(NextScene);
    }

    private void AddMoneyStage(int amount)
    {
        _stagedMoneyTotal += amount;

        if (!addedMoneyText) return;

        addedMoneyText.text = $"+{_stagedMoneyTotal:N0}";

        if (GetAddedMoneyAlpha() > 0f)
        {
            if (_addedMoneyFadeRoutine != null)
            {
                StopCoroutine(_addedMoneyFadeRoutine);
                _addedMoneyFadeRoutine = null;
            }

            SetAddedMoneyAlpha(_addedMoneyVisibleAlpha);
            return;
        }

        if (_addedMoneyFadeRoutine != null)
        {
            StopCoroutine(_addedMoneyFadeRoutine);
        }

        _addedMoneyFadeRoutine = StartCoroutine(FadeAddedMoneyRoutine(0f, _addedMoneyVisibleAlpha, addedMoneyFadeDuration));
    }

    private IEnumerator FadeAddedMoneyRoutine(float startAlpha, float endAlpha, float duration)
    {
        duration = Mathf.Max(0f, duration);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);
            SetAddedMoneyAlpha(Mathf.Lerp(startAlpha, endAlpha, t));
            yield return null;
        }

        SetAddedMoneyAlpha(endAlpha);
        _addedMoneyFadeRoutine = null;
    }

    private IEnumerator MoveAddedMoneyToTotalRoutine()
    {
        if (!addedMoneyText || !_addedMoneyRect || !totalMoneyText || _stagedMoneyTotal <= 0)
        {
            yield break;
        }

        if (_addedMoneyFadeRoutine != null)
        {
            StopCoroutine(_addedMoneyFadeRoutine);
            _addedMoneyFadeRoutine = null;
        }

        RectTransform totalMoneyRect = totalMoneyText.transform as RectTransform;
        if (!totalMoneyRect) yield break;

        Vector2 startPosition = _addedMoneyRect.anchoredPosition;
        Vector2 endPosition = GetTargetAnchoredPosition(totalMoneyRect);
        float duration = Mathf.Max(0f, addedMoneyMoveDuration);
        float elapsed = 0f;
        bool totalUpdated = false;
        SetAddedMoneyAlpha(_addedMoneyVisibleAlpha);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration <= 0f ? 1f : Mathf.Clamp01(elapsed / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            if (!totalUpdated && t >= totalUpdateMoveProgress)
            {
                totalMoneyText.text = HealthBars.Instance.money.ToString();
                totalUpdated = true;
            }

            _addedMoneyRect.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, t);
            SetAddedMoneyAlpha(Mathf.Lerp(_addedMoneyVisibleAlpha, 0f, t));
            yield return null;
        }

        if (!totalUpdated)
        {
            totalMoneyText.text = HealthBars.Instance.money.ToString();
        }

        ResetAddedMoneyText();
    }

    private Vector2 GetTargetAnchoredPosition(RectTransform targetRect)
    {
        if (_addedMoneyRect.parent == targetRect.parent)
        {
            return targetRect.anchoredPosition;
        }

        RectTransform addedMoneyParent = _addedMoneyRect.parent as RectTransform;
        if (!addedMoneyParent) return _addedMoneyStartPosition;

        Vector3 targetWorldPosition = targetRect.TransformPoint(targetRect.rect.center);
        Vector3 targetLocalPosition = addedMoneyParent.InverseTransformPoint(targetWorldPosition);
        return targetLocalPosition;
    }

    private void CacheAddedMoneyText()
    {
        if (!addedMoneyText) return;

        _addedMoneyRect = addedMoneyText.transform as RectTransform;
        if (_addedMoneyRect)
        {
            _addedMoneyStartPosition = _addedMoneyRect.anchoredPosition;
        }

        _addedMoneyVisibleAlpha = addedMoneyText.color.a;
    }

    private void ResetAddedMoneyText()
    {
        _stagedMoneyTotal = 0;

        if (!addedMoneyText) return;

        addedMoneyText.text = string.Empty;
        SetAddedMoneyAlpha(0f);

        if (_addedMoneyRect)
        {
            _addedMoneyRect.anchoredPosition = _addedMoneyStartPosition;
        }
    }

    private float GetAddedMoneyAlpha()
    {
        return addedMoneyText ? addedMoneyText.color.a : 0f;
    }

    private void SetAddedMoneyAlpha(float alpha)
    {
        if (!addedMoneyText) return;

        Color color = addedMoneyText.color;
        color.a = alpha;
        addedMoneyText.color = color;
    }
}
