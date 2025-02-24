using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain Object", menuName = "Marching Cubes/Terrain Object")]
public class TerrainObject : ScriptableObject
{
    [Header("Prefab")]
    public GameObject terrainObjectPrefab;

    [Header("Poisson Sphere")]
    public float poissonSphereRadius = 1.0f;

    [Header("Spawn configuration")]
    public float minYLevel;
    public bool hasMaxYLevel;
    [ConditionalHide(nameof(hasMaxYLevel), true)]
    public float maxYLevel;
    [Range(-1f, 1f)]
    public float minNormalsThreshold;

    public bool hasMaxNormals;
    [ConditionalHide(nameof(hasMaxNormals), true)]
    [Range(-1f, 1f)]
    public float maxNormalsThreshold;

    [Header("Terrain Sampling")]
    public uint numTerrainSamples = 5;
    public float sampleRadius = 4;

    public float heightDifferenceThreshold;

    [Range(-1f, 1f)]
    public float normalDifferenceThreshold;
}
