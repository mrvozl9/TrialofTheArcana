using UnityEngine;
using TMPro;          // For TextMeshProUGUI
using System.Collections; // For IEnumerator

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("UI Settings")]
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float fadeDuration = 0.5f;    // How fast it fades in/out
    [SerializeField] private float messageDuration = 2f;   // How long message stays visible

    private Coroutine messageRoutine;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (messageText != null)
        {
            Color c = messageText.color;
            c.a = 0f;
            messageText.color = c;
        }
    }

    /// <summary>
    /// Show a message on screen with fade effect.
    /// </summary>
    public void ShowMessage(string text)
    {
        if (messageRoutine != null)
            StopCoroutine(messageRoutine);

        messageRoutine = StartCoroutine(DisplayMessage(text));
    }

    private IEnumerator DisplayMessage(string text)
    {
        if (messageText == null)
            yield break;

        messageText.text = text;

        // Fade In
        yield return StartCoroutine(FadeTextAlpha(0f, 1f, fadeDuration));

        // Wait
        yield return new WaitForSeconds(messageDuration);

        // Fade Out
        yield return StartCoroutine(FadeTextAlpha(1f, 0f, fadeDuration));

        messageText.text = "";
    }

    private IEnumerator FadeTextAlpha(float start, float end, float duration)
    {
        float elapsed = 0f;
        Color color = messageText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            color.a = Mathf.Lerp(start, end, t);
            messageText.color = color;
            yield return null;
        }

        color.a = end;
        messageText.color = color;
    }
}
