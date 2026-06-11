using System.Collections;
using System;
using TMPro;
using UnityEngine;

public class bill : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;   
    [SerializeField] private int totalMoney;

    public void ShowMoney(int total)
    {
        if (!_text) return;

        ClearMoney();

        int[] parts = GetMoneyParts(total);
        for (int i = 0; i < parts.Length; i++)
            _text.text += $"${parts[i]:N0}\n";
    }

    public IEnumerator ShowMoneyRoutine(
        int total,
        float delayBetweenStages,
        AudioClip moneySound,
        float soundVolume,
        Action<int> onStageRevealed = null)
    {
        if (!_text) yield break;

        ClearMoney();
        delayBetweenStages = Mathf.Max(0f, delayBetweenStages);

        int[] parts = GetMoneyParts(total);
        for (int i = 0; i < parts.Length; i++)
        {
            _text.text += $"${parts[i]:N0}\n";
            onStageRevealed?.Invoke(parts[i]);
            PlayMoneySound(moneySound, soundVolume);

            yield return new WaitForSecondsRealtime(delayBetweenStages);
        }
    }

    public void ClearMoney()
    {
        if (_text) _text.text = string.Empty;
    }

    private int[] GetMoneyParts(int total)
    {
        int displayTotal = totalMoney > 0 ? totalMoney : total;
        int basePart = displayTotal / 3;
        int remainder = displayTotal % 3;

        int[] parts = { basePart, basePart, basePart };
        for (int i = 0; i < remainder; i++)
            parts[i]++;

        return parts;
    }

    private static void PlayMoneySound(AudioClip moneySound, float soundVolume)
    {
        if (!AudioManager.Instance || !moneySound) return;

        AudioManager.Instance.PlaySfx(moneySound, soundVolume);
    }
}
