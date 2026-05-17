using UnityEngine;
using System.Collections.Generic;

public class NPCDialogue : MonoBehaviour
{
    [Header("NPC")]
    [SerializeField] private string npcName = "NPC";

    [Header("Dialogue Lines")]
    [SerializeField] private List<string> dialogueLines = new List<string>();

    [Header("Interact")]
    [SerializeField] private KeyCode interactKey = KeyCode.F;
    [SerializeField] private float interactDistance = 2f;

    [Header("Prompt")]
    [SerializeField] private NPCWorldPrompt worldPrompt;
    [SerializeField] private Vector3 promptOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private string promptText = "F to interact";

    private Transform player;
    private bool inRange;
    private bool waitingDialogueClose;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (worldPrompt == null) worldPrompt = GetComponent<NPCWorldPrompt>();
        if (worldPrompt == null) worldPrompt = gameObject.AddComponent<NPCWorldPrompt>();

        worldPrompt.SetOffset(promptOffset);
        worldPrompt.SetText(promptText);
        worldPrompt.Hide();
    }

    private void Update()
    {
        if (player == null) return;

        // Dialogue açýkken bekle, kapanýnca prompt tekrar duruma göre
        if (waitingDialogueClose)
        {
            if (DialogueManager.Instance == null || !DialogueManager.Instance.IsOpen)
            {
                waitingDialogueClose = false;
                // oyuncu hala menzildeyse prompt tekrar göster
                if (inRange) worldPrompt.Show();
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);
        bool nowInRange = dist <= interactDistance;

        if (nowInRange && !inRange)
        {
            inRange = true;
            worldPrompt.Show();
        }
        else if (!nowInRange && inRange)
        {
            inRange = false;
            worldPrompt.Hide();
        }

        if (inRange && Input.GetKeyDown(interactKey))
        {
            if (DialogueManager.Instance != null && dialogueLines != null && dialogueLines.Count > 0)
            {
                worldPrompt.Hide();
                DialogueManager.Instance.StartDialogue(npcName, dialogueLines);
                waitingDialogueClose = true;
            }
        }
    }

    private void OnDisable()
    {
        if (worldPrompt != null) worldPrompt.Hide();
    }
}








