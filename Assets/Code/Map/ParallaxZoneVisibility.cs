using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class ParallaxZoneVisibility : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private string zoneName = "Default Zone";

    [Header("Parallax Sets")]
    [Tooltip("The parallax parent GameObject to SHOW in this zone")]
    [SerializeField] private GameObject parallaxSetToShow;

    [Tooltip("Other parallax parent GameObjects to HIDE when in this zone")]
    [SerializeField] private GameObject[] parallaxSetsToHide;

    [Header("Transition Settings")]
    [SerializeField] private float crossfadeDuration = 0.5f;
    [SerializeField] private bool useInstantSwitch = false;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private Color gizmoColor = Color.cyan;
    [SerializeField] private bool showDebugLogs = false;

    private BoxCollider2D zoneCollider;
    private bool playerIsInZone = false;
    private Coroutine transitionCoroutine;

    void Start()
    {
        zoneCollider = GetComponent<BoxCollider2D>();
        zoneCollider.isTrigger = true;

        // Check if player starts in this zone
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null && zoneCollider.bounds.Contains(player.transform.position))
        {
            if (showDebugLogs) Debug.Log($"[{zoneName}] Player started in zone");
            playerIsInZone = true;
            ShowZoneParallaxImmediate();
        }
        else
        {
            // Start with this zone's parallax hidden
            if (parallaxSetToShow != null)
            {
                SetParallaxAlpha(parallaxSetToShow, 0f);
                parallaxSetToShow.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !playerIsInZone)
        {
            if (showDebugLogs) Debug.Log($"[{zoneName}] Player entered zone");
            playerIsInZone = true;

            // Stop any ongoing transition
            if (transitionCoroutine != null)
            {
                StopCoroutine(transitionCoroutine);
            }

            if (useInstantSwitch)
            {
                ShowZoneParallaxImmediate();
            }
            else
            {
                transitionCoroutine = StartCoroutine(CrossfadeToThisZone());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && playerIsInZone)
        {
            if (showDebugLogs) Debug.Log($"[{zoneName}] Player exited zone");
            playerIsInZone = false;

            // Don't immediately hide - let the next zone handle the transition
            // This zone's parallax will be faded out by the next zone's OnTriggerEnter
        }
    }

    private void ShowZoneParallaxImmediate()
    {
        // Show this zone's parallax immediately
        if (parallaxSetToShow != null)
        {
            parallaxSetToShow.SetActive(true);
            SetParallaxAlpha(parallaxSetToShow, 1f);
        }

        // Hide other parallax sets immediately
        foreach (var parallaxSet in parallaxSetsToHide)
        {
            if (parallaxSet != null)
            {
                SetParallaxAlpha(parallaxSet, 0f);
                parallaxSet.SetActive(false);
            }
        }
    }

    private IEnumerator CrossfadeToThisZone()
    {
        // CRITICAL: Activate new parallax FIRST before starting fade
        if (parallaxSetToShow != null)
        {
            parallaxSetToShow.SetActive(true);
            SetParallaxAlpha(parallaxSetToShow, 0f); // Start invisible
        }

        float elapsed = 0f;

        // Crossfade: fade OUT old parallax while fading IN new parallax SIMULTANEOUSLY
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);

            // Fade IN this zone's parallax
            if (parallaxSetToShow != null)
            {
                SetParallaxAlpha(parallaxSetToShow, t);
            }

            // Fade OUT other parallax sets SIMULTANEOUSLY
            foreach (var parallaxSet in parallaxSetsToHide)
            {
                if (parallaxSet != null && parallaxSet.activeSelf)
                {
                    SetParallaxAlpha(parallaxSet, 1f - t);
                }
            }

            yield return null;
        }

        // Ensure final states
        if (parallaxSetToShow != null)
        {
            SetParallaxAlpha(parallaxSetToShow, 1f);
        }

        foreach (var parallaxSet in parallaxSetsToHide)
        {
            if (parallaxSet != null)
            {
                SetParallaxAlpha(parallaxSet, 0f);
                parallaxSet.SetActive(false);
            }
        }

        transitionCoroutine = null;
    }

    private void SetParallaxAlpha(GameObject parallaxSet, float alpha)
    {
        if (parallaxSet == null) return;

        // Get all SpriteRenderers in this parallax set (including children)
        SpriteRenderer[] spriteRenderers = parallaxSet.GetComponentsInChildren<SpriteRenderer>(true);

        foreach (var sr in spriteRenderers)
        {
            Color color = sr.color;
            color.a = alpha;
            sr.color = color;
        }
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.offset, col.size);

            Gizmos.color = gizmoColor;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
}