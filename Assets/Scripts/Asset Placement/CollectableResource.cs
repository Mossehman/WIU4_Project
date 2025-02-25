using UnityEngine;

public class CollectableResource : MonoBehaviour, IDamageable
{
    public int maxHealth = 5;
    [SerializeField] private int health;
    public ItemDrops[] lootTable;

    public void Damage(int damageValue)
    {
        health -= damageValue;
        if (health <= 0) { OnDeath(); }
    }

    public void OnDeath()
    {
        if (lootTable != null && lootTable.Length > 0)
        {
            int lootDrop = Random.Range(0, lootTable.Length);
            foreach (GameObject obj in lootTable[lootDrop].itemDropList)
            {
                Instantiate(obj, transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }

    private void Awake()
    {
        health = maxHealth;
    }
}

[System.Serializable]
public struct ItemDrops
{
    public GameObject[] itemDropList;
}
