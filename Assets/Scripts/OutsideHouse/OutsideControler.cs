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
    [SerializeField] private float revealDelay = 3f;
    [SerializeField] private float fadeDuration = 0.5f;
    
    [Header("NextScene")]
    [SerializeField] private string NextScene;
    void Start()
    {
        canvasGroup.alpha = 0;     
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
        int fixedIncome = 24;
        int organIncome = healthBars.organMoney;
        
        billFixed.ShowMoney(fixedIncome);
        billOrgan.ShowMoney(organIncome);
        
        int total = fixedIncome + organIncome;
        healthBars.money += total;
        
        totalMoneyText.text = healthBars.money.ToString();
        yield return new WaitForSecondsRealtime(2f);
        
        SceneController.Instance.LoadScene(NextScene);
    }
}
