using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacableSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0 && transform.tag == "Hotbar")
        {
            GameObject dropped = eventData.pointerDrag;
            Draggable draggableItem = dropped.GetComponent<Draggable>();
            draggableItem._parentAfterDrag = transform;
        }
    }
}
