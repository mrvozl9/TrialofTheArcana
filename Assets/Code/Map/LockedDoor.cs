using UnityEngine;
using System.Collections;

public class LockedDoor : MonoBehaviour
{
    [Header("Door Settings")]
    public bool isLocked = true;
    public GameObject doorVisual;

    [Header("Audio")]
    public AudioClip doorOpenSoundClip;

    private AudioSource audioSource;
    private Collider2D doorCollider;

    void Awake()
    {
        // Cache collider
        doorCollider = GetComponent<Collider2D>();

        // Try to find AudioSource on visual first
        if (doorVisual != null)
            audioSource = doorVisual.GetComponent<AudioSource>();

        // If not found, check on the same object
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // If still not found, error
        if (audioSource == null)
        {
            Debug.LogError("LockedDoor: No AudioSource found on door or doorVisual!");
            return;
        }

        // AudioSource settings
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        PlayerController player = collision.gameObject.GetComponent<PlayerController>();

        if (player == null) return;

        if (player.hasKey)
        {
            isLocked = false;
            player.hasKey = false;

            // Play sound
            if (doorOpenSoundClip != null)
            {
                audioSource.PlayOneShot(doorOpenSoundClip);
            }
            else
            {
                Debug.LogWarning("Door opened, but no sound assigned!");
            }

            // Show UI
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("Door unlocked!");

            Debug.Log("Door unlocked!");

            // Disable collider immediately so player can walk through
            if (doorCollider != null)
                doorCollider.enabled = false;

            // Remove visual after short delay
            StartCoroutine(CloseDoorAfterSound());
        }
        else
        {
            if (UIManager.Instance != null)
                UIManager.Instance.ShowMessage("You need a key to open this door!");

            Debug.Log("You need a key to open this door.");
        }
    }

    private IEnumerator CloseDoorAfterSound()
    {
        // Small delay to let sound play
        yield return new WaitForSeconds(0.3f);

        // Hide door visual
        if (doorVisual != null)
            doorVisual.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}
