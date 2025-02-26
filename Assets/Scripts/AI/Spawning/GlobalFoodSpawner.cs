using UnityEngine;

public class GlobalFoodSpawner : MonoBehaviour
{
    public Transform[] spawnPositions;
    public GameObject foodPrefab;

    public int minSpawns = -10;

    private void Start()
    {
        if (spawnPositions != null && spawnPositions.Length > 0)
        {
            int numSpawns = Random.Range(minSpawns, spawnPositions.Length);
            if (numSpawns > 0)
            {
                for (int i = 0; i <= numSpawns; i++)
                {
                    int randomSpawnPoint = Random.Range(0, spawnPositions.Length);
                    GameObject food = Instantiate(foodPrefab, spawnPositions[randomSpawnPoint].position, Quaternion.identity);
                    food.transform.parent = transform.parent;
                }
            }

        }

        foreach (Transform t in spawnPositions)
        {
            Destroy(t.gameObject);
        }
    }
}
