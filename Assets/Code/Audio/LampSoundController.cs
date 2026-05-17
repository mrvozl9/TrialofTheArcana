using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Collider2D))] // Hem AudioSource hem Collider zorunlu
public class LampSoundController : MonoBehaviour
{
    private AudioSource audioSource;
    public string playerTag = "Player"; // Oyuncunun Tag'i
    public float fadeDuration = 1.0f; // Sesin kaybolma süresi

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // Kodla yöneteceðimiz için baþlangýçta çalmasýn
        audioSource.volume = 0f; // Baþlangýçta ses kapalý olsun
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Alana girince sesi baþlat ve yavaþça aç
            if (audioSource != null && audioSource.clip != null && !audioSource.isPlaying)
            {
                audioSource.Play();
                StopAllCoroutines(); // Önceki fade iþlemlerini durdur
                StartCoroutine(FadeIn(1.0f)); // Sesi max seviyeye aç
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            // Alandan çýkýnca sesi yavaþça kapat ve durdur
            if (audioSource != null && audioSource.isPlaying)
            {
                StopAllCoroutines(); // Önceki fade iþlemlerini durdur
                StartCoroutine(FadeOutAndStop()); // Sesi yavaþça kapat
            }
        }
    }

    // Sesi belirlenen sürede yavaþça açar
    private IEnumerator FadeIn(float targetVolume)
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }

    // Sesi belirlenen sürede yavaþça kapatýr ve durdurur
    private IEnumerator FadeOutAndStop()
    {
        float startVolume = audioSource.volume;
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }
        audioSource.volume = 0f;
        audioSource.Stop();
    }
}