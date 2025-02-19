using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    // Keep track of which chunk index this chunk lies on in world space (this is for debugging purposes)
    [SerializeField] private Vector2Int chunkID;
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private MeshCollider meshCollider;
    public Vector3 meshOffset = Vector3.zero;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public MeshFilter GetMeshFilter() { return meshFilter; }
    public MeshRenderer GetMeshRenderer() {  return meshRenderer; }
}
