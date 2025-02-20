using UnityEngine;

[CreateAssetMenu(fileName = "Distance Fog effect", menuName = "Post Processing/Distance Fog")]
public class DistanceFog_Post : CustomPostProcessing
{
    [Range(0.0f, 1.0f)]
    public float skyboxFactor = 0.4f    ;

    [Range(0.0f, 1.0f)]
    public float fogStartingDepth;
    [Range(0.0f, 1.0f)]
    public float fogEndDepth;

    public Color fogColor;

    public override void SendDataToShader(Material mat)
    {
        mat.SetFloat("_fogMinDepth", fogStartingDepth);
        mat.SetFloat("_fogMaxDepth", fogEndDepth);

        mat.SetColor("_fogColor", fogColor);
        mat.SetFloat("_skyboxBlend", skyboxFactor);
    }
}
