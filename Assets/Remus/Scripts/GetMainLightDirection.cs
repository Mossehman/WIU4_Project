using UnityEngine;

[ExecuteInEditMode]
public class GetMainLightDirection : MonoBehaviour
{
    [SerializeField] private Material skyboxMaterial;

    private void Update()
    {
        skyboxMaterial.SetVector("_MainLightDirection", transform.forward);
    }
}