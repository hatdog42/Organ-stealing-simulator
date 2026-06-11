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
        if (!TVController.Instance)
        {
            Debug.LogError($"[MiniGameIcon] '{name}' cannot open a minigame because no TVController exists in the scene.");
            return;
        }

        if (useSelectedMajorMiniGameCamera)
        {
            TVController.Instance.OpenSelectedMajorMiniGame();
            return;
        }

        if (!linkedCamera)
        {
            Debug.LogError($"[MiniGameIcon] '{name}' has no linkedCamera.");
            return;
        }

        TVController.Instance.OpenMiniGame(linkedCamera);
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
