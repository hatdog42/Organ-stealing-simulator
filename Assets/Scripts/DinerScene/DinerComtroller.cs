using UnityEngine;
using UnityEngine.InputSystem;

public class DinerComtroller : MonoBehaviour
{
    private void Update()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 world = Camera.main.ScreenToWorldPoint(mousePos);
        
        var hit = Physics2D.OverlapPoint(world);
        if(!hit) return;
        
        if (hit.TryGetComponent<Plate>(out var plate)) 
            plate.ToggleFood();
    }
}
