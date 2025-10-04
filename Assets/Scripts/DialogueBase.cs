using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueBase : MonoBehaviour
{
    [Header("UI (required)")]
    [SerializeField] protected TMP_Text dialogueText;
    
    [Header("Typing")]
    [SerializeField, Range(0.001f, 0.1f)] protected float charDelay = 0.02f;
    
    protected Coroutine Typing;
    
    protected void PlayLine(string line)
    {
        if (Typing != null) StopCoroutine(Typing);
        Typing = StartCoroutine(TypeRoutine(line ?? string.Empty));
    }
    private IEnumerator TypeRoutine(string line)
    {
        if (!dialogueText) yield break;

        dialogueText.text = "";
        int i = 0;

        while (i < line.Length)
        {
            dialogueText.text += line[i];
            
            float delay = charDelay;
            char c = line[i];

            i++;
            yield return new WaitForSecondsRealtime(delay);
        }
        Typing = null;
    }
}
