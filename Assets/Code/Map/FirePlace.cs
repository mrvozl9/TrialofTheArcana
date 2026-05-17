using UnityEngine;
using System.Collections;

public class FirePlace : MonoBehaviour
{
    public enum Type { BreakablePlatform, Star, Statue }
    public Type objectType;

    [Header("Platform Settings")]
    public float breakDelay = 0.5f;
    public float destroyDelay = 2f;

    [Header("Statue Settings")]
    public int starsRequired = 2;
    public Transform teleportTarget;

    [Header("Audio Settings")]
    [Tooltip("Yıldız tipi için AudioSource. Yıldız toplandığında ses çalar.")]
    public AudioSource starAudioSource;

    [Tooltip("Platform breaking sound.")]
    public AudioClip platformBreakClip;
    public float platformBreakVolume = 1f;

    [Tooltip("Statue hint sound when player is near.")]
    public AudioClip statueHintClip;
    [Tooltip("Statue success sound.")]
    public AudioClip statueSuccessClip;
    [Tooltip("Statue failure sound.")]
    public AudioClip statueFailClip;
    [Tooltip("Volume for statue sounds.")]
    public float statueVolume = 1f;

    [Tooltip("Sound played when player teleports.")]
    public AudioClip teleportClip;
    [Tooltip("Volume for teleport sound.")]
    public float teleportVolume = 1f;

    private bool hasBroken = false;
    private bool playerNear = false;
    private bool hintPlayed = false; // Prevent spamming hint sound
    private Rigidbody2D rb;
    private Collider2D col;
    private PlayerController player;
    private SpriteRenderer sr;
    private bool isCollected = false;

    public static int playerStars = 0;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();

        if (objectType == Type.BreakablePlatform && rb != null)
            rb.isKinematic = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (objectType == Type.BreakablePlatform && !hasBroken && collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(BreakPlatform());
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (isCollected) return;

        player = collision.GetComponent<PlayerController>();

        switch (objectType)
        {
            case Type.Star:
                if (player != null)
                {
                    isCollected = true;
                    playerStars++;

                    if (sr != null) sr.enabled = false;
                    if (col != null) col.enabled = false;

                    if (starAudioSource != null && starAudioSource.clip != null)
                        AudioSource.PlayClipAtPoint(starAudioSource.clip, transform.position, starAudioSource.volume);

                    if (UIManager.Instance != null)
                        UIManager.Instance.ShowMessage("Collected a star! Total: " + playerStars);
                    Debug.Log("Collected star! Total: " + playerStars);

                    Destroy(gameObject);
                }
                break;

            case Type.Statue:
                playerNear = true;

                // Play hint sound only once per approach
                if (!hintPlayed && statueHintClip != null)
                {
                    AudioSource.PlayClipAtPoint(statueHintClip, transform.position, statueVolume);
                    hintPlayed = true;
                }

                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Press F to offer stars to statue.");
                Debug.Log("Press F to offer stars to statue.");
                break;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (objectType == Type.Statue && collision.CompareTag("Player"))
        {
            playerNear = false;
            hintPlayed = false; // Reset hint for next approach
        }
    }

    private void Update()
    {
        if (objectType == Type.Statue && playerNear && Input.GetKeyDown(KeyCode.F))
        {
            if (playerStars >= starsRequired)
            {
                playerStars -= starsRequired;
                player.transform.position = teleportTarget.position;

                // Play statue success sound
                if (statueSuccessClip != null)
                    AudioSource.PlayClipAtPoint(statueSuccessClip, transform.position, statueVolume);

                // Play teleport sound
                if (teleportClip != null)
                    AudioSource.PlayClipAtPoint(teleportClip, player.transform.position, teleportVolume);

                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("Teleported successfully!");
                Debug.Log("Teleported!");
            }
            else
            {
                // Play statue failure sound
                if (statueFailClip != null)
                    AudioSource.PlayClipAtPoint(statueFailClip, transform.position, statueVolume);

                if (UIManager.Instance != null)
                    UIManager.Instance.ShowMessage("You need " + starsRequired + " stars!");
                Debug.Log("You need " + starsRequired + " stars!");
            }
        }
    }

    IEnumerator BreakPlatform()
    {
        hasBroken = true;

        if (UIManager.Instance != null)
            UIManager.Instance.ShowMessage("Platform is breaking!");
        Debug.Log("Platform is breaking!");

        yield return new WaitForSeconds(breakDelay);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.gravityScale = 2f;
        }

        if (col != null)
            col.enabled = false;

        // Play platform break sound
        if (platformBreakClip != null)
            AudioSource.PlayClipAtPoint(platformBreakClip, transform.position, platformBreakVolume);

        Debug.Log("Platform destroyed!");

        Destroy(gameObject, destroyDelay);
    }
}
