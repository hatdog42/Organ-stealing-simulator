using UnityEngine;
using UnityEngine.InputSystem;

public class OrganControler : MonoBehaviour
{
    private Camera _camera;
    private SpriteRenderer _spriteRenderer;
    [SerializeField]private string nextScene;
    [SerializeField]private LayerMask clickableMask;
    private bool _choiceMade;
    
    void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if(!_camera) _camera = Camera.main;
    }
    private void Update()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        Vector3 world = _camera.ScreenToWorldPoint(mousePos);
        world.z = 0f;
        transform.position = world;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (_choiceMade) return;
        
        var hit = Physics2D.OverlapPoint(world, clickableMask);
        if (!hit) return;
        _choiceMade = true;
        _spriteRenderer.enabled = false;
        
        if (hit.CompareTag($"OrganBox"))
        {
            OrganBoxChosen();
        }
        else if (hit.CompareTag($"MopBucket"))
        {
            MopBucketChosen();
        }
        
        _spriteRenderer.enabled = false;
    }
    private void OrganBoxChosen()
    {
        HealthBars.Instance.ApplyKilledPatient(stoleOrgans: false);
        HealthBars.Instance.bChooseOrganBox = true;
        SceneController.Instance.LoadNextOrLoop(); 
    }

    private void MopBucketChosen()
    {
        HealthBars.Instance.ApplyKilledPatient(stoleOrgans: true);
        HealthBars.Instance.bChooseOrganBox = false;
        SceneController.Instance.LoadNextOrLoop(); 
    }
}
