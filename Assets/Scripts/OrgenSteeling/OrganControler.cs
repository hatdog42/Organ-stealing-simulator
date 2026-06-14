using UnityEngine;
using UnityEngine.InputSystem;

public class OrganControler : MonoBehaviour
{
    private enum OrganContainerChoice
    {
        None,
        OrganBox,
        MopBucket
    }

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
        if (PauseMenueControler.IsPaused) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        
        Vector3 world = _camera.ScreenToWorldPoint(mousePos);
        world.z = 0f;
        transform.position = world;

        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (_choiceMade) return;

        OrganContainerChoice choice = GetContainerChoiceAt(world);
        if (choice == OrganContainerChoice.None) return;

        _choiceMade = true;
        _spriteRenderer.enabled = false;

        if (choice == OrganContainerChoice.OrganBox)
        {
            OrganBoxChosen();
        }
        else if (choice == OrganContainerChoice.MopBucket)
        {
            MopBucketChosen();
        }
    }

    private OrganContainerChoice GetContainerChoiceAt(Vector2 world)
    {
        Collider2D[] hits = Physics2D.OverlapPointAll(world, clickableMask);

        foreach (Collider2D hit in hits)
        {
            if (!hit || hit.transform.IsChildOf(transform)) continue;

            OrganContainerChoice choice = GetContainerChoice(hit.transform);
            if (choice != OrganContainerChoice.None) return choice;
        }

        return OrganContainerChoice.None;
    }

    private static OrganContainerChoice GetContainerChoice(Transform hitTransform)
    {
        for (Transform current = hitTransform; current; current = current.parent)
        {
            if (current.CompareTag("OrganBox")) return OrganContainerChoice.OrganBox;
            if (current.CompareTag("MopBucket")) return OrganContainerChoice.MopBucket;
        }

        return OrganContainerChoice.None;
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
