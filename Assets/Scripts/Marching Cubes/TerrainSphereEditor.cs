using UnityEngine;

/// <summary>
/// Helper script to edit and fine tune the marching cubes mesh via placing gameObjects in the scene with this component added
/// </summary>
public class TerrainSphereEditor : MonoBehaviour
{
    public float radius = 1.0f;
    public float weightModifier = -1.0f;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
