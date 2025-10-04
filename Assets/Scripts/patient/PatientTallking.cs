using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatientTallking : DialogueBase
{
     [Header("UI")]
     [SerializeField]private Image patientImage;
     [SerializeField]private TMP_Text patientName;
     [SerializeField] private TMP_Text dialogueText;
     
     [Header("Typing")]
     [SerializeField, Range(0.001f, 0.1f)] private float charDelay = 0.02f;
     
     [Header("Data")]
     [SerializeField] private PatientData patientData;
     
     private Coroutine _typing;

    
    
    void Start()
    {
        var selectedPatient = HealthBars.Instance?.SelectedPatient;
        
        if (patientImage) patientImage.sprite = selectedPatient.face;
        if (patientName) patientName.text = selectedPatient.FullName;

        string line = patientData.GetRandomLine(selectedPatient.personality);
        
        PlayLine(line);
    }

    
}
