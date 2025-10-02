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
    public List<string> personalities;

    [Header("Traits")]
    public List<string> traits;
}
