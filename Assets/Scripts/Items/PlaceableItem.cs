using Player.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Placeable", menuName = "Items/Placeable Item")]
public class PlaceableItem : BaseItem
{
    [Header("Placeable item")]
    [SerializeField] private GameObject objToPlacePreview;
    private GameObject objPlacePreview;
    [SerializeField] private float placementRange;
    [SerializeField, Range(0f, 1f)] private float placementNormalsThreshold = 0.5f;

    [SerializeField] private LayerMask placeableSurfaces;

    [SerializeField] private GameObject objToPlace;
    
    public override void OnItemHeld(GameObject holder)
    {
        RaycastHit hit;

        if (Physics.Raycast(holder.transform.position, Camera.main.transform.forward, out hit, placementRange, placeableSurfaces))
        {

            float normalsThreshold = Vector3.Dot(hit.normal, Vector3.down);
            if (normalsThreshold > placementNormalsThreshold)
            {
                return;
            }
            if (objPlacePreview == null)
            {
                objPlacePreview = Instantiate(objToPlacePreview);
            }

            objPlacePreview.transform.position = hit.point;
            objPlacePreview.transform.LookAt(hit.point - hit.normal);
        }
        else
        {
            Destroy(objPlacePreview);
            objPlacePreview = null;
        }
    }

    public override void OnItemRightClick(GameObject holder)
    {
        if (objToPlacePreview == null || objPlacePreview == null) return;

        GameObject placedObj = Instantiate(objToPlace, objPlacePreview.transform.position, objPlacePreview.transform.rotation);
        placedObj.GetComponent<PlaceableObjectScript>().item = this;    
        Destroy(objPlacePreview);
        objPlacePreview = null;
        holder.GetComponent<PlayerInventory>().RemoveItem(getID(), 1);


    }

    public override void UpdateItem(GameObject holder)
    {
        if (!isHeld && objPlacePreview != null) {
            Destroy(objPlacePreview);
        }
    }

    public override void OnRemoved(GameObject holder)
    {
        if (objPlacePreview != null)
        {
            Destroy(objPlacePreview);
        }
    }
}
