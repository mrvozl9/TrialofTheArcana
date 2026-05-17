using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Adds size, light, color, and pulse variation to butterflies.
/// Handles light dimming when perched.
/// </summary>
public class ButterflyVariation : MonoBehaviour
{
    [Header("Size Variation")]
    [SerializeField] private Vector2 scaleRange = new Vector2(0.7f, 1.2f);

    [Header("Light Base Settings")]
    [SerializeField] private Vector2 intensityRange = new Vector2(0.4f, 1.2f);
    [SerializeField] private Vector2 radiusRange = new Vector2(0.8f, 2f);

    [Header("Light Pulse")]
    [SerializeField] private float pulseSpeedMin = 1.2f;
    [SerializeField] private float pulseSpeedMax = 3f;
    [SerializeField] private float pulseAmount = 0.25f;

    [Header("Perched Light")]
    [SerializeField] private float perchedIntensityMultiplier = 0.5f;

    [Header("Light Color Range")]
    [SerializeField] private Color colorA = new Color(0.6f, 0.8f, 1f);
    [SerializeField] private Color colorB = new Color(1f, 0.6f, 0.9f);

    private Light2D light2D;
    private float baseIntensity;
    private float pulseSpeed;
    private float pulseOffset;
    private bool isPerched;

    private void Awake()
    {
        ApplySize();
        SetupLight();
    }

    private void Update()
    {
        PulseLight();
    }

    private void ApplySize()
    {
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        transform.localScale = Vector3.one * scale;
    }

    private void SetupLight()
    {
        light2D = GetComponentInChildren<Light2D>();
        if (light2D == null) return;

        baseIntensity = Random.Range(intensityRange.x, intensityRange.y);
        light2D.intensity = baseIntensity;
        light2D.pointLightOuterRadius = Random.Range(radiusRange.x, radiusRange.y);
        light2D.color = Color.Lerp(colorA, colorB, Random.value);

        pulseSpeed = Random.Range(pulseSpeedMin, pulseSpeedMax);
        pulseOffset = Random.Range(0f, 100f);
    }

    private void PulseLight()
    {
        if (light2D == null) return;

        float targetBase = isPerched
            ? baseIntensity * perchedIntensityMultiplier
            : baseIntensity;

        float pulse =
            Mathf.Sin((Time.time + pulseOffset) * pulseSpeed) * pulseAmount;

        light2D.intensity = targetBase + pulse;
    }

    // 🔔 Called by ButterflyFollower
    public void SetPerched(bool perched)
    {
        isPerched = perched;
    }
}
