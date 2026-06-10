using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MiniGameIcon : MonoBehaviour, IClickable
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private bool useSelectedMajorMiniGameCamera;
    [SerializeField] private Camera linkedCamera;

    private void Start()
    {
        spriteRenderer =  GetComponent<SpriteRenderer>();
    }

    public void OnClick(Vector3 worldPos)
    {
        Camera cameraToOpen = linkedCamera;
        if (useSelectedMajorMiniGameCamera && SurgerySceneControler.Instance)
        {
            cameraToOpen = SurgerySceneControler.Instance.SelectedMajorMiniGameCameraOrDefault(linkedCamera);
        }

        if (!cameraToOpen)
        {
            Debug.LogError($"[MiniGameIcon] '{name}' has no linkedCamera.");
            return;
        }

        TVController.Instance.OpenMiniGame(cameraToOpen);
    }

    public void OnHoverEnter()
    {
        spriteRenderer.sprite = hoverSprite;
    }

    public void OnHoverExit()
    {
        spriteRenderer.sprite = baseSprite;
    }
}
