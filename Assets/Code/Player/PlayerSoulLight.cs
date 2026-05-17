using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerSoulLight : MonoBehaviour
{
    [Header("Base Light")]
    [SerializeField] private float baseIntensity = 0.6f;
    [SerializeField] private float pulseAmount = 0.1f;
    [SerializeField] private float pulseSpeed = 1.2f;

    [Header("Butterfly Influence")]
    [SerializeField] private float intensityPerButterfly = 0.15f;
    [SerializeField] private float maxExtraIntensity = 0.6f;

    private Light2D light2D;
    private int perchedButterflies;
    private float pulseOffset;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
        pulseOffset = Random.Range(0f, 100f);
    }

    private void Update()
    {
        float pulse =
            Mathf.Sin((Time.time + pulseOffset) * pulseSpeed) * pulseAmount;

        float extra =
            Mathf.Min(perchedButterflies * intensityPerButterfly, maxExtraIntensity);

        light2D.intensity = baseIntensity + pulse + extra;
    }

    // 🦋 Called by butterflies
    public void OnButterflyPerched()
    {
        perchedButterflies++;
    }

    public void OnButterflyLeft()
    {
        perchedButterflies = Mathf.Max(0, perchedButterflies - 1);
    }
}
