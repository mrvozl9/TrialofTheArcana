using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class ParticleZoneVisibility : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private string zoneName = "Default Zone";

    [Header("Particle Systems")]
    [Tooltip("The particle system parent GameObject to SHOW in this zone")]
    [SerializeField] private GameObject particleSetToShow;

    [Tooltip("Other particle system GameObjects to HIDE when in this zone")]
    [SerializeField] private GameObject[] particleSetsToHide;

    [Header("Transition Settings")]
    [SerializeField] private float crossfadeDuration = 1f;
    [SerializeField] private bool useInstantSwitch = false;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    [Header("Debug")]
    [SerializeField] private Color gizmoColor = Color.yellow;
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
            ShowZoneParticlesImmediate();
        }
        else
        {
            // Start with this zone's particles hidden
            if (particleSetToShow != null)
            {
                StopAllParticlesInSet(particleSetToShow);
                SetParticleAndAudioAlpha(particleSetToShow, 0f);
                particleSetToShow.SetActive(false);
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
                ShowZoneParticlesImmediate();
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
        }
    }

    private void ShowZoneParticlesImmediate()
    {
        // Show and play this zone's particles
        if (particleSetToShow != null)
        {
            particleSetToShow.SetActive(true);
            PlayAllParticlesInSet(particleSetToShow);
            SetParticleAndAudioAlpha(particleSetToShow, 1f);
        }

        // Hide and stop other particle sets
        foreach (var particleSet in particleSetsToHide)
        {
            if (particleSet != null)
            {
                StopAllParticlesInSet(particleSet);
                SetParticleAndAudioAlpha(particleSet, 0f);
                particleSet.SetActive(false);
            }
        }
    }

    private IEnumerator CrossfadeToThisZone()
    {
        // Activate new particle set FIRST
        if (particleSetToShow != null)
        {
            particleSetToShow.SetActive(true);
            PlayAllParticlesInSet(particleSetToShow);
            SetParticleAndAudioAlpha(particleSetToShow, 0f); // Start invisible and silent
        }

        float elapsed = 0f;

        // Crossfade: fade OUT old particles/audio while fading IN new ones SIMULTANEOUSLY
        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / crossfadeDuration);

            // Fade IN this zone's particles and audio
            if (particleSetToShow != null)
            {
                SetParticleAndAudioAlpha(particleSetToShow, t);
            }

            // Fade OUT other particle sets and audio SIMULTANEOUSLY
            foreach (var particleSet in particleSetsToHide)
            {
                if (particleSet != null && particleSet.activeSelf)
                {
                    SetParticleAndAudioAlpha(particleSet, 1f - t);
                }
            }

            yield return null;
        }

        // Ensure final states
        if (particleSetToShow != null)
        {
            SetParticleAndAudioAlpha(particleSetToShow, 1f);
        }

        foreach (var particleSet in particleSetsToHide)
        {
            if (particleSet != null)
            {
                StopAllParticlesInSet(particleSet);
                SetParticleAndAudioAlpha(particleSet, 0f);
                particleSet.SetActive(false);
            }
        }

        transitionCoroutine = null;
    }

    private void SetParticleAndAudioAlpha(GameObject particleSet, float alpha)
    {
        if (particleSet == null) return;

        // Handle Particle Systems
        ParticleSystem[] particleSystems = particleSet.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            if (ps == null) continue;

            var main = ps.main;
            Color color = main.startColor.color;
            color.a = alpha;
            main.startColor = color;

            // Update existing particles
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
            int count = ps.GetParticles(particles);
            for (int i = 0; i < count; i++)
            {
                Color particleColor = particles[i].startColor;
                particleColor.a = alpha;
                particles[i].startColor = particleColor;
            }
            ps.SetParticles(particles, count);
        }

        // Handle Audio Sources
        AudioSource[] audioSources = particleSet.GetComponentsInChildren<AudioSource>(true);
        foreach (var audioSource in audioSources)
        {
            if (audioSource == null) continue;

            // Store original volume in a way that persists
            if (!audioSource.gameObject.TryGetComponent<ParticleAudioVolumeTracker>(out var tracker))
            {
                tracker = audioSource.gameObject.AddComponent<ParticleAudioVolumeTracker>();
                tracker.originalVolume = audioSource.volume;
            }

            audioSource.volume = tracker.originalVolume * alpha;

            // Start playing if fading in and not already playing
            if (alpha > 0.01f && !audioSource.isPlaying && audioSource.enabled)
            {
                audioSource.Play();
            }
            // Stop if fully faded out
            else if (alpha <= 0.01f && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void PlayAllParticlesInSet(GameObject particleSet)
    {
        if (particleSet == null) return;

        ParticleSystem[] particleSystems = particleSet.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            if (ps != null && !ps.isPlaying)
            {
                ps.Play();
            }
        }

        // Also start audio sources
        AudioSource[] audioSources = particleSet.GetComponentsInChildren<AudioSource>(true);
        foreach (var audioSource in audioSources)
        {
            if (audioSource != null && audioSource.enabled && !audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    private void StopAllParticlesInSet(GameObject particleSet)
    {
        if (particleSet == null) return;

        ParticleSystem[] particleSystems = particleSet.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }

        // Also stop audio sources
        AudioSource[] audioSources = particleSet.GetComponentsInChildren<AudioSource>(true);
        foreach (var audioSource in audioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
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

// Helper component to track original audio volume
public class ParticleAudioVolumeTracker : MonoBehaviour
{
    public float originalVolume = 1f;
}