using UnityEngine;

/// <summary>
/// Helper script to edit and fine tune the marching cubes mesh via placing gameObjects in the scene with this component added
/// </summary>
public class TerrainSphereEditor : MonoBehaviour
{
    public float radius = 1.0f;
    public float weightModifier = -1.0f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private void Start()
    {
        SendEditData();
        ReloadChunks();
    }

    void SendEditData()
    {
        SphereEditor newEditor = new SphereEditor();
        newEditor.radius = radius;
        newEditor.position = transform.position;
        newEditor.noiseModifier = weightModifier;

        MarchingCubesGenerator.instance.terrainEdits.Add(newEditor);
    }

    void ReloadChunks()
    {
        Vector3 minPos = transform.position - new Vector3(radius, 0, radius);
        Vector3 maxPos = transform.position + new Vector3(radius, 0, radius);

        Vector3Int minChunk = MarchingCubesGenerator.instance.PosToChunkIndex(minPos);
        Vector3Int maxChunk = MarchingCubesGenerator.instance.PosToChunkIndex(maxPos);

        for (int x = minChunk.x; x <= maxChunk.x; x++)
        {
            for (int z = minChunk.z; z <= maxChunk.z; z++)
            {
                Chunk chunkData;
                if (MarchingCubesGenerator.instance.loadedChunks.TryGetValue(new Vector3Int(x, 0, z), out chunkData))
                {
                    MarchingCubesGenerator.instance.RequestChunkUpdate(chunkData);
                }
            }
        }

        Destroy(gameObject);
    }
}
