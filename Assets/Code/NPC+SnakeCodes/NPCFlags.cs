using UnityEngine;

/// <summary>
/// Tracks which NPCs have been interacted with (similar to CardFlags)
/// </summary>
public static class NPCFlags
{
    // Track which ability NPCs have been used
    public static bool dashNPCUsed = false;
    public static bool floatNPCUsed = false;
    public static bool wallHoldNPCUsed = false;

    /// <summary>
    /// Mark an NPC as used based on ability type
    /// </summary>
    public static void SetNPCUsed(AbilityNPC.AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityNPC.AbilityType.Dash:
                dashNPCUsed = true;
                Debug.Log("🐱 Dash NPC marked as used");
                break;
            case AbilityNPC.AbilityType.Float:
                floatNPCUsed = true;
                Debug.Log("🦅 Float NPC marked as used");
                break;
            case AbilityNPC.AbilityType.WallHold:
                wallHoldNPCUsed = true;
                Debug.Log("🧗 Wall Hold NPC marked as used");
                break;
        }
    }

    /// <summary>
    /// Check if an NPC has been used
    /// </summary>
    public static bool IsNPCUsed(AbilityNPC.AbilityType abilityType)
    {
        switch (abilityType)
        {
            case AbilityNPC.AbilityType.Dash:
                return dashNPCUsed;
            case AbilityNPC.AbilityType.Float:
                return floatNPCUsed;
            case AbilityNPC.AbilityType.WallHold:
                return wallHoldNPCUsed;
            default:
                return false;
        }
    }

    /// <summary>
    /// Load NPC states from PlayerData
    /// </summary>
    public static void LoadFromData(PlayerData data)
    {
        if (data == null)
        {
            Debug.LogWarning("⚠️ Attempted to load NPCs from null data");
            return;
        }

        dashNPCUsed = data.dashNPCUsed;
        floatNPCUsed = data.floatNPCUsed;
        wallHoldNPCUsed = data.wallHoldNPCUsed;

        Debug.Log($"🎮 NPCs loaded: Dash={dashNPCUsed}, Float={floatNPCUsed}, WallHold={wallHoldNPCUsed}");
    }

    /// <summary>
    /// Reset all NPC flags (for new game)
    /// </summary>
    public static void ResetAll()
    {
        dashNPCUsed = false;
        floatNPCUsed = false;
        wallHoldNPCUsed = false;
        Debug.Log("🧹 NPC flags reset");
    }

    /// <summary>
    /// Debug: Print current NPC status
    /// </summary>
    public static void PrintStatus()
    {
        Debug.Log($"🎮 NPC Status - Dash: {dashNPCUsed}, Float: {floatNPCUsed}, WallHold: {wallHoldNPCUsed}");
    }
}