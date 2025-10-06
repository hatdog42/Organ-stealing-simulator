using TMPro;
using UnityEngine;

public class bill : MonoBehaviour
{
    [SerializeField] private TMP_Text _text;   
    [SerializeField] private int totalMoney;
    

    public void ShowMoney(int total)
    {
        int basePart = totalMoney / 3;
        int remainder = totalMoney % 3;

        int[] parts = { basePart, basePart, basePart };
        for (int i = 0; i < remainder; i++)
            parts[i]++; 
        
        for (int i = 0; i < 3; i++)
            _text.text += $"${parts[i]:N0}\n";
    }
}
