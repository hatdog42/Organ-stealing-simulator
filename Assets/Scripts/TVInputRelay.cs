using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TVInputRelay : MonoBehaviour
{
    public event Action<Vector3> PointerDown;
    public event Action<Vector3> PointerDrag;
    public event Action<Vector3> PointerUp;
    
    [Header("Cameras")] [SerializeField] private Camera mainCam;
    [SerializeField] private Camera miniGameCam;
    
    
    private SpriteRenderer _spriteRenderer;
    private Collider2D _tvCollider;
    
    private bool _dragging;


    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _tvCollider = GetComponent<Collider2D>();
        if (!mainCam) mainCam = Camera.main;
        
    }

    public bool TryMapScreenToMiniWorld(Vector2 screenPos, out Vector3 miniWorldPos)
    {
        if (mainCam == null || miniGameCam == null || _spriteRenderer == null || _tvCollider == null)
        {
            miniWorldPos = default;
            Debug.LogWarning("TryMapScreenToMiniWorld failed");
            return false;
        }
        float zToTV = Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector3 worldOnTV = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zToTV));

        Collider2D hit = Physics2D.OverlapPoint(worldOnTV);
        if (!hit || hit != _tvCollider)
        {
            miniWorldPos = default;
            return false;
        }

        Bounds wb = _spriteRenderer.bounds;
        float u = Mathf.InverseLerp(wb.min.x, wb.max.x, worldOnTV.x);
        float v = Mathf.InverseLerp(wb.min.y, wb.max.y, worldOnTV.y);

        miniWorldPos = miniGameCam.ViewportToWorldPoint(new Vector3(u, v, 0));
        miniWorldPos.z = 0;
        return true;
    }
    void Update()
    {
        var screen = Mouse.current.position.ReadValue();
        if (!TryMapScreenToMiniWorld(screen, out var miniWorld))
        {
            if (Mouse.current.leftButton.wasReleasedThisFrame && _dragging)
                PointerUp?.Invoke(miniWorld);
            _dragging = false;
            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _dragging = true;
            PointerDown?.Invoke(miniWorld);
        }
        else if (_dragging && Mouse.current.leftButton.isPressed)
        {
            PointerDrag?.Invoke(miniWorld);
        }
        else if (_dragging && Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _dragging = false;
            PointerUp?.Invoke(miniWorld);
        }
    }

    public void SetMiniGameCam(Camera targetCamera)
    {
        miniGameCam = targetCamera;
    }
}
