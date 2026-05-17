using UnityEngine;
using System.Collections;

public class RevealArea : MonoBehaviour
{
    public float fadeSpeed = 1.5f;

    private SpriteRenderer sr;
    private bool fading = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (fading) return;

        if (other.CompareTag("Player"))
        {
            fading = true;
            StartCoroutine(FadeOut());
        }
    }

    IEnumerator FadeOut()
    {
        Color c = sr.color;
        while (c.a > 0f)
        {
            c.a -= fadeSpeed * Time.deltaTime;
            sr.color = c;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}