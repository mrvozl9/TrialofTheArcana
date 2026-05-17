using System.Collections;
using UnityEngine;

// Ses bileşeninin objede olmasını zorunlu kılar
[RequireComponent(typeof(AudioSource))]
public class VanishPlatform : MonoBehaviour
{
    [Header("Vanish Timing")]
    public float disappearTime = 1f;
    public float vanishDelay = 0.1f; // slight delay before disappearing

    [Header("Particle Effect")]
    [SerializeField] private GameObject flowerPetalBurstPrefab;
    [SerializeField] private Transform burstSpawnPoint;

    private Collider2D col;
    private SpriteRenderer rend;
    private AudioSource audioSource;

    private bool isVanished;

    void Awake()
    {
        col = GetComponent<Collider2D>();
        rend = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        // If no spawn point assigned, use flower position
        if (burstSpawnPoint == null)
            burstSpawnPoint = transform;
    }

    public void StartVanishSequence()
    {
        if (isVanished) return;

        // 🔊 Play break sound
        if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }

        StopAllCoroutines();
        StartCoroutine(VanishAndReappear());
    }

    IEnumerator VanishAndReappear()
    {
        isVanished = true;

        // Small delay so pogo bounce feels good
        yield return new WaitForSeconds(vanishDelay);

        // 🌸 Spawn petal orb burst
        if (flowerPetalBurstPrefab != null)
        {
            Instantiate(
                flowerPetalBurstPrefab,
                burstSpawnPoint.position,
                Quaternion.identity
            );
        }

        // Disable flower
        if (rend != null)
            rend.enabled = false;

        if (col != null)
            col.enabled = false;

        // Stay vanished
        yield return new WaitForSeconds(disappearTime);

        // Reappear
        if (rend != null)
            rend.enabled = true;

        if (col != null)
            col.enabled = true;

        isVanished = false;
    }
}
