using UnityEngine;
using UnityEngine.Rendering.Universal;

public class TemplateRenderFeature : ScriptableRendererFeature
{
    [Tooltip("Create a CustomPostProcessing object, set all the parameters and place it here, should get applied as a post effect in game view")]
    [SerializeField] private CustomPostProcessing[] postShaders;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        foreach (var shader in postShaders)
        {
            if (shader == null || shader.GetPass() == null) {  continue; }
            renderer.EnqueuePass(shader.GetPass()); // queue the CustomSRP to be renderered as a post processing effect
        }
    }
    
    public override void Create()
    {
        // on feature creation, initialize all our shaders and their respective data
        foreach (var shader in postShaders)
        {
            if (shader == null) { continue; }
            shader.Init();
        }
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (!IsMainCamera(renderingData.cameraData)) return;

        foreach (var shader in postShaders)
        {
            if (shader == null) { continue; }
            shader.GetPass().SetTarget(renderer.cameraColorTargetHandle);   // set our input target to the camera's color texture
        }
    }

    private bool IsMainCamera(CameraData cameraData)
    {
#if UNITY_EDITOR
        // Handle scene view camera
        if (cameraData.cameraType == CameraType.SceneView) return false;
#endif

        return cameraData.camera.CompareTag("MainCamera") ||
               cameraData.camera == Camera.main;
    }
}
