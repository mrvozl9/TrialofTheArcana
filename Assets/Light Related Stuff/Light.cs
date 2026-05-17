using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NoiseLightFlicker : MonoBehaviour
{
    public Light2D light2D;
    public float intensity = 1f;
    public float noiseStrength = 0.3f;
    public float noiseSpeed = 1f;

    void Start()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * noiseSpeed, 0f);
        light2D.intensity = intensity + (noise - 0.5f) * noiseStrength;
    }
}
