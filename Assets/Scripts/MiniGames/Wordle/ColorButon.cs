using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class ColorButon : MonoBehaviour
{
    public enum ColorIndex
    {
        Auto = -1,
        Green = 0,
        Cyan = 1,
        Blue = 2,
        Purple = 3,
        Pink = 4,
        Red = 5,
        Orange = 6,
        Yellow = 7,
    }

    [SerializeField] private ColorIndex colorIndex = ColorIndex.Auto;

    private void OnMouseDown()
    {
        if (!WordleManager.Instance)
        {
            Debug.LogError($"No WordleManager found for color button '{name}'.");
            return;
        }

        WordleManager.Instance.PickColor(GetColorIndex());
    }

    public int GetColorIndex()
    {
        if (colorIndex != ColorIndex.Auto) return (int)colorIndex;

        return transform.GetSiblingIndex();
    }
}
