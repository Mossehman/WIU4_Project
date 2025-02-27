using UnityEngine;

[CreateAssetMenu(fileName = "New Tool", menuName = "Items/Tool")]
public class ToolItem : BaseItem
{
    [Header("Tools")]
    public int damage;
    public float reach = 5.0f;
    public float range = 2.0f;

    public float useCooldown = 0.5f;
    public bool holdToUse = false;

    protected bool isUsingItem = false;
    protected float useCooldownTimer = 0.0f;

    public LayerMask toolEffectorLayers;    // this should be the layers that the tool can affect
    public LayerMask worldLayers;           // this should basically be every layer besides the player's layer

    public bool isWeapon = true;

    [Header("Sound")]
    public string swing;
    public string hitSFX;

    public override void OnItemHeld(GameObject holder) { 
        return; 
    }
    public override void OnItemRightClick(GameObject holder) { 
        return; 
    }

    public override void UpdateItem(GameObject holder)
    {
        if (useCooldownTimer > 0) {
            useCooldownTimer -= Time.deltaTime;
        }

    }

    public override void OnItemLeftUp(GameObject holder)
    {
        isUsingItem = false;
    }

    public override void OnItemLeftClick(GameObject holder) {
        if ((isUsingItem && !holdToUse) || useCooldownTimer > 0) { return; }

        Animator animator = holder.GetComponent<Animator>();
        if (animator != null)
        {
            if (isWeapon)
            {
                animator.SetTrigger("Attack");
            }
            else
            {
                animator.SetTrigger("Mine");
            }
        }

        AudioEventSystem.PlaySound(swing, default, default, default, true);
        RaycastHit hit;
        if (Physics.Raycast(holder.transform.position, holder.transform.forward, out hit, reach, worldLayers))
        {   
            if (((1 << hit.collider.gameObject.layer) & toolEffectorLayers) == 0) { return; }
            var components = hit.collider.gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                var damageable = component as IDamageable;
                if (damageable == null) continue;

                AudioEventSystem.PlaySound(hitSFX, default, default, default, true);

                damageable.Damage(damage);
                ///TODO: Play a sfx for when the weapon is used, maybe make it do different damage depending on which layermask was hit
                break;
            }
        }


        isUsingItem = true;
        useCooldownTimer = useCooldown;
        return; 
    }


}
