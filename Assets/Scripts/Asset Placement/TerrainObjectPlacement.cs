using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class to procedurally place objects down into the marching cubes scene
/// </summary>
public class TerrainObjectPlacement : MonoBehaviour
{
    public uint maxObjectsPerChunk;
    public uint objectPlacementChance;
    public bool drawGizmos = false;

    [Header("Assets")]
    public TerrainObject[] terrainAssets;
    public List<ObjectPlacementData> placementData = new List<ObjectPlacementData>();

    [Header("Raycast sampling")]
    public LayerMask terrainLayerMask;
    public float raycastYOffset = 1.0f;
    public float raycastLength = 2.0f;

    // For debugging, remove later
    private List<Vector3> spawnPos = new List<Vector3>();
    private List<Vector3> samplePos = new List<Vector3>();

    public void GenerateSpawnPoints(int seed, Vector3 position, Vector3 normal)
    {
        if (placementData.Count >= maxObjectsPerChunk || terrainAssets.Length == 0) { return; }

        int toSpawn = Random.Range(0, (int)objectPlacementChance);
        if (toSpawn > 0) { return; }

        ObjectPlacementData newData = new ObjectPlacementData();
        newData.position = position;
        newData.normal = normal;
        placementData.Add(newData);
    }

    public void SpawnObjects(Transform t)
    {
        if (placementData.Count == 0) { return; }
        for (int i = 0; i < placementData.Count; i++)
        {
            int objToSpawnIndex = Random.Range(0, terrainAssets.Length);
            TerrainObject objToSpawn = terrainAssets[objToSpawnIndex];
            if ((placementData[i].position.y < objToSpawn.minYLevel)) { continue; }
            else if (placementData[i].position.y > objToSpawn.maxYLevel && objToSpawn.hasMaxYLevel) { continue; }
            float dotProduct = Vector3.Dot(placementData[i].normal.normalized, Vector3.down);
            if (dotProduct < objToSpawn.minNormalsThreshold || (dotProduct > objToSpawn.maxNormalsThreshold && objToSpawn.hasMaxNormals)) { continue; }

            if (!checkNearSurfaces(placementData[i].position, placementData[i].normal, objToSpawn)) { continue; }
            GameObject terrainAssetToSpawn = Instantiate(objToSpawn.terrainObjectPrefab);
            terrainAssetToSpawn.transform.position = placementData[i].position;
            terrainAssetToSpawn.transform.parent = t;
            terrainAssetToSpawn.transform.LookAt(placementData[i].position - placementData[i].normal);

        }

        placementData.Clear();
        //Random.InitState((int)Time.time);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) { return; }

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

    private bool checkNearSurfaces(Vector3 spawnPos, Vector3 normal, TerrainObject objToSpawn)
    {
        uint numSamples = objToSpawn.numTerrainSamples;
        float range = objToSpawn.sampleRadius;


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

                if (differenceInPositions > objToSpawn.heightDifferenceThreshold * objToSpawn.heightDifferenceThreshold ||
                    differenceInNormals < objToSpawn.normalDifferenceThreshold) return false;
            }
            else
            {
                return false;
            }

        }

        return true;
    }
}

[System.Serializable]
public struct ObjectPlacementData
{
    public Vector3 position;
    public Vector3 normal;
}
