using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class OrganControler : MonoBehaviour
{
    private Camera _camera;
    private SpriteRenderer _spriteRenderer;
    [SerializeField]private string nextScene;
    [SerializeField]private LayerMask clickableMask;
    
    [FormerlySerializedAs("PsyceChange")]
    [Header("HealthBar stats")]
    [SerializeField]private int psyceChangeOrgan;
    [SerializeField]private int psyceChangeIceBox;
    
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
        
        var hit = Physics2D.OverlapPoint(world, clickableMask);
        if (!hit) return;
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
        HealthBars.Instance.ChangePsych(psyceChangeOrgan);
        HealthBars.Instance.bChooseOrganBox = true;
        SceneController.Instance.LoadNextOrLoop(); 
    }

    private void MopBucketChosen()
    {
        HealthBars.Instance.organMoney += 10;
        HealthBars.Instance.ChangePsych(psyceChangeIceBox);
        HealthBars.Instance.bChooseOrganBox = false;
        SceneController.Instance.LoadNextOrLoop(); 
    }
}