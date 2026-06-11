using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Exposition : DialogueBase
{
    [SerializeField] private float waitAfterText = 2f;
    [SerializeField] private string nextSceneName;

    [Header("Letter Formatting")]
    [SerializeField, Min(1f)] private float letterFontSize = 28f;
    [SerializeField] private Vector4 letterTextMargin = new(36f, 32f, 36f, 32f);
    [SerializeField] private TextAlignmentOptions letterAlignment = TextAlignmentOptions.TopLeft;
    
    [TextArea(5, 10)]
    [SerializeField] private string expositionText = 
        "I got a letter in the mail today-\n\n" +
        "It mentioned an opportunity to earn more money... in exchange for some... cruel acts.\n" +
        "Organs. They want me to possibly kill my patients for their organs and put it in the mop bucket.\n" +
        "I do not know who sent it. But my family... We need the money.\n" +
        "But is it right of me to kill for my own survival? I have to make a choice.";

    protected override bool UsePrefabDialogue => false;

    protected override void Awake()
    {
        base.Awake();
        ApplyLetterFormatting();
    }

    private void Start()
    {
        StartCoroutine(PlayExpsition());
    }

    private IEnumerator PlayExpsition()
    {
        PlayLine(expositionText);
        
        while (Typing != null) yield return null;
        yield return new WaitForSecondsRealtime(waitAfterText); 
        
        SceneController.Instance.LoadScene(nextSceneName);
    }

    private void ApplyLetterFormatting()
    {
        if (!dialogueText) return;

        dialogueText.fontSize = letterFontSize;
        dialogueText.alignment = letterAlignment;
        dialogueText.margin = letterTextMargin;
        dialogueText.textWrappingMode = TextWrappingModes.Normal;
        dialogueText.overflowMode = TextOverflowModes.Overflow;
    }
}
