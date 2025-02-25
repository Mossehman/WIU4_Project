using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Items/Tool  ")]
public class ToolItem : BaseItem
{
    [Header("Tools")]
    public int damage;
    public float reach = 5.0f;
    public float range = 2.0f;

    public float useCooldown = 0.5f;
    public bool holdToUse = false;

    private bool isUsingItem = false;
    private float useCooldownTimer = 0.0f;

    public LayerMask toolEffectorLayers;    // this should be the layers that the tool can affect
    public LayerMask worldLayers;           // this should basically be every layer besides the player's layer

    public override void OnItemHeld(GameObject holder) { 
        return; 
    }
    public override void OnItemRightClick(GameObject holder) { 
        return; 
    }
    public override void OnItemLeftClick(GameObject holder) {
        Debug.Log("Hello!");

        if (isUsingItem && holdToUse) { return; }

        RaycastHit hit;
        if (Physics.Raycast(holder.transform.position, holder.transform.forward, out hit, reach, worldLayers))
        {   
            if (((1 << hit.collider.gameObject.layer) & toolEffectorLayers) == 0) { return; }
            var components = hit.collider.gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                var damageable = component as IDamageable;
                if (damageable == null) continue;

                damageable.Damage(damage);

                ///TODO: Play a sfx for when the weapon is used, maybe make it do different damage depending on which layermask was hit
                return;
            }
        }

        return; 
    }


}
