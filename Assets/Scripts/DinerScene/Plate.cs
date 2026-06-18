using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Plate : MonoBehaviour, IClickable
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private SpriteRenderer foodSprite;
    [SerializeField] private int foodCost = 10;
    [SerializeField] private SoundId plateOn = SoundId.PlateOn;
    [SerializeField] private SoundId plateOff = SoundId.PlateOff;
    [SerializeField, Range(0f, 1f)] private float plateSoundVolume = 1f;
    private bool HasFood {get; set;}

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetFood(false);
        foodSprite.enabled = false;
    }
    public void OnClick(Vector3 worldPos)
    {
        ToggleFood();
    }

    public void OnHoverEnter()
    {
        spriteRenderer.sprite = hoverSprite;
    }

    public void OnHoverExit()
    {
        spriteRenderer.sprite = baseSprite;
    }

    public void ToggleFood()
    {
        if (!HasFood)
        {
            if (HealthBars.Instance.money < foodCost)
            {
                Debug.Log("Not enough money to buy food!");
                return;
            }
        
            HealthBars.Instance.money -= foodCost;
            SetFood(true);
            HealthBars.Instance.RegisterFamilyMealPurchased();
            foodSprite.enabled = true;
            PlayPlateSound(plateOn);
        }
        else
        {
            HealthBars.Instance.money += foodCost;
            SetFood(false);
            HealthBars.Instance.RegisterFamilyMealRemoved();
            foodSprite.enabled = false;
            PlayPlateSound(plateOff);
        }
    }

    private void SetFood(bool hasFood)
    {
        HasFood = hasFood;
    }

    private void PlayPlateSound(SoundId soundId)
    {
        AudioManager.Instance?.PlaySfx(soundId, plateSoundVolume);
    }
}


