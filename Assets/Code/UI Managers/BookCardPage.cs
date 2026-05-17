using UnityEngine;
using UnityEngine.UI;

public class BookCardPage : MonoBehaviour
{
    public enum CardType
    {
        Fool,
        Magician
    }

    public CardType cardType;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = CardFlags.IsUnlocked(cardType);
        image.sprite = unlocked ? unlockedSprite : lockedSprite;
    }

    public static void RefreshAll()
    {
        BookCardPage[] pages = FindObjectsOfType<BookCardPage>(true);
        foreach (var page in pages)
            page.Refresh();

        Debug.Log("🃏 Card pages refreshed");
    }
}
