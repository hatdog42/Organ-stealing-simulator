using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MiniGameIcon : MonoBehaviour, IClickable
{
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite baseSprite;
    [SerializeField] private Sprite hoverSprite;
    [SerializeField] private Camera linkedCamera;

    private void Start()
    {
        spriteRenderer =  GetComponent<SpriteRenderer>();
    }

    public void OnClick(Vector3 worldPos)
    {
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
