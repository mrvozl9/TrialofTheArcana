using UnityEngine;

/// <summary>
/// Spawns and manages multiple butterflies around the player.
/// </summary>
public class ButterflyManager : MonoBehaviour
{
    [Header("Butterfly Settings")]
    [SerializeField] private GameObject butterflyPrefab;
    [SerializeField] private int butterflyCount = 3;
    [SerializeField] private Transform player;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 2f;
    [SerializeField] private bool spawnOnStart = true;

    private GameObject[] butterflies;

    private void Start()
    {
        if (spawnOnStart)
        {
            SpawnButterflies();
        }
    }

    public void SpawnButterflies()
    {
        if (butterflyPrefab == null)
        {
            Debug.LogError("Butterfly prefab is not assigned!");
            return;
        }

        // Find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        butterflies = new GameObject[butterflyCount];

        for (int i = 0; i < butterflyCount; i++)
        {
            // Spawn at random positions around player
            float angle = (360f / butterflyCount) * i;
            Vector3 spawnPos = player.position +
                new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius,
                    Random.Range(0.5f, 2f),
                    0
                );

            GameObject butterfly = Instantiate(butterflyPrefab, spawnPos, Quaternion.identity);
            butterfly.transform.SetParent(transform);
            butterfly.name = $"Butterfly_{i}";

            // Configure the follower with unique settings
            ButterflyFollower follower = butterfly.GetComponent<ButterflyFollower>();
            if (follower != null)
            {
                // Add slight variation to each butterfly
                follower.enabled = true;
            }

            butterflies[i] = butterfly;
        }

        Debug.Log($"Spawned {butterflyCount} butterflies around the player!");
    }

    public void ClearButterflies()
    {
        if (butterflies == null) return;

        foreach (GameObject butterfly in butterflies)
        {
            if (butterfly != null)
            {
                Destroy(butterfly);
            }
        }

        butterflies = null;
    }

    private void OnDestroy()
    {
        ClearButterflies();
    }
}