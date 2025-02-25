using UnityEngine;

[CreateAssetMenu(fileName = "Outline Shader", menuName = "Post Processing/Outline Shader")]
public class OutlineShader_Post : CustomPostProcessing
{
    public Color outlineColor;
    public float outlineScale;

    public Texture normalBuffer;

    [Header("Depth sampling")]
    public float depthThreshold = 1.5f;
    public float depthNormalThreshold = 0.5f;
    public float depthNormalThresholdScale = 7.0f;

    [Header("Normal sampling")]
    public float normalThreshold = 0.4f;

    public override void SendDataToShader(Material mat)
    {
        mat.SetColor("_outlineColor", outlineColor);

        mat.SetTexture("_normalBuffer", normalBuffer);
        mat.SetFloat("_scale", outlineScale);
        mat.SetFloat("_depthThreshold", depthThreshold);

        mat.SetFloat("_normalThreshold", normalThreshold);

        Matrix4x4 clipToView = Camera.main.projectionMatrix.inverse;
        mat.SetMatrix("_clipToView", clipToView);

        mat.SetFloat("_depthNormalThreshold", depthNormalThreshold);
        mat.SetFloat("_depthNormalThresholdScale", depthNormalThresholdScale);
    }
}
