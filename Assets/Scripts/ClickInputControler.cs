using UnityEngine;
using UnityEngine.InputSystem;

public class ClickInputControler : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask clickableMask = ~0;
    
    private IClickable _hovered;
    private IClickable _clicked;
    
    void Start()
    {
        if(!cam) cam = Camera.main;
    }

    void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 world = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, Mathf.Abs(cam.transform.position.z)));
        
        Collider2D hit = Physics2D.OverlapPoint(world, clickableMask);
        var hoverCandidate = hit ? hit.GetComponentInParent<IClickable>() : null;

        if (hoverCandidate != _hovered)
        {
            _hovered?.OnHoverExit();
            _hovered = hoverCandidate;
            _hovered?.OnHoverEnter();
        }
        
        if (Mouse.current.leftButton.wasReleasedThisFrame && _hovered != null)
        {
            _hovered.OnClick(world);
        }
    }
}
