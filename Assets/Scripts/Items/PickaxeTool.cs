using UnityEngine;

[CreateAssetMenu(fileName = "New Pickaxe", menuName = "Items/Pickaxe Tool")]
public class PickaxeTool : ToolItem
{
    [Header("Pickaxe")]
    public GameObject terrainEditorPrefab;

    public override void OnItemLeftClick(GameObject holder)
    {
        if ((isUsingItem && !holdToUse) || useCooldownTimer > 0) { return; }

        RaycastHit hit;
        if (Physics.Raycast(holder.transform.position, Camera.main.transform.forward, out hit, reach, worldLayers))
        {
            if (((1 << hit.collider.gameObject.layer) & toolEffectorLayers) == 0) { return; }
            Instantiate(terrainEditorPrefab, hit.point, Quaternion.identity);
            Debug.Log("Instantiated prefab!!!");
        }
        isUsingItem = true;
        useCooldownTimer = useCooldown;
        return;
    }
}
