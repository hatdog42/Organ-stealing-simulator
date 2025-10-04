using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CircleCollider2D))]
public class Plate : MonoBehaviour
{
    [SerializeField]private SpriteRenderer foodSprite;

    private bool HasFood {get; set;}

    private void Start()
    {
        SetFood(false, applyHappiness:false);
        foodSprite.enabled = false;
    }

    public void ToggleFood()
    {
        SetFood(!HasFood, applyHappiness: false);
        foodSprite.enabled = !foodSprite.enabled;
    }

    private void SetFood(bool hasFood, bool applyHappiness)
    {
        HasFood = hasFood;
        if (!applyHappiness) return;
        
         
        int delta = HasFood ? 10 : -10;
        HealthBars.Instance.ChangeFamily(delta);
    }
}


