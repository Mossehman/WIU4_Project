using UnityEngine;

[CreateAssetMenu(fileName = "New Pickaxe", menuName = "Items/Pickaxe Tool")]
public class PickaxeTool : ToolItem
{
    [Header("Pickaxe")]
    public GameObject terrainEditorPrefab;
    public LayerMask terrainLayer;

    public override void OnItemLeftClick(GameObject holder)
    {
        if ((isUsingItem && !holdToUse) || useCooldownTimer > 0) { return; }
        AudioEventSystem.PlaySoundSmart(swing, ref AudioManager.Instance.DedicatedSFX, default, default, false, false, 1, true);

        RaycastHit hit;
        if (Physics.Raycast(holder.transform.position, Camera.main.transform.forward, out hit, reach, worldLayers))
        {
            if (((1 << hit.collider.gameObject.layer) & terrainLayer) != 0) {
                Instantiate(terrainEditorPrefab, hit.point, Quaternion.identity);
                AudioEventSystem.PlaySoundSmart(hitSFX, ref AudioManager.Instance.DedicatedSFX, default, default, false, false, 1, true);
            }
            else if (((1 << hit.collider.gameObject.layer) & toolEffectorLayers) != 0)
            {
                var components = hit.collider.gameObject.GetComponents<MonoBehaviour>();
                foreach (var component in components)
                {
                    var damageable = component as IDamageable;
                    if (damageable == null) continue;
                    AudioEventSystem.PlaySoundSmart(hitSFX, ref AudioManager.Instance.DedicatedSFX, default, default, false, false, 1, true);
                    damageable.Damage(damage);
                    ///TODO: Play a sfx for when the weapon is used, maybe make it do different damage depending on which layermask was hit
                    break;
                }
            }

        }
        Animator animator = holder.GetComponent<Animator>();
        if (animator != null) { animator.SetTrigger("Mine"); }
        isUsingItem = true;
        useCooldownTimer = useCooldown;
        return;
    }
}
