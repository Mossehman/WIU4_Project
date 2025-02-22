using UnityEngine;

public class PlanetRotator : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0, 0, 0);

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}