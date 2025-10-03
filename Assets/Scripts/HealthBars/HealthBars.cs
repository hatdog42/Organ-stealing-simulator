using UnityEngine;

class HealthBars : MonoBehaviour
{
    public static HealthBars instance { get; private set; }

    [Range(0, 100)] public int psyche;
    [Range(0, 100)] public int family;
    [Range(0, 100)] public int reputation;
    [Min(0)]public int money;
    
    public enum psycheState {Stabel, Unstabel, Broken}
    public enum familyState {Stabel, Unstabel, Broken}
    public enum reputationState {Stabel, Unstabel, Broken}

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public psycheState CurrentPsycheState()
    {
        return psyche switch
        {
            > 66 => psycheState.Stabel,
            > 33 => psycheState.Unstabel,
            _ => psycheState.Broken
        };
    }

    public familyState CurrentFamilyState()
    {
        return family switch
        {
            > 66 => familyState.Stabel,
            > 33 => familyState.Unstabel,
            _ => familyState.Broken
        };
    }

    public reputationState CurrentReputationState()
    {
        return reputation switch
        {
            > 66 => reputationState.Stabel,
            > 33 => reputationState.Unstabel,
            _ => reputationState.Broken
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
}

