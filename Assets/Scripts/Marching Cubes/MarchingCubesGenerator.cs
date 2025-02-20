using System.Collections.Generic;
using System.ComponentModel;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class MarchingCubesGenerator : MonoBehaviour
{
    const int numThreads = 8;

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

        Vector3 centerPoint = mesh.bounds.center; // or set your own center


        mesh.indexFormat = IndexFormat.UInt32;


        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;
        mesh.RecalculateNormals();
        Vector3[] normals = mesh.normals;

        Vector2[] uvs = new Vector2[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            // Get the vertex position (optionally offset by a center)
            Vector3 pos = vertices[i] - centerPoint;
            // Retrieve the normal for blending weights.
            Vector3 n = normals[i];

            // Compute the blending weights from the absolute normal components.
            // These determine how much each planar projection contributes.
            Vector3 blending = new Vector3(Mathf.Abs(n.x), Mathf.Abs(n.y), Mathf.Abs(n.z));
            float total = blending.x + blending.y + blending.z;
            if (total > 0)
                blending /= total;
            else
                blending = new Vector3(0.33f, 0.33f, 0.33f);

            // Compute UVs for each projection:
            // Projection onto YZ plane (ignores X): use (Y, Z)
            Vector2 uvX = new Vector2(pos.y, pos.z);
            // Projection onto XZ plane (ignores Y): use (X, Z)
            Vector2 uvY = new Vector2(pos.x, pos.z);
            // Projection onto XY plane (ignores Z): use (X, Y)
            Vector2 uvZ = new Vector2(pos.x, pos.y);

            // Blend the three sets of UVs using the computed weights.
            Vector2 uv = uvX * blending.x + uvY * blending.y + uvZ * blending.z;
            uvs[i] = uv;
        }


        mesh.uv = uvs;  

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

        placementScript.PlaceObjects(marchingCubesNoise.seed, chunk.meshOffset + center, bounds);

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

        for (int x = startingChunkIndex.x - chunkRenderDistance.x; x <= startingChunkIndex.x + chunkRenderDistance.x; x++) 
        {
            for (int y = startingChunkIndex.y - chunkRenderDistance.y; y <= startingChunkIndex.y + chunkRenderDistance.y; y++)
            {
                for (int z = startingChunkIndex.z - chunkRenderDistance.z; z <= startingChunkIndex.z + chunkRenderDistance.z; z++)
                {
                    Vector3Int currChunkID = new Vector3Int(x, y, z);
                    currentChunks.Add(currChunkID);
                    if (loadedChunks.ContainsKey(currChunkID)) { continue; } //chunk has already been loaded, no need to add it
                    newChunkPositions.Push(currChunkID);

                    //GameObject newChunk = Instantiate(chunkPrefab);
                    //newChunk.transform.position = new Vector3(x * bounds.x, y * bounds.y, z * bounds.z);
                    //newChunk.transform.parent = transform;
                    //newChunk.GetComponent<Chunk>().meshOffset = new Vector3(x * bounds.x, y * bounds.y, z * bounds.z);
                    //loadedChunks.Add(new Vector3Int(x, y, z), newChunk.GetComponent<Chunk>());
                    //GenerateMesh(newChunk.GetComponent<Chunk>());
                }
            }

        }

        //List<Vector3Int> chunksToRemove  = new List<Vector3Int>(); 
        //List<Vector3Int> chunkIDsToAdd  = new List<Vector3Int>();  
        //List<Chunk> chunksToAdd  = new List<Chunk>();

        Dictionary<Vector3Int, Chunk> existingChunksCopy = new Dictionary<Vector3Int, Chunk>(loadedChunks);

        foreach (var existingChunk in existingChunksCopy)
        {
            if (newChunkPositions.Count == 0) { break; }
            Vector3Int chunkPos = existingChunk.Key;
            Chunk chunkData = existingChunk.Value;

            if (currentChunks.Contains(chunkPos)) { continue; }
            loadedChunks.Remove(chunkPos);
            chunkData.gameObject.transform.position = new Vector3(newChunkPositions.Peek().x * bounds.x, newChunkPositions.Peek().y * bounds.y, newChunkPositions.Peek().z * bounds.z);
            chunkData.gameObject.GetComponent<Chunk>().meshOffset = new Vector3(newChunkPositions.Peek().x * bounds.x, newChunkPositions.Peek().y * bounds.y, newChunkPositions.Peek().z * bounds.z);
            GenerateMesh(chunkData.gameObject.GetComponent<Chunk>());
            loadedChunks.TryAdd(newChunkPositions.Peek(), chunkData);
            newChunkPositions.Pop();
            break;

            //Destroy(chunkData.gameObject);
            //chunksToRemove.Add(chunkPos);
        }

        //foreach (var chunkToRemove in chunksToRemove)
        //{
        //    loadedChunks.Remove(chunkToRemove);
        //}
        //
        //for (int i = 0; i < chunkIDsToAdd.Count; i++)
        //{
        //    loadedChunks.TryAdd(chunkIDsToAdd[i], chunksToAdd[i]);
        //}
    }

    private void InitialiseChunks()
    {
        if (player == null) { return; }

        Vector3Int startingChunkIndex = PosToChunkIndex(player.position);

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
                    loadedChunks.Add(currChunkID, newChunk.GetComponent<Chunk>());
                    //Debug.Log(currChunkID.ToString());
                    GenerateMesh(newChunk.GetComponent<Chunk>());
                }
            }
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

    Vector2 CubeMapUV(Vector3 d)
    {
        float absX = Mathf.Abs(d.x);
        float absY = Mathf.Abs(d.y);
        float absZ = Mathf.Abs(d.z);
        float u, v;

        // Determine the dominant axis.
        if (absX >= absY && absX >= absZ)
        {
            // X is dominant.
            if (d.x > 0)
            {
                u = -d.z / absX;
                v = d.y / absX;
            }
            else
            {
                u = d.z / absX;
                v = d.y / absX;
            }
        }
        else if (absY >= absX && absY >= absZ)
        {
            // Y is dominant.
            if (d.y > 0)
            {
                u = d.x / absY;
                v = -d.z / absY;
            }
            else
            {
                u = d.x / absY;
                v = d.z / absY;
            }
        }
        else
        {
            // Z is dominant.
            if (d.z > 0)
            {
                u = d.x / absZ;
                v = d.y / absZ;
            }
            else
            {
                u = -d.x / absZ;
                v = d.y / absZ;
            }
        }

        // Map from [-1, 1] to [0, 1].
        return new Vector2((u + 1f) * 0.5f, (v + 1f) * 0.5f);
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
