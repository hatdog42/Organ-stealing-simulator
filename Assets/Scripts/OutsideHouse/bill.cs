using System.Collections;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class bill : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;   

    public void ShowMoney(int total)
    {
        if (!_text) return;

        ClearMoney();

        int[] parts = GetMoneyParts(total);
        ShowMoney(parts);
    }

    public void ShowMoney(IList<int> amounts)
    {
        if (!_text || amounts == null) return;

        ClearMoney();

        for (int i = 0; i < amounts.Count; i++)
            _text.text += $"${amounts[i]:N0}\n";
    }

    public IEnumerator ShowMoneyRoutine(
        int total,
        float delayBetweenStages,
        AudioClip moneySound,
        float soundVolume,
        Action<int> onStageRevealed = null)
    {
        int[] parts = GetMoneyParts(total);
        yield return ShowMoneyRoutine(parts, delayBetweenStages, moneySound, soundVolume, onStageRevealed);
    }

    public IEnumerator ShowMoneyRoutine(
        IList<int> amounts,
        float delayBetweenStages,
        AudioClip moneySound,
        float soundVolume,
        Action<int> onStageRevealed = null)
    {
        if (!_text || amounts == null) yield break;

        ClearMoney();
        delayBetweenStages = Mathf.Max(0f, delayBetweenStages);

        for (int i = 0; i < amounts.Count; i++)
        {
            _text.text += $"${amounts[i]:N0}\n";
            onStageRevealed?.Invoke(amounts[i]);
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
        int basePart = total / 3;
        int remainder = total % 3;

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
