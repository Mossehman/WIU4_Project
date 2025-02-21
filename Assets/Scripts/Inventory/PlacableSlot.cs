using Player.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacableSlot : MonoBehaviour, IDropHandler
{
    private ItemOrigin _origin;
    private ItemDestination _destination;

    private void Start()
    {
        EventManager.CreateEvent("OnItemMove");

        if (transform.tag == "Inventory") { _destination = ItemDestination.INVENTORY; }
        else if (transform.tag == "Hotbar") { _destination = ItemDestination.HOTBAR; }
        else if (transform.tag == "Storage") { _destination = ItemDestination.STORAGE; }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log(transform);
        GameObject dropped = eventData.pointerDrag;
        Draggable Draggable = dropped.GetComponent<Draggable>();

        if (Draggable._parentAfterDrag.transform.tag == "Inventory") { _origin = ItemOrigin.INVENTORY; }
        else if (Draggable._parentAfterDrag.transform.tag == "Hotbar") { _origin = ItemOrigin.HOTBAR; }
        else if (Draggable._parentAfterDrag.transform.tag == "Storage") { _origin = ItemOrigin.STORAGE; }

        if (transform.childCount == 0 && transform.tag != "Inventory")
        {
            Draggable._parentAfterDrag = transform;
            EventManager.Fire("OnItemMove", dropped, _origin, _destination);
        }
        else if (transform.tag == "Inventory")
        {
            Draggable._parentAfterDrag = transform;
            EventManager.Fire("OnItemMove", dropped, _origin, _destination);
        }
        else
        {
            GameObject current = transform.GetChild(0).gameObject;
            Draggable currentDraggable = current.GetComponent<Draggable>();

            currentDraggable.transform.SetParent(Draggable._parentAfterDrag);
            Draggable._parentAfterDrag = transform;
        }

    }
}
