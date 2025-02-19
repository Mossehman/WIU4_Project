using Player.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacableSlot : MonoBehaviour, IDropHandler
{
    private void Start()
    {
        EventManager.CreateEvent("OnDropItem");
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(transform);
        if (transform.childCount == 0 && transform.tag != "Inventory")
        {
            GameObject dropped = eventData.pointerDrag;
            Draggable Draggable = dropped.GetComponent<Draggable>();
            Draggable._parentAfterDrag = transform;
            EventManager.Fire("OnDropItem", dropped, ItemDestination.HOTBAR);
        }
        else if (transform.tag == "Inventory")
        {
            GameObject dropped = eventData.pointerDrag;
            Draggable Draggable = dropped.GetComponent<Draggable>();
            Draggable._parentAfterDrag = transform;
            EventManager.Fire("OnDropItem", dropped, ItemDestination.INVENTORY);
        }
        else
        {
            GameObject dropped = eventData.pointerDrag;
            Draggable Draggable = dropped.GetComponent<Draggable>();

            GameObject current = transform.GetChild(0).gameObject;
            Draggable currentDraggable = current.GetComponent<Draggable>();

            currentDraggable.transform.SetParent(Draggable._parentAfterDrag);
            Draggable._parentAfterDrag = transform;
        }
    }
}
