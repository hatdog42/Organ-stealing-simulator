using UnityEngine;
using UnityEngine.InputSystem;

public class TVInputRelay : MonoBehaviour
{
    [Header("Cameras")] 
    public Camera mainCam;       
    public Camera miniGameCam;

    [Header("Target inside mini-game world")]
    public Transform dragTarget;

    private bool _dragging;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (!mainCam) mainCam = Camera.main;
    }

    void Update()
    {
        Vector2 m = Mouse.current.position.ReadValue();
        float zToTV = Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector3 worldOnTV = mainCam.ScreenToWorldPoint(new Vector3(m.x, m.y, zToTV));
        
        var col = Physics2D.OverlapPoint(worldOnTV);
        if (Mouse.current.leftButton.wasPressedThisFrame)
            _dragging = (col && col.gameObject == gameObject);
        if (Mouse.current.leftButton.wasReleasedThisFrame)
            _dragging = false;

        if (!_dragging) return;
        
        Bounds wb = _spriteRenderer.bounds; 
        float u = Mathf.InverseLerp(wb.min.x, wb.max.x, worldOnTV.x);
        float v = Mathf.InverseLerp(wb.min.y, wb.max.y, worldOnTV.y);

        Vector3 miniWorld = miniGameCam.ViewportToWorldPoint(new Vector3(u, v, 0));
        miniWorld.z = 0;
        
        if (dragTarget.TryGetComponent<Rigidbody2D>(out var rb) && rb.bodyType == RigidbodyType2D.Dynamic) 
            rb.MovePosition(miniWorld);
        else dragTarget.position = miniWorld;
    }
}
