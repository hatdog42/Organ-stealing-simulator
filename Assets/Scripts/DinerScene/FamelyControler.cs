using UnityEngine;

public class FamelyControler : MonoBehaviour
{
    [SerializeField] private HealthBars healthBars;

    [System.Serializable]
    public class FamilyMemberSprites
    {
        public SpriteRenderer renderer;
        public Sprite happy;
        public Sprite neutral;
        public Sprite unhappy;
    }

    [SerializeField] private FamilyMemberSprites[] familyMembers;

    private HealthBars.FamilyState _currentState;

    void Awake()
    {
        if (!healthBars) healthBars = FindAnyObjectByType<HealthBars>();
    }

    void Update()
    {
        var newState = healthBars.CurrentFamilyState();
        if (newState == _currentState) return;

        _currentState = newState;
        UpdateFamilySprites();
    }

    private void UpdateFamilySprites()
    {
        foreach (var member in familyMembers)
        {
            if (!member.renderer) continue;

            member.renderer.sprite = _currentState switch
            {
                HealthBars.FamilyState.Happy => member.happy,
                HealthBars.FamilyState.Neutral => member.neutral,
                HealthBars.FamilyState.UnHappy => member.unhappy,
                _ => member.renderer.sprite
            };
        }
    }
    
}
