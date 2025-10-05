using TMPro;
using UnityEngine;

public class ShowMoney : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

    private void Update()
    {
        moneyText.text = HealthBars.Instance.money.ToString();
    }
}

