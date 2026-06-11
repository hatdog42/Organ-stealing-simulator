using UnityEngine;
using UnityEngine.SceneManagement;
using MiniGames;

public class PatientGenerator : MonoBehaviour
{
    public PatientData patientData;
    public PatientChartUI patient1UI;
    public PatientChartUI patient2UI;

    [Header("Major MiniGame")]
    [SerializeField] private bool forceDebugMiniGame = true;
    [SerializeField] private MajorMiniGameOption[] majorMiniGamePool =
    {
        new MajorMiniGameOption(MajorMiniGameType.DebugButtons, "Debug Buttons"),
        new MajorMiniGameOption(MajorMiniGameType.Maze, "Maze")
    };

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

        return p;
    }

    private MajorMiniGameOption PickMajorMiniGame()
    {
        if (forceDebugMiniGame)
        {
            return new MajorMiniGameOption(MajorMiniGameType.DebugButtons, "Debug Buttons");
        }

        if (majorMiniGamePool == null || majorMiniGamePool.Length == 0)
        {
            return new MajorMiniGameOption(MajorMiniGameType.Maze, "Maze");
        }

        return majorMiniGamePool[Random.Range(0, majorMiniGamePool.Length)];
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
    public PatientJob jobProfile;
    public PersonalityDialogue personalityProfile;
    public PatientTrait traitProfile;
    public int baseOrganMoney;
    public string sex;
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
