using UnityEngine;

public class PatientGenerator : MonoBehaviour
{
    public PatientData patientData;
    public PatientChartUI patient1UI;
    public PatientChartUI patient2UI;

    private Patient patient1;
    private Patient patient2;


    void Start()
    {
        patient1 = GeneratePatient();
        patient2 = GeneratePatient();
        
        patient1UI.Bind(patient1);
        patient2UI.Bind(patient2);
    }
    public Patient GeneratePatient()
    {
        Patient p = new Patient();
        
        bool isMale = Random.value > 0.5f;
        if (isMale)
            p.firstName = patientData.maleFirstNames[Random.Range(0, patientData.maleFirstNames.Count)];
        else
            p.firstName = patientData.femaleFirstNames[Random.Range(0, patientData.femaleFirstNames.Count)];
        p.lastName = patientData.lastNames[Random.Range(0, patientData.lastNames.Count)];
        p.age = Random.Range(patientData.minAge, patientData.maxAge + 1);
        p.job = patientData.jobs[Random.Range(0, patientData.jobs.Count)];
        p.personality = patientData.personalities[Random.Range(0, patientData.personalities.Count)];
        p.trait = patientData.traits[Random.Range(0, patientData.traits.Count)];

        return p;
    }
}

[System.Serializable]
public class Patient
{
    public string firstName;
    public string lastName;
    public int age;
    public string job;
    public string personality;
    public string trait;

    public string FullName => $"{firstName} {lastName}";
}