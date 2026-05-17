using UnityEngine;

public static class CardFlags
{
    public static bool hasFool;
    public static bool hasMagician;

    public static bool IsUnlocked(BookCardPage.CardType type)
    {
        switch (type)
        {
            case BookCardPage.CardType.Fool:
                return hasFool;
            case BookCardPage.CardType.Magician:
                return hasMagician;
        }
        return false;
    }

    /// <summary>
    /// Load card states from PlayerData
    /// </summary>
    public static void LoadFromData(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogWarning("⚠️ Attempted to load cards from null data");
            return;
        }

        hasFool = data.hasFool;
        hasMagician = data.hasMagician;

        Debug.Log($"🃏 Cards loaded: Fool={hasFool}, Magician={hasMagician}");
    }

    /// <summary>
    /// Save card states to PlayerData
    /// </summary>
    public static void SaveToData(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogWarning("⚠️ Attempted to save cards to null data");
            return;
        }

        data.hasFool = hasFool;
        data.hasMagician = hasMagician;

        Debug.Log($"💾 Cards saved: Fool={hasFool}, Magician={hasMagician}");
    }

    /// <summary>
    /// Reset all card flags (for new game)
    /// </summary>
    public static void ResetAll()
    {
        hasFool = false;
        hasMagician = false;
        Debug.Log("🧹 Card flags reset");
    }

    /// <summary>
    /// Get total number of unlocked cards
    /// </summary>
    public static int GetUnlockedCount()
    {
        int count = 0;
        if (hasFool) count++;
        if (hasMagician) count++;
        return count;
    }

    /// <summary>
    /// Debug: Print current card status
    /// </summary>
    public static void PrintStatus()
    {
        Debug.Log($"🃏 Card Status - Fool: {hasFool}, Magician: {hasMagician} (Total: {GetUnlockedCount()}/2)");
    }
}