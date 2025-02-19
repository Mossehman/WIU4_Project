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
    public virtual ComputeBuffer GenerateNoise(ComputeBuffer output, Vector3Int numPointsPerAxis, Vector3 bounds, Vector3 worldBounds, Vector3 center, Vector3 offset, Vector3 spacing)
    {
        Vector3Int numThreadsPerAxis = new Vector3Int(Mathf.CeilToInt(numPointsPerAxis.x / (float)numThreads), Mathf.CeilToInt(numPointsPerAxis.y / (float)numThreads), Mathf.CeilToInt(numPointsPerAxis.z / (float)numThreads));


        shader.SetBuffer(0, "points", output);
        shader.SetInts("numPointsPerAxis", numPointsPerAxis.x, numPointsPerAxis.y, numPointsPerAxis.z);
        shader.SetVector("bounds", bounds);
        shader.SetVector("center", new Vector4(center.x, center.y, center.z));
        shader.SetVector("offset", new Vector4(offset.x, offset.y, offset.z));
        shader.SetVector("spacing", spacing);
        shader.SetVector("worldSize", worldBounds);

        shader.Dispatch(0, numThreadsPerAxis.x, numThreadsPerAxis.y, numThreadsPerAxis.z);

        foreach (var buffer in buffersToRelease) {
            if (buffer == null) continue;
            buffer.Release();
        }

        return output;
    }


}
