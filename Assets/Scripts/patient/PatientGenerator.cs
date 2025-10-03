using UnityEngine;
using UnityEngine.SceneManagement;

public class PatientGenerator : MonoBehaviour
{
    public PatientData patientData;
    public PatientChartUI patient1UI;
    public PatientChartUI patient2UI;

    private Patient _patient1;
    private Patient _patient2;


    void Start()
    {
        _patient1 = GeneratePatient();
        _patient2 = GeneratePatient();
        
        patient1UI.Bind(_patient1, OnPatientSelected);
        patient2UI.Bind(_patient2, OnPatientSelected);
    }
    public Patient GeneratePatient()
    {
        Patient p = new Patient();
        
        bool isMale = Random.value > 0.5f;
        if (isMale)
        {
             p.firstName = patientData.maleFirstNames[Random.Range(0, patientData.maleFirstNames.Count)];
             p.face = patientData.maleFaces[Random.Range(0, patientData.maleFaces.Count)];
        }
        else
        {
            p.firstName = patientData.femaleFirstNames[Random.Range(0, patientData.femaleFirstNames.Count)];
            p.face = patientData.femaleFaces[Random.Range(0, patientData.femaleFaces.Count)];
        }
        
        p.lastName = patientData.lastNames[Random.Range(0, patientData.lastNames.Count)];
        p.age = Random.Range(patientData.minAge, patientData.maxAge + 1);
        p.job = patientData.jobs[Random.Range(0, patientData.jobs.Count)];
        p.personality = patientData.personalities[Random.Range(0, patientData.personalities.Count)];
        p.trait = patientData.traits[Random.Range(0, patientData.traits.Count)];

        return p;
    }

    private void OnPatientSelected(Patient chosen)
    {
        HealthBars.Instance.SetSelectedPatient(chosen);
        
        SceneManager.LoadScene("Surgery test"); 
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
    public Sprite face;
    public string FullName => $"{firstName} {lastName}";
}