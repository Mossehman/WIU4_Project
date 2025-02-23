using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    // Keep track of which chunk index this chunk lies on in world space (this is for debugging purposes)
    [SerializeField] private bool cull = true;
    public Vector2Int chunkID;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;
    public Vector3 meshOffset = Vector3.zero;
    [HideInInspector] private Bounds bounds;

    public bool drawNodes = false;

    public List<Vector3Int> AStarIndexes = new List<Vector3Int>();
    public List<Vector3> AStarPositions = new List<Vector3>();

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void GenerateBounds(Vector3 meshBounds)
    {
        bounds = new Bounds(transform.position, meshBounds);
    }

    private void Update()
    {
        if (!cull) { return; }
        if (meshRenderer.enabled && !FrustumCull(bounds))
        {
            meshRenderer.enabled = false;
        }
        else if (!meshRenderer.enabled && FrustumCull(bounds))
        {
            meshRenderer.enabled = true;
        }
    }

    public bool FrustumCull(Bounds meshBounds)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        return GeometryUtility.TestPlanesAABB(planes, meshBounds);

    }

    private void OnDrawGizmos()
    {
        if (!drawNodes) { return; }
        Gizmos.color = Color.blue;
        foreach (var pos in AStarPositions)
        {
            Gizmos.DrawWireCube(pos, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    public MeshFilter GetMeshFilter() { return meshFilter; }
    public MeshRenderer GetMeshRenderer() {  return meshRenderer; }

    public MeshCollider GetMeshCollider() { return meshCollider; }
}
