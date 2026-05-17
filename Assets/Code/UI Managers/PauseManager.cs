using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using System.Collections.Generic;

public class PauseManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel;

    [Tooltip("Pause açılınca gizlenecek UI root/panel objeleri (HUD, QuestTrackerPanel, TutorialPanel, vb.).")]
    [SerializeField] private List<GameObject> uiToHideOnPause = new List<GameObject>();

    [Header("Player Reference")]
    public GameObject player;

    [Header("Pause Background Video")]
    public VideoPlayer pauseVideo;
    public bool restartVideoOnPause = true;

    [Header("Audio")]
    [SerializeField] private AudioClip pauseSound;
    [SerializeField] private AudioClip resumeSound;
    [SerializeField] private AudioClip buttonClickSound;
    private AudioSource audioSource;

    public static bool isGamePaused = false;

    // UI'ları resume ederken eski aktifliklerine göre geri açmak için
    private readonly Dictionary<GameObject, bool> previousUIStates = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                Debug.LogWarning("⚠️ Player object not found!");
        }

        if (pausePanel != null)
            pausePanel.SetActive(false);
        else
            Debug.LogWarning("⚠️ Pause Panel not assigned in Inspector!");

        // Video başlangıç ayarı
        if (pauseVideo != null)
        {
            pauseVideo.playOnAwake = false;
            pauseVideo.isLooping = true;
            pauseVideo.Stop();
            // İsteğe bağlı: ilk açılış kasmasını azaltır
            pauseVideo.Prepare();
        }

        Time.timeScale = 1f;
        isGamePaused = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isGamePaused) ResumeGame();
            else PauseGame();
        }
    }

    public void ResumeGame()
    {
        // Video durdur
        if (pauseVideo != null)
            pauseVideo.Stop();

        // Pause panel kapat
        if (pausePanel != null)
            pausePanel.SetActive(false);

        // UI'ları eski hallerine göre geri aç
        RestoreHiddenUI();

        Time.timeScale = 1f;
        isGamePaused = false;

        PlaySound(resumeSound);
    }

    public void PauseGame()
    {
        // UI'ları gizle (pause panel açmadan önce state kaydet)
        HideUIForPause();

        // Pause panel aç
        if (pausePanel != null)
            pausePanel.SetActive(true);

        Time.timeScale = 0f;
        isGamePaused = true;

        // Video oynat
        if (pauseVideo != null)
        {
            if (!pauseVideo.isPrepared) pauseVideo.Prepare();
            if (restartVideoOnPause) pauseVideo.time = 0;
            pauseVideo.Play();
        }

        PlaySound(pauseSound);
    }

    public void SaveGame()
    {
        PlaySound(buttonClickSound);

        if (player != null)
        {
            SaveSystem.Instance.SaveGame(player);
            UIManager.Instance?.ShowMessage("Game Saved!");
        }
        else
        {
            Debug.LogError("❌ Player object not found! Cannot save.");
        }
    }

    public void LoadMenu()
    {
        PlaySound(buttonClickSound);

        if (player != null)
            SaveSystem.Instance.SaveGame(player);

        if (pauseVideo != null) pauseVideo.Stop();

        // Menüye dönerken oyun normale
        Time.timeScale = 1f;
        isGamePaused = false;

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        PlaySound(buttonClickSound);

        if (player != null)
            SaveSystem.Instance.SaveGame(player);

        if (pauseVideo != null) pauseVideo.Stop();

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void HideUIForPause()
    {
        previousUIStates.Clear();

        for (int i = 0; i < uiToHideOnPause.Count; i++)
        {
            GameObject ui = uiToHideOnPause[i];
            if (ui == null) continue;

            // aktiflik durumunu kaydet
            previousUIStates[ui] = ui.activeSelf;

            // kapat
            ui.SetActive(false);
        }
    }

    private void RestoreHiddenUI()
    {
        foreach (var kv in previousUIStates)
        {
            if (kv.Key == null) continue;
            kv.Key.SetActive(kv.Value); // eski state'e döndür
        }

        previousUIStates.Clear();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}


