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

            if (lowDetailMesh != null)
            {
                lowDetailMesh.SetActive(true);
            }
        }
        else
        {
            highDetailMesh.SetActive(true);
            if (lowDetailMesh != null)
            {
                lowDetailMesh.SetActive(false);
            }
        }
    }
}
