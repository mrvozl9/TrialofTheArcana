using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// SIMPLE Snake NPC - Just collects pearls and gives cup
/// Dialogue flow added:
/// - Intro dialogue (one-time)
/// - Completion dialogue (one-time)
/// - Repeat dialogue (after completion)
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SnakeNPC : MonoBehaviour
{
    public enum Type { Pearl, Snake }
    public Type objectType;

    [Header("Snake Settings")]
    public int pearlsRequired = 1;

    [Header("Rewards")]
    public GameObject cupPrefab;
    public Transform cupSpawnPoint;

    [Header("Audio")]
    public AudioClip pearlClip;
    public AudioClip hintSound;
    public AudioClip rewardSound;
    public AudioClip failSound;
    [Range(0f, 1f)] public float collectionVolume = 1f;

    [Header("Dialogue (you will write the lines)")]
    [SerializeField] private string snakeName = "Snake";
    [SerializeField] private List<string> introDialogue = new List<string>();        // one-time
    [SerializeField] private List<string> completionDialogue = new List<string>();   // one-time
    [SerializeField] private List<string> repeatDialogue = new List<string>();       // repeatable

    [Header("UI Prompt Text")]
    [SerializeField] private string promptTalk = "Press F to talk to the snake.";
    [SerializeField] private string promptGive = "Press F to give the pearl to the snake.";
    [SerializeField] private string needPearlMsg = "You need a pearl to give to the snake.";
    [SerializeField] private string findPearlMsg = "Find a pearl for the snake.";

    private bool playerNear = false;

    // State
    private bool introDone = false;          // intro dialogue played?
    private bool questComplete = false;      // reward already given?
    private bool completionDone = false;     // completion dialogue played?

    // Dialogue gating
    private bool waitingDialogue = false;

    private enum AfterDialogueAction { None, StartQuestAfterIntro, GiveRewardAfterCompletion }
    private AfterDialogueAction afterDialogueAction = AfterDialogueAction.None;

    private AudioSource audioSource;

    public static int playerPearls = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (objectType == Type.Pearl)
        {
            CollectPearl();
            return;
        }

        if (objectType == Type.Snake)
        {
            playerNear = true;

            // Prompt logic
            if (!introDone)
            {
                ShowMessage(promptTalk);
            }
            else if (!questComplete)
            {
                ShowMessage(promptGive);
            }
            else
            {
                ShowMessage(promptTalk); // after completion: can still talk (repeat dialogue)
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (objectType == Type.Snake && collision.CompareTag("Player"))
        {
            playerNear = false;
            // İstersen çıkınca mesajı temizle:
            // ShowMessage("");
        }
    }

    void Update()
    {
        if (objectType != Type.Snake || !playerNear)
            return;

        // Eğer diyalog açtıysak, kapanmasını bekle ve sonra aksiyonu uygula
        if (waitingDialogue)
        {
            if (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen)
            {
                waitingDialogue = false;
                RunAfterDialogueAction();
            }
            return;
        }

        if (!Input.GetKeyDown(KeyCode.F))
            return;

        // 1) Intro (one-time)
        if (!introDone)
        {
            introDone = true;

            PlaySound(hintSound);

            // Intro dialogue varsa aç, yoksa direkt quest başlat
            if (DialogueManager.Instance != null && introDialogue != null && introDialogue.Count > 0)
            {
                DialogueManager.Instance.StartDialogue(snakeName, introDialogue);
                waitingDialogue = true;
                afterDialogueAction = AfterDialogueAction.StartQuestAfterIntro;
            }
            else
            {
                // Intro yoksa direkt quest start
                StartQuestAfterIntro();
            }

            return;
        }

        // 2) Quest tamamlandıktan sonra: repeatable dialogue
        if (questComplete)
        {
            if (DialogueManager.Instance != null && repeatDialogue != null && repeatDialogue.Count > 0)
            {
                DialogueManager.Instance.StartDialogue(snakeName, repeatDialogue);
                waitingDialogue = true;
                afterDialogueAction = AfterDialogueAction.None;
            }
            else
            {
                // Repeat dialogue yoksa bir şey yapma / veya küçük bir mesaj
                // ShowMessage("...");
            }
            return;
        }

        // 3) Quest devam ediyor: pearl kontrol
        if (playerPearls >= pearlsRequired && !questComplete)
        {
            // Completion dialogue (one-time), sonra reward verilecek
            if (!completionDone)
            {
                completionDone = true;

                if (DialogueManager.Instance != null && completionDialogue != null && completionDialogue.Count > 0)
                {
                    DialogueManager.Instance.StartDialogue(snakeName, completionDialogue);
                    waitingDialogue = true;
                    afterDialogueAction = AfterDialogueAction.GiveRewardAfterCompletion;
                }
                else
                {
                    // Completion dialogue yoksa direkt reward
                    GiveRewardAfterCompletion();
                }

                return;
            }

            // completionDone true ise (teoride buraya pek düşmez), yine de reward ver
            GiveRewardAfterCompletion();
            return;
        }

        // 4) Pearl yok -> fail mesajı
        if (!questComplete)
        {
            PlaySound(failSound);
            ShowMessage(needPearlMsg);
        }
    }

    private void RunAfterDialogueAction()
    {
        switch (afterDialogueAction)
        {
            case AfterDialogueAction.StartQuestAfterIntro:
                StartQuestAfterIntro();
                break;

            case AfterDialogueAction.GiveRewardAfterCompletion:
                GiveRewardAfterCompletion();
                break;
        }

        afterDialogueAction = AfterDialogueAction.None;
    }

    private void StartQuestAfterIntro()
    {
        // Bu kısım eski koddaki quest log mantığını korur, sadece "give me a pearl..." cümlesi yok.
        QuestLog.Instance?.StartQuest("Snake");
        QuestLog.Instance?.Add("Snake", "Find a pearl for the snake");

        ShowMessage(findPearlMsg);
    }

    private void GiveRewardAfterCompletion()
    {
        if (questComplete) return;

        // Pearl düş
        playerPearls -= pearlsRequired;

        // Reward
        DropCup();
        PlaySound(rewardSound);

        // Mark cup collected
        MagicianNPC.hasCup = true;

        // Quest log complete
        QuestLog.Instance?.Add("Snake", "Gave pearl to the snake");
        QuestLog.Instance?.CompleteQuest("Snake");

        questComplete = true;

        // After completion, prompt can go back to talk
        ShowMessage(promptTalk);

        // Save istersen burada çağırabilirsin (projene göre)
        // SaveSystem.Instance?.SaveGame(GameObject.FindGameObjectWithTag("Player"));
    }

    void CollectPearl()
    {
        playerPearls++;
        ShowMessage($"Collected a pearl! ({playerPearls} total)");
        PlaySound(pearlClip);

        // Update quest if active
        if (QuestLog.Instance?.IsQuestActive("Snake") == true)
        {
            QuestLog.Instance?.Add("Snake", "Collected a pearl");
        }

        Destroy(gameObject, 0.5f);
    }

    void DropCup()
    {
        if (cupPrefab && cupSpawnPoint)
            Instantiate(cupPrefab, cupSpawnPoint.position, Quaternion.identity);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip && audioSource)
            audioSource.PlayOneShot(clip, collectionVolume);
    }

    void ShowMessage(string msg)
    {
        if (UIManager.Instance)
            UIManager.Instance.ShowMessage(msg);
    }
}
