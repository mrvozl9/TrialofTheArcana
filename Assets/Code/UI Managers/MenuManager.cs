using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Button References")]
    public Button continueButton;
    public Button newGameButton;
    public Button quitButton;

    [Header("Save Info Display (Optional)")]
    public TextMeshProUGUI saveInfoText;

    [Header("Audio")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip menuMusicClip;
    private AudioSource audioSource;
    private AudioSource musicSource;

    [Header("Settings")]
    [SerializeField] private string firstLevelSceneName = "MainScene";

    void Start()
    {
        SetupAudio();
        UpdateContinueButton();
    }

    private void SetupAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        musicSource.playOnAwake = false;

        if (menuMusicClip != null)
        {
            musicSource.clip = menuMusicClip;
            musicSource.Play();
        }
    }

    private void UpdateContinueButton()
    {
        bool hasSavedGame = SaveSystem.Instance.HasSavedGame();

        if (continueButton != null)
        {
            continueButton.interactable = hasSavedGame;

            if (hasSavedGame && saveInfoText != null)
            {
                saveInfoText.text = SaveSystem.Instance.GetSavePreview();
            }
            else if (saveInfoText != null)
            {
                saveInfoText.text = "No saved game";
            }

            Debug.Log(hasSavedGame ? "✅ Saved game found!" : "⚠️ No saved game found.");
        }
        else
        {
            Debug.LogError("❌ Continue Button not assigned in Inspector!");
        }
    }

    public void ContinueGame()
    {
        PlaySound(buttonClickSound);

        PlayerData data = SaveSystem.Instance.LoadGame();

        if (data != null)
        {
            Debug.Log($"▶️ Continuing saved game: {data.sceneName}");
            SceneManager.LoadScene(data.sceneName);
        }
        else
        {
            Debug.LogError("❌ Failed to load game data!");
        }
    }

    public void StartNewGame()
    {
        PlaySound(buttonClickSound);
        Debug.Log("🆕 START NEW GAME pressed");

        // Delete save and reset everything
        SaveSystem.Instance.DeleteSave();
        CardFlags.ResetAll(); // Already resets in DeleteSave, but redundancy is safe

        SceneManager.LoadScene(firstLevelSceneName);
    }




    public void QuitGame()
    {
        PlaySound(buttonClickSound);

        Debug.Log("👋 Quitting game...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void DeleteSaveData()
    {
        PlaySound(buttonClickSound);

        SaveSystem.Instance.DeleteSave();
        UpdateContinueButton();

        Debug.Log("🗑️ Save data deleted!");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}