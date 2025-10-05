using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PatientData", menuName = "Scriptable Objects/PatientData")]
public class PatientData : ScriptableObject
{
    [Header("Name Pools")]
    public List<string> maleFirstNames;
    public List<string> femaleFirstNames;
    public List<string> lastNames;

    [Header("Age Range")]
    public int minAge = 18;
    public int maxAge = 115;

    [Header("Jobs")]
    public List<string> jobs;

    [Header("Personalities (Nurse Notes)")]
    public List<PersonalityDialogue> personalities = new();

    [Header("Traits")]
    public List<string> traits;
    
    [Header("Faces")]
    public PatientSprites sprites;
    
    public string GetRandomLine(string personality)
    {
        var entry = personalities.Find(p => p.personality == personality);
        if (entry == null || entry.dialogueLines.Count == 0)
            return "The patient stays silent...";
    
        int index = Random.Range(0, entry.dialogueLines.Count);
        return entry.dialogueLines[index];
    }
}

[System.Serializable]
public class PersonalityDialogue
{
    public string personality;
    [TextArea(2, 5)] public List<string> dialogueLines = new();
}

[System.Serializable]
public class PatientSprites
{
    [Header("Male Sprites")]
    public List<Sprite> maleFaces;
    public List<Sprite> maleBodies;
    
    [Header("Female Sprites")]
    public List<Sprite> femaleFaces;
    public List<Sprite> femaleBodies;
}