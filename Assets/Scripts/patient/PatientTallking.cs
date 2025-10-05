using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatientTallking : DialogueBase
{
     [Header("UI")]
     [SerializeField]private Image patientImage;
     [SerializeField]private TMP_Text patientName;
     
     [Header("Data")]
     [SerializeField] private PatientData patientData;
    
     [Header("NextScene")]
     [SerializeField] private string nextScene;
    void Start()
    {
        var selectedPatient = HealthBars.Instance?.SelectedPatient;
        
        if (patientImage) patientImage.sprite = selectedPatient.body;
        if (patientName) patientName.text = selectedPatient.FullName;

        string line = patientData.GetRandomLine(selectedPatient.personality);

        StartCoroutine(PatientDialogue(line));
    }

    private IEnumerator PatientDialogue(string line)
    {
        yield return new WaitForSecondsRealtime(1f);

        PlayLine(line);
        float duration = line.Length * charDelay + 1f;
        yield return new WaitForSecondsRealtime(duration);
        
        SceneController.Instance.LoadScene(nextScene);
    }
}