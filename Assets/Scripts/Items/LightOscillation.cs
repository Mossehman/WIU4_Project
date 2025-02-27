using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace Assets.Scripts.Items
{
    [RequireComponent(typeof(Light))]
    public class LightOscillation : MonoBehaviour
    {
        [SerializeField] float frequency = 1f;
        [SerializeField] float amplitude = 1f;
        new Light light;
        float baseintensity = 1f;
        void Start()
        {
            light = GetComponent<Light>();
            if (light != null)
            {
                baseintensity = light.intensity;
            }
        }

        void Update()
        {
            if (light == null) return; 
            float oscillation = Mathf.Sin(Time.time * frequency) * amplitude;
            light.intensity = baseintensity + oscillation;
        }
    }
}