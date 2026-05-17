using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LavaLightFlicker : MonoBehaviour
{
    [Header("Light")]
    [SerializeField] private Light2D light2D;

    [Header("Flicker Settings")]
    [SerializeField] private float baseIntensity = 1.2f;
    [SerializeField] private float flickerAmount = 0.4f;
    [SerializeField] private float flickerSpeed = 3f;

    [Header("Radius Flicker")]
    [SerializeField] private float baseRadius = 5f;
    [SerializeField] private float radiusFlicker = 0.5f;

    private float noiseOffset;

    private void Awake()
    {
        if (light2D == null)
            light2D = GetComponent<Light2D>();

        noiseOffset = Random.value * 100f;
    }

    private void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, noiseOffset);

        light2D.intensity = baseIntensity + (noise - 0.5f) * flickerAmount;
        light2D.pointLightOuterRadius = baseRadius + (noise - 0.5f) * radiusFlicker;
    }
}
