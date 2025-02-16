using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubesGenerator : MonoBehaviour
{
    [Header("Compute")]
    public ComputeNoise marchingCubesNoise;
    public ComputeShader marchingCubes;

    [Header("Marching Cubes config")]

    public int numPointsPerAxis = 30;
    public float bounds = 1;
    public Vector3 center = Vector3.zero;
    public Vector3 offset = Vector3.zero;


    public void GenerateNoise()
    {
        
    }

    private ComputeBuffer GenerateNoise(ComputeBuffer points, Vector3 worldBounds, float spacing)
    {
        return null;
    }

    private void OnValidate()
    {
        
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
