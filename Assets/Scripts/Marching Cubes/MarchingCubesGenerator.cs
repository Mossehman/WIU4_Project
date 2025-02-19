using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubesGenerator : MonoBehaviour
{
    const int numThreads = 8;

    [Header("Compute")]
    public MarchingCubesNoise marchingCubesNoise;
    public ComputeShader marchingCubes;

    [Header("Marching Cubes config")]
    public float surfaceLevel = 0.4f;
    public int numPointsPerAxis = 30;
    public float bounds = 1;
    public Vector3 center = Vector3.one;
    public Vector3 offset = Vector3.zero;
    public Material mat;

    private ComputeBuffer pointsBuffer;
    private ComputeBuffer trianglesBuffer;
    private ComputeBuffer triCountBuffer;

    [Header("Chunks")]
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private Transform player;                                                  // This is the position the chunks will use to see if they should be generated, unrendered or destroyed
    [SerializeField] private Vector2Int chunkRenderDistance;                                    // The number of chunks that will be rendered based on the camera's position
    private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();   // Maintains a list of all active chunks to unrender and destroy them accordingly


    //private Dictionary<Vector2Int, Chunk> unloadedChunks = new Dictionary<Vector2Int, Chunk>(); // When player render distance is on the edge of an existing chunk, set it to inactive instead of destroying it

    bool updated = false;


    public void GenerateMesh(Chunk chunk)
    {
        // Get the number of voxels/cubes we will need to march through in the compute shader
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)numThreads);
        float spacing = bounds / (numPointsPerAxis - 1);

        Vector3 worldBounds = Vector3.one * bounds;
        marchingCubesNoise.GenerateNoise(pointsBuffer, numPointsPerAxis, bounds, worldBounds, center, chunk.meshOffset, spacing);

        trianglesBuffer.SetCounterValue(0); //reset the index of the triangle buffer back to the start of the list
        marchingCubes.SetBuffer(0, "cubePoints", pointsBuffer);
        marchingCubes.SetBuffer(0, "triangles", trianglesBuffer);
        marchingCubes.SetInt("numPointsPerAxis", numPointsPerAxis);
        marchingCubes.SetFloat("surfaceLevel", surfaceLevel);

        marchingCubes.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

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
    int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
    int numVoxelsPerAxis = numPointsPerAxis - 1;
    int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
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

        Vector2Int startingChunkIndex = PosToChunkIndex(player.position);
        HashSet<Vector2Int> currentChunks = new HashSet<Vector2Int>(); //keep a lookup table of the chunks we are currently in

        for (int x = startingChunkIndex.x - chunkRenderDistance.x; x <= startingChunkIndex.x + chunkRenderDistance.x; x++) {
            for (int z = startingChunkIndex.y - chunkRenderDistance.y; z <= startingChunkIndex.y + chunkRenderDistance.y; z++) {
                currentChunks.Add(new Vector2Int(x, z));
                if (loadedChunks.ContainsKey(new Vector2Int(x, z))) { continue; } //chunk has already been loaded, no need to add it

                GameObject newChunk = Instantiate(chunkPrefab);
                newChunk.transform.position = new Vector3(x * bounds, transform.position.y, z * bounds);
                newChunk.transform.parent = transform;
                newChunk.GetComponent<Chunk>().meshOffset = new Vector3(x * bounds, transform.position.y, z * bounds);
                loadedChunks.Add(new Vector2Int(x, z), newChunk.GetComponent<Chunk>());
                GenerateMesh(newChunk.GetComponent<Chunk>());
            }

        }

        List<Vector2Int> chunksToRemove  = new List<Vector2Int>();  

        foreach (var existingChunk in loadedChunks)
        {
            Vector2Int chunkPos = existingChunk.Key;
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

    private Vector2Int PosToChunkIndex(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / bounds);
        int z = Mathf.RoundToInt(position.z / bounds);

        return new Vector2Int(x, z);
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
