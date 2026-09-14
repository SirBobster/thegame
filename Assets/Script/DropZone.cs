using UnityEngine;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler
{
    public enum ZoneType
    {
        Valuable,
        Trash
    }

    [Header("Тип зоны")]
    [SerializeField] private ZoneType zoneType;

    public ZoneType Zone => zoneType;

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;

        GameObject draggedObject = eventData.pointerDrag.gameObject;

        // Сначала проверяем обычный предмет.
        DraggableItem item =
            draggedObject.GetComponent<DraggableItem>();

        if (item != null)
        {
            item.TryDrop(this);
            return;
        }

        // Затем проверяем тяжёлую балку.
        HeavyDebris debris =
            draggedObject.GetComponent<HeavyDebris>();

        if (debris != null)
        {
            debris.TryDrop(this);
        }
    }

    public bool Accepts(DraggableItem.ItemType itemType)
    {
        if (zoneType == ZoneType.Valuable)
            return itemType == DraggableItem.ItemType.Valuable;

        return itemType == DraggableItem.ItemType.Trash;
    }
}