using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifeTime = 2f;
    public int damage = 1;

    [Header("Audio")]
    public AudioClip impactSound;

    [Header("Visual Effects (Optional)")]
    public GameObject impactEffect;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Try to damage enemy
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            OnBulletHit(collision.transform.position);
            return;
        }

        // Check if hit ground
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            OnBulletHit(collision.transform.position);
        }
    }

    /// <summary>
    /// Handle bullet impact (audio + effects)
    /// </summary>
    private void OnBulletHit(Vector3 hitPosition)
    {
        // ✅ PLAY IMPACT SOUND
        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, hitPosition);
        }

        // ✅ SPAWN IMPACT EFFECT
        if (impactEffect != null)
        {
            GameObject effect = Instantiate(impactEffect, hitPosition, Quaternion.identity);
            Destroy(effect, 1f); // Auto-destroy effect after 1 second
        }

        // Destroy bullet
        Destroy(gameObject);
    }
}