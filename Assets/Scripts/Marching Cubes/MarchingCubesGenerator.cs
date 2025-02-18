using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer)), ExecuteInEditMode]
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
    [SerializeField] private Vector3Int chunkCount = new Vector3Int(1, 1, 1);
    [SerializeField] private Transform chunkRenderPos;                                          // This is the position the chunks will use to see if they should be generated, unrendered or destroyed
    [SerializeField] private Vector2Int chunkRenderDistance;                                    // The number of chunks that will be rendered based on the camera's position
    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();         // Maintains a list of all active chunks to unrender and destroy them accordingly
    private Dictionary<Vector2Int, Chunk> unloadedChunks = new Dictionary<Vector2Int, Chunk>(); // When player render distance is on the edge of an existing chunk, set it to inactive instead of destroying it

    bool updated = false;


    public void GenerateMesh()
    {
        // Get the number of voxels/cubes we will need to march through in the compute shader
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numThreadsPerAxis = Mathf.CeilToInt(numVoxelsPerAxis / (float)numThreads);
        float spacing = bounds / (numPointsPerAxis - 1);

        Vector3 worldBounds = new Vector3(chunkCount.x, chunkCount.y, chunkCount.z) * bounds;
        marchingCubesNoise.GenerateNoise(pointsBuffer, numPointsPerAxis, bounds, worldBounds, center, offset, spacing);

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
        GetComponent<MeshFilter>().sharedMesh = mesh;
        ReleaseBuffers();
    }



    void OnDestroy()
    {
        // release buffers if we delete the generator (eg: when we change scenes)
        if (Application.isPlaying)
        {
            ReleaseBuffers();
        }
    }

    void CreateBuffers()
    {
        int numPoints = numPointsPerAxis * numPointsPerAxis * numPointsPerAxis;
        int numVoxelsPerAxis = numPointsPerAxis - 1;
        int numVoxels = numVoxelsPerAxis * numVoxelsPerAxis * numVoxelsPerAxis;
        int maxTriangleCount = numVoxels * 5;

        // Always create buffers in editor (since buffers are released immediately to prevent memory leak)
        // Otherwise, only create if null or if size has changed
        if (!Application.isPlaying || (pointsBuffer == null || numPoints != pointsBuffer.count))
        {
            if (Application.isPlaying)
            {
                ReleaseBuffers();
            }
            trianglesBuffer = new ComputeBuffer(maxTriangleCount, sizeof(float) * 3 * 3, ComputeBufferType.Append);
            pointsBuffer = new ComputeBuffer(numPoints, sizeof(float) * 4);
            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);

        }
    }

    private void OnValidate()
    {
        // Do not update the mesh when inspector data changes if we in play mode
        if (Application.isPlaying) return;
        updated = true;
    }

    private void OnApplicationQuit()
    {
        ReleaseBuffers();
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
        CreateBuffers();
        GenerateMesh();
        ReleaseBuffers();
    }

    private void GenerateChunks()
    {

    }

    private Vector2Int posToChunkIndex(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / bounds);
        int z = Mathf.RoundToInt(position.z / bounds);

        return new Vector2Int(x, z);
    }

    // Update is called once per frame
    void Update()
    {
        if (!updated) return;
        updated = false;

        CreateBuffers();
        GenerateMesh();

        if (!Application.isPlaying)
        {
            ReleaseBuffers();
        }
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
