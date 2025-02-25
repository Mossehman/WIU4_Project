using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutlineGBufferCache : MonoBehaviour
{
    public OutlineShader_Post outlineShader;
    private void Update()
    {
        outlineShader.normalBuffer = Shader.GetGlobalTexture("_GBuffer2");
    }
}
