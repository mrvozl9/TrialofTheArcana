using UnityEngine;

public class Shooting2D : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;

    private float lastDirection = 1f;  // 1 = right, -1 = left
    private PlayerAudioController audioController;

    private void Awake()
    {
        // Get the PlayerAudioController component
        audioController = GetComponent<PlayerAudioController>();

        if (audioController == null)
        {
            Debug.LogWarning("PlayerAudioController not found! Shooting sounds won't play.");
        }
    }

    void Update()
    {
        // Track player facing direction using horizontal input
        float moveInput = Input.GetAxisRaw("Horizontal");
        if (moveInput != 0)
            lastDirection = moveInput;

        // Shoot when pressing X
        if (Input.GetKeyDown(KeyCode.X))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Bullet prefab or firePoint not assigned!");
            return;
        }

        // ✅ PLAY ATTACK SOUND
        if (audioController != null)
        {
            audioController.PlayAttackSound();
        }

        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            // SHOOT UP
            if (Input.GetKey(KeyCode.UpArrow))
            {
                rb.velocity = new Vector2(0, bulletSpeed);
            }
            // SHOOT DOWN
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                rb.velocity = new Vector2(0, -bulletSpeed);
            }
            // LEFT / RIGHT (default)
            else
            {
                rb.velocity = new Vector2(lastDirection * bulletSpeed, 0);
            }
        }
    }
}