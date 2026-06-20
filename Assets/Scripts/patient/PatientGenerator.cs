using UnityEngine;
using UnityEngine.SceneManagement;
using MiniGames;
using UnityEngine.Serialization;

public class PatientGenerator : MonoBehaviour
{
    public PatientData patientData;
    public PatientChartUI patient1UI;
    public PatientChartUI patient2UI;

    [Header("Major MiniGame")]
    [FormerlySerializedAs("forceDebugMiniGame")]
    [SerializeField] private bool forceMiniGame;
    [SerializeField] private MajorMiniGameType forcedMiniGame = MajorMiniGameType.DebugButtons;
    [SerializeField] private MajorMiniGameOption[] majorMiniGamePool =
    {
        new MajorMiniGameOption(MajorMiniGameType.Maze, "Artery Navigation"),
        new MajorMiniGameOption(MajorMiniGameType.Wordle, "Organ Sequencing"),
        new MajorMiniGameOption(MajorMiniGameType.Fishing, "Bacteria Fishing")
    };

    private Patient _patient1;
    private Patient _patient2;
    private bool _selectionStarted;


    void Start()
    {
        _patient1 = GeneratePatient();
        _patient2 = GeneratePatient();
        
        patient1UI.Bind(_patient1, OnPatientSelected, OnPatientSelectionStarted);
        patient2UI.Bind(_patient2, OnPatientSelected, OnPatientSelectionStarted);
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
        p.jobProfile = patientData.GetRandomJob();
        p.personalityProfile = patientData.GetRandomPersonality();
        p.traitProfile = patientData.GetRandomTrait();
        p.baseOrganMoney = patientData.baseOrganMoney;

        p.job = p.jobProfile.job;
        p.personality = p.personalityProfile.personality;
        p.trait = p.traitProfile.trait;
        
        MajorMiniGameOption majorMiniGame = PickMajorMiniGame();
        p.majorMiniGame = majorMiniGame.type;
        p.majorMiniGameName = majorMiniGame.displayName;

        AssignPatientVoice(p);

        return p;
    }

    private static void AssignPatientVoice(Patient patient)
    {
        if (patient == null || !AudioManager.Instance) return;

        patient.voiceClip = AudioManager.Instance.GetRandomPatientVoice(patient.sex);
    }

    private MajorMiniGameOption PickMajorMiniGame()
    {
        if (MajorMiniGameDebugSettings.ForceDebugMiniGame)
        {
            return GetMajorMiniGameOption(MajorMiniGameType.DebugButtons);
        }

        if (forceMiniGame)
        {
            return GetMajorMiniGameOption(forcedMiniGame);
        }

        if (majorMiniGamePool == null || majorMiniGamePool.Length == 0)
        {
            return new MajorMiniGameOption(MajorMiniGameType.Maze, GetDefaultMiniGameDisplayName(MajorMiniGameType.Maze));
        }

        return majorMiniGamePool[Random.Range(0, majorMiniGamePool.Length)];
    }

    private MajorMiniGameOption GetMajorMiniGameOption(MajorMiniGameType type)
    {
        if (majorMiniGamePool != null)
        {
            foreach (MajorMiniGameOption option in majorMiniGamePool)
            {
                if (option != null && option.type == type)
                {
                    return option;
                }
            }
        }

        return new MajorMiniGameOption(type, GetDefaultMiniGameDisplayName(type));
    }

    private static string GetDefaultMiniGameDisplayName(MajorMiniGameType type)
    {
        switch (type)
        {
            case MajorMiniGameType.Maze:
                return "Artery Navigation";
            case MajorMiniGameType.DebugButtons:
                return "Debug Buttons";
            case MajorMiniGameType.Wordle:
                return "Organ Sequencing";
            case MajorMiniGameType.Fishing:
                return "Bacteria Fishing";
            default:
                return type.ToString();
        }
    }

    private void OnPatientSelected(Patient chosen)
    {
        if (chosen == null)
        {
            Debug.LogError($"{nameof(PatientGenerator)} received an empty patient selection.", this);
            return;
        }

        if (!HealthBars.Instance)
        {
            Debug.LogError($"{nameof(PatientGenerator)} cannot select a patient because no {nameof(HealthBars)} exists.", this);
            return;
        }

        HealthBars.Instance.SetSelectedPatient(chosen);
        Debug.Log($"Selected patient '{chosen.FullName}' with major minigame '{chosen.majorMiniGameName}' ({chosen.majorMiniGame}).", this);
        
        SceneController.Instance.LoadScene("TalkToPatient");
    }

    private void OnPatientSelectionStarted(Patient chosen)
    {
        if (_selectionStarted) return;

        _selectionStarted = true;
        patient1UI?.SetSelectionEnabled(false);
        patient2UI?.SetSelectionEnabled(false);
        HealthBars.Instance?.SetSelectedPatient(chosen);
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
    public PatientJob jobProfile;
    public PersonalityDialogue personalityProfile;
    public PatientTrait traitProfile;
    public int baseOrganMoney;
    public string sex;
    public AudioClip voiceClip;
    public Sprite face;
    public Sprite body;
    public MiniGames.MajorMiniGameType majorMiniGame;
    public string majorMiniGameName;
    public string FullName => $"{firstName} {lastName}";
    public int ReputationChangeWhenSaved => jobProfile?.reputationChangeWhenSaved ?? 4;
    public int ReputationChangeWhenKilled => jobProfile?.reputationChangeWhenKilled ?? -10;
    public int PsycheChangeWhenSaved => personalityProfile?.psycheChangeWhenSaved ?? 3;
    public int PsycheChangeWhenKilled => personalityProfile?.psycheChangeWhenKilled ?? -15;
    public int OrganMoney => Mathf.Max(0, Mathf.RoundToInt(baseOrganMoney * (traitProfile?.organMoneyMultiplier ?? 1f)) + (traitProfile?.organMoneyBonus ?? 0));
}
