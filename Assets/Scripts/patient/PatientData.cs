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

    [Header("Balance")]
    [Min(0)] public int baseOrganMoney = 14;

    [Header("Jobs")]
    public List<PatientJob> jobs = new();

    [Header("Personalities (Nurse Notes)")]
    public List<PersonalityDialogue> personalities = new();

    [Header("Traits")]
    public List<PatientTrait> traits = new();
    
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

    public PatientJob GetRandomJob()
    {
        if (jobs == null || jobs.Count == 0)
            return new PatientJob { job = "Unemployed", reputationChangeWhenSaved = 2, reputationChangeWhenKilled = -6 };

        return jobs[Random.Range(0, jobs.Count)];
    }

    public PersonalityDialogue GetRandomPersonality()
    {
        if (personalities == null || personalities.Count == 0)
            return new PersonalityDialogue { personality = "Quiet", psycheChangeWhenSaved = 2, psycheChangeWhenKilled = -12 };

        return personalities[Random.Range(0, personalities.Count)];
    }

    public PatientTrait GetRandomTrait()
    {
        if (traits == null || traits.Count == 0)
            return new PatientTrait { trait = "Average", organMoneyMultiplier = 1f };

        return traits[Random.Range(0, traits.Count)];
    }
}

[System.Serializable]
public class PatientJob
{
    public string job;
    [Tooltip("Reputation change if this patient survives surgery.")]
    public int reputationChangeWhenSaved = 4;
    [Tooltip("Reputation change if this patient dies.")]
    public int reputationChangeWhenKilled = -10;
}

[System.Serializable]
public class PersonalityDialogue
{
    public string personality;
    [Tooltip("Psyche change if this patient survives surgery.")]
    public int psycheChangeWhenSaved = 3;
    [Tooltip("Psyche change if this patient dies.")]
    public int psycheChangeWhenKilled = -15;
    [TextArea(2, 5)] public List<string> dialogueLines = new();
}

[System.Serializable]
public class PatientTrait
{
    public string trait;
    [Min(0f)] public float organMoneyMultiplier = 1f;
    public int organMoneyBonus;
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
