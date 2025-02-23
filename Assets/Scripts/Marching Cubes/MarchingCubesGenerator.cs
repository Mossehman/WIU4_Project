using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class MarchingCubesGenerator : MonoBehaviour
{
    const int numThreads = 8;

    public static MarchingCubesGenerator instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            this.AStar = GetComponent<AStarBounds>();
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    [Header("Compute")]
    public TerrainNoiseConfig noiseConfig;
    public MarchingCubesNoise marchingCubesNoise;
    public TerrainObjectPlacement placementScript;
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
    private HashSet<Vector3Int> loadedAStarChunks = new HashSet<Vector3Int>();   // Maintains a list of all active chunks to unrender and destroy them accordingly


    [Header("AStar")]
    [SerializeField] private AStarBounds AStar;
    private ComputeBuffer AStarNodeBuffer;
    private ComputeBuffer AStarBufferCount;

    //private Dictionary<Vector2Int, Chunk> unloadedChunks = new Dictionary<Vector2Int, Chunk>(); // When player render distance is on the edge of an existing chunk, set it to inactive instead of destroying it

    bool updated = false;


    public void InitialiseNoiseData()
    {
        marchingCubesNoise.seed = noiseConfig.seed;
        marchingCubesNoise.octaves = noiseConfig.octaves;
        marchingCubesNoise.lacunarity = noiseConfig.lacunarity;
        marchingCubesNoise.persistence = noiseConfig.persistence;
        marchingCubesNoise.noiseScale = noiseConfig.noiseScale;
        marchingCubesNoise.noiseWeight = noiseConfig.noiseWeight;
        marchingCubesNoise.closeEdges = noiseConfig.sealTop;
        marchingCubesNoise.floorOffset = noiseConfig.floorOffset;
        marchingCubesNoise.weightMultiplier = noiseConfig.weightMultiplier;
        marchingCubesNoise.hardFloorHeight = noiseConfig.hardFloorHeight;
        marchingCubesNoise.hardFloorWeight = noiseConfig.hardFloorWeight;

        marchingCubesNoise.shaderParams = noiseConfig.shaderParams;
    }

    public void SaveNoiseData()
    {
        noiseConfig.seed = marchingCubesNoise.seed;
        noiseConfig.octaves = marchingCubesNoise.octaves;
        noiseConfig.lacunarity = marchingCubesNoise.lacunarity;
        noiseConfig.persistence = marchingCubesNoise.persistence;
        noiseConfig.noiseScale = marchingCubesNoise.noiseScale;
        noiseConfig.noiseWeight = marchingCubesNoise.noiseWeight;
        noiseConfig.sealTop = marchingCubesNoise.closeEdges;
        noiseConfig.floorOffset = marchingCubesNoise.floorOffset;
        noiseConfig.weightMultiplier = marchingCubesNoise.weightMultiplier;
        noiseConfig.hardFloorHeight = marchingCubesNoise.hardFloorHeight;
        noiseConfig.hardFloorWeight = marchingCubesNoise.hardFloorWeight;

        noiseConfig.shaderParams = marchingCubesNoise.shaderParams;
    }

    public void UnloadAStar(Chunk chunk)
    {
        chunk.AStarPositions.Clear();
        if (chunk.AStarIndexes.Count <= 0) { return; }
        foreach (var voxelIndex in chunk.AStarIndexes)
        {
            AStar.nodes.Remove(voxelIndex);
        }
        chunk.AStarIndexes.Clear();
    }

    /// <summary>
    /// Generate the AStar nodes for a specific chunk's surface
    /// </summary>
    /// <param name="chunk">The chunk to create the navigation surface for</param>
    public void GenerateAStar(Chunk chunk)
    {
        if (AStar == null || AStarNodeBuffer == null) { return; }

        Vector3Int numVoxelsPerAxis = new Vector3Int(numPointsPerAxis.x - 1, numPointsPerAxis.y - 1, numPointsPerAxis.z - 1);
        ComputeBuffer.CopyCount(AStarNodeBuffer, AStarBufferCount, 0);
        int[] AStarCount = { 0 };
        AStarBufferCount.GetData(AStarCount);
        int numNodes = AStarCount[0];

        AStarComputeNode[] aStarNodes = new AStarComputeNode[numNodes];
        AStarNodeBuffer.GetData(aStarNodes, 0, 0, numNodes);

        UnloadAStar(chunk);

        Vector3 spacing = new Vector3(
            bounds.x / numVoxelsPerAxis.x,
            bounds.y / numVoxelsPerAxis.y,
            bounds.z / numVoxelsPerAxis.z
        );

        for (int i = 0; i < numNodes; i++)
        {
            Vector3Int localVoxelIndex = new Vector3Int(aStarNodes[i].voxelIndex.x - (int)(numVoxelsPerAxis.x * 0.5f),
                                                        aStarNodes[i].voxelIndex.y - (int)(numVoxelsPerAxis.y * 0.5f),
                                                        aStarNodes[i].voxelIndex.z - (int)(numVoxelsPerAxis.z * 0.5f));

            
            Vector3Int globalVoxelIndex = new Vector3Int(
                chunk.chunkID.x * numVoxelsPerAxis.x + localVoxelIndex.x,
                localVoxelIndex.y,
                chunk.chunkID.y * numVoxelsPerAxis.z + localVoxelIndex.z
            );

            
            Vector3 nodePosition = chunk.meshOffset + new Vector3(
                localVoxelIndex.x * spacing.x,
                localVoxelIndex.y * spacing.y,
                localVoxelIndex.z * spacing.z
            );

            AStar.GenerateNode(globalVoxelIndex, nodePosition, true);
            chunk.AStarPositions.Add(nodePosition);
        }
    }

    /// <summary>
    /// Tells the chunk to update it's mesh data based on the marching cubes noise
    /// </summary>
    /// <param name="chunk">The chunk to update</param>
    public void GenerateMesh(Chunk chunk, bool generateObjects = true)
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
        marchingCubes.SetVector("chunkCenter", chunk.meshOffset);

        AStarNodeBuffer.SetCounterValue(0);
        marchingCubes.SetBuffer(0, "aStarNodeBuffer", AStarNodeBuffer);
        
        marchingCubes.Dispatch(0, numThreadsPerAxis.x, numThreadsPerAxis.y, numThreadsPerAxis.z);

        ComputeBuffer.CopyCount(trianglesBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int numTris = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[numTris];
        trianglesBuffer.GetData(tris, 0, 0, numTris);
        var vertexIndexMap = new Dictionary<Vector2Int, int>();

        Mesh mesh = new Mesh();
        mesh.Clear();

        var vertices = new Vector3[numTris * 3];
        var meshTriangles = new int[numTris * 3];

        for (int i = 0; i < numTris; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int sharedVertexIndex;
                if (vertexIndexMap.TryGetValue(tris[i][j].id, out sharedVertexIndex))
                {
                    meshTriangles[i * 3 + j] = sharedVertexIndex;
                }
                else
                {
                    vertexIndexMap.Add(tris[i][j].id, i * 3 + j);
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = tris[i][j].position;
                }
            }

            if (generateObjects)
            {
                placementScript.GenerateSpawnPoints(marchingCubesNoise.seed, tris[i].center + chunk.meshOffset, tris[i].normal);
            }

        }

        Vector3 centerPoint = mesh.bounds.center; // or set your own center


        mesh.indexFormat = IndexFormat.UInt32;


        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
        Vector3[] normals = mesh.normals;

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 pos = vertices[i] - centerPoint;    
            Vector3 n = normals[i];
            
            Vector3 blending = new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z));
            float total = blending.x + blending.y + blending.z;
            if (total > 0)
                blending /= total;
            else
                blending = new Vector3(0.33f, 0.33f, 0.33f);
    
            Vector2 uvX = new Vector2(pos.y, pos.z);
            Vector2 uvY = new Vector2(pos.x, pos.z);
            Vector2 uvZ = new Vector2(pos.x, pos.y);
            
            Vector2 uv = uvX * blending.x + uvY * blending.y + uvZ * blending.z;
            uvs[i] = uv;
        }


        mesh.uv = uvs;

        chunk.GenerateBounds(bounds);
        chunk.GetMeshFilter().mesh.Clear();
        chunk.GetMeshFilter().sharedMesh = mesh;
        if (chunk.GetMeshCollider() != null)
        {
            if (chunk.GetMeshCollider().sharedMesh != null)
            {
                chunk.GetMeshCollider().sharedMesh.Clear();

            }
            chunk.GetMeshCollider().sharedMesh = mesh;
        }
        if (generateObjects)
        {
            placementScript.SpawnObjects(chunk.transform);
        }

        //GenerateAStar(chunk);

        //placementScript.PlaceObjects(marchingCubesNoise.seed, chunk.meshOffset + center, bounds);

    }

    public Vector3Int PositionToGlobalVoxelIndex(Vector3 position)
    {
        Vector3Int numVoxelsPerAxis = new Vector3Int(numPointsPerAxis.x - 1, numPointsPerAxis.y - 1, numPointsPerAxis.z - 1);

        Vector3 spacing = new Vector3(
            bounds.x / numVoxelsPerAxis.x,
            bounds.y / numVoxelsPerAxis.y,
            bounds.z / numVoxelsPerAxis.z
        );

        Vector3Int voxelIndex = new Vector3Int(Mathf.FloorToInt(position.x / spacing.x), Mathf.FloorToInt(position.y / spacing.y), Mathf.FloorToInt(position.z / spacing.z));
        return voxelIndex;
    }

    public Vector3 VoxelToPosition(Vector3Int voxelPosition)
    {
        Vector3Int numVoxelsPerAxis = new Vector3Int(numPointsPerAxis.x - 1, numPointsPerAxis.y - 1, numPointsPerAxis.z - 1);

        Vector3 spacing = new Vector3(
            bounds.x / numVoxelsPerAxis.x,
            bounds.y / numVoxelsPerAxis.y,
            bounds.z / numVoxelsPerAxis.z
        );

        return new Vector3(voxelPosition.x * spacing.x, voxelPosition.y * spacing.y, voxelPosition.z * spacing.z);
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
        SaveNoiseData();
        ReleaseBuffers();
    }
    void CreateBuffers()
    {
        int numPoints = numPointsPerAxis.x * numPointsPerAxis.y * numPointsPerAxis.z;
        Vector3Int numVoxelsPerAxis = new Vector3Int(numPointsPerAxis.x - 1, numPointsPerAxis.y - 1, numPointsPerAxis.z - 1);
        int numVoxels = numVoxelsPerAxis.x * numVoxelsPerAxis.y * numVoxelsPerAxis.z;
        int maxTriangleCount = numVoxels * 5;
    
        if (pointsBuffer == null || numPoints != pointsBuffer.count)
        {
            ReleaseBuffers();
            trianglesBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 5 + sizeof(int) * 2 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

            int stride = sizeof(int) * 4;
            // Each AStar node has a vector3Int (for voxel index) and a bool (for passability)
            AStarNodeBuffer = new ComputeBuffer(numVoxels, stride, ComputeBufferType.Append);
            AStarBufferCount = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
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

        if (AStarNodeBuffer != null)
        {
            AStarNodeBuffer.Release();
            AStarBufferCount.Release();
        }
    }

    void Start()
    {
        InitialiseNoiseData();


        DestroyAllChunks();
        CreateBuffers();
        InitialiseChunks();
        //GenerateChunks();
        //ReleaseBuffers();
    }

    /// <summary>
    /// This should run everytime the player's current chunk updates
    /// </summary>
    private void GenerateChunks()
    {
        if (player == null) { return; }

        Vector3Int startingChunkIndex = PosToChunkIndex(player.position);

        Stack<Vector3Int> newChunkPositions = new Stack<Vector3Int>();

        HashSet<Vector3Int> currentChunks = new HashSet<Vector3Int>(); //keep a lookup table of the chunks we are currently in
        HashSet<Vector3Int> aStarChunks = new HashSet<Vector3Int>();

        for (int x = startingChunkIndex.x - chunkRenderDistance.x; x <= startingChunkIndex.x + chunkRenderDistance.x; x++) 
        {
            for (int y = startingChunkIndex.y - chunkRenderDistance.y; y <= startingChunkIndex.y + chunkRenderDistance.y; y++)
            {
                for (int z = startingChunkIndex.z - chunkRenderDistance.z; z <= startingChunkIndex.z + chunkRenderDistance.z; z++)
                {
                    Vector3Int currChunkID = new Vector3Int(x, y, z);
                    if (AStar != null && 
                        Mathf.Abs(x - startingChunkIndex.x) <= AStar.AStarRange &&
                        Mathf.Abs(y - startingChunkIndex.y) <= AStar.AStarRange &&
                        Mathf.Abs(z - startingChunkIndex.z) <= AStar.AStarRange)
                    {
                        aStarChunks.Add(currChunkID);
                    }

                    currentChunks.Add(currChunkID);
                    if (loadedChunks.ContainsKey(currChunkID)) { continue; } //chunk has already been loaded, no need to add it
                    newChunkPositions.Push(currChunkID);
                }
            }

        }

        Dictionary<Vector3Int, Chunk> existingChunksCopy = new Dictionary<Vector3Int, Chunk>(loadedChunks);
        UnityEngine.Random.InitState(marchingCubesNoise.seed);

        foreach (var existingChunk in existingChunksCopy)
        {
            if (newChunkPositions.Count == 0) { break; }
            Vector3Int chunkPos = existingChunk.Key;
            Chunk chunkData = existingChunk.Value;

            if (!aStarChunks.Contains(chunkPos)) {
                loadedAStarChunks.Remove(chunkPos);
                UnloadAStar(chunkData); 
            }
            if (currentChunks.Contains(chunkPos)) { continue; }
            loadedChunks.Remove(chunkPos);
            chunkData.gameObject.transform.position = new Vector3(newChunkPositions.Peek().x * bounds.x, newChunkPositions.Peek().y * bounds.y, newChunkPositions.Peek().z * bounds.z);
            chunkData.gameObject.GetComponent<Chunk>().meshOffset = new Vector3(newChunkPositions.Peek().x * bounds.x, newChunkPositions.Peek().y * bounds.y, newChunkPositions.Peek().z * bounds.z);
            chunkData.GetComponent<Chunk>().chunkID = new Vector2Int(newChunkPositions.Peek().x, newChunkPositions.Peek().z);


            for (int i = 0; i < chunkData.transform.childCount; i++)
            {
                Destroy(chunkData.transform.GetChild(i).gameObject);
            }

            GenerateMesh(chunkData.gameObject.GetComponent<Chunk>());
            //if (aStarChunks.Contains(newChunkPositions.Peek())) { GenerateAStar(chunkData); }
            loadedChunks.TryAdd(newChunkPositions.Peek(), chunkData);
            newChunkPositions.Pop();
            return;
        }

        foreach(var aStarChunkID in aStarChunks)
        {
            Chunk chunkData;

            loadedChunks.TryGetValue(aStarChunkID, out chunkData);
            Vector3Int AStarChunkID = new Vector3Int(chunkData.chunkID.x, 0, chunkData.chunkID.y);
            if (aStarChunks.Contains(AStarChunkID) && !loadedAStarChunks.Contains(AStarChunkID))
            {
                GenerateMesh(chunkData, false);
                loadedAStarChunks.Add(AStarChunkID);
                GenerateAStar(chunkData);
                return;
            }
        }

        UnityEngine.Random.InitState((int)Time.time);
    }

    private void InitialiseChunks()
    {
        if (player == null) { return; }

        Vector3Int startingChunkIndex = PosToChunkIndex(player.position);
        UnityEngine.Random.InitState(marchingCubesNoise.seed);

        for (int x = startingChunkIndex.x - chunkRenderDistance.x; x <= startingChunkIndex.x + chunkRenderDistance.x; x++)
        {
            for (int y = startingChunkIndex.y - chunkRenderDistance.y; y <= startingChunkIndex.y + chunkRenderDistance.y; y++)
            {
                for (int z = startingChunkIndex.z - chunkRenderDistance.z; z <= startingChunkIndex.z + chunkRenderDistance.z; z++)
                {
                    Vector3Int currChunkID = new Vector3Int(x, y, z);

                    GameObject newChunk = Instantiate(chunkPrefab);
                    newChunk.transform.position = new Vector3(x * bounds.x, y * bounds.y, z * bounds.z);
                    newChunk.transform.parent = transform;
                    newChunk.GetComponent<Chunk>().meshOffset = new Vector3(x * bounds.x, y * bounds.y, z * bounds.z);
                    newChunk.GetComponent<Chunk>().chunkID = new Vector2Int(x, z);
                    loadedChunks.Add(currChunkID, newChunk.GetComponent<Chunk>());
                    //Debug.Log(currChunkID.ToString());
                    GenerateMesh(newChunk.GetComponent<Chunk>());


                    if (AStar != null &&
                        Mathf.Abs(x - startingChunkIndex.x) <= AStar.AStarRange &&
                        Mathf.Abs(y - startingChunkIndex.y) <= AStar.AStarRange &&
                        Mathf.Abs(z - startingChunkIndex.z) <= AStar.AStarRange)
                    {
                        loadedAStarChunks.Add(currChunkID);
                        GenerateAStar(newChunk.GetComponent<Chunk>());
                    }
                }
            }
        }
        UnityEngine.Random.InitState((int)Time.time);
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
        //if (updated)
        //{
        //    DestroyAllChunks();
        //    updated = false;
        //}

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

    public AStarBounds GetAStar()
    {
        return AStar;
    }
}


struct Triangle
{
    public Vertex a;
    public Vertex b;
    public Vertex c;

    public Vector3 center;
    public Vector3 normal;

    public Vertex this[int i]
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


struct Vertex
{
    public Vector3 position;
    public Vector2Int id;
}

public struct SphereEditor
{
    public Vector3 position;
    public float radius;
    public float noiseModifier;
}
