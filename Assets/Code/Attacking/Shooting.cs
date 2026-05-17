using UnityEngine;

public class Shooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float shootInterval = 1.5f;
    public float bulletSpeed = 5f;

    [Header("Audio")]
    public AudioClip shootSound;

    private Transform player;
    private AudioSource audioSource;

    private void Awake()
    {
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        if (player == null)
        {
            Debug.LogError($"{gameObject.name}: Player not found!");
        }

        InvokeRepeating(nameof(Shoot), shootInterval, shootInterval);
    }

    void Shoot()
    {
        if (player == null) return;

        // ✅ PLAY SHOOT SOUND
        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }

        // Spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);

        // Calculate direction
        Vector2 direction = (player.position - firePoint.position).normalized;

        // Apply velocity
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = direction * bulletSpeed;
        }
    }
}