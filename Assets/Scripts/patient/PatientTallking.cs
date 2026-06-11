using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatientTallking : DialogueBase
{
     [Header("UI")]
     [SerializeField]private Image patientImage;
     [SerializeField]private TMP_Text patientName;
     [SerializeField] private GameObject legacyPatientDialogueRoot;
     
     [Header("Data")]
     [SerializeField] private PatientData patientData;
    
     [Header("NextScene")]
     [SerializeField] private string nextScene;

     [Header("Timing")]
     [SerializeField, Min(0f)] private float waitBeforeDialogue = 1f;
     [SerializeField, Min(0f)] private float waitAfterDialogue = 1f;

     private string currentPatientName;

     protected override bool ShowTextBoxAtTop => false;
     protected override string DialogueSpeakerName => currentPatientName;

     protected override void Awake()
     {
         base.Awake();
         HideLegacyPatientDialogueBackground();
     }

    void Start()
    {
        var selectedPatient = HealthBars.Instance?.SelectedPatient;

        if (selectedPatient == null)
        {
            Debug.LogError($"{nameof(PatientTallking)} could not find a selected patient.", this);
            return;
        }
        
        if (patientImage) patientImage.sprite = selectedPatient.body;
        else Debug.LogError($"{nameof(PatientTallking)} is missing a patient image reference.", this);

        currentPatientName = selectedPatient.FullName;
        SetDialogueSpeakerName(currentPatientName);
        if (patientName) patientName.gameObject.SetActive(false);

        string line = patientData.GetRandomLine(selectedPatient.personality);

        StartCoroutine(PatientDialogue(line));
    }

    private IEnumerator PatientDialogue(string line)
    {
        yield return new WaitForSecondsRealtime(waitBeforeDialogue);

        PlayLine(line);

        while (Typing != null) yield return null;
        
        yield return new WaitForSecondsRealtime(waitAfterDialogue);
        
        SceneController.Instance.LoadScene(nextScene);
    }

    private void HideLegacyPatientDialogueBackground()
    {
        if (!PrefabDialogueEnabled || !legacyPatientDialogueRoot) return;

        legacyPatientDialogueRoot.SetActive(false);
    }
}
