using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ComputeNoise
{
    public const int numThreads = 8;


    public ComputeShader shader;
    protected List<ComputeBuffer> buffersToRelease = new List<ComputeBuffer>();

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
        int numThreadsPerAxis = Mathf.CeilToInt((float)numPointsPerAxis / (float)numThreads);


        shader.SetBuffer(0, "points", output);
        shader.SetInt("numPointsPerAxis", numPointsPerAxis);
        shader.SetFloat("bounds", bounds);
        shader.SetVector("center", new Vector4(center.x, center.y, center.z));
        shader.SetVector("offset", new Vector4(offset.x, offset.y, offset.z));
        shader.SetFloat("spacing", spacing);
        shader.SetVector("worldSize", worldBounds);

        shader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        foreach (var buffer in buffersToRelease) {
            if (buffer == null) continue;
            buffer.Release();
        }

        return output;
    }


}
