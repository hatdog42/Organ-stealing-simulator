using UnityEngine;

[RequireComponent(typeof(TVInputRelay))]
public class MiniGameDragHandler : MonoBehaviour
{
    [Header("Which objects can be dragged")]
    public LayerMask draggableMask = ~0;  // default: everything

    private TVInputRelay relay;
    private Transform draggedObject;
    private Vector3 grabOffset;

    void Awake()
    {
        relay = GetComponent<TVInputRelay>();

        // Subscribe to relay events
        relay.PointerDown += OnPointerDown;
        relay.PointerDrag += OnPointerDrag;
        relay.PointerUp   += OnPointerUp;
    }

    void OnDestroy()
    {
        // Unsubscribe on destroy
        relay.PointerDown -= OnPointerDown;
        relay.PointerDrag -= OnPointerDrag;
        relay.PointerUp   -= OnPointerUp;
    }

    private void OnPointerDown(Vector3 miniWorld)
    {
        // Check if we clicked on a draggable object in the minigame
        Collider2D hit = Physics2D.OverlapPoint(miniWorld, draggableMask);
        if (!hit) return;

        draggedObject = hit.transform;
        grabOffset = draggedObject.position - miniWorld;
    }

    private void OnPointerDrag(Vector3 miniWorld)
    {
        if (draggedObject == null) return;

        Vector3 targetPos = miniWorld + grabOffset;

        // Use Rigidbody2D if available (for physics-friendly movement)
        if (draggedObject.TryGetComponent<Rigidbody2D>(out var rb) && rb.bodyType == RigidbodyType2D.Dynamic)
            rb.MovePosition(targetPos);
        else
            draggedObject.position = targetPos;
    }

    private void OnPointerUp(Vector3 miniWorld)
    {
        // Drop the object
        draggedObject = null;
    }
}