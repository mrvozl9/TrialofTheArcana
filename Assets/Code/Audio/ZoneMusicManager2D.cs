using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(BoxCollider2D))]
public class ZoneMusicManager2D : MonoBehaviour
{
    [Header("Zone Settings")]
    [SerializeField] private string zoneName = "Unnamed Zone"; // Debug için alan adı

    [Header("Audio Settings")]
    [SerializeField] private AudioClip musicClip; // Her zona özel müzik klibi
    [SerializeField, Range(0f, 1f)] private float volume = 1f; // Hedef ses seviyesi

    [Header("Timing Settings")]
    [SerializeField] private float startDelay = 8f;   // Başlangıç gecikmesi
    [SerializeField] private float loopDelay = 8f;    // Loop'lar arası bekleme süresi
    [SerializeField] private float fadeInDuration = 3.0f; // Fade-in süresi
    [SerializeField] private float fadeOutDuration = 2.0f; // Fade-out süresi (zona çıkışta)

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private AudioSource audioSource;
    private BoxCollider2D zoneCollider;
    private bool playerInZone = false;
    private Coroutine musicCoroutine;
    private Coroutine fadeOutCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        zoneCollider = GetComponent<BoxCollider2D>();

        // AudioSource'u ayarla
        audioSource.clip = musicClip;
        audioSource.loop = false; // Manuel loop yapacağız
        audioSource.playOnAwake = false;
        audioSource.volume = 0f;

        // Trigger olarak ayarla
        zoneCollider.isTrigger = true;

        // Karakter bölgede başlıyorsa kontrol et
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null && zoneCollider.bounds.Contains(player.transform.position))
        {
            Debug.Log($"[{zoneName}] Karakter bölge içinde doğdu. Müzik başlatılıyor.");
            playerInZone = true;
            musicCoroutine = StartCoroutine(MusicLoopRoutine());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"[{zoneName}] Karakter bölgeye girdi.");
            playerInZone = true;

            // Fade-out varsa iptal et
            if (fadeOutCoroutine != null)
            {
                StopCoroutine(fadeOutCoroutine);
                fadeOutCoroutine = null;
            }

            // Müzik coroutine'i başlat
            if (musicCoroutine == null)
            {
                musicCoroutine = StartCoroutine(MusicLoopRoutine());
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"[{zoneName}] Karakter bölgeden çıktı.");
            playerInZone = false;

            // Müzik loop'unu durdur
            if (musicCoroutine != null)
            {
                StopCoroutine(musicCoroutine);
                musicCoroutine = null;
            }

            // Fade-out başlat
            if (fadeOutCoroutine == null && audioSource.isPlaying)
            {
                fadeOutCoroutine = StartCoroutine(FadeOutRoutine());
            }
            else
            {
                audioSource.Stop();
                audioSource.volume = 0f;
            }
        }
    }

    private IEnumerator MusicLoopRoutine()
    {
        // Başlangıç gecikmesi
        Debug.Log($"[{zoneName}] Müzik için {startDelay} saniye bekleniyor...");
        yield return new WaitForSeconds(startDelay);

        // Sürekli döngü
        while (playerInZone)
        {
            if (musicClip != null)
            {
                // Müziği çal ve fade-in yap
                audioSource.volume = 0f;
                audioSource.Play();
                Debug.Log($"[{zoneName}] Müzik başladı (Fade-In)");

                // Fade-in
                float timer = 0f;
                while (timer < fadeInDuration)
                {
                    if (!playerInZone) yield break;

                    timer += Time.deltaTime;
                    audioSource.volume = Mathf.Lerp(0f, volume, timer / fadeInDuration);
                    yield return null;
                }

                audioSource.volume = volume;

                // Müziğin bitmesini bekle
                while (audioSource.isPlaying)
                {
                    if (!playerInZone) yield break;
                    yield return null;
                }
            }

            // Loop gecikmesi
            Debug.Log($"[{zoneName}] Müzik bitti, {loopDelay} saniye bekleniyor...");
            yield return new WaitForSeconds(loopDelay);
        }

        audioSource.volume = 0f;
    }

    private IEnumerator FadeOutRoutine()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;

        Debug.Log($"[{zoneName}] Fade-out başladı");

        while (timer < fadeOutDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeOutDuration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = 0f;
        fadeOutCoroutine = null;

        Debug.Log($"[{zoneName}] Fade-out tamamlandı");
    }

    // Debug için Gizmos
    private void OnDrawGizmos()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.offset, col.size);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(col.offset, col.size);
        }
    }
}