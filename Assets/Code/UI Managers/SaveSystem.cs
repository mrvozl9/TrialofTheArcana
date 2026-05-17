using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string sceneName;

    public float positionX;
    public float positionY;
    public float positionZ;
    public float rotationY;

    public int currentHearts;
    public int maxHearts;
    public bool hasKey;

    public bool canDash;
    public bool canFloat;
    public bool canWallHold;
    public bool canInvincibility;

    // ✅ NPC FLAGS (FIXED)
    public bool dashNPCUsed;
    public bool floatNPCUsed;
    public bool wallHoldNPCUsed;

    public float respawnX;
    public float respawnY;
    public float respawnZ;

    // Tutorial
    public bool tutorialCompleted;

    // CARD SAVE DATA
    public bool hasFool;
    public bool hasMagician;

    // QUEST SAVE DATA
    public List<string> activeQuestNames = new List<string>();
    public List<QuestStepData> questSteps = new List<QuestStepData>();
}

[System.Serializable]
public class QuestStepData
{
    public string questName;
    public List<string> steps;
}

public class SaveSystem : MonoBehaviour
{
    private static SaveSystem instance;
    public static SaveSystem Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("SaveSystem");
                instance = go.AddComponent<SaveSystem>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    private const string SAVE_KEY = "SavedGame";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // ================= SAVE =================
    public void SaveGame(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        TutorialManager tutorial = FindObjectOfType<TutorialManager>();

        PlayerData data = new PlayerData
        {
            sceneName = SceneManager.GetActiveScene().name,

            positionX = player.transform.position.x,
            positionY = player.transform.position.y,
            positionZ = player.transform.position.z,
            rotationY = player.transform.eulerAngles.y,

            currentHearts = controller.CurrentHearts,
            maxHearts = controller.MaxHearts,
            hasKey = controller.HasKey,

            canDash = controller.CanDash,
            canFloat = controller.CanFloatSkill,
            canWallHold = controller.IsWallHoldUnlocked,
            canInvincibility = controller.CanInvincibility,

            // ✅ SAVE NPC FLAGS (FIXED)
            dashNPCUsed = NPCFlags.IsNPCUsed(AbilityNPC.AbilityType.Dash),
            floatNPCUsed = NPCFlags.IsNPCUsed(AbilityNPC.AbilityType.Float),
            wallHoldNPCUsed = NPCFlags.IsNPCUsed(AbilityNPC.AbilityType.WallHold),

            respawnX = player.transform.position.x,
            respawnY = player.transform.position.y,
            respawnZ = player.transform.position.z,

            tutorialCompleted =
                tutorial != null && tutorial.enabled == false ||
                (controller.CanDash && controller.CanFloatSkill),

            // SAVE CARDS
            hasFool = CardFlags.hasFool,
            hasMagician = CardFlags.hasMagician
        };

        // SAVE QUESTS
        if (QuestLog.Instance != null)
        {
            var activeQuests = QuestLog.Instance.GetActiveQuests();

            foreach (var quest in activeQuests)
            {
                data.activeQuestNames.Add(quest.Key);
                data.questSteps.Add(new QuestStepData
                {
                    questName = quest.Key,
                    steps = new List<string>(quest.Value)
                });
            }
        }

        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data, true));
        PlayerPrefs.Save();

        Debug.Log($"💾 Game saved (NPCs - Dash:{data.dashNPCUsed}, Float:{data.floatNPCUsed}, WallHold:{data.wallHoldNPCUsed})");
    }

    // ================= LOAD =================
    public PlayerData LoadGame()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
            return null;

        return JsonUtility.FromJson<PlayerData>(
            PlayerPrefs.GetString(SAVE_KEY));
    }

    public void ApplyLoadedData(GameObject player, PlayerData data)
    {
        // ✅ RESTORE NPC FLAGS FIRST (FIXED)
        if (data.dashNPCUsed)
            NPCFlags.SetNPCUsed(AbilityNPC.AbilityType.Dash);
        if (data.floatNPCUsed)
            NPCFlags.SetNPCUsed(AbilityNPC.AbilityType.Float);
        if (data.wallHoldNPCUsed)
            NPCFlags.SetNPCUsed(AbilityNPC.AbilityType.WallHold);

        // RESTORE CARDS
        CardFlags.hasFool = data.hasFool;
        CardFlags.hasMagician = data.hasMagician;

        // RESTORE QUESTS
        if (QuestLog.Instance != null && data.questSteps != null)
        {
            foreach (var questData in data.questSteps)
            {
                if (data.activeQuestNames.Contains(questData.questName))
                {
                    QuestLog.Instance.StartQuest(questData.questName);

                    foreach (var step in questData.steps)
                    {
                        QuestLog.Instance.Add(questData.questName, step);
                    }
                }
            }
        }

        PlayerController controller = player.GetComponent<PlayerController>();

        player.transform.position =
            new Vector3(data.positionX, data.positionY, data.positionZ);

        player.transform.eulerAngles =
            new Vector3(0, data.rotationY, 0);

        if (data.canDash) controller.UnlockDash();
        if (data.canFloat) controller.UnlockFloat();
        if (data.canWallHold) controller.UnlockWallHold();
        if (data.canInvincibility) controller.UnlockInvincibility();

        TutorialManager tutorial = FindObjectOfType<TutorialManager>();
        if (tutorial != null && data.tutorialCompleted)
        {
            tutorial.enabled = false;
        }

        // REFRESH UI AFTER LOADING
        BookCardPage.RefreshAll();
        BookQuestListPage.RefreshAll();
        BookQuestDetailsPage.RefreshAll();

        Debug.Log($"📂 Game loaded (NPCs - Dash:{data.dashNPCUsed}, Float:{data.floatNPCUsed}, WallHold:{data.wallHoldNPCUsed})");
    }

    // ================= PREVIEW =================
    public string GetSavePreview()
    {
        if (!HasSavedGame())
            return "No save data";

        PlayerData data = LoadGame();
        if (data == null)
            return "No save data";

        return
            $"Scene: {data.sceneName}\n" +
            $"Hearts: {data.currentHearts}/{data.maxHearts}";
    }

    // ================= UTIL =================
    public bool HasSavedGame()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();

        // ✅ RESET NPC FLAGS (FIXED)
        NPCFlags.ResetAll();

        // RESET CARDS
        CardFlags.ResetAll();

        // RESET QUESTS
        if (QuestLog.Instance != null)
            QuestLog.Instance.ResetAllQuests();

        Debug.Log("🗑 Save deleted, NPCs, cards & quests reset");
    }
}