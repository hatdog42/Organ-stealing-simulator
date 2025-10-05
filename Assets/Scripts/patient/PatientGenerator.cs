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
            
            int index = Random.Range(0, patientData.sprites.maleBodies.Count);
            p.face = patientData.sprites.maleFaces[index];
            p.body = patientData.sprites.maleBodies[index];
            
            p.sex = "M";
        }
        else
        {
            p.firstName = patientData.femaleFirstNames[Random.Range(0, patientData.femaleFirstNames.Count)];
            
            int index = Random.Range(0, patientData.sprites.femaleBodies.Count);
            p.face = patientData.sprites.femaleFaces[index];
            p.body = patientData.sprites.femaleBodies[index];
           
            p.sex = "F";
        }
        
        p.lastName = patientData.lastNames[Random.Range(0, patientData.lastNames.Count)];
        p.age = Random.Range(patientData.minAge, patientData.maxAge + 1);
        p.job = patientData.jobs[Random.Range(0, patientData.jobs.Count)];
        p.personality = patientData.personalities[Random.Range(0, patientData.personalities.Count)].personality;
        p.trait = patientData.traits[Random.Range(0, patientData.traits.Count)];

        return p;
    }

    private void OnPatientSelected(Patient chosen)
    {
        HealthBars.Instance.SetSelectedPatient(chosen);
        
        SceneController.Instance.LoadScene("TalkToPatient");
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
    public string sex;
    public Sprite face;
    public Sprite body;
    public string FullName => $"{firstName} {lastName}";
}