using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Particle Effects")]
    public GameObject leafParticlePrefab; // Assign your leaf particle prefab here
    public Transform particleSpawnPoint; // Optional: specific spawn point for particles

    void Start()
    {
        currentHealth = maxHealth;
    }

    // Call this when the enemy takes damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Spawn leaf particles when damaged
        SpawnLeafParticles();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void SpawnLeafParticles()
    {
        if (leafParticlePrefab != null)
        {
            // Determine spawn position
            Vector3 spawnPosition = particleSpawnPoint != null
                ? particleSpawnPoint.position
                : transform.position;

            // Instantiate the particle effect
            GameObject particles = Instantiate(leafParticlePrefab, spawnPosition, Quaternion.identity);

            // Destroy the particle system after it finishes playing
            ParticleSystem ps = particles.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                Destroy(particles, ps.main.duration + ps.main.startLifetime.constantMax);
            }
            else
            {
                // Fallback if no ParticleSystem component found
                Destroy(particles, 2f);
            }
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}