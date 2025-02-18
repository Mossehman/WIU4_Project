using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlacableSlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            GameObject dropped = eventData.pointerDrag;
            Draggable Draggable = dropped.GetComponent<Draggable>();
            Draggable._parentAfterDrag = transform;
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
