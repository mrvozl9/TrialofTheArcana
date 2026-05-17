using UnityEngine;

public class CupCollectible : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip cupClip;
    [Range(0f, 1f)]
    public float collectionVolume = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        MagicianNPC.hasCup = true;

        // Play sound
        if (cupClip != null)
            AudioSource.PlayClipAtPoint(cupClip, transform.position, collectionVolume);

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Collected the Cup!");

        Debug.Log("Cup collected by player.");

        Destroy(gameObject);
    }
}
