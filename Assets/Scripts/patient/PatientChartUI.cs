using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatientChartUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text ageText;
    public TMP_Text jobText;
    public TMP_Text traitText;
    public TMP_Text personalityText;
    public TMP_Text sexText;
    public Image patientImage;
    public Button selectButton; 
    
    private Patient _shownPatient;

    public void Bind(Patient patient, System.Action<Patient> onSelect)
    {
        _shownPatient = patient;

        nameText.text = patient.FullName;
        ageText.text = patient.age.ToString();
        jobText.text = patient.job;
        traitText.text = patient.trait;
        personalityText.text = patient.personality.ToString();
        sexText.text = patient.sex;
        patientImage.sprite = patient.face;
        
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => onSelect(_shownPatient));
    }
}
