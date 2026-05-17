using UnityEngine;

using UnityEngine;

/// <summary>
/// Bouncy Mushroom that launches the player upward when stepped on,
/// plays sound, animation, and optional particle effects.
/// </summary>
public class BouncyMushroom : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float bounceForce = 25f;

    [Header("Feedback")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem impactParticles; // Optional: spores/dust

    private void Awake()
    {
        // Auto-grab components if not assigned manually
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (animator == null) animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Called by PlayerController when the player lands on the mushroom.
    /// </summary>
    public float GetBounceForce()
    {
        // 1. Play animation
        if (animator != null)
        {
            // Make sure you have a Trigger parameter named "Bounce" in your Animator
            animator.SetTrigger("Bounce");
        }

        // 2. Play sound
        if (audioSource != null)
        {
            audioSource.Play();
        }

        // 3. Play particle effects (optional)
        if (impactParticles != null)
        {
            impactParticles.Play();
        }

        // 4. Return force to player
        return bounceForce;
    }
}
