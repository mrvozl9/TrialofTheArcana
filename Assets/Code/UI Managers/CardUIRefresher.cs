using UnityEngine;

public static class CardUIRefresher
{
    public static void RefreshAllCards()
    {
        BookCardPage[] pages = Object.FindObjectsOfType<BookCardPage>(true);

        foreach (var page in pages)
        {
            page.Refresh();
        }

        Debug.Log($"🔄 Refreshed {pages.Length} card pages");
    }
}
