using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class to procedurally place objects down into the marching cubes scene
/// </summary>
public class TerrainObjectPlacement : MonoBehaviour
{
    public uint maxObjectsPerChunk;
    public uint objectPlacementChance;

    [Header("Assets")]
    public GameObject[] terrainAssets;
    [HideInInspector] public ComputeBuffer terrainObjectsBuffer;
    
    private int placedObjects = 0;

    public List<Vector3> positions = new List<Vector3>();
    public List<Vector3> normals = new List<Vector3>();

    public void PlaceObjects(int seed, Vector3 position, Vector3 normal)
    {
        if (placedObjects > maxObjectsPerChunk || terrainAssets.Length == 0) { return; }

        if (position.y < 13) { return; }
        float dotProduct = Vector3.Dot(normal.normalized, Vector3.down);
        if (dotProduct < 0.7f) { return; }  

        int toSpawn = Random.Range(0, (int)objectPlacementChance);
        if (toSpawn > 0) { return; }

        GameObject assetTest = Instantiate(terrainAssets[0]);
        assetTest.transform.position = position;
        assetTest.transform.LookAt(position - normal);
        placedObjects++;

        //Random.InitState((int)Time.time);

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        //Gizmos.DrawCube(position, new Vector3(bounds.x, 100, bounds.z));

        for (int i = 0; i < positions.Count; i++)
        {
            Gizmos.DrawLine(positions[i], positions[i] - normals[i]);
        }

    }
}

public struct ObjectPlacementData
{
    public Vector3 position;
    public Vector3 normal;
}
