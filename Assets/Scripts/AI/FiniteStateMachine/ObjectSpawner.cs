using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float spawnrate = 1.5f;
    [SerializeField] private int burstspawn = 20;
    [SerializeField] private float squaresize = 200;
    [SerializeField] private float deadsize = 20;

    void Start()
    {
        for (int i = 0; i < burstspawn; i++)
            SpawnFruit();
        if (spawnrate > 0)
        InvokeRepeating(nameof(SpawnFruit), spawnrate, spawnrate);
    }

    void SpawnFruit()
    {
        float x = UnityEngine.Random.Range(-squaresize, squaresize);
        x = x <= deadsize ? deadsize : x >= -deadsize ? -deadsize : x;
        float z = UnityEngine.Random.Range(-squaresize, squaresize);
        z = z <= deadsize ? deadsize : z >= -deadsize ? -deadsize : z;
        GameObject obj = Instantiate(prefab, new Vector3(x + transform.position.x, transform.position.y, z + transform.position.z), Quaternion.identity);
    }
}
