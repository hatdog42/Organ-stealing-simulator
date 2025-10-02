using TMPro;
using UnityEngine;

public class PatientChartUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text ageText;
    public TMP_Text jobText;
    public TMP_Text traitText;
    public TMP_Text personalityText;
    
    private Patient _shownPatient;

    public void Bind(Patient patient)
    {
        _shownPatient = patient;

        nameText.text = patient.FullName;
        ageText.text = patient.age.ToString();
        jobText.text = patient.job;
        traitText.text = patient.trait;
        personalityText.text = patient.personality.ToString();
    }
}
