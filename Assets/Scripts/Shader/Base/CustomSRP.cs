using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Template Scriptable Render Pipeline using the Custom Post Processing class, runs the abstract method for sending data to the shader
/// </summary>
public class CustomSRP : ScriptableRenderPass
{
    private Material material;
    private CustomPostProcessing post;

    private RTHandle inputHandle;
    private RTHandle outputHandle;
    
    public CustomSRP(CustomPostProcessing post)
    {
        if (post == null)
        {
            Debug.LogError("Post Processing Shader was null! This should not be happening!");
            return;
        }

        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;    // ensures that the render pass only runs before post processing (to apply the effects)
        this.post = post;                                                   // store a reference to our post processing shader
        this.material = new Material(post.GetShader());                     // generate the material based on the shader
    }


    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var CameraData = renderingData.cameraData;

        // we only want our post processor to be applied in the game view
        if (CameraData.camera.cameraType != CameraType.Game)
        {
            return;
        }

        // check for material
        if (material == null)
        {
            return;
        }

        CommandBuffer cmd = CommandBufferPool.Get();
        if (post.ToProfile())
        {
            using (new ProfilingScope(cmd, post.GetProfiler()))
            {
                BlitTexture(cmd);
            }
        }
        else
        {
            BlitTexture(cmd);
        }

        /// TODO: look into a using a static command buffer instead such that we only execute the command buffer once (after adding all the rendering commands into said buffer)
        context.ExecuteCommandBuffer(cmd);

        // always remember to clear your command buffers after use to avoid memory leaks!!!
        cmd.Clear();
        CommandBufferPool.Release(cmd);
    }


    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        var desc = cameraTextureDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;

        // configure our output handle to be the camera texture descriptor
        RenderingUtils.ReAllocateIfNeeded(ref outputHandle, desc, FilterMode.Bilinear, TextureWrapMode.Clamp);
        ConfigureTarget(outputHandle);
    }

    /// <summary>
    /// Configures the input target for our shader (camera color texture)
    /// </summary>
    /// <param name="handle">The camera color target</param>
    public void SetTarget(RTHandle handle)
    {
        ConfigureInput(post.GetRenderInput());
        inputHandle = handle;
    }

    // this only exists because my OCD will not let me write the same chunk of code twice
    private void BlitTexture(CommandBuffer cmd)
    {
        post.SendDataToShader(material);

        for (int i = 0; i < material.passCount; ++i)
        {
            cmd.Blit(inputHandle, outputHandle);
            if (outputHandle != null)
            {
                cmd.SetGlobalTexture("_CameraTexture", outputHandle.nameID);
            }

            cmd.Blit(outputHandle, inputHandle, material, i);
        }

    }

    public Material GetMaterial() { return this.material; }
}
