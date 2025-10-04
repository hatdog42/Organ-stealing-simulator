using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MiniGameIcon : MonoBehaviour, IClickable
{
    [SerializeField] private Camera linkedCamera;
    public void OnClick(Vector3 worldPos)
    {
        if (!linkedCamera)
        {
            Debug.LogError($"[MiniGameIcon] '{name}' has no linkedCamera.");
            return;
        }
        TVController.Instance.OpenMiniGame(linkedCamera);
    }
}
