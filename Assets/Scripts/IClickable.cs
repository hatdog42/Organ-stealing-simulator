using UnityEngine;

public interface IClickable
{
    void OnClick(Vector3 worldPos);
    void OnHoverEnter();
    void OnHoverExit();
}
