using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PatientTallking : MonoBehaviour
{
    public Image PatientImage;
    public TMP_Text patientName;

    void Start()
    {
        PatientImage.sprite = HealthBars.Instance.SelectedPatient.face;
        patientName.text = HealthBars.Instance.SelectedPatient.FullName;
    }
}
