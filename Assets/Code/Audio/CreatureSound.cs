using UnityEngine;
using System.Collections;

public class CreatureSoundRange : MonoBehaviour
{
    public Transform player;
    public float soundRange = 5f;      // Ses mesafesi (artırılabilir, collider ile alakası yok)
    public float fadeTime = 1f;

    private AudioSource audioSource;
    private Coroutine currentFade;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0;
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= soundRange)
            FadeTo(1f);    // Ses aç
        else
            FadeTo(0f);    // Ses kapat
    }

    void FadeTo(float targetVolume)
    {
        if (currentFade != null)
            StopCoroutine(currentFade);

        currentFade = StartCoroutine(FadeCoroutine(targetVolume));
    }

    IEnumerator FadeCoroutine(float target)
    {
        float start = audioSource.volume;
        float t = 0;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(start, target, t / fadeTime);
            yield return null;
        }
    }
}
