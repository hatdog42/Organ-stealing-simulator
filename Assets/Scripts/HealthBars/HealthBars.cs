using UnityEngine;

class HealthBars : MonoBehaviour
{
    public static HealthBars Instance { get; private set; }
    public Patient SelectedPatient { get; private set; }

    
    [Range(0, 100)] public int psyche;
    [Range(0, 100)] public int family;
    [Range(0, 100)] public int reputation;
    [Min(0)]public int money;
    
    public enum PsycheState {Stabel, Unstabel, Broken}
    public enum FamilyState {Stabel, Unstabel, Broken}
    public enum ReputationState {Stabel, Unstabel, Broken}

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public PsycheState CurrentPsycheState()
    {
        return psyche switch
        {
            > 66 => PsycheState.Stabel,
            > 33 => PsycheState.Unstabel,
            _ => PsycheState.Broken
        };
    }

    public FamilyState CurrentFamilyState()
    {
        return family switch
        {
            > 66 => FamilyState.Stabel,
            > 33 => FamilyState.Unstabel,
            _ => FamilyState.Broken
        };
    }

    public ReputationState CurrentReputationState()
    {
        return reputation switch
        {
            > 66 => ReputationState.Stabel,
            > 33 => ReputationState.Unstabel,
            _ => ReputationState.Broken
        };
    }
    
    public void ChangePsych(int amount)
    {
        psyche = Mathf.Clamp(psyche + amount, 0, 100);
    }

    public void ChangeFamily(int amount)
    {
        family = Mathf.Clamp(family + amount, 0, 100);
    }

    public void ChangeReputation(int amount)
    {
        reputation = Mathf.Clamp(reputation + amount, 0, 100);
    }
    
    public void SetSelectedPatient(Patient p)
    {
        SelectedPatient = p;
        Debug.Log($"[HealthBars] Selected patient: {p?.FullName}");
    }
}

