using UnityEngine;

[CreateAssetMenu(fileName = "Marching Cubes Noise Gen", menuName = "Noise/Compute/Marching Cubes")]
public class MarchingCubesNoise : ComputeNoise
{
    [Header("Noise")]
    public int seed;
    public int octaves = 4;
    public float lacunarity = 2;
    public float persistence = .5f;
    public float noiseScale = 1;
    public float noiseWeight = 1;
    public bool closeEdges;
    public float floorOffset = 1;
    public float weightMultiplier = 1;

    public float hardFloorHeight;
    public float hardFloorWeight;

    public Vector4 shaderParams;

    public override ComputeBuffer GenerateNoise(ComputeBuffer output, int numPointsPerAxis, float bounds, Vector3 worldBounds, Vector3 center, Vector3 offset, float spacing)
    {
        System.Random randomVal = new System.Random(seed);
        Vector3[] noiseOffsets = new Vector3[octaves];
        float offsetRange = 1000;

        for (int i = 0; i < octaves; i++)
        {
            noiseOffsets[i] = new Vector3((float)randomVal.NextDouble() * 2 - 1, (float)randomVal.NextDouble() * 2 - 1, (float)randomVal.NextDouble() * 2 - 1) * offsetRange;
        }
        ComputeBuffer offsetsBuffer = new ComputeBuffer(noiseOffsets.Length, sizeof(float) * 3);

        shader.SetVector("center", new Vector3(center.x, center.y, center.z));
        shader.SetInt("octaves", Mathf.Max(1, octaves));
        shader.SetFloat("lacunarity", lacunarity);
        shader.SetFloat("persistence", persistence);
        shader.SetFloat("noiseScale", noiseScale);
        shader.SetFloat("noiseWeight", noiseWeight);
        shader.SetBool("closeEdges", closeEdges);
        shader.SetBuffer(0, "offsets", offsetsBuffer);
        shader.SetFloat("floorOffset", floorOffset);
        shader.SetFloat("weightMultiplier", weightMultiplier);
        shader.SetFloat("hardFloor", hardFloorHeight);
        shader.SetFloat("hardFloorWeight", hardFloorWeight);
        
        shader.SetVector("params", shaderParams);

        return base.GenerateNoise(output, numPointsPerAxis, bounds, worldBounds, center, offset, spacing);

    }
}
