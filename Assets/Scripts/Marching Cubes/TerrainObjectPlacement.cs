using UnityEngine;

/// <summary>
/// Helper class to procedurally place objects down into the marching cubes scene
/// </summary>
public class TerrainObjectPlacement : MonoBehaviour
{
    public uint maxObjectsPerChunk;

    [Header("Assets")]
    public GameObject[] terrainAssets;
    [HideInInspector] public ComputeBuffer terrainObjectsBuffer;
}

public struct TerrainPlacementData
{
    public Vector3 position;
    public Vector3 normal;
}
