using UnityEngine;

[CreateAssetMenu(fileName = "Marching Cubes Config", menuName = "Marching Cubes/Default config")]
public class TerrainNoiseConfig : ScriptableObject
{
    [Header("Noise data")]
    public int seed;
    public int octaves = 4;
    public float lacunarity = 0.4f;
    public float persistence = 4.44f;
    public float noiseScale = 6.94f;
    public float noiseWeight = 0.89f;

    public bool sealTop = true;

    public float floorOffset = 6.7f;
    public float weightMultiplier = 1.87f;
    public float hardFloorHeight = 18.4f;
    public float hardFloorWeight = 18.4f;

    public Vector4 shaderParams = new Vector4(1, 0, 0, 0);
}
