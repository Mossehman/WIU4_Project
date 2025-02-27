using UnityEngine;

public class ScannableObject : MonoBehaviour
{
    public string displayName;
    [TextArea]
    public string description;
    public float yOffset = 10.0f;
    public Sprite sprite;
}
