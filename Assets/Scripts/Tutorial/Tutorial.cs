using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : DialogueBase
{
    private static bool hasPlayedChoosePatientTutorial;

    [SerializeField] private bool playTutorial = true;
    [SerializeField] private string choosePatientSceneName = "ChosePatient";
    [SerializeField] private bool playOncePerSession = true;
    [SerializeField, Min(0f)] private float waitBeforeFirstLine = 0.5f;
    [SerializeField] private string speakerName = "Dr. Shad Iman";
    [SerializeField] private bool hidePatientChoicesDuringTutorial = true;

    [SerializeField, TextArea(2, 4)] private string[] choosePatientLines =
    {
        "I presume you are the new doctor. I am Dr. Shad Iman. Your first charts are on the desk. Read them closely.",
        "A patient's trait hints at what the inside market might pay for certain... assets.",
        "You have to be wary of important people. Lose a beloved citizen and people whisper. Lose a nobody and fewer doors close.",
        "The nurse's note is a glimpse under the mask. You should not feel too bad if a douchebag has an accident."
    };

    private PatientGenerator patientGenerator;
    private OfficeControler officeControler;
    private bool shouldPlayTutorial;
    private bool patientGeneratorWasEnabled;
    private bool officeControlerWasEnabled;
    private GameObject patient1Root;
    private GameObject patient2Root;
    private bool patient1WasActive;
    private bool patient2WasActive;

    protected override string DialogueSpeakerName => speakerName;
    protected override bool ShowTextBoxAtTop => false;
    protected override bool HideTextBoxAfterLine => false;

    protected override void Awake()
    {
        base.Awake();

        shouldPlayTutorial = ShouldPlayChoosePatientTutorial();
        if (!shouldPlayTutorial) return;

        patientGenerator = GetComponent<PatientGenerator>();
        if (!patientGenerator) patientGenerator = FindAnyObjectByType<PatientGenerator>();

        officeControler = FindAnyObjectByType<OfficeControler>();

        if (patientGenerator)
        {
            patientGeneratorWasEnabled = patientGenerator.enabled;
            patientGenerator.enabled = false;
        }

        if (officeControler)
        {
            officeControlerWasEnabled = officeControler.enabled;
            officeControler.enabled = false;
        }

        if (patientGenerator && hidePatientChoicesDuringTutorial)
        {
            CacheAndSetPatientChoiceVisible(patientGenerator.patient1UI, false, ref patient1Root, ref patient1WasActive);
            CacheAndSetPatientChoiceVisible(patientGenerator.patient2UI, false, ref patient2Root, ref patient2WasActive);
        }
    }

    private void Start()
    {
        if (!shouldPlayTutorial)
        {
            enabled = false;
            return;
        }

        StartCoroutine(PlayChoosePatientTutorial());
    }

    private IEnumerator PlayChoosePatientTutorial()
    {
        hasPlayedChoosePatientTutorial = true;

        if (waitBeforeFirstLine > 0f)
        {
            yield return new WaitForSecondsRealtime(waitBeforeFirstLine);
        }

        foreach (string line in choosePatientLines)
        {
            PlayLine(line);
            while (Typing != null) yield return null;
        }

        yield return HideTextBox();
        RestorePatientChoices();

        if (patientGenerator && patientGeneratorWasEnabled)
        {
            patientGenerator.enabled = true;
        }

        if (officeControler && officeControlerWasEnabled)
        {
            officeControler.enabled = true;
        }

        enabled = false;
    }

    private bool ShouldPlayChoosePatientTutorial()
    {
        if (!playTutorial) return false;
        if (playOncePerSession && hasPlayedChoosePatientTutorial) return false;
        return SceneManager.GetActiveScene().name == choosePatientSceneName;
    }

    private static void CacheAndSetPatientChoiceVisible(
        PatientChartUI patientChart,
        bool visible,
        ref GameObject patientRoot,
        ref bool wasActive)
    {
        if (!patientChart) return;

        patientRoot = patientChart.gameObject;
        wasActive = patientRoot.activeSelf;
        patientRoot.SetActive(visible);
    }

    private void RestorePatientChoices()
    {
        if (!hidePatientChoicesDuringTutorial) return;

        if (patient1Root) patient1Root.SetActive(patient1WasActive);
        if (patient2Root) patient2Root.SetActive(patient2WasActive);
    }
}
