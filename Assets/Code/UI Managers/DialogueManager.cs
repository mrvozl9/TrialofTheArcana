using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextIndicator;

    [Header("Buttons")]
    [SerializeField] private UnityEngine.UI.Button skipButton; // <-- Skip butonu

    [Header("Settings")]
    [SerializeField] private float typeSpeed = 0.02f;

    [Header("Player Lock")]
    [Tooltip("Player hareketini kontrol eden scriptleri buraya sürükle. Diyalog açýlýnca kapanýr, bitince açýlýr.")]
    [SerializeField] private MonoBehaviour[] playerMovementScriptsToDisable;

    private List<string> lines;
    private int index;
    private bool isTyping;

    // NPCDialogue için uyumluluk
    public bool IsOpen => dialoguePanel != null && dialoguePanel.activeSelf;
    public void CloseDialogue() => EndDialogue();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (nextIndicator != null) nextIndicator.SetActive(false);

        // Skip butonu týklamasý
        if (skipButton != null)
            skipButton.onClick.AddListener(SkipDialogue);
    }

    public void StartDialogue(string speakerName, List<string> dialogueLines)
    {
        if (dialoguePanel == null || dialogueText == null || nameText == null)
        {
            Debug.LogError("DialogueManager UI references missing!");
            return;
        }

        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            Debug.LogWarning("StartDialogue: dialogueLines boþ.");
            return;
        }

        // Player kilitle
        SetPlayerMovementLocked(true);

        nameText.text = speakerName;
        lines = dialogueLines;
        index = 0;

        dialoguePanel.SetActive(true);
        if (nextIndicator != null) nextIndicator.SetActive(false);

        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (isTyping)
            {
                StopAllCoroutines();
                dialogueText.text = lines[index];
                isTyping = false;
                if (nextIndicator != null) nextIndicator.SetActive(true);
            }
            else
            {
                NextLine();
            }
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char c in lines[index])
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        isTyping = false;
        if (nextIndicator != null) nextIndicator.SetActive(true);
    }

    private void NextLine()
    {
        if (nextIndicator != null) nextIndicator.SetActive(false);

        if (index < lines.Count - 1)
        {
            index++;
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    private void SkipDialogue()
    {
        // Skip: direkt kapat
        EndDialogue();
    }

    private void EndDialogue()
    {
        StopAllCoroutines();
        isTyping = false;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (dialogueText != null) dialogueText.text = "";
        if (nameText != null) nameText.text = "";
        if (nextIndicator != null) nextIndicator.SetActive(false);

        // Player kilidini kaldýr
        SetPlayerMovementLocked(false);
    }

    private void SetPlayerMovementLocked(bool locked)
    {
        if (playerMovementScriptsToDisable == null) return;

        for (int i = 0; i < playerMovementScriptsToDisable.Length; i++)
        {
            if (playerMovementScriptsToDisable[i] == null) continue;
            playerMovementScriptsToDisable[i].enabled = !locked;
        }

        // Ýstersen burada Rigidbody2D hýzýný da sýfýrlayabilirsin:
        // var rb = FindObjectOfType<Rigidbody2D>();
        // if (locked && rb != null) rb.velocity = Vector2.zero;
    }
}


