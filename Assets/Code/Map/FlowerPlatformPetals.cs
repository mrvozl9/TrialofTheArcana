using UnityEngine;

/// <summary>
/// Spawns a petal burst when the player touches the flower platform.
/// </summary>
public class FlowerPlatformPetals : MonoBehaviour
{
    [Header("Petal Effect")]
    [SerializeField] private ParticleSystem petalBurstPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 1.5f;

    private bool canTrigger = true;

    private void Awake()
    {
        if (spawnPoint == null)
            spawnPoint = transform;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!canTrigger) return;

        if (collision.collider.CompareTag("Player"))
        {
            SpawnPetals();
        }
    }

    private void SpawnPetals()
    {
        canTrigger = false;

        ParticleSystem petals =
            Instantiate(petalBurstPrefab, spawnPoint.position, Quaternion.identity);

        petals.Play();
        Destroy(petals.gameObject, 3f);

        Invoke(nameof(ResetTrigger), cooldown);
    }

    private void ResetTrigger()
    {
        canTrigger = true;
    }
}
