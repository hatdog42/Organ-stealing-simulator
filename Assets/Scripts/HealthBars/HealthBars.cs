using System.Collections.Generic;
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

    [Header("Wage Balance")]
    [SerializeField, Min(0)] private int savedPatientWage = 8;
    [SerializeField, Min(0)] private int killedPatientWage = 7;

    [Header("Fallback Patient Outcome Balance")]
    [SerializeField] private int fallbackSavedPsycheChange = 3;
    [SerializeField] private int fallbackSavedReputationChange = 4;
    [SerializeField] private int fallbackKilledPsycheChange = -15;
    [SerializeField] private int fallbackKilledReputationChange = -10;
    [SerializeField] private int fallbackOrganMoney = 14;

    [Header("Organ Theft Balance")]
    [SerializeField] private int organTheftPsycheChange = -8;
    [SerializeField] private int organTheftReputationChange = -3;

    [Header("Family Dinner Balance")]
    [SerializeField, Min(0)] private int maximumMealsPerDinner = 4;
    [SerializeField] private int familyChangeWithoutFood = -55;
    [SerializeField] private int familyChangePerMeal = 13;

    private int _mealsBoughtThisDinner;
    private bool _familyDinnerApplied;
    private readonly List<int> _pendingWageMoney = new();
    private readonly List<int> _pendingOrganMoney = new();
    
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

    public void ApplySavedPatient()
    {
        Patient patient = SelectedPatient;
        ChangePsych(patient?.PsycheChangeWhenSaved ?? fallbackSavedPsycheChange);
        ChangeReputation(patient?.ReputationChangeWhenSaved ?? fallbackSavedReputationChange);
        RegisterPatientPayout(savedPatientWage, 0);
        SelectedPatient = null;
    }

    public void ApplyKilledPatient(bool stoleOrgans)
    {
        Patient patient = SelectedPatient;
        ChangePsych(patient?.PsycheChangeWhenKilled ?? fallbackKilledPsycheChange);
        ChangeReputation(patient?.ReputationChangeWhenKilled ?? fallbackKilledReputationChange);

        int earnedOrganMoney = 0;
        if (stoleOrgans)
        {
            ChangePsych(organTheftPsycheChange);
            ChangeReputation(organTheftReputationChange);
            earnedOrganMoney = patient?.OrganMoney ?? fallbackOrganMoney;
            organMoney += earnedOrganMoney;
        }

        RegisterPatientPayout(killedPatientWage, earnedOrganMoney);
        SelectedPatient = null;
    }

    private void RegisterPatientPayout(int wageAmount, int organAmount)
    {
        _pendingWageMoney.Add(Mathf.Max(0, wageAmount));
        _pendingOrganMoney.Add(Mathf.Max(0, organAmount));
    }

    public int[] CollectWageMoneyStages()
    {
        return CollectMoneyStages(_pendingWageMoney);
    }

    public int[] CollectOrganMoneyStages()
    {
        int[] collected = CollectMoneyStages(_pendingOrganMoney);
        organMoney = 0;
        return collected;
    }

    public int CollectOrganMoney()
    {
        int collected = 0;
        for (int i = 0; i < _pendingOrganMoney.Count; i++)
            collected += _pendingOrganMoney[i];

        if (collected <= 0)
            collected = organMoney;

        _pendingOrganMoney.Clear();
        organMoney = 0;
        return collected;
    }

    private static int[] CollectMoneyStages(List<int> stages)
    {
        int[] collected = stages.ToArray();
        stages.Clear();
        return collected;
    }

    public void BeginFamilyDinner()
    {
        _mealsBoughtThisDinner = 0;
        _familyDinnerApplied = false;
    }

    public void RegisterFamilyMealPurchased()
    {
        if (_familyDinnerApplied) return;

        _mealsBoughtThisDinner = Mathf.Clamp(_mealsBoughtThisDinner + 1, 0, maximumMealsPerDinner);
    }

    public void RegisterFamilyMealRemoved()
    {
        if (_familyDinnerApplied) return;

        _mealsBoughtThisDinner = Mathf.Clamp(_mealsBoughtThisDinner - 1, 0, maximumMealsPerDinner);
    }

    public void ApplyFamilyDinnerResult()
    {
        if (_familyDinnerApplied) return;

        int meals = Mathf.Clamp(_mealsBoughtThisDinner, 0, maximumMealsPerDinner);
        int familyChange = familyChangeWithoutFood + meals * familyChangePerMeal;
        ChangeFamily(familyChange);
        _familyDinnerApplied = true;
    }
    
    public void SetSelectedPatient(Patient patient)
    {
        SelectedPatient = patient;
    }

    public void ResetForNewGame()
    {
        _psyche = 100;
        _family = 100;
        _reputation = 100;
        money = 0;
        organMoney = 0;
        bChooseOrganBox = false;
        SelectedPatient = null;
        _pendingWageMoney.Clear();
        _pendingOrganMoney.Clear();
        BeginFamilyDinner();
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
        ResetForNewGame();
        Debug.Log("Reset ending test values.");
    }
#endif
}
