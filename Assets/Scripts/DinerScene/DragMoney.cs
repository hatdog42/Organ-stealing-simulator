using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class DragMoney : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Vector3 offset;

    public void OnBeginDrag(PointerEventData e)
    {
        var world = ScreenToWorld(e.position);
        world.z = transform.position.z;
        offset = transform.position - world;
    }

    public void OnDrag(PointerEventData e)
    {
        var world = ScreenToWorld(e.position);
        world.z = 0;           
        transform.position = world + offset;
    }

    public void OnEndDrag(PointerEventData e) {}

    static Vector3 ScreenToWorld(Vector2 screen)
    {
        var cam = Camera.main ? Camera.main : Camera.current;
        var w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, Mathf.Abs(cam.transform.position.z)));
        w.z = 0f;
        return w;
    }
}
