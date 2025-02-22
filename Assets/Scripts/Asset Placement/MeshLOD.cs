using UnityEngine;

public class MeshLOD : MonoBehaviour
{
    public GameObject highDetailMesh;
    public GameObject lowDetailMesh;

    public float distanceThreshold = 50.0f;

    private void Update()
    {
        float distToCamera = Vector3.SqrMagnitude(transform.position - Camera.main.transform.position);
        if (distToCamera > distanceThreshold * distanceThreshold)
        {
            highDetailMesh.SetActive(false);
            lowDetailMesh.SetActive(true);
        }
        else
        {
            highDetailMesh.SetActive(true);
            lowDetailMesh.SetActive(false);
        }
    }
}
