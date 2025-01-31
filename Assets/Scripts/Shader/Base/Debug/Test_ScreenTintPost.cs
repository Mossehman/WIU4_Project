using UnityEngine;

[CreateAssetMenu(fileName = "Screen Tint effect", menuName = "Post Processing/Debug/Screen Tint")]
public class Test_ScreenTintPost : CustomPostProcessing
{
    public Color color = Color.white;

    public override void SendDataToShader(Material mat)
    {
        mat.SetColor("_Color", color);
    }
}
