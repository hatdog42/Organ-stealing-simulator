using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ExitButon : MonoBehaviour, IClickable
{
    public void OnClick(Vector3 worldPos)
    {
        TVController.Instance.CloseMiniGame();
    }
}
