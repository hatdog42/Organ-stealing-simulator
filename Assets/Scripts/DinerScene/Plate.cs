using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CircleCollider2D))]
public class Plate : MonoBehaviour, IClickable
{
    [SerializeField]private SpriteRenderer foodSprite;
    [SerializeField] private int foodCost = 10;
    private bool HasFood {get; set;}

    private void Start()
    {
        SetFood(false, applyHappiness:false);
        foodSprite.enabled = false;
    }
    public void OnClick(Vector3 worldPos)
    {
        ToggleFood();
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


