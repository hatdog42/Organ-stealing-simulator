using UnityEngine;
using UnityEngine.InputSystem;

public class ClickInputControler : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask clickableMask = ~0;
    
    private IClickable _clicked;
    
    void Start()
    {
        if(!cam) cam = Camera.main;
    }

    void Update()
    {
        if (!Mouse.current.leftButton.wasReleasedThisFrame) return;
        
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Mathf.Abs(cam.transform.position.z)));
        
        Collider2D hit = Physics2D.OverlapPoint(world, clickableMask);
        if (!hit) return;
        
        var clickable = hit.GetComponentInParent<IClickable>();
        clickable?.OnClick(world);
    }
}
