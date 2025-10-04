using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CircleCollider2D))]
public class Plate : MonoBehaviour, IClickable
{
    [SerializeField]private SpriteRenderer foodSprite;

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


