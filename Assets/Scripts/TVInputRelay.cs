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
        return TryMapScreenToMiniWorld(screenPos, out miniWorldPos, true);
    }

    private bool TryMapScreenToMiniWorld(Vector2 screenPos, out Vector3 miniWorldPos, bool requireInsideTv)
    {
        if (mainCam == null || miniGameCam == null || _spriteRenderer == null || _tvCollider == null)
        {
            miniWorldPos = default;
            Debug.LogWarning("TryMapScreenToMiniWorld failed");
            return false;
        }
        float zToTV = Mathf.Abs(mainCam.transform.position.z - transform.position.z);
        Vector3 worldOnTV = mainCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, zToTV));

        if (requireInsideTv && !IsPointOnTv(worldOnTV))
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

    private bool IsPointOnTv(Vector3 worldOnTV)
    {
        Collider2D hit = Physics2D.OverlapPoint(worldOnTV);
        return hit && hit == _tvCollider;
    }

    void Update()
    {
        if (PauseMenueControler.IsPaused)
        {
            _dragging = false;
            return;
        }

        if (Mouse.current == null)
        {
            _dragging = false;
            return;
        }

        var screen = Mouse.current.position.ReadValue();

        if (!_dragging)
        {
            if (!Mouse.current.leftButton.wasPressedThisFrame
                || !TryMapScreenToMiniWorld(screen, out var miniWorld))
            {
                return;
            }

            _dragging = true;
            PointerDown?.Invoke(miniWorld);
            return;
        }

        if (!TryMapScreenToMiniWorld(screen, out var dragMiniWorld, false))
        {
            _dragging = false;
            return;
        }

        if (Mouse.current.leftButton.isPressed)
        {
            PointerDrag?.Invoke(dragMiniWorld);
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _dragging = false;
            PointerUp?.Invoke(dragMiniWorld);
        }
        else
        {
            _dragging = false;
        }
    }

    public void SetMiniGameCam(Camera targetCamera)
    {
        miniGameCam = targetCamera;
    }
}
