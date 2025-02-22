using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Helper class to procedurally place objects down into the marching cubes scene
/// </summary>
public class TerrainObjectPlacement : MonoBehaviour
{
    public uint maxObjectsPerChunk;
    public uint objectPlacementChance;

    [Header("Assets")]
    public GameObject[] terrainAssets;

    [Header("Raycast Check")]
    public uint numSamples = 5;
    public float sampleRadius = 2.0f;
    public LayerMask terrainLayerMask;
    public float raycastYOffset = 1.0f;
    public float raycastLength = 2.0f;
    public float differenceInHeightThreshold = 1.0f;
    [Range(-1.0f, 1.0f)]
    public float differenceInNormalsThreshold = 0.1f;
   

    //public List<Vector3> positions = new List<Vector3>();
    //public List<Vector3> normals = new List<Vector3>();
    public List<ObjectPlacementData> placementData = new List<ObjectPlacementData>();

    // For debugging, remove later
    private List<Vector3> spawnPos = new List<Vector3>();
    private List<Vector3> samplePos = new List<Vector3>();

    public void GenerateSpawnPoints(int seed, Vector3 position, Vector3 normal)
    {
        if (placementData.Count >= maxObjectsPerChunk || terrainAssets.Length == 0) { return; }

        if (position.y < 13) { return; }
        float dotProduct = Vector3.Dot(normal.normalized, Vector3.down);
        if (dotProduct < 0.7f) { return; }  

        int toSpawn = Random.Range(0, (int)objectPlacementChance);
        if (toSpawn > 0) { return; }

        ObjectPlacementData newData = new ObjectPlacementData();
        newData.position = position;
        newData.normal = normal;
        placementData.Add(newData);

        //if (!checkNearSurfaces(position, normal, numSamples, sampleRadius)) { return; }
        //
        //GameObject assetTest = Instantiate(terrainAssets[0]);
        //assetTest.transform.position = position;
        //assetTest.transform.LookAt(position - normal);
        //placedObjects++;

        //Random.InitState((int)Time.time);

    }

    public void SpawnObjects(Transform t)
    {
        if (placementData.Count == 0) { return; }
        for (int i = 0; i < placementData.Count; i++)
        {
            if (!checkNearSurfaces(placementData[i].position, placementData[i].normal, numSamples, sampleRadius)) { continue; }
            GameObject assetTest = Instantiate(terrainAssets[0]);
            assetTest.transform.position = placementData[i].position;
            assetTest.transform.parent = t;
            assetTest.transform.LookAt(placementData[i].position - placementData[i].normal);

        }

        placementData.Clear();
        //Random.InitState((int)Time.time);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        //Gizmos.DrawCube(position, new Vector3(bounds.x, 100, bounds.z));

        for (int i = 0; i < spawnPos.Count; i++)
        {
            Gizmos.DrawSphere(spawnPos[i], 1.0f);
        }

        Gizmos.color = Color.red;
        //Gizmos.DrawCube(position, new Vector3(bounds.x, 100, bounds.z));

        for (int i = 0; i < samplePos.Count; i++)
        {
            Gizmos.DrawLine(samplePos[i], new Vector3(samplePos[i].x, samplePos[i].y + raycastYOffset, samplePos[i].z));
        }

    }

    public void Poisson()
    {

    }

    private bool checkNearSurfaces(Vector3 spawnPos, Vector3 normal, uint numSamples, float range)
    {
        this.spawnPos.Add(spawnPos);
        for (int i = 0; i < numSamples; i++)
        {
            Vector3 raycastPos = Random.insideUnitSphere * range + spawnPos;
            raycastPos.y = spawnPos.y + raycastYOffset;
            RaycastHit hit;
            if (Physics.Raycast(raycastPos, Vector3.down, out hit, raycastYOffset + raycastLength, terrainLayerMask))
            {
                this.samplePos.Add(hit.point);
                float differenceInNormals = Vector3.Dot(normal, -hit.normal);
                float differenceInPositions = Vector3.SqrMagnitude(spawnPos - hit.point);

                if (differenceInPositions > differenceInHeightThreshold * differenceInHeightThreshold ||
                    differenceInNormals < differenceInNormalsThreshold) return false;
            }
            else
            {
                return false;
            }

        }

        return true;
    }
}

public struct ObjectPlacementData
{
    public Vector3 position;
    public Vector3 normal;
}
