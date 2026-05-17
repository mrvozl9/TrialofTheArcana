using UnityEngine;
using System.Collections.Generic;

public class QuestLog : MonoBehaviour
{
    public static QuestLog Instance;

    private Dictionary<string, List<string>> quests = new();
    private HashSet<string> activeQuests = new();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Initialize quest structures
        quests["Tutorial"] = new();
        quests["Snake"] = new();
        quests["Magician"] = new();
    }

    // 🔑 REQUIRED BY NPCs
    public bool IsQuestActive(string questName)
    {
        return activeQuests.Contains(questName);
    }

    public void StartQuest(string questName)
    {
        if (!quests.ContainsKey(questName)) return;

        if (activeQuests.Add(questName))
        {
            Debug.Log($"📜 Quest started: {questName}");
            BookQuestListPage.RefreshAll();
        }
    }

    // ✅ Modified to prevent duplicate steps
    public void Add(string questName, string line)
    {
        if (!quests.ContainsKey(questName)) return;

        if (!activeQuests.Contains(questName))
            StartQuest(questName);

        // Check if step already exists
        if (quests[questName].Contains(line))
        {
            Debug.Log($"⚠️ Step already exists: {questName} - {line}");
            return;
        }

        quests[questName].Add(line);
        Debug.Log($"✅ Quest step added: {questName} - {line}");
        BookQuestListPage.RefreshAll();
        BookQuestDetailsPage.RefreshAll();
    }

    public void CompleteQuest(string questName)
    {
        if (!quests.ContainsKey(questName)) return;
        Add(questName, "✓ COMPLETED");
    }

    public Dictionary<string, List<string>> GetActiveQuests()
    {
        var result = new Dictionary<string, List<string>>();
        foreach (var q in activeQuests)
            result[q] = quests[q];
        return result;
    }

    public List<string> GetQuestSteps(string questName)
    {
        return quests.ContainsKey(questName) ? quests[questName] : null;
    }

    // ✅ NEW: Method to restore quest data from save (used internally by SaveSystem)
    public void RestoreQuestData(string questName, List<string> steps)
    {
        if (!quests.ContainsKey(questName)) return;

        quests[questName] = new List<string>(steps);
        activeQuests.Add(questName);
        Debug.Log($"📂 Restored quest: {questName} ({steps.Count} steps)");
    }

    public void ResetAllQuests()
    {
        quests.Clear();
        activeQuests.Clear();

        // Re-initialize base quests
        quests["Tutorial"] = new();
        quests["Snake"] = new();
        quests["Magician"] = new();

        Debug.Log("📜 All quests reset");
    }

}