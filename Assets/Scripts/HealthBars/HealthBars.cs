using UnityEngine;

class HealthBars : MonoBehaviour
{
    public static HealthBars Instance { get; private set; }
    public Patient SelectedPatient { get; private set; }

    
    [Range(0, 100)] private int _psyche;
    [Range(0, 100)] private int _family;
    [Range(0, 100)] private int _reputation;
    
    [Min(0)]public int money;
    
    public enum PsycheState {Stable, Unstable, Broken}
    public enum FamilyState {Happy, UnHappy, Broken}
    public enum ReputationState {Stable, Unstable, Broken}

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
        return _psyche switch
        {
            > 66 => PsycheState.Stable,
            > 33 => PsycheState.Unstable,
            _ => PsycheState.Broken
        };
    }

    public FamilyState CurrentFamilyState()
    {
        return _family switch
        {
            > 66 => FamilyState.Happy,
            > 33 => FamilyState.UnHappy,
            _ => FamilyState.Broken
        };
    }

    public ReputationState CurrentReputationState()
    {
        return _reputation switch
        {
            > 66 => ReputationState.Stable,
            > 33 => ReputationState.Unstable,
            _ => ReputationState.Broken
        };
    }
    
    public void ChangePsych(int amount)
    {
        _psyche = Mathf.Clamp(_psyche + amount, 0, 100);
    }

    public void ChangeFamily(int amount)
    {
        _family = Mathf.Clamp(_family + amount, 0, 100);
    }

    public void ChangeReputation(int amount)
    {
        _reputation = Mathf.Clamp(_reputation + amount, 0, 100);
    }
    
    public void SetSelectedPatient(Patient patient)
    {
        SelectedPatient = patient;
    }
}

