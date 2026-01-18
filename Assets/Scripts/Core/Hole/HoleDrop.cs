using UnityEngine;
using UnityEngine.EventSystems;

public class HoleDrop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        var cube = eventData.pointerDrag?.GetComponent<CubeToDrag>();

        if (cube == null) return;

        cube.DroppedInHole(eventData);
    }
}
