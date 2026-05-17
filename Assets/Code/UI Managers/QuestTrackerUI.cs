using UnityEngine;
using TMPro;
using System.Collections;

public class QuestTrackerUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text questLineText;
    [SerializeField] private RectTransform questIcon;


    [Header("Animation")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private float pulseScale = 1.08f;
    [SerializeField] private float pulseDuration = 0.12f;

    [Header("Style")]
    [SerializeField] private string prefix = "";

    private Coroutine animCo;

    private void Awake()
    {
        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null && panelRoot != null)
            canvasGroup = panelRoot.AddComponent<CanvasGroup>();
    }

    public void SetQuestText(string text, bool animate = true)
    {
        if (questLineText == null) return;

        string finalText = string.IsNullOrEmpty(prefix) ? text : (prefix + text);

        if (!animate)
        {
            questLineText.text = finalText;
            return;
        }

        if (animCo != null) StopCoroutine(animCo);
        animCo = StartCoroutine(AnimateTextChange(finalText));
    }

    public void SetVisible(bool visible)
    {
        if (panelRoot != null)
            panelRoot.SetActive(visible);
    }

    private IEnumerator AnimateTextChange(string newText)
    {
        if (canvasGroup != null)
        {
            // Fade out
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            if (questIcon != null)
            {
                Vector3 iconBase = questIcon.localScale;

                t = 0f;
                while (t < pulseDuration)
                {
                    t += Time.unscaledDeltaTime;
                    questIcon.localScale = Vector3.Lerp(iconBase, iconBase * pulseScale, t / pulseDuration);
                    yield return null;
                }

                t = 0f;
                while (t < pulseDuration)
                {
                    t += Time.unscaledDeltaTime;
                    questIcon.localScale = Vector3.Lerp(iconBase * pulseScale, iconBase, t / pulseDuration);
                    yield return null;
                }

                questIcon.localScale = iconBase;
            }

        }

        questLineText.text = newText;

        // Pulse
        if (panelRoot != null)
        {
            Vector3 baseScale = panelRoot.transform.localScale;

            float t1 = 0f;
            while (t1 < pulseDuration)
            {
                t1 += Time.unscaledDeltaTime;
                panelRoot.transform.localScale = Vector3.Lerp(baseScale, baseScale * pulseScale, t1 / pulseDuration);
                yield return null;
            }

            float t2 = 0f;
            while (t2 < pulseDuration)
            {
                t2 += Time.unscaledDeltaTime;
                panelRoot.transform.localScale = Vector3.Lerp(baseScale * pulseScale, baseScale, t2 / pulseDuration);
                yield return null;
            }

            panelRoot.transform.localScale = baseScale;
        }

        if (canvasGroup != null)
        {
            // Fade in
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
    }
}


