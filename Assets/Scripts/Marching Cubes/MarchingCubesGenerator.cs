using System.Collections.Generic;
using UnityEngine;

public class MarchingCubesGenerator : MonoBehaviour
{
    const int numThreads = 8;

    [Header("Compute")]
    public MarchingCubesNoise marchingCubesNoise;
    public ComputeShader marchingCubes;

    [Header("Marching Cubes config")]
    public float surfaceLevel = 0.4f;
    public Vector3Int numPointsPerAxis = new Vector3Int(30, 30, 30);
    public Vector3 bounds = new Vector3(20, 20, 20);
    public Vector3 center = Vector3.one;
    public Vector3 offset = Vector3.zero;
    public Material mat;

    private ComputeBuffer pointsBuffer;
    private ComputeBuffer trianglesBuffer;
    private ComputeBuffer triCountBuffer;

    [Header("Chunks")]
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private Transform player;                                                  // This is the position the chunks will use to see if they should be generated, unrendered or destroyed
    [SerializeField] private Vector3Int chunkRenderDistance;                                    // The number of chunks that will be rendered based on the camera's position
    private Dictionary<Vector3Int, Chunk> loadedChunks = new Dictionary<Vector3Int, Chunk>();   // Maintains a list of all active chunks to unrender and destroy them accordingly


    //private Dictionary<Vector2Int, Chunk> unloadedChunks = new Dictionary<Vector2Int, Chunk>(); // When player render distance is on the edge of an existing chunk, set it to inactive instead of destroying it

    bool updated = false;


    public void GenerateMesh(Chunk chunk)
    {
        // Get the number of voxels/cubes we will need to march through in the compute shader
        Vector3Int numVoxelsPerAxis = new Vector3Int(numPointsPerAxis.x - 1, numPointsPerAxis.y - 1, numPointsPerAxis.z - 1);
        Vector3Int numThreadsPerAxis = new Vector3Int(Mathf.CeilToInt(numVoxelsPerAxis.x / (float)numThreads), Mathf.CeilToInt(numVoxelsPerAxis.y / (float)numThreads), Mathf.CeilToInt(numVoxelsPerAxis.z / (float)numThreads));
        Vector3 spacing = new Vector3(bounds.x / numVoxelsPerAxis.x, bounds.y / numVoxelsPerAxis.y, bounds.z / numVoxelsPerAxis.z);

        Vector3 worldBounds = bounds;
        TerrainSphereEditor[] spheres = FindObjectsOfType<TerrainSphereEditor>();
        SphereEditor[] sphereEdits = new SphereEditor[spheres.Length];
        for (int i = 0; i < spheres.Length; i++)
        {
            sphereEdits[i] = new SphereEditor();
            sphereEdits[i].position = spheres[i].gameObject.transform.position;
            sphereEdits[i].radius = spheres[i].radius;
            sphereEdits[i].noiseModifier = spheres[i].weightModifier;
        }
        marchingCubesNoise.spheres = sphereEdits;

        marchingCubesNoise.GenerateNoise(pointsBuffer, numPointsPerAxis, bounds, worldBounds, center, chunk.meshOffset, spacing);

        trianglesBuffer.SetCounterValue(0); //reset the index of the triangle buffer back to the start of the list
        marchingCubes.SetBuffer(0, "cubePoints", pointsBuffer);
        marchingCubes.SetBuffer(0, "triangles", trianglesBuffer);
        marchingCubes.SetInts("numPointsPerAxis", numPointsPerAxis.x, numPointsPerAxis.y, numPointsPerAxis.z);
        marchingCubes.SetFloat("surfaceLevel", surfaceLevel);

        marchingCubes.Dispatch(0, numThreadsPerAxis.x, numThreadsPerAxis.y, numThreadsPerAxis.z);

        ComputeBuffer.CopyCount(trianglesBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        trianglesBuffer.GetData(tris, 0, 0, numTris);


        Mesh mesh = new Mesh();
        mesh.Clear();

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }

        }

        Vector3 minExtent = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxExtent = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int i = 0; i < vertices.Length; i++)
        {
            minExtent = Vector3.Min(minExtent, vertices[i]);
            maxExtent = Vector3.Max(maxExtent, vertices[i]);
        }

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            uvs[i] = new Vector2(
                Mathf.InverseLerp(minExtent.x, maxExtent.x, vertices[i].x),
                Mathf.InverseLerp(minExtent.z, maxExtent.z, vertices[i].z)
            );
        }


        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;


        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        mesh.uv = uvs;  
        

        mesh.RecalculateNormals();
        chunk.GetMeshFilter().sharedMesh = mesh;
        if (chunk.GetMeshCollider() != null)
        {
            chunk.GetMeshCollider().sharedMesh = mesh;
        }

    }



    void OnDestroy()
    {
        if (Application.isPlaying)
        {
            DestroyAllChunks();
            ReleaseBuffers();
        }
    }

    void OnApplicationQuit()
    {
        ReleaseBuffers();
    }
    void CreateBuffers()
{
    int numPoints = numPointsPerAxis.x * numPointsPerAxis.y * numPointsPerAxis.z;
    Vector3Int numVoxelsPerAxis = new Vector3Int(numPointsPerAxis.x - 1, numPointsPerAxis.y - 1, numPointsPerAxis.z - 1);
    int numVoxels = numVoxelsPerAxis.x * numVoxelsPerAxis.y * numVoxelsPerAxis.z;
    int maxTriangleCount = numVoxels * 5;

    if (pointsBuffer == null) { Debug.Log("Buffer was null!!!"); }

    if (pointsBuffer == null || numPoints != pointsBuffer.count)
    {
        ReleaseBuffers();
        trianglesBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
        pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
        triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
    }
}

    private void OnValidate()
    {
        // Do not update the mesh when inspector data changes if we in play mode
        if (!Application.isPlaying) return;
        updated = true;
    }

    void ReleaseBuffers()
    {
        if (trianglesBuffer != null)
        {
            trianglesBuffer.Release();
            pointsBuffer.Release();
            triCountBuffer.Release();
        }
    }

    void Start()
    {
        DestroyAllChunks();
        CreateBuffers();
        GenerateChunks();
        //ReleaseBuffers();
    }

    /// <summary>
    /// This should run everytime the player's current chunk updates
    /// </summary>
    private void GenerateChunks()
    {
        if (player == null) { return; }

        Vector3Int startingChunkIndex = PosToChunkIndex(player.position);
        HashSet<Vector3Int> currentChunks = new HashSet<Vector3Int>(); //keep a lookup table of the chunks we are currently in

        for (int x = startingChunkIndex.x - chunkRenderDistance.x; x <= startingChunkIndex.x + chunkRenderDistance.x; x++) 
        {
            for (int y = startingChunkIndex.y - chunkRenderDistance.y; y <= startingChunkIndex.y + chunkRenderDistance.y; y++)
            {
                for (int z = startingChunkIndex.z - chunkRenderDistance.z; z <= startingChunkIndex.z + chunkRenderDistance.z; z++)
                {
                    currentChunks.Add(new Vector3Int(x, y, z));
                    if (loadedChunks.ContainsKey(new Vector3Int(x, y, z))) { continue; } //chunk has already been loaded, no need to add it

                    GameObject newChunk = Instantiate(chunkPrefab);
                    newChunk.transform.position = new Vector3(x * bounds.x, y * bounds.y, z * bounds.z);
                    newChunk.transform.parent = transform;
                    newChunk.GetComponent<Chunk>().meshOffset = new Vector3(x * bounds.x, y * bounds.y, z * bounds.z);
                    loadedChunks.Add(new Vector3Int(x, y, z), newChunk.GetComponent<Chunk>());
                    GenerateMesh(newChunk.GetComponent<Chunk>());
                }
            }

        }

        List<Vector3Int> chunksToRemove  = new List<Vector3Int>();  

        foreach (var existingChunk in loadedChunks)
        {
            Vector3Int chunkPos = existingChunk.Key;
            Chunk chunkData = existingChunk.Value;

            if (currentChunks.Contains(chunkPos)) { continue; }
            Destroy(chunkData.gameObject);
            chunksToRemove.Add(chunkPos);
        }

        foreach (var chunkToRemove in chunksToRemove)
        {
            loadedChunks.Remove(chunkToRemove);
        }
    }

    private Vector3Int PosToChunkIndex(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / bounds.x);
        int y = Mathf.RoundToInt(transform.position.y / bounds.y);
        int z = Mathf.RoundToInt(position.z / bounds.z);

        return new Vector3Int(x, y, z);
    }

    // Update is called once per frame
    void Update()
    {
        if (updated)
        {
            DestroyAllChunks();
            updated = false;
        }

        GenerateChunks();

        //if (!updated && !Application.isPlaying) return;
        //updated = false;

        //CreateBuffers();

        //if (!Application.isPlaying)
        //{
        //    ReleaseBuffers();
        //}
    }

    private void DestroyAllChunks()
    {
        foreach (var chunk in loadedChunks)
        {
            Destroy(chunk.Value.gameObject);
        }

        loadedChunks.Clear();
    }
}



struct Triangle
{
    public Vector3 a;
    public Vector3 b;
    public Vector3 c;

    public Vector3 this[int i]
    {
        get
        {
            switch (i)
            {
                case 0:
                    return a;
                case 1:
                    return b;
                default:
                    return c;
            }
        }
    }
}

public struct SphereEditor
{
    public Vector3 position;
    public float radius;
    public float noiseModifier;
}
