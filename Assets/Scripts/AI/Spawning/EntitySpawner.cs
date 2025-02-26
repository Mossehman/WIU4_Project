using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    public EntitySpawnData[] spawnData;

    private void Start()
    {
        if (spawnData != null && spawnData.Length > 0)
        {
            int spawnIndex = Random.Range(0, spawnData.Length);
            EntitySpawnData data = spawnData[spawnIndex];

            int randomSpawnCount = Random.Range((int)data.minSpawn, (int)data.maxSpawn);
            for (int i = 0; i < randomSpawnCount; i++)
            {
                Debug.Log("Spawned entity!");
                Instantiate(data.entityPrefab, transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }


}
