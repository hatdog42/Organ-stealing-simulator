using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ExitButon : MonoBehaviour, IClickable
{
    public void OnClick(Vector3 worldPos)
    {
        TVController.Instance.CloseMiniGame();
    }

    public void OnHoverEnter()
    {
        
    }

    public void OnHoverExit()
    {
        
    }
}
