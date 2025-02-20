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

    private Vector3 position = Vector3.zero;
    private Vector3 bounds = Vector3.one * -1;
    private bool setVals = false;

    public void PlaceObjects(int seed, Vector3 position, Vector3 bounds)
    {
        if (setVals) { return; }

        this.position = position;
        this.bounds = bounds;
        setVals = true;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(position, new Vector3(bounds.x, 100, bounds.z));
    }
}

public struct ObjectPlacementData
{
    public Vector3 position;
    public Vector3 normal;
}
