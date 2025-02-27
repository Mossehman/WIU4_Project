using Player.Inventory;
using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Items/Consumable Item")]
public class ConsumableItem : BaseItem
{
    public float nourishmentValue = 20;
    public float consumeTime = 1.5f;
    private float consumeTimer = 0.0f;
    bool isEating = false;

    Animator playerAnimator;
    PlayerStats stats;


    public override void OnItemRightClick(GameObject holder)
    {
        stats = holder.GetComponent<PlayerStats>();
        playerAnimator = holder.GetComponent<Animator>();

        if (stats._stamina >= stats.maxStamina || playerAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Consume")) { return; }
        playerAnimator.SetTrigger("Consume");
        isEating = true;
        
    }

    public override void UpdateItem(GameObject holder)
    {
        if (!isEating) { return; }
        consumeTimer += Time.deltaTime;

        if (consumeTimer >= consumeTime)
        {
            stats._stamina += nourishmentValue;
            stats._stamina = Mathf.Clamp(stats._stamina, 0, stats.maxStamina);
            Debug.Log("Consumed!");

            holder.GetComponent<PlayerInventory>().ItemRemoveSelf(getID());

        }
    }
}
