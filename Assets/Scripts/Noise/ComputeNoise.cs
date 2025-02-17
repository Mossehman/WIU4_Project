using System.Collections.Generic;
using UnityEngine;

public abstract class ComputeNoise : ScriptableObject
{
    public const int numThreads = 8;


    public ComputeShader shader;
    private List<ComputeBuffer> buffersToRelease = new List<ComputeBuffer>();

    /// <summary>
    /// Generates a 3d noise grid of points via compute shader dispatch
    /// <br></br>
    /// This is mainly configured for Marching Cubes
    /// </summary>
    /// <param name="output"></param>
    /// <param name="numPointsPerAxis"></param>
    /// <param name="bounds"></param>
    /// <param name="worldBounds"></param>
    /// <param name="center"></param>
    /// <param name="offset"></param>
    /// <param name="spacing"></param>
    /// <returns></returns>
    public virtual ComputeBuffer GenerateNoise(ComputeBuffer output, int numPointsPerAxis, float bounds, Vector3 worldBounds, Vector3 center, Vector3 offset, float spacing)
    {
        int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / numThreads);

        shader.SetBuffer(0, "points", output);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("bounds", bounds);
        shader.SetVector("centre", new Vector3(center.x, center.y, center.z));
        shader.SetVector("offset", new Vector3(offset.x, offset.y, offset.z));
        shader.SetFloat("spacing", spacing);
        shader.SetVector("worldSize", worldBounds);

        shader.Dispatch(0, numThreadsPerAxis, numPointsPerAxis, numThreadsPerAxis);

        foreach (var buffer in buffersToRelease) {
            if (buffer == null) continue;
            buffer.Release();
        }

        return output;
    }


}
