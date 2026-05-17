using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HealthUIManager : MonoBehaviour
{
    [Header("UI Atamaları")]
    public GameObject heartPrefab;
    public Transform heartContainer;

    private List<GameObject> hearts = new List<GameObject>();

    private void OnEnable()
    {
        PlayerController.OnHealthChanged += UpdateHealthUI;
    }

    private void OnDisable()
    {
        PlayerController.OnHealthChanged -= UpdateHealthUI;
    }

    private void UpdateHealthUI(int currentHealth, int maxHealth)
    {
        // 1️⃣ Max can kadar kalp oluştur / sil
        if (hearts.Count < maxHealth)
        {
            for (int i = hearts.Count; i < maxHealth; i++)
            {
                GameObject newHeart = Instantiate(heartPrefab, heartContainer);
                hearts.Add(newHeart);
            }
        }
        else if (hearts.Count > maxHealth)
        {
            for (int i = hearts.Count - 1; i >= maxHealth; i--)
            {
                Destroy(hearts[i]);
                hearts.RemoveAt(i);
            }
        }

        // 2️⃣ SAĞDAN SOLA doğru kalpleri "görsel olarak" kapat
        int heartsToHide = maxHealth - currentHealth;

        for (int i = 0; i < hearts.Count; i++)
        {
            bool shouldHide = i >= hearts.Count - heartsToHide;

            GameObject heart = hearts[i];

            // Görseli kapat (Image / child ne varsa)
            heart.GetComponent<Image>().enabled = !shouldHide;

            // AMA layout'ta yerini koru
            LayoutElement le = heart.GetComponent<LayoutElement>();
            if (le != null)
                le.ignoreLayout = false;
        }
    }
}



