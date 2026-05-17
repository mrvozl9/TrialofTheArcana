using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIBook : MonoBehaviour
{
    [Header("References")]
    public RectTransform bookPanel;
    public Image leftPage;
    public Image rightPage;

    [Header("Corner Click Areas - Assign UI Buttons!")]
    public Button leftCornerButton;   // Click to flip LEFT
    public Button rightCornerButton;  // Click to flip RIGHT

    [Header("Pages (10 SPRITES TOTAL)")]
    [Tooltip("0=Front Cover, 1-2=Spread1, 3-4=Spread2, 5-6=Spread3, 7-8=Spread4, 9=Back Cover")]
    public Sprite[] pages; // Should have 10 sprites total

    [Header("Special Page Spreads")]
    public GameObject minimapSpread;
    [Tooltip("Page indices 1-2")]
    public int minimapStartPage = 1;

    public GameObject questSpread;
    [Tooltip("Page indices 3-4")]
    public int questStartPage = 3;

    public GameObject cardSpread;
    [Tooltip("Page indices 7-8")]
    public int cardStartPage = 7;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip flipSound;

    private int currentPageIndex;

    void Start()
    {
        Debug.Log("📚 UIBook Start - Setting up buttons...");

        // Setup button listeners
        if (leftCornerButton != null)
        {
            leftCornerButton.onClick.AddListener(FlipLeft);
            Debug.Log("✓ Left button listener added");
        }
        else
        {
            Debug.LogError("❌ Left Corner Button is NULL!");
        }

        if (rightCornerButton != null)
        {
            rightCornerButton.onClick.AddListener(FlipRight);
            Debug.Log("✓ Right button listener added");
        }
        else
        {
            Debug.LogError("❌ Right Corner Button is NULL!");
        }

        ResetBook();
    }

    // 🔹 CALLED WHEN BOOK OPENS - Start with front cover on RIGHT
    public void ResetBook()
    {
        Debug.Log("📚 ResetBook called");
        currentPageIndex = 0; // Front cover
        UpdatePages();
        UpdateSpecialPages();
        UpdateButtonVisibility();
    }

    public void FlipRight()
    {
        Debug.Log($"🔄 FlipRight called - Current page: {currentPageIndex}");

        // From front cover (0) on RIGHT -> first spread (1-2) MINIMAP
        if (currentPageIndex == 0)
        {
            currentPageIndex = 1;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to minimap spread {currentPageIndex}-{currentPageIndex + 1}");
            return;
        }

        // From minimap (1-2) -> quest spread (3-4)
        if (currentPageIndex == 1)
        {
            Debug.Log("📍 Detected minimap page, flipping to quest spread...");
            currentPageIndex = 3;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to quest spread {currentPageIndex}-{currentPageIndex + 1}");
            return;
        }

        // From quest spread (3-4) -> normal spread (5-6)
        if (currentPageIndex == 3)
        {
            currentPageIndex = 5;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to spread pages {currentPageIndex}-{currentPageIndex + 1}");
            return;
        }

        // From spread (5-6) -> card spread (7-8)
        if (currentPageIndex == 5)
        {
            currentPageIndex = 7;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to card spread {currentPageIndex}-{currentPageIndex + 1}");
            return;
        }

        // From card spread (7-8) -> back cover (9) on LEFT
        if (currentPageIndex == 7 && pages.Length > 9)
        {
            currentPageIndex = 9;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to back cover");
            return;
        }

        Debug.LogWarning($"⚠️ Can't flip right - already at end (currentPageIndex={currentPageIndex})");
    }

    public void FlipLeft()
    {
        Debug.Log($"🔄 FlipLeft called - Current page: {currentPageIndex}");

        // From back cover (9) on LEFT -> card spread (7-8)
        if (currentPageIndex == 9)
        {
            currentPageIndex = 7;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to card spread {currentPageIndex}-{currentPageIndex + 1}");
            return;
        }

        // From card spread (7-8) -> normal spread (5-6)
        if (currentPageIndex == 7)
        {
            currentPageIndex = 5;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to spread pages {currentPageIndex}-{currentPageIndex + 1}");
            return;
        }

        // From spread (5-6) -> quest spread (3-4)
        if (currentPageIndex == 5)
        {
            currentPageIndex = 3;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to quest spread {currentPageIndex}-{currentPageIndex + 1}");
            return;
        }

        // From quest spread (3-4) -> minimap (1-2)
        if (currentPageIndex == 3)
        {
            Debug.Log("📍 Detected quest spread, flipping to minimap...");
            currentPageIndex = 1;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to minimap spread {currentPageIndex}-{currentPageIndex + 1}");
            return;
        }

        // From minimap (1-2) -> front cover (0) on RIGHT
        if (currentPageIndex == 1)
        {
            Debug.Log("📍 Detected minimap page, flipping to cover...");
            currentPageIndex = 0;
            PlayFlip();
            UpdatePages();
            UpdateSpecialPages();
            UpdateButtonVisibility();
            Debug.Log($"✅ Flipped to front cover");
            return;
        }

        Debug.LogWarning($"⚠️ Can't flip left - already at start (currentPageIndex={currentPageIndex})");
    }

    private void UpdatePages()
    {
        Debug.Log($"🔧 UpdatePages called - currentPageIndex: {currentPageIndex}");

        // Safety check
        if (pages == null || pages.Length < 10)
        {
            Debug.LogError($"❌ Pages array must have 10 sprites! Currently has: {(pages != null ? pages.Length : 0)}");
            return;
        }

        // 📕 FRONT COVER - Show on RIGHT page only (like opening a book)
        if (currentPageIndex == 0)
        {
            leftPage.enabled = false;
            rightPage.enabled = true;
            rightPage.sprite = pages[0];
            Debug.Log("📕 Showing front cover on RIGHT");
            return;
        }

        // 📕 BACK COVER - Show on LEFT page only
        if (currentPageIndex == 9)
        {
            leftPage.enabled = true;
            leftPage.sprite = pages[9];
            rightPage.enabled = false;
            Debug.Log("📕 Showing back cover on LEFT");
            return;
        }

        // 🗺️ Check if this is the minimap spread (pages 1-2)
        if (currentPageIndex == minimapStartPage)
        {
            leftPage.enabled = false;
            rightPage.enabled = false;
            Debug.Log("🗺️ Minimap spread - hiding page sprites");
            return;
        }

        // 📜 Check if this is the quest spread (pages 3-4)
        if (currentPageIndex == questStartPage)
        {
            leftPage.enabled = false;
            rightPage.enabled = false;
            Debug.Log("📜 Quest spread - hiding page sprites");
            return;
        }

        // 🃏 Check if this is the card spread (pages 7-8)
        if (currentPageIndex == cardStartPage)
        {
            leftPage.enabled = false;
            rightPage.enabled = false;
            Debug.Log("🃏 Card spread - hiding page sprites");
            return;
        }

        // 📄 Show normal two-page spread
        if (currentPageIndex < pages.Length - 1)
        {
            leftPage.enabled = true;
            rightPage.enabled = true;
            leftPage.sprite = pages[currentPageIndex];
            rightPage.sprite = pages[currentPageIndex + 1];
            Debug.Log($"📄 Showing pages {currentPageIndex} and {currentPageIndex + 1}");
        }
    }

    private void UpdateSpecialPages()
    {
        Debug.Log($"🔧 UpdateSpecialPages called - currentPageIndex: {currentPageIndex}");

        // Minimap spread (pages 1-2)
        if (minimapSpread != null)
        {
            bool showMinimap = (currentPageIndex == minimapStartPage);
            minimapSpread.SetActive(showMinimap);

            if (showMinimap)
                Debug.Log("🗺️ Minimap spread activated");
            else
                Debug.Log("🗺️ Minimap spread deactivated");
        }
        else
        {
            Debug.LogWarning("⚠️ minimapSpread is NULL!");
        }

        // Quest spread (pages 3-4)
        if (questSpread != null)
        {
            bool showQuests = (currentPageIndex == questStartPage);
            questSpread.SetActive(showQuests);

            if (showQuests)
            {
                Debug.Log("📜 Quest spread activated");
                BookQuestListPage.RefreshAll();
            }
            else
            {
                Debug.Log("📜 Quest spread deactivated");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ questSpread is NULL!");
        }

        // Card spread (pages 7-8)
        if (cardSpread != null)
        {
            bool showCards = (currentPageIndex == cardStartPage);
            cardSpread.SetActive(showCards);

            if (showCards)
            {
                Debug.Log("🃏 Card spread activated - refreshing cards");
                BookCardPage.RefreshAll();
            }
            else
            {
                Debug.Log("🃏 Card spread deactivated");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ cardSpread is NULL!");
        }
    }

    private void UpdateButtonVisibility()
    {
        Debug.Log($"🔧 UpdateButtonVisibility - currentPageIndex: {currentPageIndex}");

        // Hide left arrow on front cover
        if (leftCornerButton != null)
        {
            bool shouldShow = (currentPageIndex != 0);
            leftCornerButton.gameObject.SetActive(shouldShow);
            Debug.Log($"Left button visible: {shouldShow}");
        }

        // Hide right arrow on back cover
        if (rightCornerButton != null)
        {
            bool shouldShow = (currentPageIndex != 9);
            rightCornerButton.gameObject.SetActive(shouldShow);
            Debug.Log($"Right button visible: {shouldShow}");
        }
    }

    private void PlayFlip()
    {
        if (audioSource != null && flipSound != null)
            audioSource.PlayOneShot(flipSound);
    }

    // 🔧 DEBUG: Validate your setup in Inspector
    void OnValidate()
    {
        if (pages != null && pages.Length != 10)
        {
            Debug.LogWarning($"⚠️ UIBook needs exactly 10 page sprites! Currently has: {pages.Length}");
        }
    }

    // 🔧 TESTING: Add keyboard shortcuts for debugging
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            Debug.Log("⌨️ Manual FlipRight triggered");
            FlipRight();
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            Debug.Log("⌨️ Manual FlipLeft triggered");
            FlipLeft();
        }
    }
}