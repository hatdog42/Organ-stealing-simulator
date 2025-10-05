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
    
    void Start()
    {
        var selectedPatient = HealthBars.Instance?.SelectedPatient;
        
        if (patientImage) patientImage.sprite = selectedPatient.face;
        if (patientName) patientName.text = selectedPatient.FullName;

        string line = patientData.GetRandomLine(selectedPatient.personality);
        
        PlayLine(line);
    }

    
}
