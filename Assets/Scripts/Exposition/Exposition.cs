using System;
using System.Collections;
using UnityEngine;

public class Exposition : DialogueBase
{
    [SerializeField] private float waitAfterText = 2f;
    [SerializeField] private string nextSceneName;
    
    [TextArea(5, 10)]
    [SerializeField] private string expositionText = 
        "I got a letter in the mail today-\n\n" +
        "It mentioned an opportunity to earn more money... in exchange for some... cruel acts.\n" +
        "Organs. They want me to possibly kill my patients for their organs and put it in the mop bucket.\n" +
        "I do not know who sent it. But my family... We need the money.\n" +
        "But is it right of me to kill for my own survival? I have to make a choice.";

    private void Start()
    {
        StartCoroutine(PlayExpsition());
    }

    private IEnumerator PlayExpsition()
    {
        PlayLine(expositionText);
        
        float duration = expositionText.Length * charDelay + waitAfterText;
        yield return new WaitForSecondsRealtime(duration); 
        
        SceneController.Instance.LoadScene(nextSceneName);
    }
}
