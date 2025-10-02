using UnityEngine;

class HealthBars : MonoBehaviour
{
    public static HealthBars instance { get; private set; }

    [Range(0, 100)] public int psyche;
    [Range(0, 100)] public int family;
    [Range(0, 100)] public int reputation;

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

