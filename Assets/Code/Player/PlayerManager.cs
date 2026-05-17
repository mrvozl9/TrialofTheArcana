using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [Header("Auto-Save Settings")]
    [SerializeField] private bool enableAutoSave = true;
    [SerializeField] private float autoSaveInterval = 60f;

    private float autoSaveTimer = 0f;

    private void Start()
    {
        LoadPlayerData();

        // 🔄 Refresh card UI when scene loads
        BookCardPage.RefreshAll();
    }

    private void Update()
    {
        if (enableAutoSave)
        {
            autoSaveTimer += Time.deltaTime;

            if (autoSaveTimer >= autoSaveInterval)
            {
                SaveSystem.Instance.SaveGame(gameObject);

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.ShowMessage("Game Auto-Saved!");
                }

                Debug.Log("💾 Auto-save completed!");
                autoSaveTimer = 0f;
            }
        }
    }

    private void LoadPlayerData()
    {
        PlayerData data = SaveSystem.Instance.LoadGame();

        if (data == null)
        {
            Debug.Log("📝 No save data found. Starting fresh.");
            return;
        }

        string currentSceneName = SceneManager.GetActiveScene().name;

        if (data.sceneName == currentSceneName)
        {
            SaveSystem.Instance.ApplyLoadedData(gameObject, data);

            if (UIManager.Instance != null)
            {
                UIManager.Instance.ShowMessage("Game Loaded!");
            }
        }
        else
        {
            Debug.Log($"⚠️ Save is for scene '{data.sceneName}' but current scene is '{currentSceneName}'");
        }
    }

    public void ManualSave()
    {
        SaveSystem.Instance.SaveGame(gameObject);

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowMessage("Game Saved!");
        }

        Debug.Log("💾 Manual save completed!");
    }

    private void OnApplicationQuit()
    {
        if (enableAutoSave)
        {
            SaveSystem.Instance.SaveGame(gameObject);
            Debug.Log("💾 Game saved on quit!");
        }
    }
}