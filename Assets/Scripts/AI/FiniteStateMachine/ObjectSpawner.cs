using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float spawnrate = 1.5f;
    [SerializeField] private int burstspawn = 20;

    void Start()
    {
        for (int i = 0; i < burstspawn; i++)
            SpawnFruit();
        if (spawnrate > 0)
        InvokeRepeating(nameof(SpawnFruit), spawnrate, spawnrate);
    }

    void SpawnFruit()
    {
        int x = UnityEngine.Random.Range(-200, 200);
        int z = UnityEngine.Random.Range(-200, 200);
        GameObject obj = Instantiate(prefab, new Vector3(x, 0.5f, z), Quaternion.identity);
    }
}
