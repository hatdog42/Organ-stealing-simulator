using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CircleCollider2D))]
public class Plate : MonoBehaviour, IClickable
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField]private SpriteRenderer foodSprite;
    [SerializeField] private int foodCost = 10;
    private bool HasFood {get; set;}

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        SetFood(false, applyHappiness:false);
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
            SetFood(!HasFood, applyHappiness: false);
            foodSprite.enabled = true;
        }
        else
        {
            HealthBars.Instance.money += foodCost;
            SetFood(false, applyHappiness: true);
            foodSprite.enabled = false;
        }
    }

    private void SetFood(bool hasFood, bool applyHappiness)
    {
        HasFood = hasFood;
        if (!applyHappiness) return;
        
        int delta = HasFood ? 10 : -10;
        HealthBars.Instance.ChangeFamily(delta);
    }
}


