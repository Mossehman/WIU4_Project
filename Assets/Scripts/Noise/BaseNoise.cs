using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper class for generating noise using different algorithms (eg: perlin, simplex, whorley, etc), values are normalized between [0 - 1]
/// </summary>
/// 
public abstract class BaseNoise : ScriptableObject
{
    [Header("Seed")]
    [SerializeField] protected int seed = 0;
    [SerializeField] protected bool useRandomSeed = false;
    [SerializeField] protected uint seedRange = 10000;

    /// <summary>
    /// Generates a seed (or uses the existing one if useRandomSeed) is false
    /// </summary>
    public void InitializeSeed()
    {
        if (!useRandomSeed || seedRange == 0) { return; }
        seed = (int) Random.Range(-seedRange, seedRange);
    }

    /// <summary>
    /// Abstract function for generating 1-dimensional noise
    /// </summary>
    /// <param name="x">x coordinate for outputting the 1D noise</param>
    /// <returns></returns>
    public abstract float GenerateNoise(float x);

    /// <summary>
    /// Abstract function for generating 2-dimensional noise
    /// </summary>
    /// <param name="x">x coordinate for outputting the 2D noise</param>
    /// <param name="y">y coordinate for outputting the 2D noise</param>
    /// <returns></returns>
    public abstract float GenerateNoise(float x, float y);

    /// <summary>
    /// Abstract function for generating 3-dimensional noise
    /// </summary>
    /// <param name="x">x coordinate for outputting the 3D noise</param>
    /// <param name="y">y coordinate for outputting the 3D noise</param>
    /// <param name="z">z coordinate for outputting the 3D noise</param>
    /// <returns></returns>
    public abstract float GenerateNoise(float x, float y, float z);

    public int GetSeed() { return seed; }
    public uint GetSeedRange() { return seedRange; }
}
