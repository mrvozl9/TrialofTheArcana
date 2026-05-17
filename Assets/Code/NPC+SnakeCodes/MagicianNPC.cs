using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MagicianNPC : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip hintSound;
    public AudioClip giveItemsSound;
    public AudioClip missingItemsSound;
    [Range(0f, 1f)] public float volume = 1f;

    [Header("Dialogue")]
    [SerializeField] private string magicianName = "Magician";

    [Tooltip("İlk konuşma (tek seferlik). Bitince quest başlar.")]
    [TextArea(2, 6)]
    [SerializeField] private List<string> introDialogueLines = new List<string>();

    [Tooltip("Quest tamamlanınca oynatılacak (tek seferlik). Bitince ödüller verilir.")]
    [TextArea(2, 6)]
    [SerializeField] private List<string> completionDialogueLines = new List<string>();

    [Tooltip("Quest bittikten sonra tekrar tekrar konuşma.")]
    [TextArea(2, 6)]
    [SerializeField] private List<string> postQuestDialogueLines = new List<string>();

    [Header("Prompt Messages")]
    [SerializeField] private string promptFirstTalk = "Press F to talk to the magician.";
    [SerializeField] private string promptGiveItems = "Press F to give items to the magician.";
    [SerializeField] private string questTopMessage = "Magician: Bring me the 4 sacred elements!";

    private bool playerNear = false;
    private bool hasGivenHint = false;   // intro tamamlandı / quest başladı
    private bool questComplete = false;

    private bool waitingIntroDialogueEnd = false;
    private bool waitingCompletionDialogueEnd = false;

    private AudioSource audioSource;

    public static bool hasCup = false;
    public static bool hasStaff = false;
    public static bool hasSword = false;
    public static bool hasMedal = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerNear = true;

        // Dialogue açıkken prompt basmayalım
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsOpen) return;

        if (!hasGivenHint && !questComplete)
            ShowMessage(promptFirstTalk);
        else if (!questComplete)
            ShowMessage(promptGiveItems);
        else
            ShowMessage(promptFirstTalk);
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        playerNear = false;
        ShowMessage("");
    }

    void Update()
    {
        if (!playerNear) return;

        // Intro / completion dialogue bekleme: panel kapanınca devam et
        if (waitingIntroDialogueEnd)
        {
            if (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen)
            {
                waitingIntroDialogueEnd = false;
                OnIntroDialogueFinished();
            }
            return;
        }

        if (waitingCompletionDialogueEnd)
        {
            if (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen)
            {
                waitingCompletionDialogueEnd = false;
                OnCompletionDialogueFinished();
            }
            return;
        }

        if (!Input.GetKeyDown(KeyCode.F)) return;

        // 1) Quest bittiyse: tekrar tekrar konuşma
        if (questComplete)
        {
            if (DialogueManager.Instance != null && postQuestDialogueLines != null && postQuestDialogueLines.Count > 0)
            {
                ShowMessage("");
                DialogueManager.Instance.StartDialogue(magicianName, postQuestDialogueLines);
            }
            return;
        }

        // 2) İlk kez konuşma: intro dialogue (tek sefer)
        if (!hasGivenHint)
        {
            if (DialogueManager.Instance == null || introDialogueLines == null || introDialogueLines.Count == 0)
            {
                // Dialogue yoksa direkt quest başlat
                hasGivenHint = true;

                // Quest UI: "Find the Magician" bitti
                QuestSequenceManager.Instance?.CompleteStep(QuestStepId.FindMagician);

                StartQuestAndShowTopMessage();
                return;
            }

            ShowMessage("");
            PlaySound(hintSound);
            DialogueManager.Instance.StartDialogue(magicianName, introDialogueLines);
            waitingIntroDialogueEnd = true;
            return;
        }

        // 3) Quest aktif: item kontrol
        if (!questComplete && AllItemsCollected())
        {
            // completion dialogue (tek sefer), bitince ödüller + quest complete
            if (DialogueManager.Instance != null && completionDialogueLines != null && completionDialogueLines.Count > 0)
            {
                ShowMessage("");
                PlaySound(giveItemsSound);
                DialogueManager.Instance.StartDialogue(magicianName, completionDialogueLines);
                waitingCompletionDialogueEnd = true;
            }
            else
            {
                PlaySound(giveItemsSound);
                OnCompletionDialogueFinished();
            }
            return;
        }

        // 4) Eksik item: quest mesajı
        if (!questComplete)
        {
            PlaySound(missingItemsSound);
            ShowMessage(questTopMessage);
            // İstersen eksik listesi de göster:
            // ShowMessage($"You need: {GetMissingItemsList()}");
        }
    }

    private void OnIntroDialogueFinished()
    {
        hasGivenHint = true;

        // Quest UI: "Find the Magician" bitti
        QuestSequenceManager.Instance?.CompleteStep(QuestStepId.FindMagician);

        StartQuestAndShowTopMessage();
    }

    private void StartQuestAndShowTopMessage()
    {
        ShowMessage(questTopMessage);

        // (Eski quest log sistemin varsa aynen devam)
        QuestLog.Instance?.StartQuest("Magician");
        QuestLog.Instance?.Add("Magician", "Find 4 sacred elements:");
        QuestLog.Instance?.Add("Magician", "  - Cup");
        QuestLog.Instance?.Add("Magician", "  - Staff");
        QuestLog.Instance?.Add("Magician", "  - Sword");
        QuestLog.Instance?.Add("Magician", "  - Medal");
    }

    private void OnCompletionDialogueFinished()
    {
        if (questComplete) return;

        questComplete = true;

        // Quest UI: "BringElements" bitti
        QuestSequenceManager.Instance?.CompleteStep(QuestStepId.BringElements);

        // (Eski quest log sistemin varsa aynen devam)
        QuestLog.Instance?.Add("Magician", "Gave all items to the Magician");
        QuestLog.Instance?.CompleteQuest("Magician");

        // Ödüller
        StartCoroutine(GiveRewardsAfterDelay(2f));

        // Prompt güncelle
        if (playerNear)
            ShowMessage(promptFirstTalk);
    }

    bool AllItemsCollected()
    {
        return hasCup && hasStaff && hasSword && hasMedal;
    }

    string GetMissingItemsList()
    {
        var missing = new List<string>();
        if (!hasCup) missing.Add("Cup");
        if (!hasStaff) missing.Add("Staff");
        if (!hasSword) missing.Add("Sword");
        if (!hasMedal) missing.Add("Medal");
        return string.Join(", ", missing);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip && audioSource)
            audioSource.PlayOneShot(clip, volume);
    }

    void ShowMessage(string msg)
    {
        if (UIManager.Instance)
            UIManager.Instance.ShowMessage(msg);
    }

    IEnumerator GiveRewardsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 🃏 Give Magician card
        CardFlags.hasMagician = true;
        BookCardPage.RefreshAll();
        UIManager.Instance?.ShowMessage("You acquired the Magician card!");

        yield return new WaitForSeconds(2f);

        // 🛡️ Give Shield ability (Invincibility)
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.UnlockInvincibility();
            UIManager.Instance?.ShowMessage("You learned Shield ability! Press S to activate!");
        }
    }

    // (İstersen dışarıdan item toplandığında çağırırsın)
    public static void UpdateQuestProgress()
    {
        if (QuestLog.Instance?.IsQuestActive("Magician") == true)
        {
            int collected = 0;
            if (hasCup) collected++;
            if (hasStaff) collected++;
            if (hasSword) collected++;
            if (hasMedal) collected++;

            QuestLog.Instance?.Add("Magician", $"Collected {collected}/4 items");
        }
    }
}

