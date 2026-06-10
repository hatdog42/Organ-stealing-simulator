using UnityEngine;

class HealthBars : MonoBehaviour
{
    public static HealthBars Instance { get; private set; }
    public Patient SelectedPatient { get; private set; }

    [Range(-100, 100)] private int _psyche = 100;
    [Range(-100, 100)] private int _family = 100;
    [Range(-100, 100)] private int _reputation = 100;
    
    [Min(0)]public int money;
    [Min(0)]public int organMoney;
    
    
    public bool bChooseOrganBox;
    
    public enum PsycheState {Stable,Neutral, Unstable, Broken }
    public enum FamilyState {Happy, Neutral, UnHappy, Broken}
    public enum ReputationState {Stable,Neutral, Unstable, Broken}

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
            > 33 => PsycheState.Neutral,
            > 0 => PsycheState.Unstable,
            _ => PsycheState.Broken
        };
    }

    public FamilyState CurrentFamilyState()
    {
        return _family switch
        {
            > 66 => FamilyState.Happy,
            > 33 => FamilyState.Neutral,
            > 0 => FamilyState.UnHappy,
            _ => FamilyState.Broken
        };
    }

    public ReputationState CurrentReputationState()
    {
        return _reputation switch
        {
            > 66 => ReputationState.Stable,
            > 33 => ReputationState.Neutral,
            > 0 => ReputationState.Unstable,
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

#if UNITY_EDITOR
    [ContextMenu("Debug/Force Psyche To Zero")]
    private void DebugForcePsycheEnding()
    {
        ChangePsych(-100);
        Debug.Log("Forced psyche to zero. Go to the SanetyChek scene to test the psyche ending.");
    }

    [ContextMenu("Debug/Force Family To Zero")]
    private void DebugForceFamilyEnding()
    {
        ChangeFamily(-100);
        Debug.Log("Forced family to zero. Use the family/bed check to test the family ending.");
    }

    [ContextMenu("Debug/Force Reputation To Zero")]
    private void DebugForceReputationEnding()
    {
        ChangeReputation(-100);
        Debug.Log("Forced reputation to zero. Go to the office/patient choice check to test the reputation ending.");
    }

    [ContextMenu("Debug/Reset Ending Test Values")]
    private void DebugResetEndingTestValues()
    {
        _psyche = 100;
        _family = 100;
        _reputation = 100;
        Debug.Log("Reset ending test values.");
    }
#endif
}

